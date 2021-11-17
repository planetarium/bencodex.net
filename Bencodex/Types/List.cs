using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace Bencodex.Types
{
    /// <summary>
    /// Represents Bencodex lists.
    /// </summary>
    public class List :
        IValue,
        IImmutableList<IValue>,
        IEquatable<IImmutableList<IValue>>
    {
        /// <summary>
        /// The empty list.
        /// </summary>
        public static readonly List Empty = new List(ImmutableArray<IValue>.Empty);

        /// <summary>
        /// The singleton fingerprint for empty lists.
        /// </summary>
        public static readonly Fingerprint EmptyFingerprint =
            new Fingerprint(ValueType.List, 2L);

        private static readonly byte[] _listPrefix = new byte[1] { 0x6c };  // 'l'

        private ImmutableArray<IValue> _value;
        private long? _encodingLength;
        private ImmutableArray<byte>? _hash;

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(params IValue[] elements)
            : this(ImmutableArray.Create(elements))
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(IEnumerable<IValue> elements)
            : this(elements.ToImmutableArray())
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(in ImmutableArray<IValue> elements)
        {
            _value = elements;
            _encodingLength = null;
            _hash = null;
        }

        /// <inheritdoc cref="IValue.Type"/>
        [Pure]
        public ValueType Type => ValueType.List;

        /// <inheritdoc cref="IValue.Fingerprint"/>
        [Pure]
        public Fingerprint Fingerprint
        {
            get
            {
                if (_value.IsDefaultOrEmpty)
                {
                    return EmptyFingerprint;
                }

                if (!(_hash is { } hash))
                {
                    long encLength = 2;
                    SHA1 sha1 = SHA1.Create();
                    sha1.Initialize();
                    foreach (IValue value in _value)
                    {
                        Fingerprint fp = value.Fingerprint;
                        byte[] fpb = fp.Serialize();
                        sha1.TransformBlock(fpb, 0, fpb.Length, null, 0);
                        encLength += fp.EncodingLength;
                    }

                    sha1.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                    hash = ImmutableArray.Create(sha1.Hash);
                    _hash = hash;
                    if (_encodingLength is null)
                    {
                        _encodingLength = encLength;
                    }
                }

                return new Fingerprint(Type, EncodingLength, hash);
            }
        }

        /// <inheritdoc cref="IValue.EncodingLength"/>
        [Pure]
        public long EncodingLength =>
            _encodingLength is { } l ? l : (
                _encodingLength = _listPrefix.LongLength
                    + _value.Sum(e => e.EncodingLength)
                    + CommonVariables.Suffix.LongLength
            ).Value;

        /// <inheritdoc cref="IValue.Inspection"/>
        [Obsolete("Deprecated in favour of " + nameof(Inspect) + "() method.")]
        public string Inspection => Inspect(true);

        public int Count => _value.Length;

        public IValue this[int index] => _value[index];

        bool IEquatable<IImmutableList<IValue>>.Equals(
            IImmutableList<IValue> other
        )
        {
            return _value.SequenceEqual(other);
        }

        bool IEquatable<IValue>.Equals(IValue other) =>
            other is List o &&
            ((IEquatable<IImmutableList<IValue>>)this).Equals(o);

        IEnumerator<IValue> IEnumerable<IValue>.GetEnumerator()
        {
            foreach (IValue element in _value)
            {
                yield return element;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is List other &&
                ((IEquatable<IImmutableList<IValue>>)this).Equals(other);
        }

        /// <inheritdoc cref="object.GetHashCode()"/>
        [Pure]
        public override int GetHashCode() =>
            _value.GetHashCode();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_value).GetEnumerator();
        }

        IImmutableList<IValue> IImmutableList<IValue>.Add(IValue value)
        {
            return new List(_value.Add(value));
        }

        public List Add(IValue value)
        {
            return new List(_value.Add(value));
        }

        public List Add(string value)
        {
            return new List(_value.Add((Text)value));
        }

        public List Add(bool value)
        {
            return new List(_value.Add((Boolean)value));
        }

        public List Add(BigInteger value)
        {
            return new List(_value.Add((Integer)value));
        }

        public List Add(byte[] value)
        {
            return new List(_value.Add((Binary)value));
        }

        IImmutableList<IValue> IImmutableList<IValue>.AddRange(
            IEnumerable<IValue> items
        )
        {
            return new List(_value.AddRange(items));
        }

        IImmutableList<IValue> IImmutableList<IValue>.Clear()
        {
            return new List(_value.Clear());
        }

        int IImmutableList<IValue>.IndexOf(
            IValue item,
            int index,
            int count,
            IEqualityComparer<IValue> equalityComparer
        )
        {
            return _value.IndexOf(item, index, count, equalityComparer);
        }

        IImmutableList<IValue> IImmutableList<IValue>.Insert(
            int index,
            IValue element
        )
        {
            return new List(_value.Insert(index, element));
        }

        IImmutableList<IValue> IImmutableList<IValue>.InsertRange(
            int index,
            IEnumerable<IValue> items
        )
        {
            return new List(_value.InsertRange(index, items));
        }

        int IImmutableList<IValue>.LastIndexOf(
            IValue item,
            int index,
            int count,
            IEqualityComparer<IValue> equalityComparer
        )
        {
            return _value.LastIndexOf(item, index, count, equalityComparer);
        }

        IImmutableList<IValue> IImmutableList<IValue>.Remove(
            IValue value,
            IEqualityComparer<IValue> equalityComparer
        )
        {
            return new List(_value.Remove(value, equalityComparer));
        }

        IImmutableList<IValue> IImmutableList<IValue>.RemoveAll(
            Predicate<IValue> match
        )
        {
            return new List(_value.RemoveAll(match));
        }

        IImmutableList<IValue> IImmutableList<IValue>.RemoveAt(int index)
        {
            return new List(_value.RemoveAt(index));
        }

        IImmutableList<IValue> IImmutableList<IValue>.RemoveRange(
            IEnumerable<IValue> items,
            IEqualityComparer<IValue> equalityComparer
        )
        {
            return new List(_value.RemoveRange(items, equalityComparer));
        }

        IImmutableList<IValue> IImmutableList<IValue>.RemoveRange(
            int index,
            int count
        )
        {
            return new List(_value.RemoveRange(index, count));
        }

        IImmutableList<IValue> IImmutableList<IValue>.Replace(
            IValue oldValue,
            IValue newValue,
            IEqualityComparer<IValue> equalityComparer
        )
        {
            return new List(
                _value.Replace(oldValue, newValue, equalityComparer)
            );
        }

        IImmutableList<IValue> IImmutableList<IValue>.SetItem(
            int index,
            IValue value
        )
        {
            return new List(_value.SetItem(index, value));
        }

        [Pure]
        public IEnumerable<byte[]> EncodeIntoChunks()
        {
            int length = _listPrefix.Length;
            yield return _listPrefix;
            foreach (IValue element in this)
            {
                foreach (byte[] chunk in element.EncodeIntoChunks())
                {
                    yield return chunk;
                    length += chunk.Length;
                }
            }

            yield return CommonVariables.Suffix;
            length += CommonVariables.Suffix.Length;
            _encodingLength = length;
        }

        public void EncodeToStream(Stream stream)
        {
            long startPos = stream.Position;
            stream.WriteByte(_listPrefix[0]);
            foreach (IValue element in _value)
            {
                element.EncodeToStream(stream);
            }

            stream.WriteByte(CommonVariables.Suffix[0]);
            _encodingLength = (int?)((int)stream.Position - startPos);
        }

        /// <inheritdoc cref="IValue.Inspect(bool)"/>
        public string Inspect(bool loadAll)
        {
            switch (_value.Length)
            {
                case 0:
                    return "[]";

                case 1:
                    var el = this.First();
                    if (el is List || el is Dictionary)
                    {
                        goto default;
                    }

                    return $"[{el.Inspect(loadAll)}]";

                default:
                    IEnumerable<string> elements = this.Select(v =>
                        $"  {v.Inspect(loadAll).Replace("\n", "\n  ")},\n"
                    );
                    return $"[\n{string.Join(string.Empty, elements)}]";
            }
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString() =>
            $"{nameof(Bencodex)}.{nameof(Types)}.{nameof(List)} {Inspect(false)}";
    }
}
