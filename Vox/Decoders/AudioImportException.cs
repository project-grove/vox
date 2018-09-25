using System;

namespace Vox.Decoders
{
    public class AudioImportException : Exception
    {
        public AudioImportException() : base() { }
        public AudioImportException(string message) : base(message) { }
        public AudioImportException(string message, Exception innerException) : 
            base(message, innerException) { }
    }
}