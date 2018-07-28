using System;
using OpenAL;
using static OpenAL.AL10;
using static OpenAL.ALC10;
using static OpenAL.AL11;
using static OpenAL.ALC11;
using static Vox.ErrorHandler;
using static Vox.Internal.Util;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Vox.Internal;
using System.Buffers;

namespace Vox
{
    /// <summary>
    /// Represents a sound input device.
    /// </summary>
    public class CaptureDevice : IDisposable
    {
        private readonly static IntPtr NULL = IntPtr.Zero;
        private readonly ArrayPool<byte> _arrayPool;
        internal IntPtr _handle;

        /// <summary>
        /// Gets the sound format.
        /// </summary>
        public PCM Format { get; private set; }
        /// <summary>
        /// Gets the sound frequency.
        /// </summary>
        public int Frequency { get; private set; }
        /// <summary>
        /// Gets the buffer size.
        /// </summary>
        /// <returns></returns>
        public int BufferSize { get; private set; }

        /// <summary>
        /// Returns true if the device is currently capturing audio input.
        /// </summary>
        /// <returns></returns>
        public bool IsCapturing { get; private set; }

        /// <summary>
        /// Returns count of the available samples which can be requested.
        /// If application requests more samples, an exception will be thrown.
        /// </summary>
        public int AvailableSamples
        {
            get
            {
                var data = new int[1];
                ALC(() =>
                    alcGetIntegerv(_handle, ALC_CAPTURE_SAMPLES, 1, data),
                    "alcGetIntegerv(ALC_CAPTURE_SAMPLES)", _handle);
                return data[0];
            }
        }

        /// <summary>
        /// Returns bytes per sample count for the specified <see cref="Format" />.
        /// </summary>
        /// <returns></returns>
        public int BytesPerSample
        {
            get
            {
                switch (Format)
                {
                    case PCM.Mono8:
                    case PCM.Stereo8:
                        return 8;
                    case PCM.Mono16:
                    case PCM.Stereo16:
                        return 16;
                    default:
                        return -1;
                }
            }
        }

        /// <summary>
        /// Creates and opens the default sound input device.
        /// </summary>
        public CaptureDevice() : this(null) { }

        /// <summary>
        /// Creates and opens a sound input device with the specified parameters.
        /// </summary>
        /// <param name="name">Name of the device to open</param>
        /// <param name="frequency">Sample frequency</param>
        /// <param name="format">Sound format</param>
        /// <param name="bufSize">Buffer size</param>
        /// <param name="bufferPool">Array pool for buffers.false. If null, defaults to shared.</param>
        public CaptureDevice(string name, int frequency = 44100,
            PCM format = PCM.Mono8, int bufSize = 22050, ArrayPool<byte> bufferPool = null)
        {
            if (frequency <= 0)
                throw new ArgumentException("Frequency cannot be zero or negative");
            _handle = ALC(() =>
                alcCaptureOpenDevice(name, (uint)frequency, (int)format, bufSize),
                "alcCaptureOpenDevice", NULL);
            Format = format;
            Frequency = frequency;
            BufferSize = bufSize;
            _arrayPool = bufferPool ?? ArrayPool<byte>.Shared;
        }

        /// <summary>
        /// Starts the sound capture if it hasn't started.
        /// </summary>
        public void StartCapture()
        {
            if (disposed) throw new ObjectDisposedException(nameof(OutputDevice));
            if (IsCapturing) return;
            ALC(() => alcCaptureStart(_handle), "alcCaptureStart", _handle);
            IsCapturing = true;
        }

        /// <summary>
        /// Stops the sound capture if it's running.
        /// </summary>
        public void StopCapture()
        {
            if (disposed) throw new ObjectDisposedException(nameof(OutputDevice));
            if (!IsCapturing) return;
            ALC(() => alcCaptureStop(_handle), "alcCaptureStop", _handle);
            IsCapturing = false;
        }

        /// <summary>
        /// Toggles the sound capture.
        /// </summary>
        public void ToggleCapture()
        {
            if (disposed) throw new ObjectDisposedException(nameof(OutputDevice));
            if (IsCapturing) StopCapture(); else StartCapture();
        }

        /// <summary>
        /// Reads all available samples and passes them to the callback.
        /// </summary>
        /// <remarks>If you intend to use sample data later, <b>copy it</b>, because the buffer is pooled and will be reused.</remarks>
        public void ProcessSamples(Action<byte[]> callback) =>
            ProcessSamples(AvailableSamples, callback);

        /// <summary>
        /// Reads the specified amount of samples from the device and calls
        /// the specified callback.
        /// </summary>
        /// <param name="sampleCount">Sample count. Must be less or equal to <see cref="AvailableSamples" />.</param>
        /// <param name="callback">Callback</param>
        /// <remarks>If you intend to use sample data later, <b>copy it</b>, because the buffer is pooled and will be reused.</remarks>
        public void ProcessSamples(int sampleCount, Action<byte[]> callback)
        {
            if (disposed) throw new ObjectDisposedException(nameof(OutputDevice));
            if (AvailableSamples < sampleCount)
                throw new AudioLibraryException("Too many samples requested");
            var buffer = _arrayPool.Rent(BytesPerSample * sampleCount);
            unsafe
            {
                fixed (void* buf = buffer)
                {
                    ResetALC(_handle);
                    alcCaptureSamples(_handle, (IntPtr)buf, sampleCount);
                    CheckALC("alcCaptureSamples", _handle);
                    try
                    {
                        callback(buffer);
                    }
                    finally
                    {
                        _arrayPool.Return(buffer);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the default capture device.
        /// </summary>
        public static string GetDefaultCaptureDevice()
        {
            var ptr = ALC(() => alcGetString(NULL, ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER),
                "alcGetString(ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER", NULL);
            if (ptr == IntPtr.Zero || ptr == null) return null;
            return Marshal.PtrToStringAnsi(ptr);
        }

        /// <summary>
        /// Returns a list of available capture devices.
        /// </summary>
        public static IEnumerable<string> GetCaptureDevices()
        {
            var extPresent = ALC(() =>
                alcIsExtensionPresent(NULL, "ALC_ENUMERATION_EXT"),
                "alcIsExtensionPresent(ALC_ENUMERATION_EXT)",
                NULL);

            if (extPresent)
            {
                unsafe
                {
                    ResetALC(NULL);
                    byte* listData = (byte*)alcGetString(NULL, ALC_CAPTURE_DEVICE_SPECIFIER);
                    CheckALC("alcGetString(ALC_CAPTURE_DEVICE_SPECIFIER)", NULL);
                    return ParseDeviceString(listData);
                }
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Closes the device.
        /// </summary>
        public void Close() => ALC(() =>
            alcCaptureCloseDevice(_handle),
            "alcCaptureCloseDevice", _handle);

        bool disposed = false;
        /// <summary>
        /// Disposes and closes the device.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                Close();
                disposed = true;
            }
        }
    }
}