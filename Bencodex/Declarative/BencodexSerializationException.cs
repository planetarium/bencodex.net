using System;

namespace Bencodex.Declarative
{
    public class BencodexSerializationException : Exception
    {
        public BencodexSerializationException(string message)
            : base(message)
        {
        }
    }
}
