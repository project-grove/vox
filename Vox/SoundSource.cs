using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static OpenAL.AL10;
using static OpenAL.AL11;
using static Vox.ErrorHandler;
using static Vox.Internal.Util;

namespace Vox
{
/// <summary>
/// Describes possible sound source states.
/// </summary>
public enum SourceState
{
	Initial = AL_INITIAL,
	Playing = AL_PLAYING,
	Paused = AL_PAUSED,
	Stopped = AL_STOPPED
}

/// <summary>
/// Indicates whether a source is static (has one <see cref="SoundBuffer" /> attached)
/// or streaming (has a queue).false. If it's undetermined, the target status will be
/// changed automatically on modification.
/// </summary>
public enum SourceType
{
	Undetermined = AL_UNDETERMINED,
	Static = AL_STATIC,
	Streaming = AL_STREAMING
}

/// <summary>
/// Describes a sound source.
/// </summary>
public class SoundSource : IDisposable
{
	internal uint _handle;

	private int _rInt;
	private float _rFloat;
	private uint[] _singleUint = new uint[1];

	/// <summary>
	/// Returns the sound output device which owns this sound source.
	/// </summary>
	public OutputDevice Owner { get; private set; }

	/// <summary>
	/// Queries the current source state.
	/// </summary>
	public SourceState State
	{
		get
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			_rInt = AL_INITIAL;
			UseDevice(Owner, (v) =>
				          AL((p) => alGetSourcei(p._handle, AL_SOURCE_STATE, out p._rInt),
				             "alGetSourcei(AL_SOURCE_STATE)", v), this);
			return (SourceState) _rInt;
		}
	}

	private bool _isRelative = false;

