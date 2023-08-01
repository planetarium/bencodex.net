using System;
using System.Diagnostics.Contracts;
using System.IO;
using Bencodex.Types;

namespace Bencodex
{
    /// <summary>The most basic and the lowest-level interface to encode and
    /// decode Bencodex values.  This provides two types of input and output:
    /// <c cref="byte">Byte</c> arrays and I/O
    /// <c cref="System.IO.Stream">Stream</c>s.</summary>
    public class Codec
    {
        /// <summary>Encodes a <paramref name="value"/>, and writes it on
        /// the <paramref name="output"/> stream.</summary>
        /// <param name="value">A value to encode.</param>
        /// <param name="output">A stream that a value is printed on.</param>
        /// <exception cref="ArgumentException">Thrown when the given <paramref name="output"/>
        /// stream is not writable.</exception>
        public void Encode(IValue value, Stream output) =>
            Encoder.Encode(value, output);

        /// <summary>
        /// Encodes a <paramref name="value"/> into a single
        /// <c cref="byte">Byte</c> array, rather than split into
        /// multiple chunks.</summary>
        /// <param name="value">A value to encode.</param>
        /// <returns>A single <c cref="byte">Byte</c> array which
        /// contains the whole Bencodex representation of
        /// the <paramref name="value"/>.</returns>
        [Pure]
        public byte[] Encode(IValue value) => Encoder.Encode(value);

        /// <summary>Decodes an encoded value from an <paramref name="input"/>
        /// stream.</summary>
        /// <param name="input">An input stream to decode.</param>
        /// <returns>A decoded value.</returns>
        /// <exception cref="ArgumentException">Thrown when a given
        /// <paramref name="input"/> stream is not readable.</exception>
        /// <exception cref="DecodingException">Thrown when a binary
        /// representation of an <paramref name="input"/> stream is not a valid
        /// Bencodex encoding.</exception>
        public IValue Decode(Stream input)
        {
            if (!input.CanRead)
            {
                throw new ArgumentException("The input stream cannot be read.", nameof(input));
            }

            return new Decoder(input).Decode();
        }

        /// <summary>Decodes an encoded value from a
        /// <c cref="byte">Byte</c> array.</summary>
        /// <param name="bytes">A <c cref="byte">Byte</c> array of
        /// Bencodex encoding.</param>
        /// <returns>A decoded value.</returns>
        /// <exception cref="DecodingException">Thrown when a
        /// <paramref name="bytes"/> representation is not a valid Bencodex
        /// encoding.</exception>
        [Pure]
        public IValue Decode(byte[] bytes) => Decode(new MemoryStream(bytes, false));
    }
}
