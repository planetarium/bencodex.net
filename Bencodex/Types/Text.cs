using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Bencodex.Types
{
    public struct Text :
        IKey,
        IEquatable<Text>,
        IComparable<string>,
        IComparable<Text>,
        IEquatable<string>,
        IComparable
    {
        private string _value;

        public Text(string value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Value => _value ?? (_value = string.Empty);

        [Pure]
        byte? IKey.KeyPrefix => 0x75;  // 'u'

        public static implicit operator string(Text t)
        {
            return t.Value;
        }

        public static implicit operator Text(string s)
        {
            return new Text(s);
        }

        public static bool operator ==(Text left, Text right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Text left, Text right)
        {
            return !left.Equals(right);
        }

        bool IEquatable<string>.Equals(string other)
        {
            return other != null && Value.Equals(other);
        }

        bool IEquatable<Text>.Equals(Text other) => Value.Equals(other);

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null:
                    return false;
                case Text txt:
                    return ((IEquatable<Text>)this).Equals(txt);
                case string str:
                    return ((IEquatable<string>)this).Equals(str);
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        int IComparable<string>.CompareTo(string other)
        {
            return string.Compare(Value, other, StringComparison.Ordinal);
        }

        int IComparable<Text>.CompareTo(Text other)
        {
            return string.Compare(Value, other.Value, StringComparison.Ordinal);
        }

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;
                case Text txt:
                    return ((IComparable<Text>)this).CompareTo(txt);
                case string str:
                    return ((IComparable<string>)this).CompareTo(str);
                default:
                    throw new ArgumentException(
                        "the argument is neither Text nor String",
                        nameof(obj)
                    );
            }
        }

        [Pure]
        byte[] IKey.EncodeAsByteArray() => Encoding.UTF8.GetBytes(Value);

        [Pure]
        public IEnumerable<byte[]> EncodeIntoChunks()
        {
            yield return new byte[1]
            {
                (byte)((IKey)this).KeyPrefix,
            };
            byte[] utf8 = ((IKey)this).EncodeAsByteArray();
            foreach (byte[] chunk in ((IValue)new Binary(utf8)).EncodeIntoChunks())
            {
                yield return chunk;
            }
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
