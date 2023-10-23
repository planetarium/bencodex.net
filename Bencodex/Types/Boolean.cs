using System;
using System.Diagnostics.Contracts;

namespace Bencodex.Types
{
    /// <summary>Represents a Bencodex Boolean true (i.e., <c>t</c>)
    /// or false (i.e., <c>f</c>).</summary>
    public struct Boolean :
        IValue,
        IEquatable<Boolean>,
        IComparable<Boolean>,
        IComparable
    {
        public Boolean(bool value)
        {
            Value = value;
        }

        public bool Value { get; }

        /// <inheritdoc cref="IValue.Kind"/>
        [Pure]
        public ValueKind Kind => ValueKind.Boolean;

        /// <inheritdoc cref="IValue.EncodingLength"/>
        [Pure]
        public long EncodingLength => 1L;

        public static implicit operator bool(Boolean boolean)
        {
            return boolean.Value;
        }

        public static implicit operator Boolean(bool b)
        {
            return new Boolean(b);
        }

        public int CompareTo(Boolean other)
        {
            return Value.CompareTo(other.Value);
        }

        public int CompareTo(object? obj)
        {
            if (obj is null)
            {
                return 1;
            }

            if (obj is Boolean b)
            {
                return CompareTo(b);
            }

            throw new ArgumentException($"Object must be of type {nameof(Boolean)}");
        }

        public bool Equals(Boolean other) => Value == other.Value;

        public bool Equals(IValue other) => other is Boolean b && Equals(b);

        public override bool Equals(object obj) => obj is Boolean b && Equals(b);

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <inheritdoc cref="IValue.Inspect()"/>
        public string Inspect() => Value ? "true" : "false";

        /// <inheritdoc cref="object.ToString()"/>
        [Pure]
        public override string ToString() =>
            $"{nameof(Bencodex)}.{nameof(Types)}.{nameof(Boolean)} {Inspect()}";
    }
}
