using System;
using static OpenAL.AL10;
using static Vox.ErrorHandler;
using static Vox.Internal.Util;

namespace Vox
{
    /// <summary>
    /// Describes possible formats for <see cref="SoundBuffer">.
    /// </summary>
    public enum PCM
    {
        Mono8 = AL_FORMAT_MONO8,
        Mono16 = AL_FORMAT_MONO16,
        Stereo8 = AL_FORMAT_STEREO8,
        Stereo16 = AL_FORMAT_STEREO16
    }

    /// <summary>
    /// Represent a buffer with PCM sound data.
    /// </summary>
    public class SoundBuffer : IDisposable
    {
        internal uint _bufferId;
        public OutputDevice Owner { get; private set; }

        /// <summary>
        /// Initializes a new sound buffer for the specified device.
        /// </summary>
        /// <param name="device"></param>
        public SoundBuffer(OutputDevice device)
        {
            UseDevice(device, () =>
            {
                uint[] id = new uint[1];
                AL(() => alGenBuffers(1, id), "alGenBuffers");
                Setup(id[0], device);
            });
        }

        internal SoundBuffer(uint id, OutputDevice device) =>
            Setup(id, device);

        private void Setup(uint id, OutputDevice device)
        {
            (_bufferId, Owner) = (id, device);
            device._resources.Add(this);
        }

        private void DeleteBuffer() =>
            UseDevice(Owner, () =>
                AL(() =>
                    alDeleteBuffers(1, new uint[] { _bufferId }),
                    "alDeleteBuffers"));

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
        public void SetData(PCM format, byte[] data, int frequency = 44100) =>
            SetData(format, data, data.Length, frequency);

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
            UseDevice(Owner, () =>
                AL(() =>
                    alBufferData(_bufferId, (int)format, data, size, frequency),
                    "alBufferData"));
        }

        private bool disposed = false;
        public bool IsDisposed => disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                DeleteBuffer();
                AfterDelete();
            }
        }

        private bool Equals(SoundBuffer other)
        {
            if (other == null) return false;
            return _bufferId == other._bufferId &&
                Owner == other.Owner;
        }

        public override bool Equals(object obj) => Equals(obj as SoundBuffer);
        public override int GetHashCode()
        {
            unchecked
            {
                return (int)_bufferId * 17 + Owner.GetHashCode();
            }
        }
    }
}