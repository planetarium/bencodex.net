using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using Bencodex.Misc;

namespace Bencodex.Types
{
    /// <summary>
    /// Represents Bencodex lists.
    /// </summary>
    [Pure]
    public sealed class List :
        IValue,
        IImmutableList<IValue>,
        IEquatable<IImmutableList<IValue>>,
        IEquatable<List>
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

        private ImmutableArray<IndirectValue> _values;
        private IndirectValue.Loader? _loader;
        private long? _encodingLength;
        private ImmutableArray<byte>? _hash;

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(params IValue[] elements)
            : this((IEnumerable<IValue>)elements)
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(IEnumerable<IValue> elements)
            : this(elements.Select(v => new IndirectValue(v)).ToImmutableArray(), null)
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="indirectValues"/> and
        /// a <paramref name="loader"/> used for loading unloaded values.
        /// </summary>
        /// <param name="indirectValues">The loaded and unloaded values to include.</param>
        /// <param name="loader">The <see cref="IndirectValue.Loader"/> delegate invoked when
        /// unloaded values are needed.</param>
        public List(IEnumerable<IndirectValue> indirectValues, IndirectValue.Loader loader)
            : this(
                indirectValues is ImmutableArray<IndirectValue> ia
                    ? ia
                    : indirectValues.ToImmutableArray(),
                loader
            )
        {
        }

