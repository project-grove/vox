using System;
using Vox;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class SoundDeviceTest
    {
        private readonly ITestOutputHelper output;
        public SoundDeviceTest(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void ShouldListOutputDevices()
        {
            var devices = SoundDevice.GetOutputDevices();
            Assert.NotNull(devices);
            foreach(var device in devices)
                output.WriteLine(device);
        }

        [Fact]
        public void ShouldListCaptureDevices()
        {
            var devices = SoundDevice.GetCaptureDevices();
            Assert.NotNull(devices);
            foreach(var device in devices)
                output.WriteLine(device);
        }
    }
}
