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
			UseDevice(_owner, (v) =>
				          AL((p) =>
					             alListenerf(AL_GAIN, p), "alListenerf(AL_GAIN)", v), value);
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
			UseDevice(_owner, (v) =>
				          AL((p) =>
					             alListener3f(AL_POSITION, p.X, p.Y, p.Z),
				             "alListener3f(AL_POSITION)", v), value);
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
			UseDevice(_owner, (v) =>
				          AL((p) =>
					             alListener3f(AL_VELOCITY, p.X, p.Y, p.Z),
				             "alListener3f(AL_VELOCITY)", v), value);
			_velocity = value;
		}
	}

	private float[] _orientation = new float[6];

	private readonly OutputDevice _owner;

	internal SoundListener(OutputDevice owner)
	{
		_owner = owner;
	}

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
		_orientation[0] = at.X;
		_orientation[1] = at.Y;
		_orientation[2] = at.Z;
		_orientation[3] = up.X;
		_orientation[4] = up.Y;
		_orientation[5] = up.Z;
		UseDevice(_owner, (v) =>
			          AL((p) =>
				             alListenerfv(AL_ORIENTATION, p),
			             "alListenerfv(AL_ORIENTATION)", v), _orientation);
	}

	private Vector3 _rVector3 = Vector3.Zero;
	private float[] _rOrientation = new float[6];

	internal void UpdateValues()
	{
		AL((p) => alGetListenerf(AL_GAIN, out p._gain), "alGetListenerf(AL_GAIN)", this);

		AL((p) =>
			   alGetListener3f(AL_POSITION, out p._rVector3.X, out p._rVector3.Y,
			                   out p._rVector3.Z),
		   "alGetListener3f(AL_POSITION)", this);
		_position = _rVector3;

		AL((p) =>
			   alGetListener3f(AL_VELOCITY, out p._rVector3.X, out p._rVector3.Y,
			                   out p._rVector3.Z),
		   "alGetListener3f(AL_VELOCITY)", this);
		_velocity = _rVector3;

		AL((p) =>
			   alGetListenerfv(AL_ORIENTATION, p),
		   "alGetListenerfv(AL_ORIENTATION)", _rOrientation);
		_rOrientation.AsSpan().CopyTo(_orientation);
	}
}
}