using System;
using System.Linq;
using System.Threading;
using Vox;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class CaptureDeviceTest
    {
        private ITestOutputHelper output;
        public CaptureDeviceTest(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void ShouldListCaptureDevices()
        {
            var devices = CaptureDevice.GetCaptureDevices();
            Assert.NotNull(devices);
            foreach(var device in devices)
                output.WriteLine(device);
        }
        
        [Fact]
        public void ShouldReturnDefaultCaptureDevice()
        {
            var result = CaptureDevice.GetDefaultCaptureDevice();
            Assert.NotNull(result);
            output.WriteLine(result);
        }

        [Fact]
        public void ShouldCreateAndConfigureOutputDevice()
        {
            var frequency = 44100;
            var format = PCM.Mono8;
            var bufSize = 1024;
            var device = new CaptureDevice(null, frequency, format, bufSize);

            Assert.Equal(frequency, device.Frequency);
            Assert.Equal(format, device.Format);
            Assert.Equal(bufSize, device.BufferSize);

            device.Close();
        }

        [Fact]
        public void ShouldCaptureSamples()
        {
            var device = new CaptureDevice();
            var container = new DataContainer();
            device.StartCapture();
            Thread.Sleep(100);
            Assert.NotEqual(0, device.AvailableSamples);
            device.ProcessSamples(buf => {
                container.Data = new byte[buf.Length];
                Array.Copy(buf, container.Data, buf.Length);
            });
            device.StopCapture();
            Assert.NotEmpty(container.Data);

            var nonNullBytes = container.Data.Where(b => b != 0).Count();
            output.WriteLine($"{nonNullBytes} non-null bytes captured");
            device.Close();
        }

        /// <summary>
        /// Helper class which simulates a capture data consumer
        /// </summary>
        private class DataContainer
        {
            public byte[] Data { get; set; }
        }
    }
}