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
        // TODO: Needs a unit test.
        public static byte[] Encode(IValue value, IOffloadOptions? offloadOptions)
        {
            long estimatedLength = EstimateLength(value, offloadOptions);
            var buffer = new byte[estimatedLength];
            Encode(value, offloadOptions, buffer, 0L);
            return buffer;
        }

        // TODO: Needs a unit test.
        public static void Encode(IValue value, Stream output, IOffloadOptions? offloadOptions)
        {
            if (!output.CanWrite)
            {
                throw new ArgumentException(
                    "stream cannot be written to",
                    nameof(output)
                );
            }

            long estimatedLength = EstimateLength(value, offloadOptions);
            if (estimatedLength > 4096L)
            {
                switch (value)
                {
                    case List l:
                        output.WriteByte(0x6c);  // 'l'
                        foreach (IValue el in l)
                        {
                            Encode(el, output, offloadOptions);
                        }

                        output.WriteByte(0x65);  // 'e'
                        break;

                    case Dictionary d:
                        output.WriteByte(0x6c);  // 'l'
                        foreach (KeyValuePair<IKey, IValue> pair in d)
                        {
                            Encode(pair.Key, output, offloadOptions);
                            Encode(pair.Value, output, offloadOptions);
                        }

                        output.WriteByte(0x65);  // 'e'
                        break;
                }

                return;
            }

            byte[] buffer = Encode(value, offloadOptions);
            output.Write(buffer, 0, buffer.Length);
        }

        internal static long EstimateLength(IValue value, IOffloadOptions? offloadOptions)
        {
            if (!(offloadOptions is { } oo))
            {
                return value.EncodingLength;
            }
            else if (value is List list)
            {
                long listLen = 2L;
                foreach (IndirectValue iv in list.EnumerateIndirectValues())
                {
                    if (oo.Embeds(iv))
                    {
                        listLen += EstimateLength(iv.GetValue(list.Loader), oo);
                    }
                    else
                    {
                        long fpLen = iv.Fingerprint.CountSerializationBytes();
                        listLen += 2L + CountDecimalDigits(fpLen) + fpLen;
                    }
                }

                return listLen;
            }
            else if (value is Dictionary dict)
            {
                long dictLen = 2L;
                foreach (KeyValuePair<IKey, IndirectValue> pair in dict.EnumerateIndirectPairs())
                {
                    dictLen += pair.Key.EncodingLength;
                    IndirectValue iv = pair.Value;
                    if (oo.Embeds(iv))
                    {
                        dictLen += EstimateLength(iv.GetValue(dict.Loader), oo);
                    }
                    else
                    {
                        long fpLen = iv.Fingerprint.CountSerializationBytes();
                        dictLen += 2L + CountDecimalDigits(fpLen) + fpLen;
                    }
                }

                return dictLen;
            }

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
            IOffloadOptions? offloadOptions,
            byte[] buffer,
            long offset
        )
        {
            buffer[offset] = 0x6c;  // 'l'
            long encLen = 1L;  // This means the logical "expanded" encoding length.
            long actualBytes = 1L;  // This means the actual "collapsed" encoding length.
            foreach (IndirectValue el in value.EnumerateIndirectValues())
            {
                if (offloadOptions is { } oo && !oo.Embeds(el))
                {
                    actualBytes += EncodeFingerprint(el.Fingerprint, buffer, offset + actualBytes);
                    oo.Offload(el, value.Loader);
                }
                else
                {
                    actualBytes += Encode(
                        el.GetValue(value.Loader),
                        offloadOptions,
                        buffer,
                        offset + actualBytes
                    );
                }

                encLen += el.EncodingLength;
            }

            offset += actualBytes;
            buffer[offset] = 0x65;  // 'e'
            actualBytes++;
            encLen++;
            value.EncodingLength = encLen;
            return actualBytes;
        }

        // TODO: Needs a unit test.
        internal static long EncodeDictionary(
            in Dictionary value,
            IOffloadOptions? offloadOptions,
            byte[] buffer,
            long offset
        )
        {
            buffer[offset] = 0x64;  // 'd'
            long encLen = 1L;  // This means the logical "expanded" encoding length.
            long actualBytes = 1L;  // This means the actual "collapsed" encoding length.
            foreach (KeyValuePair<IKey, IndirectValue> pair in value.EnumerateIndirectPairs())
            {
                actualBytes += pair.Key switch
                {
                    Text tk => EncodeText(tk, buffer, offset + actualBytes),
                    Binary bk => EncodeBinary(bk, buffer, offset + actualBytes),
                    { } k => Encode(k, offloadOptions, buffer, offset + actualBytes),
                };
                if (offloadOptions is { } oo && !oo.Embeds(pair.Value))
                {
                    actualBytes += EncodeFingerprint(pair.Value.Fingerprint, buffer, offset + actualBytes);
                    oo.Offload(pair.Value, value.Loader);
                }
                else
                {
                    actualBytes += Encode(
                        pair.Value.GetValue(value.Loader),
                        offloadOptions,
                        buffer,
                        offset + actualBytes
                    );
                }

                encLen += pair.Key.EncodingLength + pair.Value.EncodingLength;
            }

            offset += actualBytes;
            buffer[offset] = 0x65;  // 'e'
            encLen++;
            actualBytes++;
            value.EncodingLength = encLen;
            return actualBytes;
        }

        // TODO: Needs a unit test.
        internal static long EncodeFingerprint(
            in Fingerprint fingerprint,
            byte[] buffer,
            long offset
        )
        {
            buffer[offset] = 0x2a;  // '*'
            offset++;
            long len = fingerprint.CountSerializationBytes();
            long lenStrLength = EncodeDigits(len, buffer, offset);
            offset += lenStrLength;
            buffer[offset] = 0x3a;  // ':'
            offset++;
            return 2L + lenStrLength + fingerprint.SerializeInto(buffer, offset);
        }

        internal static long CountDecimalDigits(long value) => value < 10L
            ? 1L
            : value < 100L
                ? 2L
                : value < 1000L
                    ? 3L
                    : value < 10000L
                        ? 4L
                        : (long)Math.Floor(Math.Log10(value)) + 1L;

        internal static long EncodeDigits(long positiveInt, byte[] buffer, long offset)
        {
            string digits = positiveInt.ToString(CultureInfo.InvariantCulture);
            if (offset < int.MaxValue)
            {
                return Encoding.ASCII.GetBytes(digits, 0, digits.Length, buffer, (int)offset);
            }

            byte[] ascii = Encoding.ASCII.GetBytes(digits);
            Array.Copy(ascii, 0L, buffer, offset, ascii.LongLength);
            return ascii.Length;
        }

        // TODO: Needs a unit test.
        internal static long Encode(
            in IValue value,
            IOffloadOptions? offloadOptions,
            byte[] buffer,
            long offset
        )
        {
            return value switch
            {
                Null _ => EncodeNull(buffer, offset),
                Types.Boolean b => EncodeBoolean(b, buffer, offset),
                Integer i => EncodeInteger(i, buffer, offset),
                Binary bin => EncodeBinary(bin, buffer, offset),
                Text t => EncodeText(t, buffer, offset),
                List l => EncodeList(l, offloadOptions, buffer, offset),
                Dictionary d => EncodeDictionary(d, offloadOptions, buffer, offset),
                _ =>
                    throw new ArgumentException(
                        "Unsupported type: " + value.GetType().FullName, nameof(value)
                    ),
            };
        }
    }
}
