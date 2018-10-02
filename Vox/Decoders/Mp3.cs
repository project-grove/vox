using System;
using System.Buffers;
using System.IO;
using NLayer;
using NVorbis;
using NVorbis.Ogg;
using Vox.Internal;
using static Vox.Internal.Util;

/// <summary>
/// Contains various audio importers for use with SoundBuffer.
/// </summary>
namespace Vox.Decoders
{
    /// <summary>
    /// Contains various methods to aid SoundBuffer creation from MP3 data.
    /// </summary>
    public static class Mp3
    {
        // byte size = sizeof(float) * BufferSize * channels
        private const int BufferSize = 64000;
        /// <summary>
        /// Defines an ArrayPool used by this class for reading.
        /// Defaults to shared.
        /// </summary>
        public static ArrayPool<byte> ArrayPool { get; set; } = ArrayPool<byte>.Shared;


        /// <summary>
        /// Creates new SoundBuffer for the current output device and loads
        /// data from an .mp3 file.
        /// </summary>
        public static SoundBuffer Load(string path, SampleQuality quality = SampleQuality.EightBits)
        {
            var buffer = new SoundBuffer(OutputDevice.Current);
            buffer.LoadFromMp3(path, quality);
            return buffer;
        }

        /// <summary>
        /// Creates new SoundBuffer for the current output device and loads
        /// data from MP3 stream.
        /// </summary>
        /// <remarks>
        /// The stream should be finite.
        /// </remarks>
        public static SoundBuffer Load(Stream stream, SampleQuality quality = SampleQuality.EightBits,
            bool closeStream = true)
        {
            var buffer = new SoundBuffer(OutputDevice.Current);
            buffer.LoadFromMp3(stream, quality, closeStream);
            return buffer;
        }

        /// <summary>
        /// Loads and resamples the data from an .mp3 file to the specified SoundBuffer.
        /// </summary>
        public static void LoadFromMp3(this SoundBuffer buffer, string path,
            SampleQuality quality = SampleQuality.EightBits) =>
            buffer.LoadFromMp3(new FileStream(path, FileMode.Open), quality, true);

        /// <summary>
        /// Loads and resamples the data from MP3 stream to the specified SoundBuffer.
        /// </summary>
        /// <remarks>
        /// The stream should be finite.
        /// </remarks>
        public static void LoadFromMp3(this SoundBuffer buffer, Stream stream,
            SampleQuality quality = SampleQuality.EightBits, bool closeStream = true)
        {
            using (var reader = new MpegFile(stream))
            {
                var channels = reader.Channels;
                var totalSamples = reader.Length / sizeof(float);
                // stream is seekable, we can get all the samples at once
                if (reader.Length < long.MaxValue)
                {
                    var data = ArrayPool.Rent((int)quality * (int)totalSamples);
                    var samples = new float[totalSamples];
                    reader.ReadSamples(samples, 0, (int)totalSamples);
                    ResampleToPCM(samples, data, samples.Length, quality);
                    buffer.SetData(GetFormat(channels, quality), data, reader.SampleRate);
                    ArrayPool.Return(data);
                }
            }
            if (closeStream) stream.Close();
        }

        private static void ResampleToPCM(float[] src, byte[] dst, int srcCount,
            SampleQuality quality, int srcIndex = 0, int dstIndex = 0)
        {
            for (int i = 0; i < srcCount; i++)
            {
                var val = src[i + srcIndex];
                switch (quality)
                {
                    case SampleQuality.EightBits:
                        unchecked
                        {
                            dst[i + dstIndex] = (byte)(((val + 1.0f) / 2.0f) * 255);
                        }
                        break;
                    case SampleQuality.SixteenBits:
                        unchecked
                        {
                            var sample = (short)(val * 32767);
                            var byte1 = (byte)(sample >> 8);
                            var byte2 = (byte)(sample & 255);
                            if (BitConverter.IsLittleEndian)
                            {
                                dst[i * 2 + dstIndex] = byte1;
                                dst[i * 2 + dstIndex + 1] = byte2;
                            }
                            else
                            {
                                dst[i * 2 + dstIndex] = byte2;
                                dst[i * 2 + dstIndex + 1] = byte1;
                            }
                            break;
                        }
                }
            }
        }

    }
}