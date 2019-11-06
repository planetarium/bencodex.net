using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Bencodex.Types
{
    /// <summary>Represents a possible value in Bencodex representation.
    /// </summary>
    /// <seealso cref="Null"/>
    /// <seealso cref="Boolean"/>
    /// <seealso cref="Integer"/>
    /// <seealso cref="Binary"/>
    /// <seealso cref="Text"/>
    /// <seealso cref="List"/>
    /// <seealso cref="Dictionary"/>
    public interface IValue
    {
        /// <summary>Encodes the value into <c cref="byte">Byte</c>
        /// arrays.</summary>
        /// <returns><c cref="byte">Byte</c> arrays of Bencodex
        /// representation of the value.</returns>
        /// <seealso cref="Codec.Encode(IValue)"/>
        /// <seealso cref="Codec.Encode(IValue, System.IO.Stream)"/>
        [Pure]
        IEnumerable<byte[]> EncodeIntoChunks();
    }
}
