using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Numerics;

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

        /// <inheritdoc cref="IValue.Kind"/>
        [Pure]
        public ValueKind Kind => ValueKind.Integer;

        /// <inheritdoc cref="IValue.EncodingLength"/>
        [Pure]
        public long EncodingLength =>
            2L + CountDecimalDigits();

        /// <inheritdoc cref="IValue.Inspection"/>
        [Obsolete("Deprecated in favour of " + nameof(Inspect) + "() method.")]
        public string Inspection => Inspect(true);

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

        /// <inheritdoc cref="IValue.Inspect(bool)"/>
        public string Inspect(bool loadAll) =>
            Value.ToString(CultureInfo.InvariantCulture);

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString() =>
            $"{nameof(Bencodex)}.{nameof(Types)}.{nameof(Integer)} {Inspect(false)}";

        internal long CountDecimalDigits() =>
            Value.Sign switch
            {
                -1 => Value > -10L
                    ? 2L
                    : Value > -100L
                        ? 3L
                        : Value > -1000L
                            ? 4L
                            : Value > -10000L
                                ? 5L
                                : Value.ToString(CultureInfo.InvariantCulture).Length,
                +1 => Value < 10UL
                    ? 1L
                    : Value < 100UL
                        ? 2L
                        : Value < 1000UL
                            ? 3L
                            : Value < 10000UL
                                ? 4L
                                : Value.ToString(CultureInfo.InvariantCulture).Length,
                _ => 1L,
            };
    }
}
