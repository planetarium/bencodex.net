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
#pragma warning disable SA1202
        public static readonly Fingerprint TrueFingerprint =
            new Fingerprint(ValueKind.Boolean, 1L, new byte[] { 1 });

        public static readonly Fingerprint FalseFingerprint =
            new Fingerprint(ValueKind.Boolean, 1L, new byte[] { 0 });
#pragma warning restore SA1202

        public Boolean(bool value)
        {
            Value = value;
        }

        public bool Value { get; }

        /// <inheritdoc cref="IValue.Kind"/>
        [Pure]
        public ValueKind Kind => ValueKind.Boolean;

        /// <inheritdoc cref="IValue.Fingerprint"/>
        [Pure]
        public Fingerprint Fingerprint => Value ? TrueFingerprint : FalseFingerprint;

        /// <inheritdoc cref="IValue.EncodingLength"/>
        [Pure]
        public long EncodingLength => 1L;

        /// <inheritdoc cref="IValue.Inspection"/>
        [Obsolete("Deprecated in favour of " + nameof(Inspect) + "() method.")]
        public string Inspection => Inspect(true);

        public static implicit operator bool(Boolean boolean)
        {
            return boolean.Value;
        }

        public static implicit operator Boolean(bool b)
        {
            return new Boolean(b);
        }

        int IComparable.CompareTo(object obj)
        {
            return Value.CompareTo(obj);
        }

        int IComparable<Boolean>.CompareTo(Boolean other)
        {
            return Value.CompareTo(other.Value);
        }

        bool IEquatable<Boolean>.Equals(Boolean other)
        {
            return Value == other.Value;
        }

        bool IEquatable<IValue>.Equals(IValue other) =>
            other is Boolean o && ((IEquatable<Boolean>)this).Equals(o);

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null:
                    return false;
                case Boolean b:
                    return Value.Equals(b.Value);
                case bool b:
                    return Value.Equals(b);
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
            Value ? "true" : "false";

        /// <inheritdoc cref="object.ToString()"/>
        [Pure]
        public override string ToString() =>
            $"{nameof(Bencodex)}.{nameof(Types)}.{nameof(Boolean)} {Inspect(false)}";
    }
}
