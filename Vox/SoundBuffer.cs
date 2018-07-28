using System;
using static OpenAL.AL10;
using static Vox.ErrorHandler;

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
        public OutputDevice Owner { get; internal set; }

        /// <summary>
        /// Initializes a new sound buffer for the specified device.
        /// </summary>
        /// <param name="device"></param>
        public SoundBuffer(OutputDevice device)
        {
            // store the currently selected device
            var curDevice = OutputDevice.Current;
            // make the specified device current for buffer generation
            device.MakeCurrent();
            uint[] id = new uint[1];
            AL(() => alGenBuffers(1, id), "alGenBuffers");
            Setup(id[0], device);
            // make the previous one current (in case it was different)
            curDevice.MakeCurrent();
        }

        internal SoundBuffer(uint id, OutputDevice device) =>
            Setup(id, device);

        private void Setup(uint id, OutputDevice device) =>
            (_bufferId, Owner) = (id, device);

        public void Dispose() =>
            AL(() =>
                alDeleteBuffers(1, new uint[] { _bufferId }),
                "alDeleteBuffers");
        
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
        public void SetData(PCM format, byte[] data, int size, int frequency) =>
            AL(() => 
                alBufferData(_bufferId, (int)format, data, size, frequency),
                "alBufferData");
    }
}