	/// <summary>
	/// Gets or sets if the source properties should be interpreted as relative
	/// to the listener's position.
	/// </summary>
	public bool IsRelative
	{
		get => _isRelative;
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			UseDevice(Owner, (v) =>
				          AL((p) => alSourcei(p.Item1._handle, AL_SOURCE_RELATIVE, p.value ? 1 : 0),
				             "alSourcei(AL_SOURCE_RELATIVE)", v), (this, value));
			_isRelative = value;
		}
	}

	private bool _isLooping = false;

	/// <summary>
	/// Gets or sets if the source is looped.
	/// </summary>
	public bool IsLooping
	{
		get => _isLooping;
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			UseDevice(Owner, (v) =>
				          AL((p) => alSourcei(p.Item1._handle, AL_LOOPING, p.value ? 1 : 0),
				             "alSourcei(AL_LOOPING)", v), (this, value));
			_isLooping = value;
		}
	}

	/// <summary>
	/// Gets or sets the first buffer on the queue.
	/// </summary>
	/// <remarks>If set to null, releases the current buffer queue (only in Initial and Stopped states).</remarks>
	public SoundBuffer CurrentBuffer
	{
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			if (value == null)
				switch (State)
				{
				case SourceState.Initial:
				case SourceState.Stopped:
					UseDevice(Owner, (v) =>
						          AL((p) => alSourcei(p, AL_BUFFER, AL_NONE),
						             "alSourcei(AL_BUFFER)", v), _handle);
					return;
				default:
					throw new AudioLibraryException(
					"Could not empty queue on an active sound source");
				}

			UseDevice(Owner, (v) =>
				          AL((p) => alSourcei(p._handle, AL_BUFFER, (int) p.value._bufferId),
				             "alSourcei(AL_BUFFER)", v), (_handle, value));
		}
		get
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			UseDevice(Owner, (v) =>
				          AL((p) => alGetSourcei(p._handle, AL_BUFFER, out p._rInt),
				             "alGetsourcei(AL_BUFFER)", v), this);
			foreach (var res in Owner._resources)
			{
				if (!(res is SoundBuffer buffer)) continue;
				if (buffer._bufferId == _rInt) return buffer;
			}

			return null;
		}
	}

	/// <summary>
	/// Returns the total number of buffers in a queue (not played, currently
	/// playing and played already).
	/// </summary>
	public int QueuedBuffers
	{
		get
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			UseDevice(Owner, (v) =>
				          AL((p) => alGetSourcei(p._handle, AL_BUFFERS_QUEUED, out p._rInt),
				             "alGetSourcei(AL_BUFFERS_QUEUED)", v), this);
			return _rInt;
		}
	}

	/// <summary>
	/// Returns the number of buffers that have been played by this source.
	/// </summary>
	public int ProcessedBuffers
	{
		get
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			UseDevice(Owner, (v) =>
				          AL((p) => alGetSourcei(p._handle, AL_BUFFERS_PROCESSED, out p._rInt),
				             "alGetSourcei(AL_BUFFERS_PROCESSED)", v), this);
			return _rInt;
		}
	}

	private float _minGain = 0.0f;

	/// <summary>
	/// Gets or sets the minimum guaranteed gain for the source ranging
	/// from 0.0 to 1.0.
	/// </summary>
	public float MinGain
	{
		get => _minGain;
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			var val = Math.Max(0.0f, Math.Min(1.0f, value));
			UseDevice(Owner, (v) =>
				          AL((p) => alSourcef(p._handle, AL_MIN_GAIN, p.val),
				             "alSourcef(AL_MIN_GAIN)", v), (_handle, val));
			_minGain = val;
		}
	}

	private float _maxGain = 1.0f;

	/// <summary>
	/// Gets or sets the maximum gain for the source ranging
	/// from 0.0 to 1.0. Setting it to 0.0 mutes the source.
	/// </summary>
	public float MaxGain
	{
		get => _maxGain;
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			var val = Math.Max(0.0f, Math.Min(1.0f, value));
			UseDevice(Owner, (v) =>
				          AL((p) => alSourcef(p._handle, AL_MAX_GAIN, p.val),
				             "alSourcef(AL_MAX_GAIN)", v), (_handle, val));
			_maxGain = val;
		}
	}

	private float _referenceDistance = 1.0f;

	/// <summary>
	/// Gets or sets the reference distance used in attenuation calculations.
	/// </summary>
	public float ReferenceDistance
	{
		get => _referenceDistance;
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			var val = Math.Max(float.Epsilon, value);
			UseDevice(Owner, (v) =>
				          AL((p) => alSourcef(p._handle, AL_REFERENCE_DISTANCE, p.val),
				             "alSourcef(AL_REFERENCE_DISTANCE)", v), (_handle, val));
			_referenceDistance = val;
		}
	}

	private float _rolloffFactor = 1.0f;

	/// <summary>
	/// Gets or sets the rolloff factor used in attenuation calculations.
	/// </summary>
	public float RolloffFactor
	{
		get => _rolloffFactor;
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			var val = Math.Max(0.0f, value);
			UseDevice(Owner, (v) =>
				          AL((p) => alSourcef(p._handle, AL_ROLLOFF_FACTOR, p.val),
				             "alSourcef(AL_ROLLOFF_FACTOR)", v), (_handle, val));
			_rolloffFactor = val;
		}
	}

	private float _maxDistance = float.MaxValue;

	/// <summary>
	/// Gets or sets the maximum distance parameter for the
	/// 'Inverse Clamped Distance' attenuation model.
	/// </summary>
	public float MaxDistance
	{
		get => _maxDistance;
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			var val = Math.Max(float.Epsilon, value);
			UseDevice(Owner, (v) =>
				          AL((p) => alSourcef(p._handle, AL_MAX_DISTANCE, p.val),
				             "alSourcef(AL_MAX_DISTANCE)", v), (_handle, val));
			_maxDistance = val;
		}
	}

	private float _pitch = 1.0f;

	/// <summary>
	/// Gets or sets the desired pitch shift, where 1.0 equals identity.
	/// Each reduction by 50% equals a pitch shift of -12 semitones (one octave),
	/// each doubling equals a pitch shift of +12 semitones (one octave).
	/// Must be greater than zero.
	/// </summary>
	public float Pitch
	{
		get => _pitch;
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			var val = Math.Max(float.Epsilon, value);
			UseDevice(Owner, (v) =>
				          AL((p) => alSourcef(p._handle, AL_PITCH, p.val),
				             "alSourcef(AL_PITCH)", v), (_handle, val));
			_pitch = val;
		}
	}

	private float _gain = 1.0f;

	/// <summary>
	/// Gets or sets the desired source gain. Must be positive.
	/// </summary>
	public float Gain
	{
		get => _gain;
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			var val = Math.Max(0f, value);
			UseDevice(Owner, (v) =>
				          AL((p) => alSourcef(p._handle, AL_GAIN, p.val),
				             "alSourcef(AL_GAIN)", v), (_handle, val));
			_gain = val;
		}
	}

	private Vector3 _position = Vector3.Zero;

	/// <summary>
	/// Gets or sets the position of the sound source.
	/// </summary>
	public Vector3 Position
	{
		get => _position;
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			UseDevice(Owner, (v) =>
				          AL(
				          (p) => alSource3f(p._handle, AL_POSITION, p.value.X, p.value.Y,
				                            p.value.Z),
				          "alSource3f(AL_POSITION)", v), (_handle, value));
			_position = value;
		}
	}

	private Vector3 _direction = Vector3.Zero;

	/// <summary>
	/// Gets or sets the direction of the sound source. If direction is a
	/// zero vector, the source is omnidirectional.
	/// </summary>
	public Vector3 Direction
	{
		get => _direction;
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			UseDevice(Owner, (v) =>
				          AL(
				          (p) => alSource3f(p._handle, AL_DIRECTION, p.value.X, p.value.Y,
				                            p.value.Z),
				          "alSource3f(AL_DIRECTION)", v), (_handle, value));
			_direction = value;
		}
	}

	private float _innerAngle = 360.0f;

	/// <summary>
	/// Inside angle of the sound cone, in degrees. Default value of 360 means
	/// that the source is omnidirectional.
	/// </summary>
	public float InnerAngle
	{
		get => _innerAngle;
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			UseDevice(Owner, (v) =>
				          AL((p) => alSourcef(p._handle, AL_CONE_INNER_ANGLE, p.value),
				             "alSource3f(AL_CONE_INNER_ANGLE)", v), (_handle, value));
			_innerAngle = value;
		}
	}

	private float _outerAngle = 360.0f;

	/// <summary>
	/// Outer angle of the sound cone, in degrees.
	/// </summary>
	public float OuterAngle
	{
		get => _outerAngle;
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			UseDevice(Owner, (v) =>
				          AL((p) => alSourcef(p._handle, AL_CONE_OUTER_ANGLE, p.value),
				             "alSource3f(AL_CONE_OUTER_ANGLE)", v), (_handle, value));
			_outerAngle = value;
		}
	}

	private float _coneOuterGain = 0.0f;

	/// <summary>
	/// The factor with which the gain is multiplied to determine the
	/// effective gain outside the cone defined by the outer angle.
	/// Must be in [0.0f, 1.0f].
	/// </summary>
	public float OuterGain
	{
		get => _coneOuterGain;
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			var val = Math.Max(0.0f, Math.Min(1.0f, value));
			UseDevice(Owner, (v) =>
				          AL((p) => alSourcef(p._handle, AL_CONE_OUTER_GAIN, p.val),
				             "alSourcef(AL_CONE_OUTER_GAIN)", v), (_handle, val));
			_coneOuterGain = val;
		}
	}

	/// <summary>
	/// The playback position, expressed in seconds.
	/// </summary>
	public float OffsetInSeconds
	{
		get
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			UseDevice(Owner, (v) =>
				          AL((p) => alGetSourcef(p._handle, AL_SEC_OFFSET, out p._rFloat),
				             "alGetSourcef(AL_SEC_OFFSET)", v), this);
			return _rFloat;
		}
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			var val = Math.Max(0.0f, value);
			UseDevice(Owner, (v) =>
				          AL((p) => alSourcef(p._handle, AL_SEC_OFFSET, p.val),
				             "alSourcef(AL_SEC_OFFSET)", v), (_handle, val));
		}
	}

	/// <summary>
	/// The playback position, expressed in samples.
	/// </summary>
	public float OffsetInSamples
	{
		get
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			UseDevice(Owner, (v) =>
				          AL((p) => alGetSourcef(p._handle, AL_SAMPLE_OFFSET, out p._rFloat),
				             "alGetSourcef(AL_SAMPLE_OFFSET)", v), this);
			return _rFloat;
		}
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			var val = Math.Max(0.0f, value);
			UseDevice(Owner, (v) =>
				          AL((p) => alSourcef(p._handle, AL_SAMPLE_OFFSET, p.val),
				             "alSourcef(AL_SAMPLE_OFFSET)", v), (_handle, val));
		}
	}

	/// <summary>
	/// The playback position, expressed in bytes.
	/// </summary>
	public int OffsetInBytes
	{
		get
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			UseDevice(Owner, (v) =>
				          AL((p) => alGetSourcei(p._handle, AL_BYTE_OFFSET, out p._rInt),
				             "alGetSourcei(AL_BYTE_OFFSET)", v), this);
			return _rInt;
		}
		set
		{
			if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
			var val = Math.Max(0, value);
			UseDevice(Owner, (v) =>
				          AL((p) => alSourcei(p._handle, AL_BYTE_OFFSET, p.val),
				             "alSourcei(AL_BYTE_OFFSET)", v), (_handle, val));
		}
	}


	/// <summary>
	/// Creates a new sound source for the current output device.
	/// </summary>
	public SoundSource() : this(OutputDevice.Current)
	{
	}

	/// <summary>
	/// Creates a new sound source for the specified output device.
	/// </summary>
	public SoundSource(OutputDevice owner)
	{
		if (owner.IsDisposed) throw new ObjectDisposedException(nameof(OutputDevice));
		UseDevice(owner, (p) =>
		          {
			          var id = new uint[1];
			          AL((a) => alGenSources(1, a), "alGenSources", id);
			          p.@this.Setup(id[0], p.owner);
		          }, (@this: this, owner));
	}

	/// <summary>
	/// Plays or restarts (if already playing) the queued sound buffers.
	/// </summary>
	public void Play()
	{
		if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
		UseDevice(Owner, (v) => AL((h) => alSourcePlay(h), "alSourcePlay", v), _handle);
	}

	/// <summary>
	/// Pauses the sound source.
	/// </summary>
	public void Pause()
	{
		if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
		UseDevice(Owner, (v) => AL((h) => alSourcePause(h), "alSourcePause", v), _handle);
	}

	/// <summary>
	/// Stops the sound source.
	/// </summary>
	public void Stop()
	{
		if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
		UseDevice(Owner, (v) => AL((h) => alSourceStop(h), "alSourceStop", v), _handle);
	}

	/// <summary>
	/// Rewinds the sound source, stopping it and then putting it in the
	/// initial state.
	/// </summary>
	public void Rewind()
	{
		if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
		UseDevice(Owner, (v) => AL((h) => alSourceRewind(h), "alSourceRewind", v), _handle);
	}

	/// <summary>
	/// Adds the specified buffer to the source playback queue.
	/// </summary>
	public void Enqueue(SoundBuffer buffer)
	{
		Enqueue(Enumerable.Repeat(buffer, 1));
	}

	/// <summary>
	/// Adds the specified buffers to the source playback queue.
	/// </summary>
	public void Enqueue(params SoundBuffer[] buffers)
	{
		Enqueue(buffers.AsEnumerable());
	}

