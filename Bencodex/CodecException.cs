using System;

namespace Bencodex
{
    /// <summary>Serves as the base class for codec exceptions.</summary>
    /// <inheritdoc />
    public class CodecException : Exception
    {
        public CodecException(string message)
            : base(message)
        {
        }

        public CodecException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
