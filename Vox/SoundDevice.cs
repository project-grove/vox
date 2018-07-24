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
        private static readonly IntPtr NULL = IntPtr.Zero;

        private static SoundDevice s_current;
        public static SoundDevice Current => s_current;

        internal IntPtr _handle;
        private readonly DeviceContext _context;

        private readonly Listener _listener = new Listener();
        public Listener Listener => _listener;

        /// <summary>
        /// Opens the default sound device and makes it current
        /// </summary>
        public SoundDevice() : this(null)
        {
            MakeCurrent();
        }

        /// <summary>
        /// Opens a sound device with the specified name
        /// </summary>
        /// <param name="name">Device name</param>
        /// <seealso cref="GetOutputDevices" />
        /// <seealso cref="GetCaptureDevices" />
        public SoundDevice(string name)
        {
            _handle = ALC(() => alcOpenDevice(name), "alcOpenDevice", IntPtr.Zero);
            _context = new DeviceContext(this);
        }

        public void MakeCurrent()
        {
            _context.MakeCurrent();
            _listener.UpdateValues();
            s_current = this;
        }

        public void Close()
        {
            var successful = ALC(() => alcCloseDevice(_handle), "alcCloseDevice", _handle);
            if (!successful)
                throw new AudioLibraryException("Could not close the device");
        }

        public static string GetDefaultOutputDevice()
        {
            var ptr = ALC(() => 
                alcGetString(NULL, ALC_DEFAULT_DEVICE_SPECIFIER),
                "alcGetString(ALC_DEFAULT_DEVICE_SPECIFIER)", NULL);
            if (ptr == IntPtr.Zero || ptr == null) return null;
            return Marshal.PtrToStringAnsi(ptr);
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

                    ErrorHandler.ResetALC(NULL);
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

        public static string GetDefaultCaptureDevice()
        {
            var ptr = ALC(() => alcGetString(NULL, ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER),
                "alcGetString(ALC_CAPTURE_DEFAULT_DEVICE_SPECIFIER", NULL);
            if (ptr == IntPtr.Zero || ptr == null) return null;
            return Marshal.PtrToStringAnsi(ptr);
        }

        public static IEnumerable<string> GetCaptureDevices()
        {
            var extPresent = ALC(() =>
                alcIsExtensionPresent(NULL, "ALC_ENUMERATION_EXT"),
                "alcIsExtensionPresent(ALC_ENUMERATION_EXT)",
                NULL);

            if (extPresent)
            {
                unsafe
                {
                    ResetALC(NULL);
                    byte* listData = (byte*)alcGetString(NULL, ALC_CAPTURE_DEVICE_SPECIFIER);
                    CheckALC("alcGetString(ALC_CAPTURE_DEVICE_SPECIFIER)", NULL);
                    return ParseDeviceString(listData);
                }
            }
            else
            {
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

        public void Dispose()
        {
            _context.Dispose();
            Close();
        }
    }
}