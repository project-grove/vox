using System;
using static OpenAL.AL10;
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