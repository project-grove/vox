using System;
using System.Numerics;
using static OpenAL.AL10;
using static Vox.ErrorHandler;
using static Vox.Internal.Util;

namespace Vox
{
    /// <summary>
    /// Represents a sound listener object.
    /// </summary>
    public class SoundListener
    {
        /// <summary>
        /// Retrieves the listener for the currently selected sound device.
        /// </summary>
        public static SoundListener Current => OutputDevice.Current?.Listener;

        private float _gain;
        /// <summary>
        /// Sound gain. Must be a positive value.
        /// </summary>
        public float Gain
        {
            get => _gain;
            set
            {
                if (value < 0.0f)
                    throw new ArgumentOutOfRangeException("Gain must be a positive number");
                if (_owner.IsDisposed)
                    throw new ObjectDisposedException(nameof(OutputDevice));
                UseDevice(_owner, () =>
                    AL(() =>
                        alListenerf(AL_GAIN, value), "alListenerf(AL_GAIN)"));
                _gain = value;
            }
        }

        private Vector3 _position;
        /// <summary>
        /// X, Y, Z position.
        /// </summary>
        public Vector3 Position
        {
            get => _position;
            set
            {
                if (_owner.IsDisposed)
                    throw new ObjectDisposedException(nameof(OutputDevice));
                UseDevice(_owner, () =>
                    AL(() =>
                        alListener3f(AL_POSITION, value.X, value.Y, value.Z),
                        "alListener3f(AL_POSITION)"));
                _position = value;
            }
        }

        private Vector3 _velocity;
        /// <summary>
        /// Velocity vector.
        /// </summary>
        public Vector3 Velocity
        {
            get => _velocity;
            set
            {
                if (_owner.IsDisposed)
                    throw new ObjectDisposedException(nameof(OutputDevice));
                UseDevice(_owner, () =>
                    AL(() =>
                        alListener3f(AL_VELOCITY, value.X, value.Y, value.Z),
                        "alListener3f(AL_VELOCITY)"));
                _velocity = value;
            }
        }

        private float[] _orientation;

        private readonly OutputDevice _owner;
        internal SoundListener(OutputDevice owner) => _owner = owner;

        /// <summary>
        /// Gets the listener's orientations described by 'at' and 'up' vectors.
        /// </summary>
        public void GetOrientation(out Vector3 at, out Vector3 up)
        {
            at = new Vector3(_orientation[0], _orientation[1], _orientation[2]);
            up = new Vector3(_orientation[3], _orientation[4], _orientation[5]);
        }

        /// <summary>
        /// Sets the listener's orientations described by 'at' and 'up' vectors.
        /// </summary>
        public void SetOrientation(Vector3 at, Vector3 up)
        {
            if (_owner.IsDisposed)
                throw new ObjectDisposedException(nameof(OutputDevice));
            _orientation[0] = at.X; _orientation[1] = at.Y; _orientation[2] = at.Z;
            _orientation[3] = up.X; _orientation[4] = up.Y; _orientation[5] = up.Z;
            UseDevice(_owner, () =>
                AL(() =>
                    alListenerfv(AL_ORIENTATION, _orientation),
                    "alListenerfv(AL_ORIENTATION)"));
        }

        internal void UpdateValues()
        {
            AL(() => alGetListenerf(AL_GAIN, out _gain), "alGetListenerf(AL_GAIN)");

            var position = new Vector3();
            AL(() =>
                alGetListener3f(AL_POSITION, out position.X, out position.Y, out position.Z),
                "alGetListener3f(AL_POSITION)");
            _position = position;

            var velocity = new Vector3();
            AL(() =>
                alGetListener3f(AL_VELOCITY, out velocity.X, out velocity.Y, out velocity.Z),
                "alGetListener3f(AL_VELOCITY)");
            _velocity = velocity;

            float[] orientation = new float[6];
            AL(() =>
                alGetListenerfv(AL_ORIENTATION, orientation),
                "alGetListenerfv(AL_ORIENTATION)");
            _orientation = orientation;
        }
    }
}