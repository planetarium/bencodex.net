using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Bencodex.Types
{
    /// <summary>Represents a Bencodex null value (i.e., <c>n</c>).</summary>
    public struct Null :
        IValue,
        IEquatable<Null>,
        IComparable<Null>,
        IComparable
    {
        [Pure]
        public string Inspection => $"null";

        public override int GetHashCode() => 0;

        int IComparable.CompareTo(object obj) => obj is Null ? 0 : -1;

        int IComparable<Null>.CompareTo(Null other) => 0;

        public override bool Equals(object obj)
        {
            return ReferenceEquals(null, obj) || obj is Null;
        }

        bool IEquatable<Null>.Equals(Null other) => true;

        [Pure]
        public IEnumerable<byte[]> EncodeIntoChunks()
        {
            yield return new byte[1] { 0x6e }; // 'n'
        }

        [Pure]
        public override string ToString() =>
            $"{nameof(Bencodex)}.{nameof(Bencodex.Types)}.{nameof(Null)}";
    }
}