        private List(
            in ImmutableArray<IndirectValue> indirectValues,
            IndirectValue.Loader? loader = null
        )
        {
            _values = indirectValues;
            _loader = loader;
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
                if (_values.IsDefaultOrEmpty)
                {
                    return EmptyFingerprint;
                }

                if (!(_hash is { } hash))
                {
                    long encLength = 2;
                    SHA1 sha1 = SHA1.Create();
                    sha1.Initialize();
                    foreach (IndirectValue value in _values)
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
        public long EncodingLength =>
            _encodingLength is { } l ? l : (
                _encodingLength = _listPrefix.LongLength
                + _values.Sum(e => e.EncodingLength)
                + CommonVariables.Suffix.LongLength
            ).Value;

        /// <inheritdoc cref="IValue.Inspection"/>
        [Obsolete("Deprecated in favour of " + nameof(Inspect) + "() method.")]
        public string Inspection => Inspect(true);

        /// <inheritdoc cref="IReadOnlyCollection{T}.Count"/>
        public int Count => _values.Length;

        /// <inheritdoc cref="IReadOnlyList{T}.this[int]"/>
        public IValue this[int index] => _values[index].GetValue(_loader);

        bool IEquatable<IImmutableList<IValue>>.Equals(IImmutableList<IValue> other)
        {
            if (Count != other.Count)
            {
                return false;
            }
            else if (_hash is { } && other is List otherList && otherList._hash is { })
            {
                return Fingerprint.Equals(otherList.Fingerprint);
            }

            for (int i = 0; i < _values.Length; i++)
            {
                IndirectValue iv = _values[i];
                IValue ov = other[i];
                if (iv.LoadedValue is { } v)
                {
                    if (!ov.Equals(v))
                    {
                        return false;
                    }

                    continue;
                }

                if (!iv.Fingerprint.Equals(ov.Fingerprint))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Equals(List other) =>
            ((IEquatable<IImmutableList<IValue>>)this).Equals(other);

        bool IEquatable<IValue>.Equals(IValue other) =>
            other is List o && Equals(o);

        IEnumerator<IValue> IEnumerable<IValue>.GetEnumerator()
        {
            foreach (IndirectValue element in _values)
            {
                yield return element.GetValue(_loader);
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
        public override int GetHashCode() =>
            _values.GetHashCode();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable<IValue>)this).GetEnumerator();

        IImmutableList<IValue> IImmutableList<IValue>.Add(IValue value) =>
            Add(value);

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(IValue value) =>
            new List(_values.Add(new IndirectValue(value)));

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.  It is automatically turned into
        /// a Bencodex <see cref="Text"/> instance.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(string value) =>
            Add((IValue)new Text(value));

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.  It is automatically turned into
        /// a Bencodex <see cref="Boolean"/> instance.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(bool value) =>
            Add((IValue)new Boolean(value));

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.  It is automatically turned into
        /// a Bencodex <see cref="Integer"/> instance.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(BigInteger value) =>
            Add((IValue)new Integer(value));

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.  It is automatically turned into
        /// a Bencodex <see cref="Binary"/> instance.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(byte[] value) =>
            Add((IValue)new Binary(value));

        IImmutableList<IValue> IImmutableList<IValue>.AddRange(
            IEnumerable<IValue> items
        ) =>
            new List(_values.AddRange(items.Select(v => new IndirectValue(v))));

        IImmutableList<IValue> IImmutableList<IValue>.Clear() =>
            List.Empty;

        [Obsolete("This operation immediately loads all unloaded values in the range on " +
                  "the memory.")]
        int IImmutableList<IValue>.IndexOf(
            IValue item,
            int index,
            int count,
            IEqualityComparer<IValue> equalityComparer
        ) =>
            _values.IndexOf(
                new IndirectValue(item),
                index,
                count,
                new IndirectValueEqualityComparer(equalityComparer, _loader)
            );

        IImmutableList<IValue> IImmutableList<IValue>.Insert(int index, IValue element) =>
            new List(_values.Insert(index, new IndirectValue(element)));

        IImmutableList<IValue> IImmutableList<IValue>.InsertRange(
            int index,
            IEnumerable<IValue> items
        )
        {
            IEnumerable<IndirectValue> vs = items.Select(v => new IndirectValue(v));
            return new List(_values.InsertRange(index, vs));
        }

        [Obsolete("This operation immediately loads all unloaded values in the range on " +
                  "the memory.")]
        int IImmutableList<IValue>.LastIndexOf(
            IValue item,
            int index,
            int count,
            IEqualityComparer<IValue> equalityComparer
        ) =>
            _values.LastIndexOf(
                new IndirectValue(item),
                index,
                count,
                new IndirectValueEqualityComparer(equalityComparer, _loader)
            );

        [Obsolete("This operation immediately loads all unloaded values on the memory.")]
        IImmutableList<IValue> IImmutableList<IValue>.Remove(
            IValue value,
            IEqualityComparer<IValue> equalityComparer
        ) =>
            new List(
                _values.Remove(
                    new IndirectValue(value),
                    new IndirectValueEqualityComparer(equalityComparer, _loader)
                ),
                _loader
            );

        [Obsolete("This operation immediately loads all unloaded values on the memory.")]
        IImmutableList<IValue> IImmutableList<IValue>.RemoveAll(
            Predicate<IValue> match
        ) =>
            new List(_values.RemoveAll(iv => match(iv.GetValue(_loader))), _loader);

        IImmutableList<IValue> IImmutableList<IValue>.RemoveAt(int index) =>
            new List(_values.RemoveAt(index));

        [Obsolete("This operation immediately loads all unloaded values on the memory.")]
        IImmutableList<IValue> IImmutableList<IValue>.RemoveRange(
            IEnumerable<IValue> items,
            IEqualityComparer<IValue> equalityComparer
        ) =>
            new List(
                _values.RemoveRange(
                    items.Select(v => new IndirectValue(v)),
                    new IndirectValueEqualityComparer(equalityComparer, _loader)
                )
            );

        IImmutableList<IValue> IImmutableList<IValue>.RemoveRange(int index, int count) =>
            new List(_values.RemoveRange(index, count));

        [Obsolete("This operation immediately loads all unloaded values on the memory.")]
        IImmutableList<IValue> IImmutableList<IValue>.Replace(
            IValue oldValue,
            IValue newValue,
            IEqualityComparer<IValue> equalityComparer
        ) =>
            new List(
                _values.Replace(
                    new IndirectValue(oldValue),
                    new IndirectValue(newValue),
                    new IndirectValueEqualityComparer(equalityComparer, _loader)
                )
            );

        IImmutableList<IValue> IImmutableList<IValue>.SetItem(int index, IValue value) =>
            new List(_values.SetItem(index, new IndirectValue(value)));

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

        /// <inheritdoc cref="IValue.EncodeToStream(Stream)"/>
        public void EncodeToStream(Stream stream)
        {
            long startPos = stream.Position;
            stream.WriteByte(_listPrefix[0]);
            foreach (IValue element in this)
            {
                element.EncodeToStream(stream);
            }

            stream.WriteByte(CommonVariables.Suffix[0]);
            _encodingLength = (int?)((int)stream.Position - startPos);
        }

        /// <inheritdoc cref="IValue.Inspect(bool)"/>
        public string Inspect(bool loadAll)
        {
            string InspectItem(IndirectValue value, bool load) =>
                loadAll || value.LoadedValue is { }
                    ? value.GetValue(_loader).Inspect(load)
                    : value.Fingerprint.ToString();

            switch (_values.Length)
            {
                case 0:
                    return "[]";

                case 1:
                    IndirectValue first = _values[0];
                    if (first.Type == ValueType.List || first.Type == ValueType.Dictionary)
                    {
                        goto default;
                    }

                    return $"[{InspectItem(first, loadAll)}]";

                default:
                    IEnumerable<string> elements = _values.Select(v =>
                        $"  {InspectItem(v, loadAll).Replace("\n", "\n  ")},\n"
                    );
                    return $"[\n{string.Join(string.Empty, elements)}]";
            }
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString() =>
            $"{nameof(Bencodex)}.{nameof(Types)}.{nameof(List)} {Inspect(false)}";
    }
}
