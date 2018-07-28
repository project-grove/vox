using System.Numerics;
using Vox;
using Xunit;

namespace Tests
{
    public class ListenerTest
    {
        [Fact]
        public void ShouldAllowGainChange()
        {
            var gain = 1.0f;
            var listener = new OutputDevice().Listener;

            listener.Gain = gain;
            Assert.Equal(gain, listener.Gain);
            listener.UpdateValues(); // get the values from underlying implementation
            Assert.Equal(gain, listener.Gain);
        }

        [Fact]
        public void ShouldAllowPositionChange()
        {
            var position = Vector3.One;
            var listener = new OutputDevice().Listener;

            listener.Position = position;
            Assert.Equal(position, listener.Position);
            listener.UpdateValues();
            Assert.Equal(position, listener.Position);
        }

        [Fact]
        public void ShouldAllowVelocityChange()
        {
            var velocity = Vector3.One;
            var listener = new OutputDevice().Listener;

            listener.Velocity = velocity;
            Assert.Equal(velocity, listener.Velocity);
            listener.UpdateValues();
            Assert.Equal(velocity, listener.Velocity);
        }

        [Fact]
        public void ShouldAllowOrientationChange()
        {
            var at = Vector3.UnitX;
            var up = Vector3.UnitZ;
            var listener = new OutputDevice().Listener;

            listener.SetOrientation(at, up);
            listener.GetOrientation(out Vector3 at1, out Vector3 up1);
            Assert.Equal(at, at1);
            Assert.Equal(up, up1);
            listener.UpdateValues();
            listener.GetOrientation(out Vector3 at2, out Vector3 up2);
            Assert.Equal(at, at2);
            Assert.Equal(up, up2);
        }
    }
}