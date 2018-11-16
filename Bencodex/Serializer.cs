using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Bencodex
{
    public class Serializer
    {
        public void Serialize(object value, Stream stream)
        {
            switch (value)
            {
                case null:
                    stream.WriteByte(0x6e); // 'n'
                    break;

                case true:
                    stream.WriteByte(0x74); // 't'
                    break;

                case false:
                    stream.WriteByte(0x66); // 'f'
                    break;

                case short v:
                    WriteInteger(v, stream);
                    break;

                case ushort v:
                    WriteInteger(v, stream);
                    break;

                case int v:
                    WriteInteger(v, stream);
                    break;

                case uint v:
                    WriteInteger(v, stream);
                    break;

                case long v:
                    WriteInteger(v, stream);
                    break;

                case ulong v:
                    WriteInteger(v, stream);
                    break;

                case BigInteger v:
                    WriteInteger(v, stream);
                    break;

                case string text:
                    WriteText(text, stream);
                    break;

                case byte[] bytes:
                    WriteBytes(bytes, stream);
                    break;

                case IList list:
                    WriteList(list, stream);
                    break;

                case ICollection l when IsGenericCollection(l, typeof(IList<>)):
                    WriteList(l, stream);
                    break;

                case IDictionary dict:
                    WriteDictionary(dict.Cast<DictionaryEntry>(), stream);
                    break;

                case ICollection d when IsGenericCollection(
                    d,
                    typeof(IDictionary<,>)):
                    WriteDictionary(
                        ToDictionaryEntries(d),
                        stream
                    );
                    break;

                default:
                    throw new ArgumentException(
                        string.Format("{0} is of an unsupported type", value),
                        nameof(value)
                    );
            }
        }

        private void WriteInteger(object v, Stream stream)
        {
            stream.WriteByte(0x69); // 'i'
            var digits = Encoding.ASCII.GetBytes(v.ToString());
            stream.Write(digits, 0, digits.Length);
            stream.WriteByte(0x65); // 'e'
        }

        private void WriteBytes(byte[] bytes, Stream stream)
        {
            var lengthDigits = Encoding.ASCII.GetBytes(
                bytes.Length.ToString()
            );
            stream.Write(lengthDigits, 0, lengthDigits.Length);
            stream.WriteByte(0x3a); // ':'
            stream.Write(bytes, 0, bytes.Length);
        }

        private void WriteText(string text, Stream stream)
        {
            WriteRawText(Encoding.UTF8.GetBytes(text), stream);
        }

        private void WriteRawText(byte[] rawText, Stream stream)
        {
            stream.WriteByte(0x75); // 'u'
            WriteBytes(rawText, stream);
        }

        private bool IsGenericCollection(IEnumerable list, Type gType)
        {
            foreach (var i in list.GetType().GetInterfaces())
            {
                if (i.IsGenericType)
                {
                    return i.GetGenericTypeDefinition() == gType;
                }
            }

            return false;
        }

        private void WriteList(ICollection list, Stream stream)
        {
            stream.WriteByte(0x6c); // 'l'
            foreach (var element in list)
            {
                Serialize(element, stream);
            }

            stream.WriteByte(0x65); // 'e'
        }

        private IEnumerable<DictionaryEntry> ToDictionaryEntries(ICollection d)
        {
            foreach (dynamic pair in d)
            {
                yield return new DictionaryEntry(pair.Key, pair.Value);
            }
        }

        private void WriteDictionary(
            IEnumerable<DictionaryEntry> pairs,
            Stream stream
        )
        {
            stream.WriteByte(0x64); // 'd'
            IEnumerable<((bool, byte[]), object)> rawPairs =
                from pair in pairs
                select (ToBytesKey(pair.Key), pair.Value);
            IOrderedEnumerable<((bool, byte[]), object)> orderedPairs = rawPairs
                .OrderBy(pair => pair.Item1, new BytesKeySorter());
            foreach (((var kWasUnicode, var k), var v) in orderedPairs)
            {
                if (kWasUnicode)
                {
                    WriteRawText(k, stream);
                }
                else
                {
                    WriteBytes(k, stream);
                }
                Serialize(v, stream);
            }

            stream.WriteByte(0x65); // 'e'
        }

        private (bool, byte[]) ToBytesKey(object key)
        {
            switch (key)
            {
                case null:
                    throw new ArgumentException(
                        "A dictionary key must not be null"
                    );

                case string textKey:
                    return (true, Encoding.UTF8.GetBytes(textKey));

                case byte[] bytesKey:
                    return (false, bytesKey);

                default:
                    throw new ArgumentException(
                        string.Format(
                            "A dictionary key is of an unsupported type:" +
                            " {0}; every key has to be either Unicode " +
                            "text or byte array",
                            key.GetType()
                        )
                    );
            }
        }
    }

    internal class BytesKeySorter : IComparer<(bool, byte[])>
    {
        public int Compare((bool, byte[]) xPair, (bool, byte[]) yPair)
        {
            bool xWasUnicode, yWasUnicode;
            byte[] x, y;
            (xWasUnicode, x) = xPair;
            (yWasUnicode, y) = yPair;
            int typeCompared = xWasUnicode.CompareTo(yWasUnicode);
            if (typeCompared != 0)
            {
                return typeCompared;
            }

            IEnumerable<int> cmpResults = x.Zip(y, (a, b) => a.CompareTo(b));
            try
            {
                return cmpResults.First(cmpResult => cmpResult != 0);
            }
            catch (InvalidOperationException)
            {
                return x.Length.CompareTo(y.Length);
            }
        }
    }
}
