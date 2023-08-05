using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using Bencodex.Misc;
using Bencodex.Types;

namespace Bencodex
{
    internal sealed class Decoder
    {
        private readonly Stream _stream;
        private byte _lastRead;
        private int _offset;

        public Decoder(Stream stream)
        {
            // We assume the stream is buffered by itself.  Otherwise the caller should wrap it
            // with BufferedStream: stream = new BufferedStream(stream);
            _stream = stream;
            _lastRead = 0;
            _offset = 0;
        }

        public IValue Decode()
        {
            IValue value = DecodeValue() ??
                throw new DecodingException(
                    $"An unexpected token byte 0x{0x65:x} at {_offset - 1}");

            return EndOfStream()
                ? value
                : throw new DecodingException(
                    $"An unexpected trailing byte remains at {_offset}.");
        }

        private IValue? DecodeValue()
        {
            switch (ReadByte())
            {
                case 0x65: // 'e'
                    return null;

                case 0x6e: // 'n'
#pragma warning disable SA1129
                    return new Null();
#pragma warning restore SA1129

                case 0x74: // 't'
                    return new Bencodex.Types.Boolean(true);

                case 0x66: // 'f'
                    return new Bencodex.Types.Boolean(false);

                case 0x69: // 'i'
                    BigInteger integer = ReadInteger();
                    return new Integer(integer);

                case 0x75: // 'u'
                    return ReadTextAfterPrefix();

                case 0x6c: // 'l'
                    var elements = new List<IValue>();
                    while (DecodeValue() is IValue element)
                    {
                        elements.Add(element);
                    }

                    return new Bencodex.Types.List(elements);

                case 0x64: // 'd'
                    var builder = ImmutableSortedDictionary.CreateBuilder<IKey, IValue>(KeyComparer.Instance);
                    IKey? lastKey = null;
                    while (DecodeKey() is IKey key)
                    {
                        IValue value = DecodeValue()
                            ?? throw new DecodingException(
                                $"An unexpected token byte 0x{0x65:x} at {_offset - 1}");

                        if (lastKey is { } k && builder.KeyComparer.Compare(k, key) >= 0)
                        {
                            throw new DecodingException(
                                $"Expected an {nameof(IKey)} greater than {k}: {key}");
                        }

                        lastKey = key;
                        builder.Add(key, value);
                    }

                    return new Dictionary(builder.ToImmutable());

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
                    return ReadBinary();

                case { } b:
                    throw new DecodingException($"An unexpected byte 0x{b:x} at {_offset - 1}.");
            }
        }

        private IKey? DecodeKey()
        {
            switch (ReadByte())
            {
                case 0x65: // 'e'
                    return null;

                case 0x75: // 'u':
                    return ReadTextAfterPrefix();

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
                    return ReadBinary();

                case { } b:
                    throw new DecodingException(
                        $"Expected a dictionary key, but got an unexpected byte 0x{b:x} at " +
                        $"{_offset - 1}."
                    );
            }
        }

        /// <summary>
        /// Fills given <paramref name="buffer"/> from the internal <see cref="Stream"/>.
        /// </summary>
        /// <param name="buffer">The buffer to fill.</param>
        /// <exception cref="DecodingException">Thrown when the internal <see cref="Stream"/>
        /// terminates before <paramref name="buffer"/> is completely filled.</exception>
        /// <remarks>This is used only for decoding a <see cref="Text"/> or
        /// a <see cref="Binary"/> after the separator token ':' has been consumed.
        /// </remarks>
        private void Read(byte[] buffer)
        {
            var length = buffer.Length;
            int read = _stream.Read(buffer, 0, length);
            _offset += read;

            if (read < length)
            {
                throw new DecodingException(
                    $"The byte stream terminates at {_offset}.");
            }
        }

        private byte ReadByte()
        {
            int read = _stream.ReadByte();
            _lastRead = read < 0
                ? throw new DecodingException($"The byte stream terminates unexpectedly at {_offset}.")
                : (byte)read;
            _offset++;
            return _lastRead;
        }

        // Checks end of stream.  Should be called only once at the very end.
        private bool EndOfStream()
        {
            return _stream.ReadByte() < 0;
        }

