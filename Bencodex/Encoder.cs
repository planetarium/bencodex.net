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
        public static byte[] Encode(IValue value)
        {
            var buffer = new byte[value.EncodingLength];
            Encode(value, buffer, 0L);
            return buffer;
        }

        public static void Encode(IValue value, Stream output)
        {
            if (!output.CanWrite)
            {
                throw new ArgumentException(
                    "stream cannot be written to",
                    nameof(output)
                );
            }

            if (value.EncodingLength > 4096L)
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

        private static long EncodeNull(in Null value, byte[] buffer, long offset)
        {
            buffer[offset] = 0x6e;  // 'n'
            return 1L;
        }

        private static long EncodeBoolean(in Types.Boolean value, byte[] buffer, long offset)
        {
            buffer[offset] = value.Value
                ? (byte)0x74 // 't'
                : (byte)0x66; // 'f'
            return 1L;
        }

        private static long EncodeInteger(in Integer value, byte[] buffer, long offset)
        {
            buffer[offset] = 0x69;  // 'i'
            offset++;
            string digits = value.Value.ToString(CultureInfo.InvariantCulture);
            if (buffer.LongLength < offset + digits.Length + 1L)
            {
                throw new IndexOutOfRangeException("The " + nameof(buffer) + " is not enough.");
            }

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

        private static long EncodeBinary(in Binary value, byte[] buffer, long offset)
        {
            int len = value.ByteArray.Length;
            string lenStr = len.ToString(CultureInfo.InvariantCulture);
            long lenStrLength = lenStr.Length;
            if (offset + lenStrLength <= int.MaxValue)
            {
                lenStrLength = Encoding.ASCII.GetBytes(lenStr, 0, lenStr.Length, buffer, (int)offset);
            }
            else
            {
                byte[] lenStrBytes = Encoding.ASCII.GetBytes(lenStr);
                lenStrLength = lenStrBytes.LongLength;
                Array.Copy(lenStrBytes, 0L, buffer, offset, lenStrLength);
            }

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

        private static long EncodeText(in Text value, byte[] buffer, long offset)
        {
            buffer[offset++] = 0x75;  // 'u'
            int utf8Length = value.Utf8Length;
            string lenStr = utf8Length.ToString(CultureInfo.InvariantCulture);
            long lenStrLength = lenStr.Length;
            if (offset + lenStr.Length <= int.MaxValue)
            {
                lenStrLength = Encoding.ASCII.GetBytes(lenStr, 0, lenStr.Length, buffer, (int)offset);
            }
            else
            {
                byte[] lenStrBytes = Encoding.ASCII.GetBytes(lenStr);
                lenStrLength = lenStrBytes.LongLength;
                Array.Copy(lenStrBytes, 0L, buffer, offset, lenStrLength);
            }

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

        private static long EncodeList(in List value, byte[] buffer, long offset)
        {
            buffer[offset] = 0x6c;  // 'l'
            long encLen = 1L;
            foreach (IValue el in value)
            {
                encLen += Encode(el, buffer, offset + encLen);
            }

            offset += encLen;
            buffer[offset] = 0x65;  // 'e'
            encLen++;
            value.EncodingLength = encLen;
            return encLen;
        }

        private static long EncodeDictionary(in Dictionary value, byte[] buffer, long offset)
        {
            buffer[offset] = 0x64;  // 'd'
            long encLen = 1L;
            foreach (KeyValuePair<IKey, IValue> pair in value)
            {
                encLen += pair.Key switch
                {
                    Text tk => EncodeText(tk, buffer, offset + encLen),
                    Binary bk => EncodeBinary(bk, buffer, offset + encLen),
                    { } k => Encode(k, buffer, offset + encLen),
                };
                encLen += Encode(pair.Value, buffer, offset + encLen);
            }

            offset += encLen;
            buffer[offset] = 0x65;  // 'e'
            encLen++;
            value.EncodingLength = encLen;
            return encLen;
        }

        private static long Encode(in IValue value, byte[] buffer, long offset)
        {
            return value switch
            {
                Null n => EncodeNull(n, buffer, offset),
                Types.Boolean b => EncodeBoolean(b, buffer, offset),
                Integer i => EncodeInteger(i, buffer, offset),
                Binary bin => EncodeBinary(bin, buffer, offset),
                Text t => EncodeText(t, buffer, offset),
                List l => EncodeList(l, buffer, offset),
                Dictionary d => EncodeDictionary(d, buffer, offset),
                _ => throw new ArgumentException("Unsupported type: " + value.GetType().FullName, nameof(value)),
            };
        }
    }
}
