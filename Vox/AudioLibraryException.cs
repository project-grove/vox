#undef TRACE // remove to enable AL call tracing

using System;
using System.Collections.Generic;
using System.Diagnostics;
using static OpenAL.AL10;
using static OpenAL.ALC10;

namespace Vox
{
/// <summary>
/// Represents an Vox library exception.
/// </summary>
public class AudioLibraryException : Exception
{
	/// <summary>
	/// Default constructor.
	/// </summary>
	public AudioLibraryException()
	{
	}

	/// <summary>
	/// Constructor which accepts a message.
	/// </summary>
	public AudioLibraryException(string message) : base(message)
	{
	}

	/// <summary>
	/// Constructor which accepts a message and wraps an existing exception.
	/// </summary>
	public AudioLibraryException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}

internal static class ErrorHandler
{
	private static Dictionary<int, string> messagesAL = new Dictionary<int, string>()
	{
		{AL_INVALID_NAME, "Invalid name"},
		{AL_INVALID_ENUM, "Invalid enum"},
		{AL_INVALID_VALUE, "Invalid value"},
		{AL_INVALID_OPERATION, "Invalid operation"},
		{AL_OUT_OF_MEMORY, "Out of memory"}
	};

	private static Dictionary<int, string> messagesALC = new Dictionary<int, string>()
	{
		{ALC_INVALID_CONTEXT, "Invalid context"},
		{ALC_INVALID_DEVICE, "Invalid device"},
		{ALC_INVALID_ENUM, "Invalid enum"},
		{ALC_INVALID_VALUE, "Invalid value"},
		{ALC_OUT_OF_MEMORY, "Out of memory"}
	};

	public static void ResetAL()
	{
		alGetError();
	}

	public static void ResetALC(IntPtr device)
	{
		alcGetError(device);
	}

	public static void CheckAL(string methodName)
	{
		var code = alGetError();
		if (code != AL_NO_ERROR)
			throw new AudioLibraryException($"{methodName}: {messagesAL[code]}");
	}

	public static void CheckALC(string methodName, IntPtr device)
	{
		var code = alcGetError(device);
		if (code != ALC_NO_ERROR)
			throw new AudioLibraryException($"{methodName}: {messagesALC[code]}");
	}

	public static void AL(Action action, string methodName)
	{
		ResetAL();
		action();
		Trace.WriteLine(methodName);
		CheckAL(methodName);
	}

	public static T AL<T>(Func<T> function, string methodName)
	{
		ResetAL();
		var result = function();
		Trace.WriteLine($"{methodName}: {result}");
		CheckAL(methodName);
		return result;
	}

	public static void ALC(Action action, string methodName, IntPtr device)
	{
		ResetALC(device);
		action();
		Trace.WriteLine(methodName);
		CheckALC(methodName, device);
	}

	public static T ALC<T>(Func<T> function, string methodName, IntPtr device)
	{
		ResetALC(device);
		var result = function();
		Trace.WriteLine($"{methodName}: {result}");
		CheckALC(methodName, device);
		return result;
	}
}
}