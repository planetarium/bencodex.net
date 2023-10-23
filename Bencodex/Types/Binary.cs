using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Bencodex.Types
{
    public readonly struct Binary :
        IKey,
        IEquatable<Binary>,
        IComparable<Binary>,
        IComparable,
        IEnumerable<byte>
    {
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

        /// <inheritdoc cref="IValue.EncodingLength"/>
        [Pure]
        public long EncodingLength =>
            ByteArray.Length.ToString(CultureInfo.InvariantCulture).Length +
            CommonVariables.Separator.LongLength +
            ByteArray.Length;

        public static explicit operator Binary(ImmutableArray<byte> bytes) =>
            new Binary(bytes);

        public static explicit operator ImmutableArray<byte>(Binary binary) =>
            binary.ByteArray;

        public static explicit operator Binary(byte[] bytes) =>
            new Binary(bytes);

        public static explicit operator byte[](Binary binary) =>
            binary.ToByteArray();

        public static bool operator ==(Binary left, Binary right) => left.Equals(right);

        public static bool operator !=(Binary left, Binary right) => !left.Equals(right);

        /// <summary>
        /// Creates a new <see cref="Binary"/> instance from a binary turned into
        /// <paramref name="hex"/>.
        /// </summary>
        /// <param name="hex">A hexadecimal representation of a binary.</param>
        /// <param name="offset">The offset of the first character to convert.
        /// </param>
        /// <param name="count">The number of characters to convert.  If omitted
        /// or -1, the rest of the string is used.</param>
        /// <returns>A new <see cref="Binary"/> instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when any of
        /// <paramref name="offset"/> or <paramref name="count"/> refers to a
        /// position outside of <paramref name="hex"/>.</exception>
        /// <exception cref="FormatException">Thrown when the given range of
        /// <paramref name="hex"/> is not a valid hexadecimal representation of
        /// a binary.</exception>
        public static Binary FromHex(string hex, int offset = 0, int count = -1)
        {
            if (offset < 0 || offset > hex.Length)
            {
                const string msg = "Offset must be non-negative and less than or equal to " +
                    "the length of the range.";
                throw new ArgumentOutOfRangeException(nameof(offset), msg);
            }
            else if (count == -1)
            {
                count = hex.Length - offset;
            }
            else if (count < 0 || offset + count > hex.Length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(count),
                    "Count must be non-negative and less than or equal to the string length."
                );
            }

            if (count % 2 != 0)
            {
                throw new ArgumentException(
                    "The length of the hex string must be even.",
                    nameof(count)
                );
            }

            byte ParseNibble(char hex)
            {
                var v = hex >= '0' && hex <= '9'
                    ? hex - '0'
                    : hex >= 'a' && hex <= 'f'
                    ? hex - 'a' + 10
                    : hex >= 'A' && hex <= 'F'
                    ? hex - 'A' + 10
                    : throw new FormatException(
                        "The string contains invalid hex character."
                    );
                return (byte)v;
            }

            var bytes = ImmutableArray.CreateBuilder<byte>(count / 2);
            for (int i = 0; i < count; i += 2)
            {
                char upper = hex[offset + i];
                char lower = hex[offset + i + 1];
                bytes.Add((byte)(ParseNibble(upper) << 4 | ParseNibble(lower)));
            }

            return new Binary(bytes.MoveToImmutable());
        }

#if !NETSTANDARD2_0
        /// <summary>
        /// Creates a new <see cref="Binary"/> instance from a binary turned into
        /// <paramref name="base64"/>.
        /// </summary>
        /// <param name="base64">A base64 representation of a binary.</param>
        /// <returns>A new <see cref="Binary"/> instance.</returns>
        /// <exception cref="FormatException">Thrown when the given
        /// <paramref name="base64"/> is not a valid base64 representation.</exception>
        public static Binary FromBase64(ReadOnlySpan<char> base64)
        {
            int length = base64.Length / 4 * 3;
            if (base64.Length > 0 && base64[base64.Length - 1] == '=')
            {
                length--;
                if (base64[base64.Length - 2] == '=')
                {
                    length--;
                }
            }

            var bytes = new byte[length];
            if (!Convert.TryFromBase64Chars(base64, bytes, out int written))
            {
                throw new FormatException("The given base64 string is invalid.");
            }

            ImmutableArray<byte> moved = Unsafe.As<byte[], ImmutableArray<byte>>(ref bytes);
            return new Binary(moved);
        }
#endif

        /// <summary>
        /// Creates a new <see cref="Binary"/> instance from a binary turned into
        /// <paramref name="base64"/>.
        /// </summary>
        /// <param name="base64">A base64 representation of a binary.</param>
        /// <returns>A new <see cref="Binary"/> instance.</returns>
        /// <exception cref="FormatException">Thrown when the given
        /// <paramref name="base64"/> is not a valid base64 representation.</exception>
        public static Binary FromBase64(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            ImmutableArray<byte> moved = Unsafe.As<byte[], ImmutableArray<byte>>(ref bytes);
            return new Binary(moved);
        }

        public override bool Equals(object? obj) => obj is Binary other && Equals(other);

        public bool Equals(IValue other) => other is Binary i && Equals(i);

        public bool Equals(Binary other) => ByteArray.SequenceEqual(other.ByteArray);

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

        public int CompareTo(Binary other)
        {
            int minLength = Math.Min(ByteArray.Length, other.ByteArray.Length);
            for (int i = 0; i < minLength; i++)
            {
                int c = ByteArray[i].CompareTo(other.ByteArray[i]);
                if (c != 0)
                {
                    return c;
                }
            }

            return ByteArray.Length.CompareTo(other.ByteArray.Length);
        }

        public int CompareTo(object? obj)
        {
            if (obj is null)
            {
                return 1;
            }

            if (obj is Binary b)
            {
                return CompareTo(b);
            }

            throw new ArgumentException($"Object must be of type {nameof(Binary)}");
        }

        public IEnumerator<byte> GetEnumerator() =>
            ((IEnumerable<byte>)ByteArray).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable)ByteArray).GetEnumerator();

        [Pure]
        public byte[] ToByteArray()
        {
            if (ByteArray.IsDefaultOrEmpty)
            {
                return Array.Empty<byte>();
            }

            return ByteArray.ToBuilder().ToArray();
        }

        /// <summary>
        /// Writes the hexadecimal representation of the binary to the given
        /// <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">A <see cref="StringBuilder"/> to write the
        /// hexadecimal representation of the binary.</param>
        [Pure]
        public void ToHex(StringBuilder builder)
        {
            const string hex = "0123456789abcdef";
            foreach (byte b in ByteArray)
            {
                builder.Append(hex[b >> 4]);
                builder.Append(hex[b & 0x0f]);
            }
        }

        /// <summary>
        /// Returns the hexadecimal representation of the binary.
        /// </summary>
        /// <returns>The hexadecimal representation of the binary.</returns>
        [Pure]
        public string ToHex()
        {
            var builder = new StringBuilder(ByteArray.Length * 2);
            ToHex(builder);
            return builder.ToString();
        }

        /// <summary>
        /// Returns the base64 representation of the binary.
        /// </summary>
        /// <returns>The base64 representation of the binary.</returns>
        [Pure]
        public string ToBase64()
        {
            ImmutableArray<byte> bytes = ByteArray;
            return Convert.ToBase64String(Unsafe.As<ImmutableArray<byte>, byte[]>(ref bytes));
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
