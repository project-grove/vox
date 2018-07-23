using Vox;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class SystemInfoTest
    {
        private ITestOutputHelper output;
        public SystemInfoTest(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void ShouldProvideOpenALInfo()
        {
            var device = new SoundDevice();

            var vendor = SystemInfo.OpenALVendor;
            var renderer = SystemInfo.OpenALRenderer;
            var version = SystemInfo.OpenALVersion;
            var extensions = SystemInfo.OpenALExtensions;

            Assert.NotNull(vendor);
            Assert.NotNull(renderer);
            Assert.NotNull(version);
            Assert.NotNull(extensions);

            output.WriteLine($"Vendor: {vendor}");
            output.WriteLine($"Renderer: {renderer}");
            output.WriteLine($"Version: {version}");
            output.WriteLine($"Extensions:\n{extensions}");
        }
    }
}