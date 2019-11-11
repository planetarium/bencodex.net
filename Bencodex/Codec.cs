using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Bencodex.Misc;
using Bencodex.Types;

namespace Bencodex
{
    /// <summary>The most basic and the lowest-level interface to encode and
    /// decode Bencodex values.  This provides two types of input and output:
    /// <c cref="byte">Byte</c> arrays and I/O
    /// <c cref="System.IO.Stream">Stream</c>s.</summary>
    public class Codec
    {
        /// <summary>
        /// Encodes a <paramref name="value"/> into a single
        /// <c cref="byte">Byte</c> array, rather than split into
        /// multiple chunks.</summary>
        /// <param name="value">A value to encode.</param>
        /// <returns>A single <c cref="byte">Byte</c> array which
        /// contains the whole Bencodex representation of
        /// the <paramref name="value"/>.</returns>
        [Pure]
        public byte[] Encode(IValue value)
        {
            var stream = new MemoryStream();
            Encode(value, stream);
            return stream.ToArray();
        }

        /// <summary>Encodes a <paramref name="value"/>,
        /// and write it on an <paramref name="output"/> stream.</summary>
        /// <param name="value">A value to encode.</param>
        /// <param name="output">A stream that a value is printed on.</param>
        /// <exception cref="ArgumentException">Thrown when a given
        /// <paramref name="output"/> stream is not writable.</exception>
        public void Encode(IValue value, Stream output)
        {
            if (!output.CanWrite)
            {
                throw new ArgumentException(
                    "stream cannot be written to",
                    nameof(output)
                );
            }

            foreach (byte[] chunk in value.EncodeIntoChunks())
            {
                output.Write(chunk, 0, chunk.Length);
            }
        }

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
                throw new ArgumentException(
                    "stream cannot be read",
                    nameof(input)
                );
            }

            var buffer = new ByteChunkQueue();
            IValue value = Decode(buffer, input);
            if (buffer.Empty)
            {
                buffer.ReadFrom(input, 1);
            }

            if (!buffer.Empty)
            {
                long offset = input.Position;
                throw new DecodingException(
                    $"an unexpected byte at {offset - buffer.ByteLength}: " +
                    $"0x{buffer.FirstByte:x}"
                );
            }

            return value;
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
        public IValue Decode(byte[] bytes)
        {
            return Decode(new MemoryStream(bytes, false));
        }

        private IValue Decode(ByteChunkQueue buffer, Stream input)
        {
            if (buffer.Empty)
            {
                buffer.ReadFrom(input, 1);
            }

            long pos = input.Position - buffer.ByteLength;
            switch (buffer.FirstByte)
            {
                case null:
                    throw new DecodingException(
                        $"stream terminates unexpectedly at {pos}"
                    );

                case 0x6e: // 'n'
                    buffer.Pop(1);
                    return default(Null);

                case 0x74: // 't'
                    buffer.Pop(1);
                    return new Bencodex.Types.Boolean(true);

                case 0x66: // 'f'
                    buffer.Pop(1);
                    return new Bencodex.Types.Boolean(false);

                case 0x69: // 'i'
                    buffer.Pop(1);
                    if (buffer.Empty)
                    {
                        buffer.ReadFrom(input, 1);
                    }

                    bool negative = false;
                    if (buffer.FirstByte == 0x2d) // '-'
                    {
                        buffer.Pop(1);
                        negative = true;
                    }

                    const byte e = 0x65;  // 'e'
                    BigInteger integer =
                        DecodeDigits(e, buffer, input, BigInteger.Parse);
                    return new Integer(negative ? -integer : integer);

                case 0x75: // 'u'
                    string text = DecodeText(buffer, input);
                    return new Text(text);

                case 0x6c: // 'l'
                    buffer.Pop(1);
                    var elements = new List<IValue>();
                    while (true)
                    {
                        if (buffer.Empty)
                        {
                            buffer.ReadFrom(input, 1);
                        }

                        if (buffer.FirstByte == 0x65) // 'e'
                        {
                            buffer.Pop(1);
                            break;
                        }

                        IValue element = Decode(buffer, input);
                        elements.Add(element);
                    }

                    return new List(elements);

                case 0x64: // 'd'
                    buffer.Pop(1);
                    var pairs = new List<KeyValuePair<IKey, IValue>>();
                    while (true)
                    {
                        if (buffer.Empty)
                        {
                            buffer.ReadFrom(input, 1);
                        }

                        byte? firstByte = buffer.FirstByte;
                        if (firstByte == null)
                        {
                            throw new DecodingException(
                                $"expected 'e' (0x65) at {pos}, but " +
                                "the stream terminates unexpectedly"
                            );
                        }

                        if (firstByte == 0x65) // 'e'
                        {
                            buffer.Pop(1);
                            break;
                        }

                        IKey key;
                        if (firstByte == 0x75) // 'u'
                        {
                            string textKey = DecodeText(buffer, input);
                            key = new Text(textKey);
                        }
                        else if (firstByte >= 0x30 && firstByte < 0x40)
                        {
                            byte[] binaryKey = DecodeBinary(buffer, input);
                            key = new Binary(binaryKey);
                        }
                        else
                        {
                            throw new DecodingException(
                                $"an unexpected byte 0x{firstByte:x} at {pos}"
                            );
                        }

                        IValue value = Decode(buffer, input);
                        pairs.Add(new KeyValuePair<IKey, IValue>(key, value));
                    }

                    return new Dictionary(pairs);

                case 0x30: // '0'
                case 0x31: // '1'
                case 0x32: // '2'
                case 0x33: // '3'
                case 0x34: // '4'
                case 0x35: // '5'
                case 0x36: // '6'
                case 0x37: // '7'
                case 0x38: // '8'
                case 0x39: // '9'
                    byte[] binary = DecodeBinary(buffer, input);
                    return new Binary(binary);
            }

            throw new DecodingException(
                $"an unexpected byte 0x{buffer.FirstByte:x} at {pos}"
            );
        }

