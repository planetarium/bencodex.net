using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Bencodex.Types
{
    public struct Integer :
        IValue,
        IEquatable<BigInteger>,
        IEquatable<Integer>,
        IComparable<BigInteger>,
        IComparable<Integer>,
        IComparable
    {
        private static readonly byte[] _prefix = new byte[1] { 0x69 };  // 'i'

        public Integer(BigInteger value)
        {
            Value = value;
        }

        public Integer(short value)
            : this(new BigInteger(value))
        {
        }

        public Integer(ushort value)
            : this(new BigInteger(value))
        {
        }

        public Integer(int value)
            : this(new BigInteger(value))
        {
        }

        public Integer(uint value)
            : this(new BigInteger(value))
        {
        }

        public Integer(long value)
            : this(new BigInteger(value))
        {
        }

        public Integer(ulong value)
            : this(new BigInteger(value))
        {
        }

        public Integer(string value, IFormatProvider? provider = null)
            : this(BigInteger.Parse(value, provider))
        {
        }

        public BigInteger Value { get; }

        /// <inheritdoc cref="IValue.Type"/>
        [Pure]
        public ValueType Type => ValueType.Integer;

        /// <inheritdoc cref="IValue.Fingerprint"/>
        [Pure]
        public Fingerprint Fingerprint
        {
            get
            {
                // If the byte representation is compact enough, use it as a digest too.
                // If it's longer than 20 bytes, make a SHA-1 hash for digest.
                byte[] bytes = Value.ToByteArray();
                IReadOnlyList<byte> digest =
                    bytes.Length <= 20 ? bytes : SHA1.Create().ComputeHash(bytes);
                return new Fingerprint(Type, EncodingLength, digest);
            }
        }

        /// <inheritdoc cref="IValue.EncodingLength"/>
        [Pure]
        public int EncodingLength =>
            2 + Value.ToString(CultureInfo.InvariantCulture).Length;

        /// <inheritdoc cref="IValue.Inspection"/>
        [Pure]
        public string Inspection =>
            Value.ToString(CultureInfo.InvariantCulture);

        public static implicit operator BigInteger(Integer i)
        {
            return i.Value;
        }

        public static implicit operator short(Integer i)
        {
            return (short)i.Value;
        }

        public static implicit operator ushort(Integer i)
        {
            return (ushort)i.Value;
        }

        public static implicit operator int(Integer i)
        {
            return (int)i.Value;
        }

        public static implicit operator uint(Integer i)
        {
            return (uint)i.Value;
        }

        public static implicit operator long(Integer i)
        {
            return (long)i.Value;
        }

        public static implicit operator ulong(Integer i)
        {
            return (ulong)i.Value;
        }

        public static implicit operator Integer(BigInteger i)
        {
            return new Integer(i);
        }

        public static implicit operator Integer(short i)
        {
            return new Integer(i);
        }

        public static implicit operator Integer(ushort i)
        {
            return new Integer(i);
        }

        public static implicit operator Integer(int i)
        {
            return new Integer(i);
        }

        public static implicit operator Integer(uint i)
        {
            return new Integer(i);
        }

        public static implicit operator Integer(long i)
        {
            return new Integer(i);
        }

        public static implicit operator Integer(ulong i)
        {
            return new Integer(i);
        }

        public static bool operator ==(Integer a, Integer b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Integer a, Integer b)
        {
            return !(a == b);
        }

        public static bool operator ==(Integer a, BigInteger b)
        {
            return a.Value.Equals(b);
        }

        public static bool operator !=(Integer a, BigInteger b)
        {
            return !(a == b);
        }

        public static bool operator ==(BigInteger a, Integer b)
        {
            return a.Equals(b.Value);
        }

        public static bool operator !=(BigInteger a, Integer b)
        {
            return !(a == b);
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj is Integer i)
            {
                return ((IComparable<Integer>)this).CompareTo(i);
            }

            return Value.CompareTo(obj);
        }

        int IComparable<BigInteger>.CompareTo(BigInteger other)
        {
            return Value.CompareTo(other);
        }

        int IComparable<Integer>.CompareTo(Integer other)
        {
            return ((IComparable<BigInteger>)this).CompareTo(other.Value);
        }

        bool IEquatable<BigInteger>.Equals(BigInteger other)
        {
            return Value.Equals(other);
        }

        bool IEquatable<Integer>.Equals(Integer other)
        {
            return Value.Equals(other.Value);
        }

        bool IEquatable<IValue>.Equals(IValue other) =>
            other is Integer o && ((IEquatable<Integer>)this).Equals(o);

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null:
                    return false;
                case Integer other:
                    return ((IEquatable<Integer>)this).Equals(other);
                case BigInteger other:
                    return ((IEquatable<BigInteger>)this).Equals(other);
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        [Pure]
        public IEnumerable<byte[]> EncodeIntoChunks()
        {
            yield return _prefix;
            string digits = Value.ToString(CultureInfo.InvariantCulture);
            yield return Encoding.ASCII.GetBytes(digits);
            yield return CommonVariables.Suffix;
        }

        public void EncodeToStream(Stream stream)
        {
            stream.WriteByte(_prefix[0]);
            string digits = Value.ToString(CultureInfo.InvariantCulture);
            byte[] digitsAscii = Encoding.ASCII.GetBytes(digits);
            stream.Write(digitsAscii, 0, digitsAscii.Length);
            stream.WriteByte(CommonVariables.Suffix[0]);
        }

        [Pure]
        public override string ToString() =>
            $"{nameof(Bencodex)}.{nameof(Bencodex.Types)}.{nameof(Integer)} {Inspection}";
    }
}
