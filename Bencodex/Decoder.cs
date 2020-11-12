using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using Bencodex.Types;

namespace Bencodex
{
    internal sealed class Decoder
    {
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
            IValue value = DecodeValue();
            if (ReadByte() is { } b)
            {
                throw new DecodingException(
                    $"An unexpected trailing byte 0x{b:x} at {_offset - 1}."
                );
            }

            return value;
        }

        private IValue DecodeValue()
        {
            const byte e = 0x65;  // 'e'

            switch (ReadByte())
            {
                case null:
                    throw new DecodingException(
                        $"The byte stream terminates unexpectedly at {_offset}."
                    );

                case 0x6e: // 'n'
#pragma warning disable SA1129
                    return new Null();
#pragma warning restore SA1129

                case 0x74: // 't'
                    return new Bencodex.Types.Boolean(true);

                case 0x66: // 'f'
                    return new Bencodex.Types.Boolean(false);

                case 0x69: // 'i'
                    BigInteger integer = ReadDigits(true, e, BigInteger.Parse);
                    return new Integer(integer);

                case 0x75: // 'u'
                    return ReadTextAfterPrefix();

                case 0x6c: // 'l'
                    var elements = new List<IValue>();
                    while (true)
                    {
                        byte b = ReadByte() ?? throw new DecodingException(
                            $"The byte stream terminates unexpectedly at {_offset}."
                        );
                        if (b == e)
                        {
                            break;
                        }

                        Back();
                        IValue element = DecodeValue();
                        elements.Add(element);
                    }

                    return new Bencodex.Types.List(elements);

                case 0x64: // 'd'
                    var pairs = new List<KeyValuePair<IKey, IValue>>();
                    while (true)
                    {
                        byte b = ReadByte() ?? throw new DecodingException(
                            $"The byte stream terminates unexpectedly at {_offset}."
                        );
                        if (b == e)
                        {
                            break;
                        }

                        Back();
                        IKey key = DecodeKey();
                        IValue value = DecodeValue();
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
                    Back();
                    return ReadBinary();

                case { } b:
                    throw new DecodingException($"An unexpected byte 0x{b:x} at {_offset - 1}.");
            }
        }

        private IKey DecodeKey()
        {
            switch (ReadByte())
            {
                case null:
                    throw new DecodingException(
                        $"Expected a dictionary key, but the byte stream terminates at {_offset}."
                    );

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

        private byte[] Read(int length)
        {
            byte[] buffer = new byte[length];
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

        private byte? ReadByte()
        {
            if (_didBack)
            {
                _didBack = false;
                _offset++;
                return _lastRead;
            }

            byte[] buffer = Read(1);
            return buffer.Length > 0 ? buffer[0] : (byte?)null;
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

        private byte[] ReadDigits(bool takeMinusSign, byte delimiter)
        {
            byte[] buffer = Read(1);

            if (buffer.Length < 1)
            {
                const string minusSignOr = "a minus sign or ";
                throw new DecodingException(
                    $"Expected {(takeMinusSign ? minusSignOr : string.Empty)}digits, " +
                    $"but the byte stream terminates at {_offset}."
                );
            }

            bool minus = false;
            if (takeMinusSign && buffer[0] == 0x2d) // '-'
            {
                minus = true;
                buffer = Read(1);
            }

            byte lastByte = buffer[0];
#pragma warning disable SA1131
            while (lastByte != delimiter)
            {
                if (!(0x30 <= lastByte && lastByte < 0x40)) // not '0'-'9'
                {
                    throw new DecodingException(
                        $"Expected a digit (0x30-0x40), but got 0x{lastByte:x} at {_offset}."
                    );
                }

                lastByte = ReadByte() ?? throw new DecodingException(
                    $"Expected a delimiter byte 0x{delimiter:x}, but the byte stream terminates " +
                    $"at {_offset}."
                );
                Array.Resize(ref buffer, buffer.Length + 1);
                buffer[buffer.Length - 1] = lastByte;
            }
#pragma warning restore SA1131

            if (minus)
            {
                for (int i = buffer.Length - 1; i > 0; i--)
                {
                    buffer[i] = buffer[i - 1];
                }

                buffer[0] = 0x2d; // '-'
            }
            else
            {
                Array.Resize(ref buffer, buffer.Length - 1);
            }

            return buffer;
        }

        private T ReadDigits<T>(bool takeMinusSign, byte delimiter, Func<byte[], T> converter)
        {
            byte[] digits = ReadDigits(takeMinusSign, delimiter);
            return converter(digits);
        }

        private T ReadDigits<T>(
            bool takeMinusSign,
            byte delimiter,
            Func<string, IFormatProvider, T> converter
        )
        {
            byte[] buffer = ReadDigits(takeMinusSign, delimiter);
            var digits = new char[buffer.Length];
            for (int i = 0; i < buffer.Length; i++)
            {
                digits[i] = (char)buffer[i];
            }

            return converter(new string(digits), CultureInfo.InvariantCulture);
        }

        private (byte[] byteArray, int offsetAfterColon) ReadByteArray()
        {
            const byte colon = 0x3a;  // ':'
            int length = ReadDigits(false, colon, Atoi);
            if (length < 1)
            {
                return (new byte[0], _offset);
            }

            int pos = _offset;
            byte[] bytes = Read(length);
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

        private int Atoi(byte[] b)
        {
            int result = 0;
            int offset = 0;
            int sign = 1;

            if (b[offset] == 0x2d) // '-'
            {
                sign = -1;
                offset++;
            }

            const int asciiZero = 0x30;  // '0'
            for (; offset < b.Length; offset++)
            {
                int digit = b[offset] - asciiZero;
                result = result * 10 + digit;
            }

            return sign * result;
        }
    }
}
