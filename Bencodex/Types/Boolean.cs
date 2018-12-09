using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Bencodex.Types
{
    /// <summary>Represents a Bencodex Boolean true (i.e., <c>t</c>)
    /// or false (i.e., <c>f</c>).</summary>
    public struct Boolean :
        IValue,
        IEquatable<bool>,
        IEquatable<Boolean>,
        IComparable<bool>,
        IComparable<Boolean>,
        IComparable
    {
        public bool Value { get; }

        public Boolean(bool value)
        {
            Value = value;
        }

        public int CompareTo(object obj)
        {
            if (obj is bool b)
            {
                return ((IComparable<bool>) this).CompareTo(b);
            }

            return Value.CompareTo(obj);
        }

        int IComparable<bool>.CompareTo(bool other) => Value.CompareTo(other);

        int IComparable<Boolean>.CompareTo(Boolean other)
        {
            return CompareTo(other.Value);
        }

        bool IEquatable<bool>.Equals(bool other)
        {
            return Value == other;
        }

        bool IEquatable<Boolean>.Equals(Boolean other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null:
                    return false;
                case Boolean b:
                    return Equals(b);
                case bool b:
                    return Equals(b);
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
            if (Value)
            {
                yield return new byte[1] {0x74};  // 't'
            }
            else
            {
                yield return new byte[1] {0x66};  // 'f'
            }
        }
    }
}
