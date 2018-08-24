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

namespace Vox
{
    /// <summary>
    /// Declares possible sound attenuation models.
    /// </summary>
    public enum DistanceModel
    {
        None = AL_NONE,
        InverseDistance = AL_INVERSE_DISTANCE,
        InverseDistanceClamped = AL_INVERSE_DISTANCE_CLAMPED,
        LinearDistance = AL_LINEAR_DISTANCE,
        LinearDistanceClamped = AL_LINEAR_DISTANCE_CLAMPED,
        ExponentDistance = AL_EXPONENT_DISTANCE,
        ExponentDistanceClamped = AL_EXPONENT_DISTANCE_CLAMPED
    }

    /// <summary>
    /// Represents a sound output device.
    /// </summary>
    public class OutputDevice : IDisposable
    {
        private static readonly IntPtr NULL = IntPtr.Zero;

        private static OutputDevice s_current;
        public static OutputDevice Current => s_current;

        internal IntPtr _handle;
        private readonly DeviceContext _context;
        internal HashSet<IDisposable> _resources = new HashSet<IDisposable>();

        private readonly SoundListener _listener;

        /// <summary>
        /// Returns the listener associated with this device.
        /// </summary>
        public SoundListener Listener => _listener;

        private DistanceModel _distanceModel;
        /// <summary>
        /// Gets or sets the sound attenuation model.
        /// </summary>
        /// <remarks>Throws an exception if the device is inactive.</remarks>
        public DistanceModel DistanceModel
        {
            get => _distanceModel;
            set
            {
                if (s_current != this)
                    throw new AudioLibraryException("Cannot get distance model of inactive device");
                AL(() => alDistanceModel((int)value), "alDistanceModel");
                _distanceModel = value;
            }
        }

        /// <summary>
        /// Opens the default sound device and makes it current.
        /// </summary>
        public OutputDevice() : this(null) => MakeCurrent();

        /// <summary>
        /// Opens a sound device with the specified name.
        /// </summary>
        /// <param name="name">Device name</param>
        /// <seealso cref="GetOutputDevices" />
        /// <seealso cref="GetCaptureDevices" />
        public OutputDevice(string name)
        {
            _handle = ALC(() => alcOpenDevice(name), "alcOpenDevice", IntPtr.Zero);
            _context = new DeviceContext(this);
            _listener = new SoundListener(this);
        }

        /// <summary>
        /// Makes this sound device current.
        /// </summary>
        public void MakeCurrent()
        {
            if (IsCurrent()) return;
            if (disposed) throw new ObjectDisposedException(nameof(OutputDevice));
            _context.MakeCurrent();
            _listener.UpdateValues();
            _distanceModel = (DistanceModel)AL(() =>
                alGetInteger(AL_DISTANCE_MODEL),
                "alGetInteger(AL_DISTANCE_MODEL)");
            s_current = this;
        }

        /// <summary>
        /// Returns true if this device is currently selected.
        /// </summary>
        public bool IsCurrent() => s_current == this;

        /// <summary>
        /// Closes the device.
        /// </summary>
        public void Close()
        {
            if (disposed) throw new ObjectDisposedException(nameof(OutputDevice));
            var successful = ALC(() => alcCloseDevice(_handle), "alcCloseDevice", _handle);
            if (!successful)
                throw new AudioLibraryException("Could not close the device");
            s_current = null;
        }


        /// <summary>
        /// Returns the default output device.
        /// </summary>
        public static string GetDefaultOutputDevice()
        {
            var ptr = ALC(() =>
                alcGetString(NULL, ALC_DEFAULT_DEVICE_SPECIFIER),
                "alcGetString(ALC_DEFAULT_DEVICE_SPECIFIER)", NULL);
            if (ptr == IntPtr.Zero || ptr == null) return null;
            return Marshal.PtrToStringAnsi(ptr);
        }

        /// <summary>
        /// Returns a list of available output devices.
        /// </summary>
        public static IEnumerable<string> GetOutputDevices()
        {
            var NULL = IntPtr.Zero;
            var extPresent = ALC(() =>
                alcIsExtensionPresent(NULL, "ALC_ENUMERATION_EXT"),
                "alcIsExtensionPresent(ALC_ENUMERATION_EXT)",
                NULL);

            if (extPresent)
            {
                var result = new List<string>(5);
                unsafe
                {
                    var enumerateAll = ALC(() =>
                        alcIsExtensionPresent(NULL, "ALC_ENUMERATE_ALL_EXT"),
                        "alcIsExtensionPresent(ALC_ENUMERATE_ALL_EXT)",
                        NULL);

                    ErrorHandler.ResetALC(NULL);
                    byte* listData = enumerateAll ?
                        (byte*)alcGetString(NULL, ALC_ALL_DEVICES_SPECIFIER) :
                        (byte*)alcGetString(NULL, ALC_DEVICE_SPECIFIER);
                    ErrorHandler.CheckALC("alcGetString", NULL);
                    return ParseDeviceString(listData);
                }
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }


        /// <summary>
        /// Resumes the output device processing (if was suspended). That means
        /// the sound playback offsets will be incremented.
        /// </summary>
        public void ResumeProcessing() => _context.ResumeProcessing();

        /// <summary>
        /// Suspends the output device processing (if wasn't suspended). That
        /// means the sound playback offsets will NOT be incremented.
        /// </summary>
        public void SuspendProcessing() => _context.SuspendProcessing();

        bool disposed = false;
        public bool IsDisposed => disposed;
        /// <summary>
        /// Closes the device and disposes the context.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                foreach(var disposable in _resources)
                    disposable.Dispose();
                _resources = null;
                _context.Dispose();
                Close();
                disposed = true;
            }
        }

        private bool Equals(OutputDevice other)
        {
            if (other == null) return false;
            return _handle == other._handle;
        }
        public override bool Equals(object obj) => Equals(obj as OutputDevice);
        public override int GetHashCode() => _handle.GetHashCode();

        internal OutputDevice Swap()
        {
            if (s_current == this) return this;
            var current = s_current;
            MakeCurrent();
            return current;
        }
    }
}