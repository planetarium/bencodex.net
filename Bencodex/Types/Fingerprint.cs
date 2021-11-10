using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using Bencodex.Misc;

namespace Bencodex.Types
{
    /// <summary>
    /// Represents a unique fingerprint of a Bencodex value.
    /// </summary>
    [Serializable]
    public readonly struct Fingerprint : IEquatable<Fingerprint>, ISerializable
    {
        /// <summary>
        /// Creates a <see cref="Fingerprint"/> value.
        /// </summary>
        /// <param name="type">The value type.</param>
        /// <param name="encodingLength">The byte length of encoded value.</param>
        public Fingerprint(in ValueType type, in int encodingLength)
            : this(type, encodingLength, ImmutableArray<byte>.Empty)
        {
        }

        /// <summary>
        /// Creates a <see cref="Fingerprint"/> value.
        /// </summary>
        /// <param name="type">The value type.</param>
        /// <param name="encodingLength">The byte length of encoded value.</param>
        /// <param name="digest">The digest of the value.  It can be empty, but cannot be
        /// <c>null</c>.</param>
        public Fingerprint(
            in ValueType type,
            in int encodingLength,
            IReadOnlyList<byte> digest
        )
            : this(
                type,
                encodingLength,
                digest is ImmutableArray<byte> ia ? ia : ImmutableArray.CreateRange(digest)
            )
        {
        }

        /// <summary>
        /// Creates a <see cref="Fingerprint"/> value.
        /// </summary>
        /// <param name="type">The value type.</param>
        /// <param name="encodingLength">The byte length of encoded value.</param>
        /// <param name="digest">The digest of the value.  It can be empty, but cannot be
        /// <c>null</c>.</param>
        public Fingerprint(
            in ValueType type,
            in int encodingLength,
            in ImmutableArray<byte> digest
        )
        {
            Type = type;
            EncodingLength = encodingLength;
            Digest = digest;
        }

        private Fingerprint(SerializationInfo info, StreamingContext context)
            : this(
                (ValueType)info.GetByte(nameof(Type)),
                info.GetInt32(nameof(EncodingLength)),
                (byte[])info.GetValue(nameof(Digest), typeof(byte[]))
            )
        {
        }

        /// <summary>
        /// The value type.
        /// </summary>
        [Pure]
        public ValueType Type { get; }

        /// <summary>
        /// The byte length of encoded value.
        /// </summary>
        [Pure]
        public int EncodingLength { get; }

        /// <summary>
        /// The digest of the value.  It can be empty, but cannot be <c>null</c>.
        /// <para>Digests are usually hash digests of their original values, but not necessarily.
        /// If a value's original representation itself is enough compact, the representation can
        /// be used as its digest too.</para>
        /// </summary>
        [Pure]
        public ImmutableArray<byte> Digest { get; }

        /// <summary>
        /// Deserialized the serialized fingerprint bytes.
        /// </summary>
        /// <param name="serialized">The bytes made by <see cref="Serialize()"/> method.</param>
        /// <returns>The deserialized <see cref="Fingerprint"/> value.</returns>
        /// <exception cref="FormatException">Thrown when the <paramref name="serialized"/> bytes
        /// is invalid.</exception>
        public static Fingerprint Deserialize(byte[] serialized)
        {
            if (serialized.Length < 5)
            {
                throw new FormatException("The serialized bytes is not valid.");
            }

            var type = (ValueType)serialized[0];
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(serialized, 1, 4);
            }

            return new Fingerprint(
                type,
                BitConverter.ToInt32(serialized, 1),
                serialized.Skip(5).ToImmutableArray()
            );
        }

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        [Pure]
        public bool Equals(Fingerprint other)
        {
            if (Type != other.Type ||
                EncodingLength != other.EncodingLength ||
                Digest.Length != other.Digest.Length)
            {
                return false;
            }

            for (int i = 0; i < Digest.Length; i++)
            {
                if (Digest[i] != other.Digest[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc cref="object.Equals(object?)"/>
        [Pure]
        public override bool Equals(object? obj) =>
            obj is Fingerprint other && Equals(other);

        /// <inheritdoc cref="object.GetHashCode()"/>
        [Pure]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Type;
                hashCode = (hashCode * 397) ^ EncodingLength;
                foreach (byte b in Digest)
                {
                    hashCode = (hashCode * 397) ^ b;
                }

                return hashCode;
            }
        }

        /// <summary>
        /// Gets the digest array of the value.
        /// </summary>
        /// <returns>The hash digest of the value.  It can be either empty or 20 bytes.</returns>
        [Pure]
        public byte[] GetDigest() => Digest.ToBuilder().ToArray();

        /// <summary>
        /// Serializes the fingerprint into bytes.
        /// <para>You can round-trip a <see cref="Fingerprint"/> value by serializing it to a byte array,
        /// and then deserializing it using the <see cref="Deserialize(byte[])"/> method.</para>
        /// </summary>
        /// <returns>The serialized bytes.  For the equivalent fingerprint, the equivalent bytes
        /// is returned.</returns>
        [Pure]
        public byte[] Serialize()
        {
            byte[] hash = GetDigest();
            byte[] encLength = BitConverter.GetBytes(EncodingLength);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(encLength);
            }

            var total = new byte[1 + encLength.Length + hash.Length];
            total[0] = (byte)Type;
            encLength.CopyTo(total, 1);
            hash.CopyTo(total, 1 + encLength.Length);
            return total;
        }

        /// <inheritdoc cref="ISerializable.GetObjectData(SerializationInfo, StreamingContext)"/>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Type), (byte)Type);
            info.AddValue(nameof(EncodingLength), EncodingLength);
            info.AddValue(nameof(Digest), GetDigest());
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString() =>
            $"{Type} {(Digest.Any() ? $"{Digest.Hex()} " : string.Empty)}[{EncodingLength} B]";
    }
}