#pragma warning disable HAA0401

	/// <summary>
	/// Adds the specified buffers to the source playback queue.
	/// </summary>
	public void Enqueue(IEnumerable<SoundBuffer> buffers)
	{
		if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
		var count = buffers.Count();
		if (count == 1)
		{
			_singleUint[0] = buffers.First()._bufferId;
			UseDevice(Owner, (v) =>
				          AL(
				          (p) => alSourceQueueBuffers(p._handle, 1, p._singleUint),
				          "alSourceQueueBuffers", v), (_handle, _singleUint));
			return;
		}

		var bufHandles = ArrayPool<uint>.Shared.Rent(count);
		var i = 0;
		foreach (var buffer in buffers)
			bufHandles[i++] = buffer._bufferId;

		UseDevice(Owner, (v) =>
			          AL((p) => alSourceQueueBuffers(p._handle, p.count, p.bufHandles),
			             "alSourceQueueBuffers", v), (_handle, bufHandles, count));

		ArrayPool<uint>.Shared.Return(bufHandles);
	}

	/// <summary>
	/// Remove specified buffers from the playback queue.
	/// </summary>
	public void Unqueue(IEnumerable<SoundBuffer> buffers)
	{
		if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
		var count = buffers.Count();
		var bufHandles = ArrayPool<uint>.Shared.Rent(count);
		var i = 0;
		foreach (var buffer in buffers)
			bufHandles[i++] = buffer._bufferId;

		UseDevice(Owner, (v) =>
			          AL((p) => alSourceUnqueueBuffers(p._handle, p.count, p.bufHandles),
			             "alSourceUnqueueBuffers", v), (_handle, count, bufHandles));

		ArrayPool<uint>.Shared.Return(bufHandles);
	}

	internal SoundSource(uint handle, OutputDevice owner)
	{
		Setup(handle, owner);
	}

	private void Setup(uint handle, OutputDevice owner)
	{
		(_handle, Owner) = (handle, owner);
		Owner._resources.Add(this);
	}

	private void DeleteSource()
	{
		_singleUint[0] = _handle;
		UseDevice(Owner, (v) => AL((p) => alDeleteSources(1, p), "alDeleteSources", v),
		          _singleUint);
	}

	internal void AfterDelete()
	{
		Owner._resources.Remove(this);
		disposed = true;
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
			DeleteSource();
			AfterDelete();
		}
	}

	~SoundSource()
	{
		Dispose();
	}

	private bool Equals(SoundSource other)
	{
		if (other == null) return false;
		return _handle == other._handle &&
		       Owner == other.Owner;
	}

	/// <summary>
	/// Checks the object for equality.
	/// </summary>
	public override bool Equals(object obj)
	{
		return Equals(obj as SoundSource);
	}

	/// <summary>
	/// Returns the object's hash code.
	/// </summary>
	public override int GetHashCode()
	{
		unchecked
		{
			return (int) _handle * 17 + Owner.GetHashCode();
		}
	}
}
}