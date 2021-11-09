using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;

namespace Bencodex.Types
{
    /// <summary>
    /// Represents a unique fingerprint of a Bencodex value.
    /// </summary>
    [Serializable]
    public readonly struct Fingerprint : IEquatable<Fingerprint>, ISerializable
    {
        private readonly (
            byte, byte, byte, byte, byte, byte, byte, byte, byte, byte,
            byte, byte, byte, byte, byte, byte, byte, byte, byte, byte
        )? _hash;

        /// <summary>
        /// Creates a <see cref="Fingerprint"/> value.
        /// </summary>
        /// <param name="type">The value type.</param>
        /// <param name="encodingLength">The byte length of encoded value.</param>
        public Fingerprint(ValueType type, int encodingLength)
            : this(type, encodingLength, Array.Empty<byte>())
        {
        }

        /// <summary>
        /// Creates a <see cref="Fingerprint"/> value.
        /// </summary>
        /// <param name="type">The value type.</param>
        /// <param name="encodingLength">The byte length of encoded value.</param>
        /// <param name="hash">The hash digest of the value.  It can be either empty or 20
        /// bytes.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="hash"/> size
        /// is invalid.</exception>
        public Fingerprint(
            ValueType type,
            int encodingLength,
            IReadOnlyList<byte> hash
        )
        {
            if (hash.Any())
            {
                if (hash.Count != 20)
                {
                    throw new ArgumentException("The hash must be 20 bytes.", nameof(hash));
                }

                _hash = (hash[0], hash[1], hash[2], hash[3], hash[4],
                         hash[5], hash[6], hash[7], hash[8], hash[9],
                         hash[10], hash[11], hash[12], hash[13], hash[14],
                         hash[15], hash[16], hash[17], hash[18], hash[19]);
            }
            else
            {
                _hash = null;
            }

            Type = type;
            EncodingLength = encodingLength;
        }

        private Fingerprint(SerializationInfo info, StreamingContext context)
            : this(
                (ValueType)info.GetByte(nameof(Type)),
                info.GetInt32(nameof(EncodingLength)),
                (byte[])info.GetValue(nameof(Hash), typeof(byte[]))
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
        /// The hash digest of the value.  It can be either empty or 20 bytes.
        /// </summary>
        [Pure]
        public ImmutableArray<byte> Hash => ImmutableArray.Create(GetHashArray());

        /// <summary>
        /// Deserialized the serialized fingerprint bytes.
        /// </summary>
        /// <param name="serialized">The bytes made by <see cref="Serialize()"/> method.</param>
        /// <returns>The deserialized <see cref="Fingerprint"/> value.</returns>
        /// <exception cref="FormatException">Thrown when the <paramref name="serialized"/> bytes
        /// is invalid.</exception>
        public static Fingerprint Deserialize(byte[] serialized)
        {
            byte type;
            byte[] hash;
            byte[] encLength;
            if (serialized.Length >= 1 + 20 + 1)
            {
                type = serialized[0];
                hash = new byte[20];
                Array.Copy(serialized, 1, hash, 0, 20);
                encLength = new byte[serialized.Length - 1 - 20];
                Array.Copy(serialized, 1 + 20, encLength, 0, encLength.Length);
            }
            else if (serialized.Length > 1)
            {
                type = serialized[0];
                hash = Array.Empty<byte>();
                encLength = new byte[serialized.Length - 1];
                Array.Copy(serialized, 1, encLength, 0, encLength.Length);
            }
            else
            {
                throw new FormatException("The serialized bytes is not valid.");
            }

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(encLength);
            }

            return new Fingerprint((ValueType)type, BitConverter.ToInt32(encLength, 0), hash);
        }

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        [Pure]
        public bool Equals(Fingerprint other) =>
            Type == other.Type &&
            EncodingLength == other.EncodingLength &&
            (
                (_hash is { } h && other._hash is { } o && h.Equals(o)) ||
                (_hash is null && other._hash is null)
            );

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
                hashCode = (hashCode * 397) ^ _hash.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Gets the hash digest of the value.
        /// </summary>
        /// <returns>The hash digest of the value.  It can be either empty or 20 bytes.</returns>
        [Pure]
        public byte[] GetHashArray() => _hash is { } h
            ? new byte[20]
                {
                    h.Item1, h.Item2, h.Item3, h.Item4, h.Item5,
                    h.Item6, h.Item7, h.Item8, h.Item9, h.Item10,
                    h.Item11, h.Item12, h.Item13, h.Item14, h.Item15,
                    h.Item16, h.Item17, h.Item18, h.Item19, h.Item20,
                }
            : Array.Empty<byte>();

        /// <summary>
        /// Serializes the fingerprint into bytes.
        /// </summary>
        /// <returns>The serialized bytes.  For the equivalent fingerprint, the equivalent bytes
        /// is returned.</returns>
        [Pure]
        public byte[] Serialize()
        {
            byte[] hash = GetHashArray();
            byte[] encLength = BitConverter.GetBytes(EncodingLength);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(encLength);
            }

            var total = new byte[1 + hash.Length + encLength.Length];
            total[0] = (byte)Type;
            hash.CopyTo(total, 1);
            encLength.CopyTo(total, 1 + hash.Length);
            return total;
        }

        /// <inheritdoc cref="ISerializable.GetObjectData(SerializationInfo, StreamingContext)"/>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Type), (byte)Type);
            info.AddValue(nameof(EncodingLength), EncodingLength);
            info.AddValue(nameof(Hash), GetHashArray());
        }
    }
}
