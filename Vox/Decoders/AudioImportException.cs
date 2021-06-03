using System;

namespace Vox.Decoders
{
/// <summary>
/// Represents an exception which is thrown if the audio import fails.
/// </summary>
public class AudioImportException : Exception
{
	/// <summary>
	/// Default constructor.
	/// </summary>
	public AudioImportException() : base()
	{
	}

	/// <summary>
	/// Constructor which accepts a message.
	/// </summary>
	public AudioImportException(string message) : base(message)
	{
	}

	/// <summary>
	/// Constructor which accepts a message and wraps an existing exception.
	/// </summary>
	public AudioImportException(string message, Exception innerException) :
		base(message, innerException)
	{
	}
}
}