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
        private static readonly ImmutableHashSet<ValueKind> _availableKinds = Enum.GetValues(typeof(ValueKind))
            .Cast<ValueKind>()
            .OrderBy(k => k)
            .ToImmutableHashSet();

        /// <summary>
        /// Creates a <see cref="Fingerprint"/> value.
        /// </summary>
        /// <param name="kind">The Bencodex type of the value.</param>
        /// <param name="encodingLength">The byte length of encoded value.</param>
        public Fingerprint(in ValueKind kind, in long encodingLength)
            : this(kind, encodingLength, ImmutableArray<byte>.Empty)
        {
        }

        /// <summary>
        /// Creates a <see cref="Fingerprint"/> value.
        /// </summary>
        /// <param name="kind">The Bencodex type of the value.</param>
        /// <param name="encodingLength">The byte length of encoded value.</param>
        /// <param name="digest">The digest of the value.  It can be empty, but cannot be
        /// <c>null</c>.</param>
        public Fingerprint(
            in ValueKind kind,
            in long encodingLength,
            IReadOnlyList<byte> digest
        )
            : this(
                kind,
                encodingLength,
                digest is ImmutableArray<byte> ia ? ia : ImmutableArray.CreateRange(digest)
            )
        {
        }

        /// <summary>
        /// Creates a <see cref="Fingerprint"/> value.
        /// </summary>
        /// <param name="kind">The Bencodex type of the value.</param>
        /// <param name="encodingLength">The byte length of encoded value.</param>
        /// <param name="digest">The digest of the value.  It can be empty, but cannot be
        /// <c>null</c>.</param>
        public Fingerprint(
            in ValueKind kind,
            in long encodingLength,
            in ImmutableArray<byte> digest
        )
        {
            Kind = kind;
            EncodingLength = encodingLength;
            Digest = digest;
        }

        private Fingerprint(SerializationInfo info, StreamingContext context)
            : this(
                (ValueKind)info.GetByte(nameof(Kind)),
                info.GetInt64(nameof(EncodingLength)),
                (byte[])info.GetValue(nameof(Digest), typeof(byte[]))
            )
        {
        }

        /// <summary>
        /// The Bencodex type of the value.
        /// </summary>
        [Pure]
        public ValueKind Kind { get; }

        /// <summary>
        /// The byte length of encoded value.
        /// </summary>
        [Pure]
        public long EncodingLength { get; }

        /// <summary>
        /// The digest of the value.  It can be empty, but cannot be <c>null</c>.
        /// <para>Digests are usually hash digests of their original values, but not necessarily.
        /// If a value's original representation itself is enough compact, the representation can
        /// be used as its digest too.</para>
        /// </summary>
        [Pure]
        public ImmutableArray<byte> Digest { get; }

        /// <summary>
        /// Tests if two <see cref="Fingerprint"/> values are equal.
        /// </summary>
        /// <param name="a">A <see cref="Fingerprint"/> value to compare.</param>
        /// <param name="b">Another <see cref="Fingerprint"/> value to compare.</param>
        /// <returns><c>true</c> if two values are equal.  Otherwise <c>false</c>.</returns>
        [Pure]
        public static bool operator ==(in Fingerprint a, in Fingerprint b) =>
            a.Equals(b);

        /// <summary>
        /// Tests if two <see cref="Fingerprint"/> values are not equal.
        /// </summary>
        /// <param name="a">A <see cref="Fingerprint"/> value to compare.</param>
        /// <param name="b">Another <see cref="Fingerprint"/> value to compare.</param>
        /// <returns><c>false</c> if two values are equal.  Otherwise <c>true</c>.</returns>
        [Pure]
        public static bool operator !=(Fingerprint a, Fingerprint b) =>
            !a.Equals(b);

        /// <summary>
        /// Deserialized the serialized fingerprint bytes.
        /// </summary>
        /// <param name="serialized">The bytes made by <see cref="Serialize()"/> method.</param>
        /// <returns>The deserialized <see cref="Fingerprint"/> value.</returns>
        /// <exception cref="FormatException">Thrown when the <paramref name="serialized"/> bytes
        /// is invalid.</exception>
        public static Fingerprint Deserialize(byte[] serialized)
        {
            if (serialized.Length < 1 + 8)
            {
                throw new FormatException("The serialized byte array is too short.");
            }

            var kind = (ValueKind)serialized[0];
            if (!_availableKinds.Contains(kind))
            {
                throw new FormatException(
                    $"Invalid value kind: {serialized[0]}; available kinds are:\n\n" +
                    string.Join("\n", _availableKinds.Select(k => $"{(byte)k}. {k}"))
                );
            }

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(serialized, 1, 8);
            }

            return new Fingerprint(
                kind,
                BitConverter.ToInt64(serialized, 1),
                serialized.Skip(1 + 8).ToImmutableArray()
            );
        }

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        [Pure]
        public bool Equals(Fingerprint other)
        {
            if (Kind != other.Kind ||
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
                var hashCode = (int)Kind;
                hashCode = (hashCode * 397) ^ EncodingLength.GetHashCode();
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
            var buffer = new byte[CountSerializationBytes()];
            SerializeInto(buffer, 0);
            return buffer;
        }

        /// <inheritdoc cref="ISerializable.GetObjectData(SerializationInfo, StreamingContext)"/>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Kind), (byte)Kind);
            info.AddValue(nameof(EncodingLength), EncodingLength);
            info.AddValue(nameof(Digest), GetDigest());
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString() =>
            $"{Kind} {(Digest.Any() ? $"{Digest.Hex()} " : string.Empty)}[{EncodingLength} B]";

        internal static Fingerprint Deserialize(byte[] buffer, long offset, long length)
        {
            var kind = (ValueKind)buffer[offset];
            if (!_availableKinds.Contains(kind))
            {
                throw new FormatException(
                    $"Invalid value kind: {buffer[offset]}; available kinds are:\n\n" +
                    string.Join("\n", _availableKinds.Select(k => $"{(byte)k}. {k}"))
                );
            }

            long encodingLength;
            if (BitConverter.IsLittleEndian || offset + 1L + 8L > int.MaxValue)
            {
                var int64Buffer = new byte[8];
                Array.Copy(buffer, offset + 1L, int64Buffer, 0L, int64Buffer.LongLength);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(int64Buffer);
                }

                encodingLength = BitConverter.ToInt64(int64Buffer, 0);
            }
            else
            {
                encodingLength = BitConverter.ToInt64(buffer, 1 + (int)offset);
            }

            if (offset + length > int.MaxValue)
            {
                var digestBuffer = new byte[length - 1L - 8L];
                Array.Copy(buffer, offset + 1L + 8L, digestBuffer, 0L, digestBuffer.LongLength);
                return new Fingerprint(kind, encodingLength, ImmutableArray.Create(digestBuffer));
            }

            return new Fingerprint(
                kind,
                encodingLength,
                ImmutableArray.Create(buffer, (int)(offset + 1L + 8L), (int)(length - 1L - 8L))
            );
        }

        internal long SerializeInto(byte[] buffer, long offset)
        {
            buffer[offset] = (byte)Kind;

            byte[] encLength = BitConverter.GetBytes(EncodingLength);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(encLength);
            }

            encLength.CopyTo(buffer, offset + 1L);

            long o = offset + 1L + encLength.Length;
            if (o + Digest.Length > int.MaxValue)
            {
                GetDigest().CopyTo(buffer, o);
            }
            else
            {
                Digest.CopyTo(buffer, (int)o);
            }

            return 1L + encLength.LongLength + Digest.Length;
        }

        internal long CountSerializationBytes() => 1L + 8L + Digest.Length;
    }
}