        private string DecodeText(ByteChunkQueue buffer, Stream input)
        {
            byte[] singleByteBuffer = buffer.Pop(1);
            if (singleByteBuffer.Length < 1)
            {
                throw new DecodingException(
                    $"expected 'u' (0x75) at {input.Position - 1}, " +
                    "but the stream terminates"
                );
            }

            if (singleByteBuffer[0] != 0x75)
            {
                throw new DecodingException(
                    $"expected 'u' (0x75) at {input.Position - 1}, " +
                    $"but 0x{singleByteBuffer[0]:x} is given"
                );
            }

            long pos = input.Position;
            byte[] utf8 = DecodeBinary(buffer, input);
            try
            {
                return Encoding.UTF8.GetString(utf8);
            }
            catch (ArgumentException e)
            {
                throw new DecodingException(
                    $"expected a UTF-8 sequence at {pos}",
                    e
                );
            }
        }

        private byte[] DecodeBinary(ByteChunkQueue buffer, Stream input)
        {
            const byte colon = 0x3a;  // ':'
            long length = DecodeDigits(colon, buffer, input, long.Parse);
            if (length < 1)
            {
                return new byte[0];
            }

            byte[] popped = buffer.Pop(length);
            if (popped.LongLength < length)
            {
                byte[] result = new byte[length];
                popped.CopyTo(result, 0);

                // FIXME: These Int64 to Int32 casts should be corrected.
                input.Read(
                    result,
                    (int)popped.LongLength,
                    (int)(length - popped.LongLength)
                );
                return result;
            }

            return popped;
        }

        private T DecodeDigits<T>(
            byte terminator,
            ByteChunkQueue buffer,
            Stream input,
            Func<string, T> converter
        )
        {
            if (buffer.Empty)
            {
                int read = buffer.ReadFrom(input, 1);
                if (read < 1)
                {
                    throw new DecodingException(
                        $"expected one or more digits at {input.Position}, " +
                        "but the byte stream terminates"
                    );
                }
            }

            long pos;
            while ((pos = buffer.IndexOf(terminator)) < 0)
            {
                int read = buffer.ReadFrom(input, 8);
                if (read < 0)
                {
                    throw new DecodingException(
                        $"expected a byte 0x{terminator:x} at " +
                        $"{input.Position}, but the byte stream terminates"
                    );
                }
            }

            byte[] digitBytes = buffer.Pop(pos);
            if (!digitBytes.All(b => b >= 0x30 && b < 0x40))
            {
                long digitsOffset =
                    input.Position - buffer.ByteLength - digitBytes.LongLength;
                throw new DecodingException(
                    $"expected 10-base digits at {digitsOffset}: " +
                    BitConverter.ToString(digitBytes)
                );
            }

            buffer.Pop(1);  // pop terminator
            string digits = Encoding.ASCII.GetString(digitBytes);
            return converter(digits);
        }
    }
}
