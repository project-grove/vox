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
	private static readonly IntPtr NULL = IntPtr.Zero;
	private readonly ArrayPool<byte> _arrayPool;
	internal IntPtr _handle;
	private bool _disposed;

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

	private readonly int[] _rInt = new int[1];

	/// <summary>
	/// Returns count of the available samples which can be requested.
	/// If application requests more samples, an exception will be thrown.
	/// </summary>
	public int AvailableSamples
	{
		get
		{
			ALC((p) => alcGetIntegerv(p._handle, ALC_CAPTURE_SAMPLES, 1, p._rInt),
			    "alcGetIntegerv(ALC_CAPTURE_SAMPLES)", _handle, (_handle, _rInt));
			return _rInt[0];
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
				return 1;
			case PCM.Mono16:
			case PCM.Stereo16:
				return 2;
			default:
				return -1;
			}
		}
	}

	/// <summary>
	/// Creates and opens the default sound input device.
	/// </summary>
	/// <seealso cref="GetCaptureDevices()" />
	/// <seealso cref="GetDefaultCaptureDevice()" />
	public CaptureDevice() : this(null)
	{
	}

	/// <summary>
	/// Creates and opens a sound input device with the specified parameters.
	/// </summary>
	/// <param name="name">Name of the device to open</param>
	/// <param name="frequency">Sample frequency</param>
	/// <param name="format">Sound format</param>
	/// <param name="bufSize">Buffer size</param>
	/// <param name="bufferPool">Array pool for buffers.false. If null, defaults to shared.</param>
	/// <seealso cref="GetCaptureDevices()" />
	/// <seealso cref="GetDefaultCaptureDevice()" />
	public CaptureDevice(string name, int frequency = 44100,
	                     PCM format = PCM.Mono8, int bufSize = 22050,
	                     ArrayPool<byte> bufferPool = null)
	{
		if (frequency <= 0)
			throw new ArgumentException("Frequency cannot be zero or negative");
		_handle = ALC(
		(p) => alcCaptureOpenDevice(p.name, (uint) p.frequency, (int) p.format, p.bufSize),
		"alcCaptureOpenDevice", NULL, (name, frequency, format, bufSize));
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
		if (_disposed) throw new ObjectDisposedException(nameof(OutputDevice));
		if (IsCapturing) return;
		ALC((h) => alcCaptureStart(h), "alcCaptureStart", _handle);
		IsCapturing = true;
	}

	/// <summary>
	/// Stops the sound capture if it's running.
	/// </summary>
	public void StopCapture()
	{
		if (_disposed) throw new ObjectDisposedException(nameof(OutputDevice));
		if (!IsCapturing) return;
		ALC((h) => alcCaptureStop(h), "alcCaptureStop", _handle);
		IsCapturing = false;
	}

	/// <summary>
	/// Toggles the sound capture.
	/// </summary>
	public void ToggleCapture()
	{
		if (_disposed) throw new ObjectDisposedException(nameof(OutputDevice));
		if (IsCapturing) StopCapture();
		else StartCapture();
	}

	/// <summary>
	/// Reads all available samples and passes them to the callback.
	/// </summary>
	/// <remarks>If you intend to use sample data later, <b>copy it</b>, because the buffer is pooled and will be reused.</remarks>
	public void ProcessSamples(Action<byte[], int> callback)
	{
		ProcessSamples(AvailableSamples, callback);
	}

	/// <summary>
	/// Reads the specified amount of samples from the device and calls
	/// the specified callback.
	/// </summary>
	/// <param name="sampleCount">Sample count. Must be less or equal to <see cref="AvailableSamples" />.</param>
	/// <param name="callback">Callback</param>
	/// <remarks>If you intend to use sample data later, <b>copy it</b>, because the buffer is pooled and will be reused.</remarks>
	public void ProcessSamples(int sampleCount, Action<byte[], int> callback)
	{
		if (_disposed) throw new ObjectDisposedException(nameof(OutputDevice));
		if (AvailableSamples < sampleCount)
			throw new AudioLibraryException("Too many samples requested");
		var buffer = _arrayPool.Rent(BytesPerSample * sampleCount);
		unsafe
		{
			fixed (void* buf = buffer)
			{
				ResetALC(_handle);
				alcCaptureSamples(_handle, (IntPtr) buf, sampleCount);
				CheckALC("alcCaptureSamples", _handle);
				try
				{
					callback(buffer, BytesPerSample * sampleCount);
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
			unsafe
			{
				ResetALC(NULL);
				var listData = (byte*) alcGetString(NULL, ALC_CAPTURE_DEVICE_SPECIFIER);
				CheckALC("alcGetString(ALC_CAPTURE_DEVICE_SPECIFIER)", NULL);
				return ParseDeviceString(listData);
			}
		else
			return Enumerable.Empty<string>();
	}

	/// <summary>
	/// Closes the device.
	/// </summary>
	public void Close()
	{
		Dispose(true);
	}

	private bool Equals(CaptureDevice other)
	{
		if (other == null) return false;
		return _handle == other._handle;
	}

	/// <summary>
	/// Checks the object for equality.
	/// </summary>
	public override bool Equals(object obj)
	{
		return Equals(obj as CaptureDevice);
	}

	/// <summary>
	/// Returns the object's hash code.
	/// </summary>
	public override int GetHashCode()
	{
		return _handle.GetHashCode();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			ALC((h) => alcCaptureCloseDevice(h), "alcCaptureCloseDevice", _handle);
			_disposed = true;
		}
	}

	~CaptureDevice()
	{
		Dispose(false);
	}

	/// <summary>
	/// Disposes the device.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
}