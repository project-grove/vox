using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Vox.Internal
{
    internal static class Util
    {
        public static unsafe List<string> ParseDeviceString(byte* listData)
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
    }
}