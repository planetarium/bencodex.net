using System;

namespace Bencodex
{
    /// <summary>The exception that is thrown when an input is not
    /// a valid Bencodex encoding so that a decoder cannot parse it.</summary>
    /// <inheritdoc />
    public class DecodingException : CodecException
    {
        public DecodingException(string message)
            : base(message)
        {
        }

        public DecodingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
