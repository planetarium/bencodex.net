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

    /// <summary>Provides some façade methods upon <c cref="IValue">IValue</c>
    /// instances.</summary>
    public static class ValueExtensions
    {
        /// <summary>
        /// Encodes a <paramref name="value"/> into a single
        /// <c cref="System.Byte">Byte</c> array, rather than split into
        /// multiple chunks.</summary>
        /// <param name="value">A value to encode.</param>
        /// <returns>A single <c cref="System.Byte">Byte</c> array which
        /// contains the whole Bencodex representation of
        /// the <paramref name="value"/>.</returns>
        [Pure]
        public static byte[] EncodeIntoByteArray(this IValue value)
        {
            var stream = new MemoryStream();
            value.EncodeIntoStream(stream);
            return stream.ToArray();
        }

        /// <summary>Encodes a <paramref name="value"/>,
        /// and write it on a <paramref name="stream"/>.</summary>
        /// <param name="value">A value to encode.</param>
        /// <param name="stream">A stream that a value is printed on.</param>
        /// <seealso cref="IValue.EncodeIntoChunks"/>
        public static void EncodeIntoStream(this IValue value, Stream stream)
        {
            foreach (byte[] chunk in value.EncodeIntoChunks())
            {
                stream.Write(chunk, 0, chunk.Length);
            }
        }
    }
}
