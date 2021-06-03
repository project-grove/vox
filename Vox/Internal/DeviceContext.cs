using System;
using System.Diagnostics;
using static OpenAL.ALC10;
using static OpenAL.ALC11;
using static Vox.ErrorHandler;

namespace Vox.Internal
{
internal class DeviceContext : IDisposable
{
	internal OutputDevice _device;
	internal IntPtr _handle;

	internal DeviceContext(OutputDevice device)
	{
		_handle = ALC((h) => alcCreateContext(h, null), "alcCreateContext", device._handle);
		_device = device;
	}

	internal void MakeCurrent()
	{
		const string msg = "Failed to set current context";
		var successful = ALC((h) => alcMakeContextCurrent(h), "alcMakeContextCurrent",
		                     _device._handle, _handle);
		if (!successful)
		{
			VoxEvents.OpenALTraceSource.TraceEvent(TraceEventType.Critical, -1, msg);
			throw new AudioLibraryException(msg);
		}
	}

	internal void ResumeProcessing()
	{
		ALC((h) => alcProcessContext(h), "alcProcessContext", _device._handle);
	}

	internal void SuspendProcessing()
	{
		ALC((h) => alcSuspendContext(h), "alcSuspendContext", _device._handle);
	}

	internal bool IsCurrent()
	{
		return OutputDevice.Current == _device;
	}

	private bool disposedValue;

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			var handle = _device._handle;
			_device = null;
			alcDestroyContext(handle);
			alcGetError(handle);
			disposedValue = true;
		}
	}

	~DeviceContext()
	{
		Dispose(false);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
}