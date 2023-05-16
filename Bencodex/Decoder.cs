using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Bencodex.Misc;
using Bencodex.Types;

namespace Bencodex
{
    internal sealed class Decoder
    {
        private readonly Stream _stream;
        private readonly IndirectValue.Loader? _indirectValueLoader;
        private byte _lastRead;
        private bool _didBack;
        private int _offset;

        public Decoder(Stream stream, IndirectValue.Loader? indirectValueLoader)
        {
            // We assume the stream is buffered by itself.  Otherwise the caller should wrap it
            // with BufferedStream: stream = new BufferedStream(stream);
            _stream = stream;
            _indirectValueLoader = indirectValueLoader;
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
            const byte indir = 0x2a;  // '*'

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
                    var indirBuilder = ImmutableArray.CreateBuilder<IndirectValue>();
                    while (true)
                    {
                        byte b = ReadByte() ?? throw new DecodingException(
                            $"The byte stream terminates unexpectedly at {_offset}."
                        );
                        if (b == e)
                        {
                            break;
                        }
                        else if (b == indir)
                        {
                            Fingerprint fp = DecodeFingerprint();
                            indirBuilder.Add(new IndirectValue(fp));
                            continue;
                        }

                        Back();
                        IValue element = DecodeValue();
                        indirBuilder.Add(new IndirectValue(element));
                    }

                    return new Bencodex.Types.List(
                        indirBuilder.MoveToImmutable().AsSpan(),
                        _indirectValueLoader
                    );

                case 0x64: // 'd'
                    var pairs = new List<KeyValuePair<IKey, IndirectValue>>();
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
                        if (_indirectValueLoader is { })
                        {
                            b = ReadByte() ?? throw new DecodingException(
                                $"The byte stream terminates unexpectedly at {_offset}."
                            );
                            if (b == indir)
                            {
                                Fingerprint fp = DecodeFingerprint();
                                var indirValue = new IndirectValue(fp);
                                pairs.Add(new KeyValuePair<IKey, IndirectValue>(key, indirValue));
                                continue;
                            }

                            Back();
                        }

                        IValue value = DecodeValue();
                        pairs.Add(
                            new KeyValuePair<IKey, IndirectValue>(key, new IndirectValue(value))
                        );
                    }

                    return new Dictionary(
                        pairs.ToImmutableSortedDictionary(KeyComparer.Instance),
                        _indirectValueLoader
                    );

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

        private Fingerprint DecodeFingerprint()
        {
            if (_indirectValueLoader is null)
            {
                throw new DecodingException(
                    $"An unexpected byte 0x2a at {_offset - 1}.  Note that it means an indirect " +
                    $"value.  To load an indirect value, {nameof(IndirectValue.Loader)} is needed."
                );
            }

            (byte[] bytes, int offset) = ReadByteArray();
            Fingerprint fp;
            try
            {
                fp = Fingerprint.Deserialize(bytes);
            }
            catch (FormatException e)
            {
                throw new DecodingException(
                    "Expected a fingerprint, bug got an unexpected byte sequence at " +
                    $"{_offset - 1}: {e.Message}."
                );
            }

            return fp;
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

        private byte? ReadByte()
        {
            if (_didBack)
            {
                _didBack = false;
                _offset++;
                return _lastRead;
            }

            int read = _stream.ReadByte();
            if (read > 0)
            {
                _lastRead = (byte)read;
            }

            _offset++;
            _didBack = false;
            return read == -1 ? (byte?)null : _lastRead;
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
            const int defaultBufferSize = 10;
            byte[] buffer = new byte[defaultBufferSize];

            var b = ReadByte();

            if (b is null)
            {
                const string minusSignOr = "a minus sign or ";
                throw new DecodingException(
                    $"Expected {(takeMinusSign ? minusSignOr : string.Empty)}digits, " +
                    $"but the byte stream terminates at {_offset}."
                );
            }

            bool minus = false;
            if (takeMinusSign && b == 0x2d) // '-'
            {
                minus = true;
                b = ReadByte();

                if (b is null)
                {
                    throw new DecodingException(
                        $"Expected digits, but the byte stream terminates at {_offset}."
                    );
                }
            }

            int digitsLength;

            if (minus)
            {
                buffer[0] = 0x2d;
                buffer[1] = b.Value;
                digitsLength = 2;
            }
            else
            {
                buffer[0] = b.Value;
                digitsLength = 1;
            }

            byte lastByte = b.Value;

            while (lastByte != delimiter)
            {
#pragma warning disable SA1131
                if (!(0x30 <= lastByte && lastByte < 0x40)) // not '0'-'9'
                {
                    throw new DecodingException(
                        $"Expected a digit (0x30-0x40), but got 0x{lastByte:x} at {_offset}."
                    );
                }
#pragma warning restore SA1131

                lastByte = ReadByte() ?? throw new DecodingException(
                    $"Expected a delimiter byte 0x{delimiter:x}, but the byte stream terminates " +
                    $"at {_offset}."
                );

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
            bool takeMinusSign,
            byte delimiter,
            Func<string, IFormatProvider, T> converter
        )
        {
            byte[] buffer = ReadDigits(takeMinusSign, delimiter);
            var digits = Unsafe.As<byte[], char[]>(ref buffer);
            return converter(new string(digits), CultureInfo.InvariantCulture);
        }

        private (byte[] ByteArray, int OffsetAfterColon) ReadByteArray()
        {
            const byte colon = 0x3a;  // ':'
            byte[] digits = ReadDigits(false, colon);
            int length = Atoi(digits);
            if (length < 1)
            {
                return (new byte[0], _offset);
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
            return new Binary(bytes.AsSpan());
        }

        private Text ReadTextAfterPrefix()
        {
            (byte[] bytes, int pos) = ReadByteArray();

            string textContent;
            try
            {
#if NETSTANDARD2_0
                textContent = Encoding.UTF8.GetString(bytes);
#else
                textContent = Encoding.UTF8.GetString(bytes.AsSpan());
#endif
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
