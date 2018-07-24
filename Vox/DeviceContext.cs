using System;
using static OpenAL.ALC10;
using static Vox.ErrorHandler;

namespace Vox
{
    internal class DeviceContext : IDisposable
    {
        internal SoundDevice _device;
        internal IntPtr _handle;
        
        internal DeviceContext(SoundDevice device)
        {
            _handle = ALC(() => 
                alcCreateContext(device._handle, null),
                "alcCreateContext", device._handle);
            _device = device;
        }

        internal void MakeCurrent()
        {
            var successful = ALC(() => 
                alcMakeContextCurrent(_handle),
                "alcMakeContextCurrent", _device._handle);
            if (!successful)
                throw new AudioLibraryException("Failed to set current context");
        }

        public void Dispose() => Destroy();

        internal void Destroy()
        {
            ALC(() => 
                alcDestroyContext(_handle),
                "alcDestroyContext", _device._handle);
        }
    }
}