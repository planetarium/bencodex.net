using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

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
        /// <summary>Encodes the value into <c cref="System.Byte">Byte</c>
        /// arrays.</summary>
        /// <returns><c cref="System.Byte">Byte</c> arrays of Bencodex
        /// representation of the value.</returns>
        /// <seealso cref="ValueExtensions.EncodeIntoStream"/>
        [Pure]
        IEnumerable<byte[]> EncodeIntoChunks();
    }
}
