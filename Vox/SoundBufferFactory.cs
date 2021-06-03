using System.Collections.Generic;
using System.Linq;
using static OpenAL.AL10;
using static Vox.ErrorHandler;
using static Vox.Internal.Util;

namespace Vox
{
/// <summary>
/// Provides helper methods to create sound output buffers.
/// </summary>
public static class SoundBufferFactory
{
	/// <summary>
	/// Creates the specified count of sound output buffers for the specified device.
	/// </summary>
	public static SoundBuffer[] Create(int count, OutputDevice device)
	{
		var ids = new uint[count];
		var buffers = new SoundBuffer[count];
		UseDevice(device, (v) =>
			          AL((p) => alGenBuffers(p.count, p.ids), "alGenBuffers", v), (count, ids));
		for (var i = 0; i < count; i++)
			buffers[i] = new SoundBuffer(ids[i], device);
		return buffers;
	}

	/// <summary>
	/// Creates the specified count of sound output buffers for the currend device.
	/// </summary>
	public static SoundBuffer[] Create(int count)
	{
		return Create(count, OutputDevice.Current);
	}

	/// <summary>
	/// Deletes the specified sound buffers. If they belong to the same output
	/// device, deletes them efficiently with one native call, otherwise
	/// falling back to Dispose. Already disposed buffers are skipped.
	/// </summary>
	public static void Delete(params SoundBuffer[] buffers)
	{
		Delete((IEnumerable<SoundBuffer>) buffers);
	}

#pragma warning disable HAA0401
	/// <summary>
	/// Deletes the specified sound buffers. If they belong to the same output
	/// device, deletes them efficiently with one native call, otherwise
	/// falling back to Dispose.
	/// </summary>
	public static void Delete(IEnumerable<SoundBuffer> buffers)
	{
		if (!buffers.Any()) return;
		var activeBuffers = buffers.Where(b => !b.IsDisposed);
		if (activeBuffers.Select(b => b.Owner).Distinct().Count() == 1)
		{
			var bufArray = activeBuffers.ToArray();
			var ids = new uint[bufArray.Length];
			for (var i = 0; i < ids.Length; i++)
				ids[i] = bufArray[i]._bufferId;
			UseDevice(bufArray[0].Owner, (v) =>
				          AL((p) => alDeleteBuffers(p.Length, p), "alDeleteBuffers", v), ids);
			foreach (var buf in bufArray)
				buf.AfterDelete();
		}
		else
		{
			foreach (var buffer in activeBuffers)
				buffer.Dispose();
		}
	}
}
}