using System;
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
        /// <summary>
        /// Returns the sound output device which owns this sound source.
        /// </summary>
        /// <returns></returns>
        public OutputDevice Owner { get; private set; }

        /// <summary>
        /// Queries the current source state.
        /// </summary>
        public SourceState State
        {
            get
            {
                int result = AL_INITIAL;
                UseDevice(Owner, () =>
                    AL(() => alGetSourcei(_handle, AL_SOURCE_STATE, out result),
                        "alGetSourcei(AL_SOURCE_STATE)"));
                return (SourceState)result;
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
                UseDevice(Owner, () =>
                    AL(() => alSourcei(_handle, AL_SOURCE_RELATIVE, value ? 1 : 0),
                        "alSourcei(AL_SOURCE_RELATIVE)"));
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
                UseDevice(Owner, () =>
                    AL(() => alSourcei(_handle, AL_LOOPING, value ? 1 : 0),
                        "alSourcei(AL_LOOPING)"));
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
                if (value == null)
                {
                    switch (State)
                    {
                        case SourceState.Initial:
                        case SourceState.Stopped:
                            UseDevice(Owner, () =>
                                AL(() => alSourcei(_handle, AL_BUFFER, AL_NONE),
                                    "alSourcei(AL_BUFFER)"));
                            return;
                        default:
                            throw new AudioLibraryException(
                                "Could not empty queue on an active sound source");
                    }
                }
                UseDevice(Owner, () =>
                    AL(() => alSourcei(_handle, AL_BUFFER, (int)value._bufferId),
                        "alSourcei(AL_BUFFER)"));
            }
            get
            {
                var bufferId = AL_NONE;
                UseDevice(Owner, () =>
                    AL(() => alGetSourcei(_handle, AL_BUFFER, out bufferId),
                        "alGetsourcei(AL_BUFFER)"));
                if (bufferId == AL_NONE) return null;
                return Owner._resources
                    .OfType<SoundBuffer>()
                    .FirstOrDefault(buf => buf._bufferId == bufferId);
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
                var result = 0;
                UseDevice(Owner, () =>
                    AL(() => alGetSourcei(_handle, AL_BUFFERS_QUEUED, out result),
                        "alGetSourcei(AL_BUFFERS_QUEUED)"));
                return result;
            }
        }

        /// <summary>
        /// Returns the number of buffers that have been played by this source.
        /// </summary>
        public int ProcessedBuffers
        {
            get
            {
                var result = 0;
                UseDevice(Owner, () =>
                    AL(() => alGetSourcei(_handle, AL_BUFFERS_PROCESSED, out result),
                        "alGetSourcei(AL_BUFFERS_PROCESSED)"));
                return result;
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
                var val = Math.Max(0.0f, Math.Min(1.0f, value));
                UseDevice(Owner, () =>
                    AL(() => alSourcef(_handle, AL_MIN_GAIN, val),
                        "alSourcef(AL_MIN_GAIN)"));
                _minGain = val;
            }
        }

        private float _maxGain = 0.0f;
        /// <summary>
        /// Gets or sets the maximum gain for the source ranging
        /// from 0.0 to 1.0. Setting it to 0.0 mutes the source.
        /// </summary>
        public float MaxGain
        {
            get => _maxGain;
            set
            {
                var val = Math.Max(0.0f, Math.Min(1.0f, value));
                UseDevice(Owner, () =>
                    AL(() => alSourcef(_handle, AL_MAX_GAIN, val),
                        "alSourcef(AL_MAX_GAIN)"));
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
                var val = Math.Min(0.0f, value);
                UseDevice(Owner, () =>
                    AL(() => alSourcef(_handle, AL_REFERENCE_DISTANCE, val),
                        "alSourcef(AL_REFERENCE_DISTANCE)"));
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
                var val = Math.Min(0.0f, value);
                UseDevice(Owner, () =>
                    AL(() => alSourcef(_handle, AL_ROLLOFF_FACTOR, val),
                        "alSourcef(AL_ROLLOFF_FACTOR)"));
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
                var val = Math.Min(0.0f, value);
                UseDevice(Owner, () =>
                    AL(() => alSourcef(_handle, AL_MAX_DISTANCE, val),
                        "alSourcef(AL_MAX_DISTANCE)"));
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
                var val = Math.Min(float.Epsilon, value);
                UseDevice(Owner, () =>
                    AL(() => alSourcef(_handle, AL_PITCH, val),
                        "alSourcef(AL_PITCH)"));
                _pitch = val;
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
                UseDevice(Owner, () =>
                    AL(() => alSource3f(_handle, AL_DIRECTION, value.X, value.Y, value.Z),
                        "alSource3f(AL_DIRECTION)"));
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
                UseDevice(Owner, () =>
                    AL(() => alSourcef(_handle, AL_CONE_INNER_ANGLE, value),
                        "alSource3f(AL_CONE_INNER_ANGLE)"));
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
                UseDevice(Owner, () =>
                    AL(() => alSourcef(_handle, AL_CONE_OUTER_ANGLE, value),
                        "alSource3f(AL_CONE_OUTER_ANGLE)"));
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
                var val = Math.Min(0.0f, Math.Max(1.0f, value));
                UseDevice(Owner, () =>
                    AL(() => alSourcef(_handle, AL_CONE_OUTER_GAIN, val),
                        "alSourcef(AL_CONE_OUTER_GAIN)"));
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
                float result = 0.0f;
                UseDevice(Owner, () =>
                    AL(() => alGetSourcef(_handle, AL_SEC_OFFSET, out result),
                        "alGetSourcef(AL_SEC_OFFSET)"));
                return result;
            }
            set
            {
                float val = Math.Min(0.0f, value);
                UseDevice(Owner, () =>
                    AL(() => alSourcef(_handle, AL_SEC_OFFSET, val),
                        "alSourcef(AL_SEC_OFFSET)"));
            }
        }

        /// <summary>
        /// The playback position, expressed in samples.
        /// </summary>
        public float OffsetInSamples
        {
            get
            {
                float result = 0.0f;
                UseDevice(Owner, () =>
                    AL(() => alGetSourcef(_handle, AL_SAMPLE_OFFSET, out result),
                        "alGetSourcef(AL_SAMPLE_OFFSET)"));
                return result;
            }
            set
            {
                float val = Math.Min(0.0f, value);
                UseDevice(Owner, () =>
                    AL(() => alSourcef(_handle, AL_SAMPLE_OFFSET, val),
                        "alSourcef(AL_SAMPLE_OFFSET)"));
            }
        }

        /// <summary>
        /// The playback position, expressed in bytes.
        /// </summary>
        public int OffsetInBytes
        {
            get
            {
                int result = 0;
                UseDevice(Owner, () =>
                    AL(() => alGetSourcei(_handle, AL_BYTE_OFFSET, out result),
                        "alGetSourcei(AL_BYTE_OFFSET)"));
                return result;
            }
            set
            {
                int val = Math.Min(0, value);
                UseDevice(Owner, () =>
                    AL(() => alSourcei(_handle, AL_BYTE_OFFSET, val),
                        "alSourcei(AL_BYTE_OFFSET)"));
            }
        }



        /// <summary>
        /// Creates a new sound source for the current output device.
        /// </summary>
        public SoundSource() : this(OutputDevice.Current) { }

        /// <summary>
        /// Creates a new sound source for the specified output device.
        /// </summary>
        public SoundSource(OutputDevice owner)
        {
            UseDevice(owner, () =>
            {
                var id = new uint[1];
                AL(() => alGenSources(1, id), "alGenSources");
                Setup(id[0], owner);
            });
        }

        /// <summary>
        /// Plays or restarts (if already playing) the queued sound buffers.
        /// </summary>    
        public void Play()
        {
            if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
            UseDevice(Owner, () => AL(() => alSourcePlay(_handle), "alSourcePlay"));
        }

        /// <summary>
        /// Pauses the sound source.
        /// </summary>
        public void Pause()
        {
            if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
            UseDevice(Owner, () => AL(() => alSourcePause(_handle), "alSourcePause"));
        }

        /// <summary>
        /// Stops the sound source.
        /// </summary>
        public void Stop()
        {
            if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
            UseDevice(Owner, () => AL(() => alSourceStop(_handle), "alSourceStop"));
        }

        /// <summary>
        /// Rewinds the sound source, stopping it and then putting it in the
        /// initial state.
        /// </summary>
        public void Rewind()
        {
            if (disposed) throw new ObjectDisposedException(nameof(SoundSource));
            UseDevice(Owner, () => AL(() => alSourceRewind(_handle), "alSourceRewind"));
        }

        /// <summary>
        /// Adds the specified buffers to the source playback queue.
        /// </summary>
        public void Enqueue(params SoundBuffer[] buffers)
        {
            if (buffers.Length == 1)
            {
                UseDevice(Owner, () =>
                    AL(() => alSourceQueueBuffers(_handle, 1, new uint[] { buffers[0]._bufferId }),
                        "alSourceQueueBuffers"));
                return;
            }
            uint[] bufHandles = new uint[buffers.Length];
            for (int i = 0; i < bufHandles.Length; i++)
                bufHandles[i] = buffers[i]._bufferId;
            UseDevice(Owner, () =>
                AL(() => alSourceQueueBuffers(_handle, bufHandles.Length, bufHandles),
                    "alSourceQueueBuffers"));
        }
        /// <summary>
        /// removes a number of buffers entries that have
        /// finished processing, in the order of appearance, from the queue.
        /// </summary>
        public void Unqueue(int entries, params SoundBuffer[] buffers)
        {
            uint[] bufHandles = new uint[buffers.Length];
            for (int i = 0; i < bufHandles.Length; i++)
                bufHandles[i] = buffers[i]._bufferId;
            UseDevice(Owner, () =>
                AL(() => alSourceUnqueueBuffers(_handle, entries, bufHandles),
                    "alSourceUnqueueBuffers"));
        }

        internal SoundSource(uint handle, OutputDevice owner) =>
            Setup(handle, owner);

        private void Setup(uint handle, OutputDevice owner)
        {
            (_handle, Owner) = (handle, owner);
            Owner._resources.Add(this);
        }

        private void DeleteSource() =>
            UseDevice(Owner, () =>
                AL(() => alDeleteSources(1, new uint[] { _handle }),
                    "alDeleteSources"));

        internal void AfterDelete()
        {
            Owner._resources.Remove(this);
            disposed = true;
        }

        private bool disposed = false;
        public bool IsDisposed => disposed;
        public void Dispose()
        {
            if (!disposed)
            {
                DeleteSource();
                AfterDelete();
            }
        }
    }
}