using System;
using System.Collections.Generic;
using static OpenAL.AL10;
using static OpenAL.ALC10;

namespace Vox
{
    public class AudioLibraryException : Exception
    {
        public AudioLibraryException() { }
        public AudioLibraryException(string message) : base(message) { }
        public AudioLibraryException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public static class ErrorHandler
    {
        private static Dictionary<int, string> messagesAL = new Dictionary<int, string>()
        {
            { AL_INVALID_NAME, "Invalid name" },
            { AL_INVALID_ENUM, "Invalid enum" },
            { AL_INVALID_VALUE, "Invalid value" },
            { AL_INVALID_OPERATION, "Invalid operation" }
        };

        private static Dictionary<int, string> messagesALC = new Dictionary<int, string>()
        {
            { ALC_INVALID_CONTEXT, "Invalid context" },
            { ALC_INVALID_DEVICE, "Invalid device" },
            { ALC_INVALID_ENUM, "Invalid enum" },
            { ALC_INVALID_VALUE, "Invalid value" }
        };

        public static void Reset() => alGetError();

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
            Reset();
            action();
            CheckAL(methodName);
        }

        public static T AL<T>(Func<T> function, string methodName)
        {
            Reset();
            var result = function();
            CheckAL(methodName);
            return result;
        }

        public static void ALC(Action action, string methodName, IntPtr device)
        {
            Reset();
            action();
            CheckALC(methodName, device);
        }

        public static T ALC<T>(Func<T> function, string methodName, IntPtr device)
        {
            Reset();
            var result = function();
            CheckALC(methodName, device);
            return result;
        }
    }
}