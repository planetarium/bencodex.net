using System;
using System.Diagnostics.Contracts;
using Bencodex.Types;

namespace Bencodex
{
    /// <summary>
    /// Defines a generic <see langword="interface"/> for an object that can be
    /// encoded into an <see cref="IValue"/>.  Any <see langword="class"/> implementing
    /// this <see langword="interface"/> should also implement either a constructor
    /// or a <see langword="static"/> factory method with an <see cref="IValue"/> as a parameter
    /// for decoding.
    /// </summary>
    /// <remarks>
    /// Note that encoding and decoding mentioned here are different from
    /// <see cref="Codec.Encode(IValue)"/> and <see cref="Codec.Decode(byte[])"/>.
    /// </remarks>
    /// <example>
    /// The following example shows an implementation of an integer point <see langword="class"/>
    /// with two distinct methods of decoding, via a constructor and a <see langword="static"/>
    /// factory method, for illustration:
    /// <code><![CDATA[
    /// public class Point : IBencodable
    /// {
    ///     public Point(int x, int y)
    ///     {
    ///         X = x;
    ///         Y = y;
    ///     }
    ///
    ///     public Point(IValue bencoded)
    ///         : this((List)bencoded)
    ///     {
    ///     }
    ///
    ///     private Point(List bencoded)
    ///         : this((Integer)bencoded[0], (Integer)bencoded[1])
    ///     {
    ///     }
    ///
    ///     public int X { get; }
    ///
    ///     public int Y { get; }
    ///
    ///     public static Point Decode(IValue bencoded)
    ///     {
    ///         return bencoded is List list
    ///             ? new Point((Integer)list[0], (Integer)list[1])
    ///             : throw new ArgumentException(
    ///                 $"Invalid type: {bencoded.GetType()}",
    ///                 nameof(bencoded));
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    public interface IBencodable
    {
        /// <summary>
        /// An <see cref="IValue"/> representation of this object that can be
        /// decoded back to instantiate an equal object.  The decoded object must
        /// be equal to the original in the sense that <see cref="IEquatable{T}.Equals(T)"/>
        /// should be <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that the only requirement is that the produced <see cref="IValue"/>
        /// can be decoded back to an equal object.  This representation may not be canonical
        /// in the sense that additional junk data may be present in an <see cref="IValue"/>
        /// that one may wish to decode and this may be discarded while decoding.
        /// </para>
        /// <para>
        /// A specific implemnetation may decide to only allow the canonical representation
        /// to be decoded.
        /// </para>
        /// </remarks>
        [Pure]
        public IValue Bencoded { get; }
    }
}
