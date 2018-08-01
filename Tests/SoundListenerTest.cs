using System;
using System.Numerics;
using Vox;
using Xunit;

namespace Tests
{
    public class SoundListenerTest : IDisposable
    {
        OutputDevice device;
        SoundListener listener;

        public SoundListenerTest()
        {
            device = new OutputDevice();
            listener = device.Listener;
        }

        [Fact]
        public void ShouldAllowGainChange()
        {
            var gain = 1.0f;
            listener.Gain = gain;
            Assert.Equal(gain, listener.Gain);
            listener.UpdateValues(); // get the values from underlying implementation
            Assert.Equal(gain, listener.Gain);
        }

        [Fact]
        public void ShouldAllowPositionChange()
        {
            var position = Vector3.One;
            listener.Position = position;
            Assert.Equal(position, listener.Position);
            listener.UpdateValues();
            Assert.Equal(position, listener.Position);
        }

        [Fact]
        public void ShouldAllowVelocityChange()
        {
            var velocity = Vector3.One;
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
            listener.SetOrientation(at, up);
            listener.GetOrientation(out Vector3 at1, out Vector3 up1);
            Assert.Equal(at, at1);
            Assert.Equal(up, up1);
            listener.UpdateValues();
            listener.GetOrientation(out Vector3 at2, out Vector3 up2);
            Assert.Equal(at, at2);
            Assert.Equal(up, up2);
        }

        public void Dispose() => device.Dispose();
    }
}