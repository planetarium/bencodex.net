using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Bencodex.Misc;

namespace Bencodex.Types
{
    public readonly struct Binary :
        IKey,
        IEquatable<ImmutableArray<byte>>,
        IEquatable<byte[]>,
        IEquatable<Binary>,
        IComparable<ImmutableArray<byte>>,
        IComparable<byte[]>,
        IComparable<Binary>,
        IComparable,
        IEnumerable<byte>
    {
        private static readonly ByteArrayComparer ByteArrayComparer =
            default(ByteArrayComparer);

        private readonly ImmutableArray<byte> _value;
        private readonly int?[] _hashCode;

        public Binary(ImmutableArray<byte> value)
        {
            _value = value;
            _hashCode = new int?[1];
        }

        public Binary(byte[] value)
            : this(value is byte[] bytes
                ? ImmutableArray.Create<byte>(bytes)
                : throw new ArgumentNullException(nameof(value)))
        {
        }

        public Binary(string text, Encoding encoding)
            : this(encoding.GetBytes(text))
        {
        }

        [Pure]
        byte? IKey.KeyPrefix => null;

        public ImmutableArray<byte> ByteArray =>
            _value.IsDefaultOrEmpty ? ImmutableArray<byte>.Empty : _value;

        [Obsolete(
            "The Binary.Value property is obsolete; use Binary.ToByteArray() " +
            "method or Binary.ByteArray property instead.")]
        public byte[] Value => ToByteArray();

        [Pure]
        public string Inspection
        {
            get
            {
                IEnumerable<string> contents = this.Select(b => $"\\x{b:x2}");
                return $"b\"{string.Join(string.Empty, contents)}\"";
            }
        }

        public static implicit operator Binary(ImmutableArray<byte> bytes) =>
            new Binary(bytes);

        public static implicit operator ImmutableArray<byte>(Binary binary) =>
            binary.ByteArray;

        public static implicit operator Binary(byte[] bytes)
        {
            return new Binary(bytes);
        }

        public static implicit operator byte[](Binary binary) =>
            binary.ToByteArray();

        public static bool operator ==(Binary left, Binary right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Binary left, Binary right)
        {
            return !left.Equals(right);
        }

        bool IEquatable<ImmutableArray<byte>>.Equals(ImmutableArray<byte> other) =>
            ByteArray.SequenceEqual(other);

        bool IEquatable<byte[]>.Equals(byte[] other) =>
            ByteArray.SequenceEqual(other);

        bool IEquatable<Binary>.Equals(Binary other) =>
            ((IEquatable<ImmutableArray<byte>>)this).Equals(other.ByteArray);

        public override bool Equals(object obj) =>
            obj switch
            {
                Binary b =>
                    ((IEquatable<Binary>)this).Equals(b),
                ImmutableArray<byte> b =>
                    ((IEquatable<ImmutableArray<byte>>)this).Equals(b),
                byte[] b =>
                    ((IEquatable<byte[]>)this).Equals(b),
                _ =>
                    false,
            };

        public override int GetHashCode()
        {
            int length = ByteArray.Length;

            if (length < 1)
            {
                return 0;
            }

            if (!(_hashCode[0] is { } hash))
            {
                unchecked
                {
                    const int p = 16777619;
                    hash = (int)2166136261;
                    foreach (byte t in ByteArray)
                    {
                        hash = (hash ^ t) * p;
                    }

                    hash += hash << 13;
                    hash ^= hash >> 7;
                    hash += hash << 3;
                    hash ^= hash >> 17;
                    hash += hash << 5;
                }

                _hashCode[0] = hash;
            }

            return hash;
        }

        int IComparable<ImmutableArray<byte>>.CompareTo(ImmutableArray<byte> other) =>
            ByteArrayComparer.Compare(ByteArray, other);

        int IComparable<byte[]>.CompareTo(byte[] other) =>
            ByteArrayComparer.Compare(ByteArray, other);

        int IComparable<Binary>.CompareTo(Binary other) =>
            ((IComparable<ImmutableArray<byte>>)this).CompareTo(other.ByteArray);

        int IComparable.CompareTo(object obj) =>
            obj switch
            {
                null =>
                    1,
                Binary binary =>
                    ((IComparable<Binary>)this).CompareTo(binary),
                ImmutableArray<byte> bytes =>
                    ((IComparable<ImmutableArray<byte>>)this).CompareTo(bytes),
                byte[] bytes =>
                    ((IComparable<byte[]>)this).CompareTo(bytes),
                _ =>
                    throw new ArgumentException(
                        "the argument is neither Binary nor Byte[]",
                        nameof(obj)
                    ),
            };

        public IEnumerator<byte> GetEnumerator() =>
            ((IEnumerable<byte>)ByteArray).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable)ByteArray).GetEnumerator();

        [Pure]
        byte[] IKey.EncodeAsByteArray() =>
            ToByteArray();

        [Pure]
        public IEnumerable<byte[]> EncodeIntoChunks()
        {
            string len = ByteArray.Length.ToString(CultureInfo.InvariantCulture);
            yield return Encoding.ASCII.GetBytes(len);
            yield return CommonVariables.Separator;  // ':'
            yield return ((IKey)this).EncodeAsByteArray();
        }

        public void EncodeToStream(Stream stream)
        {
            byte[] value = ToByteArray();
            string len = value.Length.ToString(CultureInfo.InvariantCulture);
            byte[] lenBytes = Encoding.ASCII.GetBytes(len);
            stream.Write(lenBytes, 0, lenBytes.Length);
            stream.WriteByte(CommonVariables.Separator[0]);
            stream.Write(value, 0, value.Length);
        }

        [Pure]
        public byte[] ToByteArray()
        {
            if (ByteArray.IsDefaultOrEmpty)
            {
                return new byte[0];
            }

            return ByteArray.ToBuilder().ToArray();
        }

        [Pure]
        public override string ToString() =>
            $"{nameof(Bencodex)}.{nameof(Bencodex.Types)}.{nameof(Binary)} {Inspection}";
    }
}
