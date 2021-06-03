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

	/// <summary>
	/// Returns the currently active output device, if it exists.
	/// </summary>
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
			AL((v) => alDistanceModel((int) v), "alDistanceModel", (int) value);
			_distanceModel = value;
		}
	}

	/// <summary>
	/// Opens the default sound device and makes it current.
	/// </summary>
	/// <seealso cref="GetOutputDevices()" />
	/// <seealso cref="GetDefaultOutputDevice()" />
	public OutputDevice() : this(null)
	{
		MakeCurrent();
	}

	/// <summary>
	/// Opens a sound device with the specified name.
	/// </summary>
	/// <param name="name">Device name</param>
	/// <seealso cref="GetOutputDevices()" />
	/// <seealso cref="GetDefaultOutputDevice()" />
	public OutputDevice(string name)
	{
		_handle = ALC((n) => alcOpenDevice(n), "alcOpenDevice", IntPtr.Zero, name);
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
		_distanceModel = (DistanceModel) AL(() =>
			                                    alGetInteger(AL_DISTANCE_MODEL),
		                                    "alGetInteger(AL_DISTANCE_MODEL)");
		s_current = this;
	}

	/// <summary>
	/// Returns true if this device is currently selected.
	/// </summary>
	public bool IsCurrent()
	{
		return s_current == this;
	}

	/// <summary>
	/// Closes the device.
	/// </summary>
	public void Close()
	{
		Dispose();
	}

	private void DoClose()
	{
		if (disposed) throw new ObjectDisposedException(nameof(OutputDevice));
		var successful = ALC((h) => alcCloseDevice(h), "alcCloseDevice", _handle);
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
		var extPresent = ALC((h) =>
			                     alcIsExtensionPresent(h, "ALC_ENUMERATION_EXT"),
		                     "alcIsExtensionPresent(ALC_ENUMERATION_EXT)",
		                     NULL);

		if (extPresent)
		{
			var result = new List<string>(5);
			unsafe
			{
				var enumerateAll = ALC((h) =>
					                       alcIsExtensionPresent(h, "ALC_ENUMERATE_ALL_EXT"),
				                       "alcIsExtensionPresent(ALC_ENUMERATE_ALL_EXT)",
				                       NULL);

				ResetALC(NULL);
				var listData = enumerateAll
					               ? (byte*) alcGetString(NULL, ALC_ALL_DEVICES_SPECIFIER)
					               : (byte*) alcGetString(NULL, ALC_DEVICE_SPECIFIER);
				CheckALC("alcGetString", NULL);
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
	public void ResumeProcessing()
	{
		_context.ResumeProcessing();
	}

	/// <summary>
	/// Suspends the output device processing (if wasn't suspended). That
	/// means the sound playback offsets will NOT be incremented.
	/// </summary>
	public void SuspendProcessing()
	{
		_context.SuspendProcessing();
	}

	private bool disposed = false;

	/// <summary>
	/// Returns true if this object is disposed.
	/// </summary>
	public bool IsDisposed => disposed;

	/// <summary>
	/// Closes the device and disposes the context.
	/// </summary>
	public void Dispose()
	{
		if (!disposed)
		{
			var sources = _resources.OfType<SoundSource>().ToList();
			var buffers = _resources.OfType<SoundBuffer>().ToList();
			foreach (var src in sources)
				src.Dispose();
			foreach (var buf in buffers)
				buf.Dispose();
			_resources.Clear();
			_resources = null;
			DoClose();
			_context.Dispose();
			disposed = true;
		}
	}

	~OutputDevice()
	{
		Dispose();
	}

	private bool Equals(OutputDevice other)
	{
		if (other == null) return false;
		return _handle == other._handle;
	}

	/// <summary>
	/// Checks the object for equality.
	/// </summary>
	public override bool Equals(object obj)
	{
		return Equals(obj as OutputDevice);
	}

	/// <summary>
	/// Returns the object's hash code.
	/// </summary>
	public override int GetHashCode()
	{
		return _handle.GetHashCode();
	}

	internal OutputDevice Swap()
	{
		if (s_current == this) return this;
		var current = s_current;
		MakeCurrent();
		return current;
	}
}
}