using System;
using System.Collections.Generic;
using static OpenAL.AL10;

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
        private static Dictionary<int, string> messages = new Dictionary<int, string>()
        {
            { AL_INVALID_NAME, "Invalid name" },
            { AL_INVALID_ENUM, "Invalid enum" },
            { AL_INVALID_VALUE, "Invalid value" },
            { AL_INVALID_OPERATION, "Invalid operation" }
        };

        public static void Reset() => alGetError();

        public static void Check(string methodName)
        {
            var code = alGetError();
            if (code != AL_NO_ERROR)
                throw new AudioLibraryException($"{methodName}: {messages[code]}");
        }
    }
}