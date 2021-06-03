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

public static class VoxEvents
{
	public static bool EnableTracing = false;
	public static readonly TraceSource OpenALTraceSource = new TraceSource("OpenAL");
}

internal static class ErrorHandler
{
	private static Dictionary<int, string> s_messagesAL = new Dictionary<int, string>()
	{
		{AL_INVALID_NAME, "Invalid name"},
		{AL_INVALID_ENUM, "Invalid enum"},
		{AL_INVALID_VALUE, "Invalid value"},
		{AL_INVALID_OPERATION, "Invalid operation"},
		{AL_OUT_OF_MEMORY, "Out of memory"}
	};

	private static Dictionary<int, string> s_messagesALC = new Dictionary<int, string>()
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
		{
			var msg = s_messagesAL.ContainsKey(code) ? s_messagesAL[code] : code.ToString();
			var traceMsg = $"{methodName}: {msg}";
			Trace(TraceEventType.Error, code, traceMsg);
			throw new AudioLibraryException(traceMsg);
		}
	}

	public static void CheckALC(string methodName, IntPtr device)
	{
		var code = alcGetError(device);
		if (code != ALC_NO_ERROR)
		{
			var msg = s_messagesALC.ContainsKey(code) ? s_messagesALC[code] : code.ToString();
			var traceMsg = $"{methodName}: {msg}";
			Trace(TraceEventType.Error, code, traceMsg);
			throw new AudioLibraryException(traceMsg);
		}
	}

	public static void AL(Action action, string methodName)
	{
		ResetAL();
		action();
		Trace(methodName);
		CheckAL(methodName);
	}

	public static void AL<T>(Action<T> action, string methodName, T data)
	{
		ResetAL();
		action(data);
		Trace(methodName);
		CheckAL(methodName);
	}

	public static T AL<T>(Func<T> function, string methodName)
	{
		ResetAL();
		var result = function();
		Trace($"{methodName}: {result}");
		CheckAL(methodName);
		return result;
	}

	public static void ALC(Action action, string methodName, IntPtr device)
	{
		ResetALC(device);
		action();
		Trace(methodName);
		CheckALC(methodName, device);
	}

	public static void ALC(Action<IntPtr> action, string methodName, IntPtr device)
	{
		ResetALC(device);
		action(device);
		Trace(methodName);
		CheckALC(methodName, device);
	}

	public static void ALC<T>(Action<T> action, string methodName, IntPtr device, T data)
	{
		ResetALC(device);
		action(data);
		Trace(methodName);
		CheckALC(methodName, device);
	}

	public static T ALC<T>(Func<T> function, string methodName, IntPtr device)
	{
		ResetALC(device);
		var result = function();
		Trace($"{methodName}: {result}");
		CheckALC(methodName, device);
		return result;
	}

	public static T ALC<T>(Func<IntPtr, T> function, string methodName, IntPtr device)
	{
		ResetALC(device);
		var result = function(device);
		Trace($"{methodName}: {result}");
		CheckALC(methodName, device);
		return result;
	}

	public static R ALC<T, R>(Func<T, R> function, string methodName, IntPtr device, T data)
	{
		ResetALC(device);
		var result = function(data);
		Trace($"{methodName}: {result}");
		CheckALC(methodName, device);
		return result;
	}

	private static void Trace(string message)
	{
		Trace(TraceEventType.Verbose, 0, message);
	}

	private static void Trace(TraceEventType type, int id, string message)
	{
		if (!VoxEvents.EnableTracing) return;
		VoxEvents.OpenALTraceSource.TraceEvent(type, id, message);
	}
}
}