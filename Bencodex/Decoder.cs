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
        private readonly byte[] _tinyBuffer = new byte[1];
        private readonly Stream _stream;
        private byte _lastRead;
        private bool _didBack;
        private int _offset;

        public Decoder(Stream stream)
        {
            // We assume the stream is buffered by itself.  Otherwise the caller should wrap it
            // with BufferedStream: stream = new BufferedStream(stream);
            _stream = stream;
            _lastRead = 0;
            _didBack = false;
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
            const byte e = 0x65;  // 'e'

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
                    BigInteger integer = ReadDigits(e, BigInteger.Parse);
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
                    Back();
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
                    Back();
                    return ReadBinary();

                case { } b:
                    throw new DecodingException(
                        $"Expected a dictionary key, but got an unexpected byte 0x{b:x} at " +
                        $"{_offset - 1}."
                    );
            }
        }

        private byte[] Read(byte[] buffer)
        {
            var length = buffer.Length;
            if (_didBack)
            {
                buffer[0] = _lastRead;
                length--;
            }

            int correction = _didBack ? 1 : 0;
            int read = _stream.Read(buffer, correction, length);
            if (read < length)
            {
                Array.Resize(ref buffer, read + correction);
            }

            _offset += read + correction;
            if (buffer.Length > 0)
            {
                _lastRead = buffer[buffer.Length - 1];
            }

            _didBack = false;
            return buffer;
        }

        private byte ReadByte()
        {
            if (_didBack)
            {
                _didBack = false;
                _offset++;
                return _lastRead;
            }

            int read = _stream.Read(_tinyBuffer, 0, 1);
            if (read > 0)
            {
                _lastRead = _tinyBuffer[0];
            }

            _offset++;
            _didBack = false;
            return read == 0
                ? throw new DecodingException($"The byte stream terminates unexpectedly at {_offset}.")
                : _tinyBuffer[0];
        }

        // Checks end of stream.  Should be called only once at the very end.
        private bool EndOfStream()
        {
            if (_didBack)
            {
                return false;
            }

            return _stream.Read(_tinyBuffer, 0, 1) == 0;
        }

        private void Back()
        {
            if (_offset < 1)
            {
                throw new DecodingException(
                    "Unexpected internal error: failed to rewind the stream buffer."
                );
            }

            _didBack = true;
            _offset--;
        }

        // Reads the length portion for byte strings and unicode strings.
        private int ReadLength()
        {
            const byte colon = 0x3a;    // ':'
            const int asciiZero = 0x30; // '0'
            int length = 0;

            byte lastByte = ReadByte();
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

        private byte[] ReadDigits(byte delimiter)
        {
            const int defaultBufferSize = 10;
            byte[] buffer = new byte[defaultBufferSize];

            var b = ReadByte();
            bool minus = false;
            if (b == 0x2d) // '-'
            {
                minus = true;
                b = ReadByte();
            }

            int digitsLength;

            if (minus)
            {
                buffer[0] = 0x2d;
                buffer[1] = b;
                digitsLength = 2;
            }
            else
            {
                buffer[0] = b;
                digitsLength = 1;
            }

            byte lastByte = b;

            while (lastByte != delimiter)
            {
#pragma warning disable SA1131
                if (lastByte < 0x30 || 0x39 < lastByte) // not '0'-'9'
                {
                    throw new DecodingException(
                        $"Expected a digit (0x30-0x39), but got 0x{lastByte:x} at {_offset}."
                    );
                }
#pragma warning restore SA1131

                lastByte = ReadByte();

                if (digitsLength >= buffer.Length)
                {
                    Array.Resize(ref buffer, buffer.Length * 2);
                }

                buffer[digitsLength] = lastByte;
                digitsLength++;
            }

            digitsLength--;
            Array.Resize(ref buffer, digitsLength);

            return buffer;
        }

        private T ReadDigits<T>(
            byte delimiter,
            Func<string, IFormatProvider, T> converter
        )
        {
            byte[] buffer = ReadDigits(delimiter);
            var digits = new char[buffer.Length];
            for (int i = 0; i < buffer.Length; i++)
            {
                digits[i] = (char)buffer[i];
            }

            return converter(new string(digits), CultureInfo.InvariantCulture);
        }

        private (byte[] ByteArray, int OffsetAfterColon) ReadByteArray()
        {
            int length = ReadLength();
            if (length < 1)
            {
                return (Array.Empty<byte>(), _offset);
            }

            int pos = _offset;
            byte[] buffer = new byte[length];
            byte[] bytes = Read(buffer);
            if (bytes.Length < length)
            {
                throw new DecodingException(
                    $"The byte stream terminates at {_offset} with insufficient " +
                    $"{length - bytes.Length} bytes."
                );
            }

            return (bytes, pos);
        }

        private Binary ReadBinary()
        {
            (byte[] bytes, _) = ReadByteArray();
            return new Binary(bytes);
        }

        private Text ReadTextAfterPrefix()
        {
            (byte[] bytes, int pos) = ReadByteArray();

            string textContent;
            try
            {
                textContent = Encoding.UTF8.GetString(bytes);
            }
            catch (ArgumentException e)
            {
                throw new DecodingException($"Expected a UTF-8 sequence at {pos}.", e);
            }

            return new Text(textContent);
        }
    }
}
