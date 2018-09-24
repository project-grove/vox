using System;
using System.Threading;
using Vox;
using Vox.Decoders;
using static System.Console;

namespace ogg_test
{
    class Program
    {
        static void Main(string[] args)
        {
            var outputDevice = new OutputDevice();
            var path = args.Length == 0 ? "2test.ogg" : string.Join(' ', args);
            var buffer = Ogg.Load(path);
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
