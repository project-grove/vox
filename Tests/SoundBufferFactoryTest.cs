using Vox;
using Xunit;

namespace Tests
{
    public class SoundBufferFactoryTest
    {
        [Fact]
        public void ShouldCreateSpecifiedCountOfBuffers()
        {
            var count = 5;
            var device = new OutputDevice();
            var buffers = SoundBufferFactory.Create(count);
            Assert.Equal(count, buffers.Length);
            Assert.All(buffers, buffer => Assert.Equal(device, buffer.Owner));
        }        
    }
}