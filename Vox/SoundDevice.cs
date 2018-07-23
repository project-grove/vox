using System;
using OpenAL;
using static OpenAL.AL10;
using static OpenAL.ALC10;
using static OpenAL.AL11;
using static OpenAL.ALC11;
using static Vox.ErrorHandler;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Vox
{
    public class SoundDevice : IDisposable
    {
        private static SoundDevice s_current;
        public static SoundDevice Current => s_current;
    
        internal IntPtr _handle;
        private readonly DeviceContext _context;
        public DeviceContext Context => _context;

        public SoundDevice() : this(null)
        {
            MakeCurrent();
        }

        public SoundDevice(string name)
        {
            _handle = ALC(() => alcOpenDevice(name), "alcOpenDevice", IntPtr.Zero);
            _context = new DeviceContext(this);
        }

        public void MakeCurrent() {
            _context.MakeCurrent();
            s_current = this;
        }

        public static IEnumerable<string> GetOutputDevices()
        {
            var NULL = IntPtr.Zero;
            var extPresent = ALC(() =>
                alcIsExtensionPresent(NULL, "ALC_ENUMERATION_EXT"),
                "alcIsExtensionPresent(ALC_ENUMERATION_EXT)",
                NULL);

            if (extPresent)
            {
                var result = new List<string>(5);
                unsafe
                {
                    var enumerateAll = ALC(() =>
                        alcIsExtensionPresent(NULL, "ALC_ENUMERATE_ALL_EXT"),
                        "alcIsExtensionPresent(ALC_ENUMERATE_ALL_EXT)",
                        NULL);

                    ErrorHandler.Reset();
                    byte* listData = enumerateAll ?
                        (byte*)alcGetString(NULL, ALC_ALL_DEVICES_SPECIFIER) :
                        (byte*)alcGetString(NULL, ALC_DEVICE_SPECIFIER);
                    ErrorHandler.CheckALC("alcGetString", NULL);
                    return ParseDeviceString(listData);
                }
            }
            else
            {
                return Enumerable.Empty<string>();
            }
        }

        public static IEnumerable<string> GetCaptureDevices()
        {
            var NULL = IntPtr.Zero;
            var extPresent = ALC(() =>
                alcIsExtensionPresent(NULL, "ALC_ENUMERATION_EXT"),
                "alcIsExtensionPresent(ALC_ENUMERATION_EXT)",
                NULL);

            if (extPresent)
            {
                unsafe 
                {
                    Reset();
                    byte* listData = (byte*)alcGetString(NULL, ALC_CAPTURE_DEVICE_SPECIFIER);
                    CheckALC("alcGetString(ALC_CAPTURE_DEVICE_SPECIFIER)", NULL);
                    return ParseDeviceString(listData);
                }
            } else {
                return Enumerable.Empty<string>();
            }
        }

        private static unsafe List<string> ParseDeviceString(byte* listData)
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
                    result.Add(Marshal.PtrToStringAnsi((IntPtr)start));
                    i++; start = listData + i + 1;
                }
                if (next == 0 && next_2 == 0)
                {
                    break;
                }
                i++;
            }
            return result;
        }

        public void Dispose() => _context.Dispose();
    }
}