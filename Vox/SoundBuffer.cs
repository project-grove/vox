using System;
using static OpenAL.AL10;
using static Vox.ErrorHandler;
using static Vox.Internal.Util;

namespace Vox
{
/// <summary>
/// Describes possible formats for <see cref="SoundBuffer" />.
/// </summary>
public enum PCM
{
	Mono8 = AL_FORMAT_MONO8,
	Mono16 = AL_FORMAT_MONO16,
	Stereo8 = AL_FORMAT_STEREO8,
	Stereo16 = AL_FORMAT_STEREO16
}

/// <summary>
/// Represents a buffer with PCM sound data.
/// </summary>
public class SoundBuffer : IDisposable
{
	internal uint _bufferId;

	/// <summary>
	/// Returns the sound output device which owns this sound buffer.
	/// </summary>
	public OutputDevice Owner { get; private set; }

	/// <summary>
	/// Initializes a new sound buffer for the specified device.
	/// </summary>
	/// <param name="device"></param>
	public SoundBuffer(OutputDevice device)
	{
		if (device.IsDisposed) throw new ObjectDisposedException(nameof(OutputDevice));
		UseDevice(device, (p) =>
		          {
			          var id = new uint[1];
			          AL((v) => alGenBuffers(1, v), "alGenBuffers", id);
			          p.Item1.Setup(id[0], p.device);
		          }, (this, device));
	}

	internal SoundBuffer(uint id, OutputDevice device)
	{
		Setup(id, device);
	}

	private void Setup(uint id, OutputDevice device)
	{
		(_bufferId, Owner) = (id, device);
		device._resources.Add(this);
	}

	private void DeleteBuffer()
	{
		UseDevice(
		Owner, (id) => AL((i) => alDeleteBuffers(1, new uint[] {i}), "alDeleteBuffers", id),
		_bufferId);
	}

	internal void AfterDelete()
	{
		Owner._resources.Remove(this);
		disposed = true;
	}

	/// <summary>
	/// Sets the audio buffer PCM data.
	/// </summary>
	/// <remarks>
	/// 8-bit PCM data is expressed as an unsigned value over the range 0 to 255, 128 being an
	/// audio output level of zero. 16-bit PCM data is expressed as a signed value over the
	/// range -32768 to 32767, 0 being an audio output level of zero. Stereo data is expressed
	/// in interleaved format, left channel first. Buffers containing more than one channel of data
	/// will be played without 3D spatialization.
	/// </remarks>
	/// <param name="format">Audio format of the input data</param>
	/// <param name="data">Byte array with sound data</param>
	/// <param name="frequency">Sound frequency</param>
	public void SetData(PCM format, byte[] data, int frequency = 44100)
	{
		SetData(format, data, data.Length, frequency);
	}

	/// <summary>
	/// Sets the audio buffer PCM data.
	/// </summary>
	/// <remarks>
	/// 8-bit PCM data is expressed as an unsigned value over the range 0 to 255, 128 being an
	/// audio output level of zero. 16-bit PCM data is expressed as a signed value over the
	/// range -32768 to 32767, 0 being an audio output level of zero. Stereo data is expressed
	/// in interleaved format, left channel first. Buffers containing more than one channel of data
	/// will be played without 3D spatialization.
	/// </remarks>
	/// <param name="format">Audio format of the input data</param>
	/// <param name="data">Byte array with sound data</param>
	/// <param name="size">Size of data in bytes</param>
	/// <param name="frequency">Sound frequency</param>
	public void SetData(PCM format, byte[] data, int size, int frequency)
	{
		if (disposed) throw new ObjectDisposedException(nameof(SoundBuffer));
		UseDevice(
		Owner,
		(v) => AL((p) => alBufferData(p._bufferId, (int) p.format, p.data, p.size, p.frequency),
		          "alBufferData", v), (_bufferId, format, data, size, frequency));
	}

	private bool disposed = false;

	/// <summary>
	/// Returns true if this object is disposed.
	/// </summary>
	public bool IsDisposed => disposed;

	/// <summary>
	/// Disposes this object.
	/// </summary>
	public void Dispose()
	{
		if (!disposed)
		{
			DeleteBuffer();
			AfterDelete();
		}
	}

	~SoundBuffer()
	{
		Dispose();
	}

	private bool Equals(SoundBuffer other)
	{
		if (other == null) return false;
		return _bufferId == other._bufferId &&
		       Owner == other.Owner;
	}

	/// <summary>
	/// Checks the object for equality.
	/// </summary>
	public override bool Equals(object obj)
	{
		return Equals(obj as SoundBuffer);
	}

	/// <summary>
	/// Returns the object's hash code.
	/// </summary>
	public override int GetHashCode()
	{
		unchecked
		{
			return (int) _bufferId * 17 + Owner.GetHashCode();
		}
	}
}
}