        // Reads the length portion for byte strings and unicode strings.
        private int ReadLength(bool peeked)
        {
            const byte colon = 0x3a;    // ':'
            const int asciiZero = 0x30; // '0'
            int length = 0;

            byte lastByte = peeked ? _lastRead : ReadByte();
            while (lastByte != colon)
            {
#pragma warning disable SA1131
                if (lastByte < 0x30 || 0x39 < lastByte) // not '0'-'9'
#pragma warning restore SA1131
                {
                    throw new ArgumentException(
                        $"Expected a digit (0x30-0x39), but got 0x{lastByte:x} at {_offset}."
                    );
                }

                length *= 10;
                length += lastByte - asciiZero;

                lastByte = ReadByte();
            }

            return length;
        }

        /// <summary>
        /// Reads the value portion of an encoded <see cref="Integer"/> and
        /// its end token 'e'.
        /// </summary>
        /// <param name="buffer">The buffer to fill.  Its size may be adjusted
        /// when necessary.</param>
        /// <returns>The number of bytes read until 'e' is encountered from the
        /// internal <see cref="Stream"/>.</returns>
        /// <remarks>
        /// This is called only from <see cref="ReadInteger"/> after
        /// a beginning token 'i' has been consumed.
        /// </remarks>
        private int ReadDigits(ref byte[] buffer)
        {
            const byte e = 0x65;
            int digitsLength = 0;
            byte b = ReadByte();
            while (b != e)
            {
                if (digitsLength >= buffer.Length)
                {
                    Array.Resize(ref buffer, buffer.Length * 2);
                }

                buffer[digitsLength] = b;
                digitsLength++;
                b = ReadByte();
            }

            return digitsLength;
        }

        /// <summary>
        /// Reads the value portion of an encoded <see cref="Integer"/> and
        /// its end token 'e'.
        /// </summary>
        /// <returns>A <see cref="BigInteger"/> corresponding to the
        /// value portion of an encoded <see cref="Integer"/>.</returns>
        /// <exception cref="DecodingException">Thrown for any reason
        /// where the byte array representing the value portion is
        /// invalid.</exception>
        private BigInteger ReadInteger()
        {
            const byte zero = 0x30;
            const byte plus = 0x2b;
            const byte minus = 0x2d;

            const int defaultBufferSize = 10;
            byte[] buffer = new byte[defaultBufferSize];
            int length = ReadDigits(ref buffer);

            // Checks for invalid formats allowed by BigInteger.Parse below.
            // - "": Handled by BigInteger.Parse.  Not allowed.
            // - "x": Handled by bigInteger.Parse.  Non-digits aren't allowed.
            // - "+x...": Starting with a '+'.
            // - "0x...": Starting with a '0' without immediately terminating.
            // - "-0...": Starting with a '-' followed by a '0'.
            if (length >= 2)
            {
                if (buffer[0] == plus)
                {
                    throw new DecodingException(
                        $"Encountered an unexpected byte 0x{plus:x} " +
                        $"at {_offset - length}");
                }

                if (buffer[0] == zero)
                {
                    throw new DecodingException(
                        $"Encountered an unexpected byte 0x{buffer[1]:x} " +
                        $"at {_offset - length + 1}");
                }

                if (buffer[0] == minus && buffer[1] == zero)
                {
                    throw new DecodingException(
                        $"Encountered an unexpected byte 0x{buffer[1]:x}" +
                        $"at {_offset - length + 1}");
                }
            }

            try
            {
                return BigInteger.Parse(
                    Encoding.ASCII.GetString(buffer, 0, length),
                    NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                throw new DecodingException(
                    $"Encountered an invalid encoded integer at {_offset - length}",
                    e);
            }
        }

        private Binary ReadBinary()
        {
            int length = ReadLength(peeked: true);
            byte[] buffer = new byte[length];
            Read(buffer);
            return new Binary(buffer);
        }

        private Text ReadTextAfterPrefix()
        {
            int start = _offset - 1;
            int length = ReadLength(peeked: false);
            byte[] buffer = new byte[length];
            Read(buffer);

            string textContent;
            try
            {
                textContent = Encoding.UTF8.GetString(buffer);
            }
            catch (ArgumentException e)
            {
                throw new DecodingException(
                    $"Failed to decode {nameof(Text)} starting from {start}.", e);
            }

            return new Text(textContent);
        }
    }
}
