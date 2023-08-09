using System;
using System.Diagnostics.Contracts;

namespace Bencodex.Types
{
    /// <summary>Represents a Bencodex null value (i.e., <c>n</c>).</summary>
    public readonly struct Null :
        IValue,
        IEquatable<Null>,
        IComparable<Null>,
        IComparable
    {
        /// <summary>
        /// Represents a <see cref="Null"/> instance.  Recommends to prefer this over using
        /// the default constructor or a <c>default</c> keyword.  This field is read-only.
        /// </summary>
        public static readonly Null Value =
#pragma warning disable SA1129
            new Null();
#pragma warning restore SA1129

        /// <inheritdoc cref="IValue.Kind"/>
        [Pure]
        public ValueKind Kind => ValueKind.Null;

        /// <inheritdoc cref="IValue.EncodingLength"/>
        [Pure]
        public long EncodingLength => 1L;

        /// <inheritdoc cref="IValue.Inspection"/>
        [Obsolete("Deprecated in favour of " + nameof(Inspect) + "() method.")]
        public string Inspection => Inspect(true);

        public override int GetHashCode() => 0;

        int IComparable.CompareTo(object obj) => obj is Null ? 0 : -1;

        int IComparable<Null>.CompareTo(Null other) => 0;

        public override bool Equals(object obj)
        {
            return ReferenceEquals(null, obj) || obj is Null;
        }

        bool IEquatable<IValue>.Equals(IValue other) =>
            other is Null;

        bool IEquatable<Null>.Equals(Null other) => true;

        /// <inheritdoc cref="IValue.Inspect(bool)"/>
        public string Inspect(bool loadAll) => "null";

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString() =>
            $"{nameof(Bencodex)}.{nameof(Types)}.{nameof(Null)}";
    }
}
