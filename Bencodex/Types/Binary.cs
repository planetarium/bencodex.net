using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
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
        private readonly ImmutableArray<byte>?[] _digest;

        public Binary(ImmutableArray<byte> value)
        {
            _value = value;
            _hashCode = new int?[1];
            _digest = new[] { (ImmutableArray<byte>?)null };
        }

        public Binary(params byte[] value)
            : this(value is byte[] bytes
                ? ImmutableArray.Create(bytes)
                : throw new ArgumentNullException(nameof(value)))
        {
        }

        public Binary(string text, Encoding encoding)
            : this(encoding.GetBytes(text))
        {
        }

        public ImmutableArray<byte> ByteArray =>
            _value.IsDefaultOrEmpty ? ImmutableArray<byte>.Empty : _value;

        [Obsolete(
            "The Binary.Value property is obsolete; use Binary.ToByteArray() " +
            "method or Binary.ByteArray property instead.")]
        public byte[] Value => ToByteArray();

        /// <inheritdoc cref="IValue.Kind"/>
        [Pure]
        public ValueKind Kind => ValueKind.Binary;

        /// <inheritdoc cref="IValue.Fingerprint"/>
        [Pure]
        public Fingerprint Fingerprint
        {
            get
            {
                ImmutableArray<byte> digest = _digest is { } cache
                    ? cache[0] is { } b
                        ? b
                        : ByteArray.Length > 20
                            ? ImmutableArray.Create(SHA1.Create().ComputeHash(ToByteArray()))
                            : ByteArray
                    : ImmutableArray<byte>.Empty;
                return new Fingerprint(Kind, EncodingLength, digest);
            }
        }

        /// <inheritdoc cref="IValue.EncodingLength"/>
        [Pure]
        public long EncodingLength =>
            ByteArray.Length.ToString(CultureInfo.InvariantCulture).Length +
            CommonVariables.Separator.LongLength +
            ByteArray.Length;

        /// <inheritdoc cref="IValue.Inspection"/>
        [Obsolete("Deprecated in favour of " + nameof(Inspect) + "() method.")]
        public string Inspection => Inspect(true);

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

        bool IEquatable<IValue>.Equals(IValue other) =>
            other is Binary o && ((IEquatable<Binary>)this).Equals(o);

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
        public byte[] ToByteArray()
        {
            if (ByteArray.IsDefaultOrEmpty)
            {
                return new byte[0];
            }

            return ByteArray.ToBuilder().ToArray();
        }

        /// <inheritdoc cref="IValue.Inspect(bool)"/>
        public string Inspect(bool loadAll)
        {
            IEnumerable<string> contents = this.Select(b => $"\\x{b:x2}");
            return $"b\"{string.Join(string.Empty, contents)}\"";
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString() =>
            $"{nameof(Bencodex)}.{nameof(Bencodex.Types)}.{nameof(Binary)} {Inspect(false)}";
    }
}
