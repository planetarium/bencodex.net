﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Bencodex.Types;

namespace Bencodex
{
    internal static class Encoder
    {
        // TODO: Needs a unit test.
        public static byte[] Encode(IValue value)
        {
            long estimatedLength = EstimateLength(value);
            var buffer = new byte[estimatedLength];
            Encode(value, buffer, 0L);
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
                        output.WriteByte(0x6c);  // 'l'
                        foreach (IValue el in l)
                        {
                            Encode(el, output);
                        }

                        output.WriteByte(0x65);  // 'e'
                        break;

                    case Dictionary d:
                        output.WriteByte(0x6c);  // 'l'
                        foreach (KeyValuePair<IKey, IValue> pair in d)
                        {
                            Encode(pair.Key, output);
                            Encode(pair.Value, output);
                        }

                        output.WriteByte(0x65);  // 'e'
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

        internal static long EncodeNull(byte[] buffer, long offset)
        {
            buffer[offset] = 0x6e;  // 'n'
            return 1L;
        }

        internal static long EncodeBoolean(in Types.Boolean value, byte[] buffer, long offset)
        {
            buffer[offset] = value.Value
                ? (byte)0x74 // 't'
                : (byte)0x66; // 'f'
            return 1L;
        }

        internal static long EncodeInteger(in Integer value, byte[] buffer, long offset)
        {
            buffer[offset] = 0x69;  // 'i'
            offset++;
            string digits = value.Value.ToString(CultureInfo.InvariantCulture);
            if (offset + digits.Length <= int.MaxValue)
            {
                Encoding.ASCII.GetBytes(digits, 0, digits.Length, buffer, (int)offset);
            }
            else
            {
                byte[] digitBytes = Encoding.ASCII.GetBytes(digits);
                Array.Copy(digitBytes, 0L, buffer, offset, digitBytes.LongLength);
            }

            offset += digits.Length;
            buffer[offset] = 0x65;  // 'e'
            return 1L + digits.Length + 1L;
        }

        internal static long EncodeBinary(in Binary value, byte[] buffer, long offset)
        {
            long len = value.ByteArray.Length;
            long lenStrLength = EncodeDigits(len, buffer, offset);
            offset += lenStrLength;
            buffer[offset] = 0x3a;  // ':'
            offset++;

            if (offset + len <= int.MaxValue)
            {
                value.ByteArray.CopyTo(buffer, (int)offset);
                return lenStrLength + 1L + len;
            }

            byte[] b = value.ToByteArray();
            Array.Copy(b, 0L, buffer, offset, b.LongLength);
            return lenStrLength + 1L + b.LongLength;
        }

        internal static long EncodeText(in Text value, byte[] buffer, long offset)
        {
            buffer[offset++] = 0x75;  // 'u'
            int utf8Length = value.Utf8Length;
            long lenStrLength = EncodeDigits(utf8Length, buffer, offset);
            offset += lenStrLength;
            buffer[offset] = 0x3a;  // ':'
            offset++;
            string str = value.Value;

            if (offset + str.Length <= int.MaxValue)
            {
                Encoding.UTF8.GetBytes(str, 0, str.Length, buffer, (int)offset);
                return 1L + lenStrLength + 1L + utf8Length;
            }

            byte[] utf8 = Encoding.UTF8.GetBytes(value.Value);
            Array.Copy(utf8, 0L, buffer, offset, utf8.LongLength);
            return 1L + lenStrLength + 1L + utf8.LongLength;
        }

        // TODO: Needs a unit test.
        internal static long EncodeList(
            in List value,
            byte[] buffer,
            long offset
        )
        {
            buffer[offset] = 0x6c;  // 'l'
            long encLen = 1L;  // This means the logical "expanded" encoding length.
            long actualBytes = 1L;  // This means the actual "collapsed" encoding length.
            foreach (IValue v in value)
            {
                actualBytes += Encode(v, buffer, offset + actualBytes);
                encLen += v.EncodingLength;
            }

            offset += actualBytes;
            buffer[offset] = 0x65;  // 'e'
            actualBytes++;
            encLen++;
            return actualBytes;
        }

        // TODO: Needs a unit test.
        internal static long EncodeDictionary(
            in Dictionary value,
            byte[] buffer,
            long offset
        )
        {
            buffer[offset] = 0x64;  // 'd'
            long encLen = 1L;  // This means the logical "expanded" encoding length.
            long actualBytes = 1L;  // This means the actual "collapsed" encoding length.
            foreach (KeyValuePair<IKey, IValue> pair in value)
            {
                actualBytes += pair.Key switch
                {
                    Text tk => EncodeText(tk, buffer, offset + actualBytes),
                    Binary bk => EncodeBinary(bk, buffer, offset + actualBytes),
                    { } k => throw new ArgumentException(
                        $"Unsupported type: {k.GetType()}", nameof(k)),
                };

                actualBytes += Encode(pair.Value, buffer, offset + actualBytes);

                encLen += pair.Key.EncodingLength + pair.Value.EncodingLength;
            }

            offset += actualBytes;
            buffer[offset] = 0x65;  // 'e'
            encLen++;
            actualBytes++;
            value.EncodingLength = encLen;
            return actualBytes;
        }

        internal static long CountDecimalDigits(long value)
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
            if (value < 10000000000L) return 10;
            if (value < 100000000000L) return 11;
            if (value < 1000000000000L) return 12;
            if (value < 10000000000000L) return 13;
            if (value < 100000000000000L) return 14;
            if (value < 1000000000000000L) return 15;
            if (value < 10000000000000000L) return 16;
            if (value < 100000000000000000L) return 17;
            if (value < 1000000000000000000L) return 18;
            return 19;
#pragma warning restore SA1503
        }

        internal static long EncodeDigits(long positiveInt, byte[] buffer, long offset)
        {
            const int asciiZero = 0x30; // '0'
            long length = CountDecimalDigits(positiveInt);
            for (long i = offset + length - 1; i >= offset; i--)
            {
                buffer[i] = (byte)(positiveInt % 10 + asciiZero);
                positiveInt /= 10;
            }

            return length;
        }

        // TODO: Needs a unit test.
        internal static long Encode(in IValue value, byte[] buffer, long offset)
        {
            return value switch
            {
                Null _ => EncodeNull(buffer, offset),
                Types.Boolean b => EncodeBoolean(b, buffer, offset),
                Integer i => EncodeInteger(i, buffer, offset),
                Binary bin => EncodeBinary(bin, buffer, offset),
                Text t => EncodeText(t, buffer, offset),
                List l => EncodeList(l, buffer, offset),
                Dictionary d => EncodeDictionary(d, buffer, offset),
                _ =>
                    throw new ArgumentException(
                        "Unsupported type: " + value.GetType().FullName, nameof(value)
                    ),
            };
        }
    }
}
