using System;
using System.Threading;
using Vox;
using Vox.Decoders;

namespace mp3_test
{
    class Program
    {
        static void Main(string[] args)
        {
            var outputDevice = new OutputDevice();
            var path = args.Length == 0 ? "16Hz-20kHz-Lin-1f-5sec.mp3" : string.Join(' ', args);
            var buffer = Mp3.Load(path);
            var source = new SoundSource();
            source.Enqueue(buffer);
            source.Play();
            do
            {
                Thread.Sleep(100);
            } while (source.State == SourceState.Playing);
            outputDevice.Close();
        }
    }
}
