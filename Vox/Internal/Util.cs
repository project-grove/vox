using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vox.Decoders;

/// <summary>
/// Internal namespace. If something from here will be in the API docs,
/// then we have a problem.
/// </summary>
namespace Vox.Internal
{
internal static class Util
{
	public static unsafe List<string> ParseDeviceString(byte* listData)
	{
		var result = new List<string>();
		var i = 0;
		var start = listData;
		while (true)
		{
			var cur = *listData + i;
			var next = *(listData + i + 1);
			var next_2 = *(listData + i + 2);
			if (next == 0)
			{
				result.Add(Marshal.PtrToStringAnsi((IntPtr) start));
				i++;
				start = listData + i + 1;
			}

			if (next == 0 && next_2 == 0) break;
			i++;
		}

		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void UseDevice(OutputDevice device, Action action)
	{
		if (device == null) throw new AudioLibraryException("No active output device is set");
		var oldDevice = device.Swap();
		try
		{
			action();
		}
		finally
		{
			oldDevice?.MakeCurrent();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void UseDevice(OutputDevice device, Action<OutputDevice> action)
	{
		UseDevice(device, action, device);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void UseDevice<T>(OutputDevice device, Action<T> action, T data)
	{
		if (device == null) throw new AudioLibraryException("No active output device is set");
		var oldDevice = device.Swap();
		try
		{
			action(data);
		}
		finally
		{
			oldDevice?.MakeCurrent();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T[] EnlargePooledArray<T>(this ArrayPool<T> pool, T[] src, int sizeIncrement)
	{
		var dst = pool.Rent(src.Length + sizeIncrement);
		Array.Copy(src, dst, src.Length);
		pool.Return(src);
		return dst;
	}

	public static PCM GetFormat(int channels, SampleQuality quality)
	{
		if (channels < 1 || channels > 2)
			throw new AudioImportException("Only 1 and 2-channel audio is supported");
		switch (quality)
		{
		case SampleQuality.SixteenBits:
			return channels > 1 ? PCM.Stereo16 : PCM.Mono16;
		default:
			return channels > 1 ? PCM.Stereo8 : PCM.Mono8;
		}
	}
}
}