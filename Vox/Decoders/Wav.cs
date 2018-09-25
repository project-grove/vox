using System.IO;
using static Vox.Internal.Util;

namespace Vox.Decoders
{
    public static class Wav
    {
        /// <summary>
        /// Creates new SoundBuffer for the current output device and loads
        /// data from an .wav file.
        /// </summary>
        public static SoundBuffer Load(string path)
        {
            var buffer = new SoundBuffer(OutputDevice.Current);
            buffer.LoadFromWav(path);
            return buffer;
        }

        /// <summary>
        /// Creates new SoundBuffer for the current output device and loads
        /// data from WAV stream.
        /// </summary>
        public static SoundBuffer Load(Stream stream)
        {
            var buffer = new SoundBuffer(OutputDevice.Current);
            buffer.LoadFromWav(stream);
            return buffer;
        }

        /// <summary>
        /// Loads the data from an .wav file to the specified SoundBuffer.
        /// </summary>
        public static void LoadFromWav(this SoundBuffer buffer, string path) =>
            buffer.LoadFromWav(new FileStream(path, FileMode.Open), true);

        /// <summary>
        /// Loads the data from WAV stream to the specified SoundBuffer.
        /// </summary>
        public static void LoadFromWav(this SoundBuffer buffer, Stream stream,
            bool closeStream = true)
        {
            using (var reader = new BinaryReader(stream))
            {
                Read(reader, out int channels, out int sampleRate,
                    out SampleQuality quality, out byte[] data);
                buffer.SetData(GetFormat(channels, quality), data, sampleRate);
            }
            if (closeStream) stream.Close();
        }

        private static void Read(BinaryReader reader, out int channels,
            out int sampleRate, out SampleQuality quality, out byte[] data)
        {
            var chunkId = reader.ReadInt32();
            if (chunkId != 0x52494646)
                throw new AudioImportException("Invalid header, expected RIFF");
            var chunkSize = reader.ReadInt32();
            var format = reader.ReadInt32();
            if (format != 0x57415645)
                throw new AudioImportException("Invalid header, expected WAVE");

            // fmt subchunk
            var subchunk1Id = reader.ReadInt32();
            if (subchunk1Id != 0x666d7420)
                throw new AudioImportException("Invalid data, expected 'fmt ' chunk");
            var subchunk1Size = reader.ReadInt32();
            if (subchunk1Size != 16)
                throw new AudioImportException("Non-PCM header found");
            var audioFormat = reader.ReadInt16();
            if (audioFormat != 1)
                throw new AudioImportException("Only PCM format is supported");
            channels = reader.ReadInt16();
            if (channels < 1 || channels > 2)
                throw new AudioImportException("Only 1 and 2-channel data is supported");
            sampleRate = reader.ReadInt32();
            var byteRate = reader.ReadInt32();
            var blockAlign = reader.ReadInt16();
            var bitsPerSample = reader.ReadInt16();
            switch (bitsPerSample)
            {
                case 8:
                    quality = SampleQuality.EightBits; break;
                case 16:
                    quality = SampleQuality.SixteenBits; break;
                default:
                    throw new AudioImportException("Only 8 and 16-bit data is supported");
            }

            // data subchunk
            var subchunk2Id = reader.ReadInt32();
            if (subchunk2Id != 0x64617461)
                throw new AudioImportException("Invalid data, expected 'data' chunk");
            var subchunk2Size = reader.ReadInt32();
            data = reader.ReadBytes(subchunk2Size);
        }
    }
}