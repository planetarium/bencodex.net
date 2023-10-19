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

        /// <summary>A human-readable representation for debugging.</summary>
        /// <returns>A human-readable representation.</returns>
        /// <remarks>This property is deprecated.  Use <see cref="Inspect(bool)"/>
        /// method instead.</remarks>
        [Obsolete("Deprecated in favour of " + nameof(Inspect) + "() method.")]
        string Inspection { get; }

        /// <summary>
        /// Gets a human-readable representation for debugging.
        /// <para>Unloaded values may be omitted.</para>
        /// </summary>
        /// <param name="loadAll">Load all unloaded values before showing them.  This option
        /// is applied to subtrees recursively.</param>
        /// <returns>A human-readable representation for debugging, which looks similar to Python's
        /// literal syntax.  However, if a value is a complex tree and contains any unloaded
        /// subvalues, these are omitted and their fingerprints are shown instead.</returns>
        string Inspect(bool loadAll);
    }
}
