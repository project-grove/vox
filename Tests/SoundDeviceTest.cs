using System;
using Vox;
using Xunit;

namespace Tests
{
    public class SoundDeviceTest
    {
        [Fact]
        public void ShouldListAvailableDevices()
        {
            var devices = SoundDevice.GetNames();
            Assert.NotNull(devices);
        }
    }
}
