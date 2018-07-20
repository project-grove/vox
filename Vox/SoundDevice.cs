using System;
using OpenAL;
using static OpenAL.AL10;
using static OpenAL.ALC10;
using static OpenAL.AL11;
using static OpenAL.ALC11;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Vox
{
    public class SoundDevice
    {
        public static IEnumerable<string> GetNames()
        {
            ErrorHandler.Reset();
            var extPresent = alcIsExtensionPresent(IntPtr.Zero, "ALC_ENUMERATION_EXT");
            ErrorHandler.Check("alcIsExtensionPresent");

            if (extPresent) {
                var result = new List<string>(5);
                unsafe
                {
                    ErrorHandler.Reset();
                    byte* listData = (byte*)alcGetString(IntPtr.Zero, ALC_DEVICE_SPECIFIER);
                    ErrorHandler.Check("alcGetString");

                    var markers = new List<int>(5) { 0 };
                    var i = 0;
                    while (true) {
                        var cur = *listData + i;
                        var next = *(listData + i + 1);
                        var next_2 = *(listData + i + 2);
                        if (next == 0) {
                            markers.Add(i++);
                        }
                        if (next == 0 && next_2 == 0) {
                            break;
                        }
                        i++;
                    }
                    var data = new Span<byte>(listData, i);
                    for (int j = 0; j < markers.Count - 1; j++) {
                        var start = markers[j];
                        var length = markers[j + 1] - start;
                        result.Add(Encoding.UTF8.GetString(data.Slice(start, length).ToArray()));
                    }
                    return result;
                }
            } else {
                return Enumerable.Empty<string>();
            }
        }
    }
}