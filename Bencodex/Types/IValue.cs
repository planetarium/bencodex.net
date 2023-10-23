using System;
using System.Diagnostics.Contracts;

namespace Bencodex.Types
{
    /// <summary>Represents a possible value in Bencodex representation.
    /// </summary>
    /// <seealso cref="Null"/>
    /// <seealso cref="Boolean"/>
    /// <seealso cref="Integer"/>
    /// <seealso cref="Binary"/>
    /// <seealso cref="Text"/>
    /// <seealso cref="List"/>
    /// <seealso cref="Dictionary"/>
    public interface IValue : IEquatable<IValue>
    {
        /// <summary>
        /// The Bencodex type identifier.
        /// </summary>
        [Pure]
        ValueKind Kind { get; }

        /// <summary>The number of bytes used for serializing the value.</summary>
        [Pure]
        long EncodingLength { get; }

        /// <summary>
        /// Gets a human-readable representation for debugging.
        /// </summary>
        /// <returns>A human-readable representation for debugging, which looks similar to Python's
        /// literal syntax.</returns>
        string Inspect();
    }
}
