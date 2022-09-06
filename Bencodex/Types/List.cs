using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
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
        public static readonly List Empty = new List(ImmutableArray<IndirectValue>.Empty, null)
        {
            EncodingLength = 2L,
        };

        /// <summary>
        /// The singleton fingerprint for empty lists.
        /// </summary>
        public static readonly Fingerprint EmptyFingerprint =
            new Fingerprint(ValueKind.List, 2L);

        private readonly ImmutableArray<IndirectValue> _values;
        private long _encodingLength = -1L;
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

        internal List(
            in ImmutableArray<IndirectValue> indirectValues,
            IndirectValue.Loader? loader
        )
        {
            _values = indirectValues;
            Loader = loader;
            _hash = null;
        }

        /// <inheritdoc cref="IValue.Kind"/>
        [Pure]
        public ValueKind Kind => ValueKind.List;

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
                    if (_encodingLength < 0L)
                    {
                        _encodingLength = encLength;
                    }
                }

                return new Fingerprint(Kind, EncodingLength, hash);
            }
        }

        /// <inheritdoc cref="IValue.EncodingLength"/>
        public long EncodingLength
        {
            get => _encodingLength < 2L
                ? _encodingLength = 1L
                        + _values.Sum(e => e.EncodingLength)
                        + 1L
                : _encodingLength;

            internal set => _encodingLength = value;
        }

        /// <inheritdoc cref="IValue.Inspection"/>
        [Obsolete("Deprecated in favour of " + nameof(Inspect) + "() method.")]
        public string Inspection => Inspect(true);

        /// <inheritdoc cref="IReadOnlyCollection{T}.Count"/>
        public int Count => _values.Length;

        internal IndirectValue.Loader? Loader { get; }

        /// <inheritdoc cref="IReadOnlyList{T}.this[int]"/>
        public IValue this[int index] => _values[index].GetValue(Loader);

        bool IEquatable<IImmutableList<IValue>>.Equals(IImmutableList<IValue> other)
        {
            if (Count != other.Count)
            {
                return false;
            }
            else if (other is List otherList)
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

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public bool Equals(List other) => Fingerprint.Equals(other.Fingerprint);

        bool IEquatable<IValue>.Equals(IValue other) =>
            other is List o && Equals(o);

        IEnumerator<IValue> IEnumerable<IValue>.GetEnumerator()
        {
            foreach (IndirectValue element in _values)
            {
                yield return element.GetValue(Loader);
            }
        }

        /// <inheritdoc cref="object.Equals(object?)"/>
        public override bool Equals(object? obj) => obj is List other &&
            ((IEquatable<IImmutableList<IValue>>)this).Equals(other);

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
            new List(_values.Add(new IndirectValue(value)), Loader)
            {
                EncodingLength = _encodingLength < 2L ? -1 : _encodingLength + value.EncodingLength,
            };

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(Boolean value) =>
            Add((IValue)value);

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(Integer value) =>
            Add((IValue)value);

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(Binary value) =>
            Add((IValue)value);

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(Text value) =>
            Add((IValue)value);

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.  It is automatically turned into
        /// a Bencodex <see cref="Boolean"/> instance.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(bool value) =>
            Add(new Boolean(value));

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.  It is automatically turned into
        /// a Bencodex <see cref="Integer"/> instance.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(int value) =>
            Add(new Integer(value));

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.  It is automatically turned into
        /// a Bencodex <see cref="Integer"/> instance.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(uint value) =>
            Add(new Integer(value));

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.  It is automatically turned into
        /// a Bencodex <see cref="Integer"/> instance.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(long value) =>
            Add(new Integer(value));

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.  It is automatically turned into
        /// a Bencodex <see cref="Integer"/> instance.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(ulong value) =>
            Add(new Integer(value));

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.  It is automatically turned into
        /// a Bencodex <see cref="Integer"/> instance.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(BigInteger value) =>
            Add(new Integer(value));

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.  It is automatically turned into
        /// a Bencodex <see cref="Binary"/> instance.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(byte[] value) =>
            Add(new Binary(value));

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.  It is automatically turned into
        /// a Bencodex <see cref="Binary"/> instance.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(ImmutableArray<byte> value) =>
            Add(new Binary(value));

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.  It is automatically turned into
        /// a Bencodex <see cref="Text"/> instance.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(string value) =>
            Add(new Text(value));

        IImmutableList<IValue> IImmutableList<IValue>.AddRange(
            IEnumerable<IValue> items
        ) =>
            new List(_values.AddRange(items.Select(v => new IndirectValue(v))), Loader);

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
                new IndirectValueEqualityComparer(equalityComparer, Loader)
            );

        IImmutableList<IValue> IImmutableList<IValue>.Insert(int index, IValue element) =>
            new List(_values.Insert(index, new IndirectValue(element)), Loader);

        IImmutableList<IValue> IImmutableList<IValue>.InsertRange(
            int index,
            IEnumerable<IValue> items
        )
        {
            IEnumerable<IndirectValue> vs = items.Select(v => new IndirectValue(v));
            return new List(_values.InsertRange(index, vs), Loader);
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
                new IndirectValueEqualityComparer(equalityComparer, Loader)
            );

        [Obsolete("This operation immediately loads all unloaded values on the memory.")]
        IImmutableList<IValue> IImmutableList<IValue>.Remove(
            IValue value,
            IEqualityComparer<IValue> equalityComparer
        ) =>
            new List(
                _values.Remove(
                    new IndirectValue(value),
                    new IndirectValueEqualityComparer(equalityComparer, Loader)
                ),
                Loader
            );

        [Obsolete("This operation immediately loads all unloaded values on the memory.")]
        IImmutableList<IValue> IImmutableList<IValue>.RemoveAll(
            Predicate<IValue> match
        ) =>
            new List(_values.RemoveAll(iv => match(iv.GetValue(Loader))), Loader);

        IImmutableList<IValue> IImmutableList<IValue>.RemoveAt(int index) =>
            new List(_values.RemoveAt(index), Loader);

        [Obsolete("This operation immediately loads all unloaded values on the memory.")]
        IImmutableList<IValue> IImmutableList<IValue>.RemoveRange(
            IEnumerable<IValue> items,
            IEqualityComparer<IValue> equalityComparer
        ) =>
            new List(
                _values.RemoveRange(
                    items.Select(v => new IndirectValue(v)),
                    new IndirectValueEqualityComparer(equalityComparer, Loader)
                ),
                Loader
            );

        IImmutableList<IValue> IImmutableList<IValue>.RemoveRange(int index, int count) =>
            new List(_values.RemoveRange(index, count), Loader);

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
                    new IndirectValueEqualityComparer(equalityComparer, Loader)
                ),
                Loader
            );

        IImmutableList<IValue> IImmutableList<IValue>.SetItem(int index, IValue value) =>
            new List(_values.SetItem(index, new IndirectValue(value)), Loader);

        /// <inheritdoc cref="IValue.Inspect(bool)"/>
        public string Inspect(bool loadAll)
        {
            string InspectItem(IndirectValue value, bool load) =>
                loadAll || value.LoadedValue is { }
                    ? value.GetValue(Loader).Inspect(load)
                    : value.Fingerprint.ToString();

            string inspection;
            switch (_values.Length)
            {
                case 0:
                    inspection = "[]";
                    break;

                case 1:
                    IndirectValue first = _values[0];
                    if (first.Kind == ValueKind.List || first.Kind == ValueKind.Dictionary)
                    {
                        goto default;
                    }

                    inspection = $"[{InspectItem(first, loadAll)}]";
                    break;

                default:
                    IEnumerable<string> elements = _values.Select(v =>
                        $"  {InspectItem(v, loadAll).Replace("\n", "\n  ")},\n"
                    );
                    inspection = $"[\n{string.Join(string.Empty, elements)}]";
                    break;
            }

            return inspection;
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString() =>
            $"{nameof(Bencodex)}.{nameof(Types)}.{nameof(List)} {Inspect(false)}";

        /// <summary>
        /// Enumerates <see cref="IndirectValue"/>s in the list.
        /// </summary>
        /// <returns>An enumerable of <see cref="IndirectValue"/>s, which can be either loaded or
        /// offloaded.</returns>
        internal IEnumerable<IndirectValue> EnumerateIndirectValues() => _values;
    }
}
