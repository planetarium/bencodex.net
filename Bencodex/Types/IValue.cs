using System;
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
    public interface IValue : IEquatable<IValue>
    {
        /// <summary>The number of bytes used for serializing the value.</summary>
        [Pure]
        int EncodingLength { get; }

        /// <summary>A JSON-like human-readable representation for
        /// debugging.</summary>
        /// <returns>A JSON-like representation.</returns>
        [Pure]
        string Inspection { get; }

        /// <summary>Encodes the value into <c cref="byte">Byte</c>
        /// arrays.</summary>
        /// <returns><c cref="byte">Byte</c> arrays of Bencodex
        /// representation of the value.</returns>
        /// <seealso cref="Codec.Encode(IValue)"/>
        /// <seealso cref="Codec.Encode(IValue, System.IO.Stream)"/>
        [Pure]
        IEnumerable<byte[]> EncodeIntoChunks();

        /// <summary>Writes the encoded value into <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">A stream to write the encoded value.</param>
        /// <seealso cref="Codec.Encode(IValue)"/>
        /// <seealso cref="Codec.Encode(IValue, System.IO.Stream)"/>
        /// <seealso cref="EncodeIntoChunks"/>
        void EncodeToStream(Stream stream);
    }
}
