using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;

namespace Bencodex.Types
{
    /// <summary>
    /// Represents Bencodex lists.
    /// </summary>
    [Pure]
    public sealed class List :
        IValue,
        IImmutableList<IValue>,
        IEquatable<List>
    {
        /// <summary>
        /// The empty list.
        /// </summary>
        public static readonly List Empty = new List(ImmutableArray<IValue>.Empty)
        {
            EncodingLength = 2L,
        };

        private readonly ImmutableArray<IValue> _values;
        private long _encodingLength = -1L;

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
            : this(elements.ToImmutableArray())
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(IEnumerable<Boolean> elements)
            : this(elements.Select(v => (IValue)v))
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(IEnumerable<Integer> elements)
            : this(elements.Select(v => (IValue)v))
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(IEnumerable<Binary> elements)
            : this(elements.Select(v => (IValue)v))
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(IEnumerable<Text> elements)
            : this(elements.Select(v => (IValue)v))
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(IEnumerable<bool> elements)
            : this(elements.Select(v => new Boolean(v)))
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(IEnumerable<short> elements)
            : this(elements.Select(v => new Integer(v)))
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(IEnumerable<ushort> elements)
            : this(elements.Select(v => new Integer(v)))
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(IEnumerable<int> elements)
            : this(elements.Select(v => new Integer(v)))
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(IEnumerable<uint> elements)
            : this(elements.Select(v => new Integer(v)))
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(IEnumerable<long> elements)
            : this(elements.Select(v => new Integer(v)))
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(IEnumerable<ulong> elements)
            : this(elements.Select(v => new Integer(v)))
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(IEnumerable<BigInteger> elements)
            : this(elements.Select(v => new Integer(v)))
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(IEnumerable<byte[]> elements)
            : this(elements.Select(v => new Binary(v)))
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(IEnumerable<ImmutableArray<byte>> elements)
            : this(elements.Select(v => new Binary(v)))
        {
        }

        /// <summary>
        /// Creates a <see cref="List"/> instance with <paramref name="elements"/>.
        /// </summary>
        /// <param name="elements">The element values to include.</param>
        public List(IEnumerable<string> elements)
            : this(elements.Select(v => new Text(v)))
        {
        }

        internal List(in ImmutableArray<IValue> values)
        {
            _values = values;
        }

        /// <inheritdoc cref="IValue.Kind"/>
        [Pure]
        public ValueKind Kind => ValueKind.List;

        /// <inheritdoc cref="IValue.EncodingLength"/>
        public long EncodingLength
        {
            get => _encodingLength < 2L
                ? _encodingLength = 1L
                        + _values.Sum(e => e.EncodingLength)
                        + 1L
                : _encodingLength;

            private set => _encodingLength = value;
        }

        /// <inheritdoc cref="IReadOnlyCollection{T}.Count"/>
        public int Count => _values.Length;

        /// <inheritdoc cref="IReadOnlyList{T}.this[int]"/>
        public IValue this[int index] => _values[index];

        public override bool Equals(object obj) => obj is List l && Equals(l);

        public bool Equals(IValue other) => other is List l && Equals(l);

        public bool Equals(List other)
        {
            if (Count == other.Count)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (!_values[i].Equals(other[i]))
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        IEnumerator<IValue> IEnumerable<IValue>.GetEnumerator()
        {
            foreach (IValue element in _values)
            {
                yield return element;
            }
        }

        /// <inheritdoc cref="object.GetHashCode()"/>
        public override int GetHashCode()
            => unchecked(_values.Aggregate(GetType().GetHashCode(), (accum, next)
                => (accum * 397) ^ next.GetHashCode()));

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable<IValue>)this).GetEnumerator();

        IImmutableList<IValue> IImmutableList<IValue>.Add(IValue value) =>
            Add(value);

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(IValue value) =>
            new List(_values.Add(value))
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
        public List Add(short value) =>
            Add(new Integer(value));

        /// <summary>Makes a copy of the list, and adds the specified <paramref name="value"/>
        /// to the end of the copied list.</summary>
        /// <param name="value">The value to add to the list.  It is automatically turned into
        /// a Bencodex <see cref="Integer"/> instance.</param>
        /// <returns>A new list with the value added.</returns>
        public List Add(ushort value) =>
            Add(new Integer(value));

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

        IImmutableList<IValue> IImmutableList<IValue>.AddRange(IEnumerable<IValue> items) =>
            new List(_values.AddRange(items));

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
                item,
                index,
                count,
                equalityComparer);

        IImmutableList<IValue> IImmutableList<IValue>.Insert(int index, IValue element) =>
            new List(_values.Insert(index, element));

        IImmutableList<IValue> IImmutableList<IValue>.InsertRange(
            int index,
            IEnumerable<IValue> items
        )
        {
            return new List(_values.InsertRange(index, items));
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
                item,
                index,
                count,
                equalityComparer);

        [Obsolete("This operation immediately loads all unloaded values on the memory.")]
        IImmutableList<IValue> IImmutableList<IValue>.Remove(
            IValue value,
            IEqualityComparer<IValue> equalityComparer
        ) =>
            new List(_values.Remove(value, equalityComparer));

        [Obsolete("This operation immediately loads all unloaded values on the memory.")]
        IImmutableList<IValue> IImmutableList<IValue>.RemoveAll(
            Predicate<IValue> match
        ) =>
            new List(_values.RemoveAll(iv => match(iv)));

        IImmutableList<IValue> IImmutableList<IValue>.RemoveAt(int index) =>
            new List(_values.RemoveAt(index));

        [Obsolete("This operation immediately loads all unloaded values on the memory.")]
        IImmutableList<IValue> IImmutableList<IValue>.RemoveRange(
            IEnumerable<IValue> items,
            IEqualityComparer<IValue> equalityComparer
        ) =>
            new List(_values.RemoveRange(items.Select(v => v), equalityComparer));

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
                    oldValue,
                    newValue,
                    equalityComparer)
                );

        IImmutableList<IValue> IImmutableList<IValue>.SetItem(int index, IValue value) =>
            new List(_values.SetItem(index, value));

        /// <inheritdoc cref="IValue.Inspect(bool)"/>
        public string Inspect(bool loadAll)
        {
            string InspectItem(IValue value, bool load) => value.Inspect(load);

            string inspection;
            switch (_values.Length)
            {
                case 0:
                    inspection = "[]";
                    break;

                case 1:
                    IValue first = _values[0];
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
    }
}
