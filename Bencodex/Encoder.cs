using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Bencodex.Types;

namespace Bencodex
{
    internal static class Encoder
    {
        private const byte _n = 0x6e;
        private const byte _t = 0x74;
        private const byte _f = 0x66;
        private const byte _i = 0x69;
        private const byte _c = 0x3a;   // `:`
        private const byte _e = 0x65;
        private const byte _u = 0x75;
        private const byte _l = 0x6c;
        private const byte _d = 0x64;

        // TODO: Needs a unit test.
        public static byte[] Encode(IValue value)
        {
            long estimatedLength = EstimateLength(value);
            var buffer = new byte[estimatedLength];
            int offset = 0;
            Encode(value, buffer, ref offset);
            return buffer;
        }

        // TODO: Needs a unit test.
        public static void Encode(IValue value, Stream output)
        {
            if (!output.CanWrite)
            {
                throw new ArgumentException(
                    "stream cannot be written to",
                    nameof(output)
                );
            }

            long estimatedLength = EstimateLength(value);
            if (estimatedLength > 4096L)
            {
                switch (value)
                {
                    case List l:
                        output.WriteByte(_l);
                        foreach (IValue el in l)
                        {
                            Encode(el, output);
                        }

                        output.WriteByte(_e);
                        break;

                    case Dictionary d:
                        output.WriteByte(_d);
                        foreach (KeyValuePair<IKey, IValue> pair in d)
                        {
                            Encode(pair.Key, output);
                            Encode(pair.Value, output);
                        }

                        output.WriteByte(_e);
                        break;
                }

                return;
            }

            byte[] buffer = Encode(value);
            output.Write(buffer, 0, buffer.Length);
        }

        internal static long EstimateLength(IValue value)
        {
            return value.EncodingLength;
        }

        internal static void EncodeNull(byte[] buffer, ref int offset)
        {
            buffer[offset++] = _n;
        }

        internal static void EncodeBoolean(in Types.Boolean value, byte[] buffer, ref int offset)
        {
            buffer[offset++] = value.Value ? _t : _f;
        }

        internal static void EncodeInteger(in Integer value, byte[] buffer, ref int offset)
        {
            buffer[offset++] = _i;
            string digits = value.Value.ToString(CultureInfo.InvariantCulture);
            Encoding.ASCII.GetBytes(digits, 0, digits.Length, buffer, offset);
            offset += digits.Length;
            buffer[offset++] = _e;
        }

        internal static void EncodeBinary(in Binary value, byte[] buffer, ref int offset)
        {
            int len = value.ByteArray.Length;
            EncodeDigits(len, buffer, ref offset);
            buffer[offset++] = _c;
            value.ByteArray.CopyTo(buffer, offset);
            offset += len;
            return;
        }

        internal static void EncodeText(in Text value, byte[] buffer, ref int offset)
        {
            buffer[offset++] = _u;
            int utf8Length = value.Utf8Length;
            EncodeDigits(utf8Length, buffer, ref offset);
            buffer[offset++] = _c;

            string str = value.Value;
            Encoding.UTF8.GetBytes(str, 0, str.Length, buffer, offset);
            offset += utf8Length;
            return;
        }

        // TODO: Needs a unit test.
        internal static void EncodeList(in List value, byte[] buffer, ref int offset)
        {
            buffer[offset++] = _l;
            foreach (IValue v in value)
            {
                Encode(v, buffer, ref offset);
            }

            buffer[offset++] = _e;
            return;
        }

        // TODO: Needs a unit test.
        internal static void EncodeDictionary(in Dictionary value, byte[] buffer, ref int offset)
        {
            buffer[offset++] = _d;

            foreach (KeyValuePair<IKey, IValue> pair in value)
            {
                switch (pair.Key)
                {
                    case Binary binary:
                        EncodeBinary(binary, buffer, ref offset);
                        break;
                    case Text text:
                        EncodeText(text, buffer, ref offset);
                        break;
                    default:
                        throw new ArgumentException(
                            $"Unsupported type: {pair.Key.GetType()}", nameof(pair.Key));
                }

                Encode(pair.Value, buffer, ref offset);
            }

            buffer[offset++] = _e;
            return;
        }

        internal static int CountDecimalDigits(int value)
        {
#pragma warning disable SA1503 // Braces should not be omitted
            if (value < 10L) return 1;
            if (value < 100L) return 2;
            if (value < 1000L) return 3;
            if (value < 10000L) return 4;
            if (value < 100000L) return 5;
            if (value < 1000000L) return 6;
            if (value < 10000000L) return 7;
            if (value < 100000000L) return 8;
            if (value < 1000000000L) return 9;
            return 10;
#pragma warning restore SA1503
        }

        internal static void EncodeDigits(int nonNegativeInt, byte[] buffer, ref int offset)
        {
            const int asciiZero = 0x30; // '0'
            int length = CountDecimalDigits(nonNegativeInt);
            for (int i = offset + length - 1; i >= offset; i--)
            {
                buffer[i] = (byte)(nonNegativeInt % 10 + asciiZero);
                nonNegativeInt /= 10;
            }

            offset += length;
        }

        // TODO: Needs a unit test.
        internal static void Encode(in IValue value, byte[] buffer, ref int offset)
        {
            switch (value)
            {
                case Null _:
                    EncodeNull(buffer, ref offset);
                    break;
                case Types.Boolean boolean:
                    EncodeBoolean(boolean, buffer, ref offset);
                    break;
                case Integer integer:
                    EncodeInteger(integer, buffer, ref offset);
                    break;
                case Binary binary:
                    EncodeBinary(binary, buffer, ref offset);
                    break;
                case Text text:
                    EncodeText(text, buffer, ref offset);
                    break;
                case List list:
                    EncodeList(list, buffer, ref offset);
                    break;
                case Dictionary dictionary:
                    EncodeDictionary(dictionary, buffer, ref offset);
                    break;
                default:
                    throw new ArgumentException(
                        $"Unsupported type: {value.GetType()}", nameof(value));
            }
        }
    }
}
