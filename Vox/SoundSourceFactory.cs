using System.Collections.Generic;
using System.Linq;
using static OpenAL.AL10;
using static Vox.ErrorHandler;
using static Vox.Internal.Util;

namespace Vox
{
/// <summary>
/// Provides helper methods to create sound output sources.
/// </summary>
public static class SoundSourceFactory
{
	/// <summary>
	/// Creates the specified count of sound output sources for the specified device.
	/// </summary>
	public static SoundSource[] Create(int count, OutputDevice device)
	{
		var ids = new uint[count];
		var sources = new SoundSource[count];
		UseDevice(device, () =>
			          AL(() => alGenSources(count, ids), "alGenSources"));
		for (var i = 0; i < count; i++)
			sources[i] = new SoundSource(ids[i], device);
		return sources;
	}

	/// <summary>
	/// Creates the specified count of sound output sources for the currend device.
	/// </summary>
	public static SoundSource[] Create(int count)
	{
		return Create(count, OutputDevice.Current);
	}

	/// <summary>
	/// Deletes the specified sound sources. If they belong to the same output
	/// device, deletes them efficiently with one native call, otherwise 
	/// falling back to Dispose. Already disposed sources are skipped.
	/// </summary>
	public static void Delete(params SoundSource[] sources)
	{
		Delete((IEnumerable<SoundSource>) sources);
	}

	/// <summary>
	/// Deletes the specified sound sources. If they belong to the same output
	/// device, deletes them efficiently with one native call, otherwise 
	/// falling back to Dispose.
	/// </summary>
	public static void Delete(IEnumerable<SoundSource> sources)
	{
		if (!sources.Any()) return;
		var activeSources = sources.Where(b => !b.IsDisposed);
		if (activeSources.Select(b => b.Owner).Distinct().Count() == 1)
		{
			var sourceArray = activeSources.ToArray();
			var ids = new uint[sourceArray.Length];
			for (var i = 0; i < ids.Length; i++)
				ids[i] = sourceArray[i]._handle;
			UseDevice(sourceArray[0].Owner, () =>
				          AL(() => alDeleteSources(ids.Length, ids), "alDeleteSources"));
			foreach (var buf in sourceArray)
				buf.AfterDelete();
		}
		else
		{
			foreach (var source in activeSources)
				source.Dispose();
		}
	}
}
}