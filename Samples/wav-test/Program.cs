using System;
using System.Threading;
using Vox;
using Vox.Decoders;

namespace wav_test
{
    class Program
    {
        static void Main(string[] args)
        {
            var outputDevice = new OutputDevice();
            var path = args.Length == 0 ? "a2002011001-e02-16kHz.wav" : string.Join(' ', args);
            var buffer = Wav.Load(path);
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
