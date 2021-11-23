using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

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

        /// <summary>
        /// A unique identifier of the value.  Can be used for efficient determining of two values
        /// that may be a deep tree.
        /// </summary>
        [Pure]
        Fingerprint Fingerprint { get; }

        /// <summary>The number of bytes used for serializing the value.</summary>
        [Pure]
        long EncodingLength { get; }

        /// <summary>A human-readable representation for debugging.</summary>
        /// <returns>A human-readable representation.</returns>
        /// <remarks>This property is deprecated.  Use <see cref="Inspect(bool)"/>
        /// method instead.</remarks>
        [Obsolete("Deprecated in favour of " + nameof(Inspect) + "() method.")]
        string Inspection { get; }

        /// <summary>Encodes the value into <c cref="byte">Byte</c>
        /// arrays.</summary>
        /// <returns><c cref="byte">Byte</c> arrays of Bencodex
        /// representation of the value.</returns>
        /// <seealso cref="Codec.Encode(IValue)"/>
        /// <seealso cref="Codec.Encode(IValue, System.IO.Stream)"/>
        [Pure]
        IEnumerable<byte[]> EncodeIntoChunks();

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
