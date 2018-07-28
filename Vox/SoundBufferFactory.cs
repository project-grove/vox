using System.Collections.Generic;
using static OpenAL.AL10;
using static Vox.ErrorHandler;

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
            uint[] ids = new uint[count];
            SoundBuffer[] buffers = new SoundBuffer[count];
            var curDevice = OutputDevice.Current;
            device.MakeCurrent();
            AL(() => alGenBuffers(count, ids), "alGenBuffers");
            for (int i = 0; i < count; i++)
                buffers[i] = new SoundBuffer(ids[i], device);
            curDevice.MakeCurrent();
            return buffers;
        }

        /// <summary>
        /// Creates the specified count of sound output buffers for the currend device.
        /// </summary>
        public static SoundBuffer[] Create(int count) =>
            Create(count, OutputDevice.Current);
    }
}