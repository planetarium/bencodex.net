using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Bencodex.Types
{
    public struct List :
        IValue,
        IImmutableList<IValue>,
        IEquatable<IImmutableList<IValue>>
    {
        private ImmutableArray<IValue> _value;

        public List(IEnumerable<IValue> value)
        {
            _value = value?.ToImmutableArray() ?? ImmutableArray<IValue>.Empty;
        }

        public ImmutableArray<IValue> Value =>
            _value.IsDefault ? (_value = ImmutableArray<IValue>.Empty) : _value;

        int IReadOnlyCollection<IValue>.Count => Value.Length;

        IValue IReadOnlyList<IValue>.this[int index] => Value[index];

        bool IEquatable<IImmutableList<IValue>>.Equals(
            IImmutableList<IValue> other
        )
        {
            return Value.SequenceEqual(other);
        }

        IEnumerator<IValue> IEnumerable<IValue>.GetEnumerator()
        {
            foreach (IValue element in Value)
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

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Value).GetEnumerator();
        }

        IImmutableList<IValue> IImmutableList<IValue>.Add(IValue value)
        {
            return new List(Value.Add(value));
        }

        IImmutableList<IValue> IImmutableList<IValue>.AddRange(
            IEnumerable<IValue> items
        )
        {
            return new List(Value.AddRange(items));
        }

        IImmutableList<IValue> IImmutableList<IValue>.Clear()
        {
            return new List(Value.Clear());
        }

        int IImmutableList<IValue>.IndexOf(
            IValue item,
            int index,
            int count,
            IEqualityComparer<IValue> equalityComparer
        )
        {
            return Value.IndexOf(item, index, count, equalityComparer);
        }

        IImmutableList<IValue> IImmutableList<IValue>.Insert(
            int index,
            IValue element
        )
        {
            return new List(Value.Insert(index, element));
        }

        IImmutableList<IValue> IImmutableList<IValue>.InsertRange(
            int index,
            IEnumerable<IValue> items
        )
        {
            return new List(Value.InsertRange(index, items));
        }

        int IImmutableList<IValue>.LastIndexOf(
            IValue item,
            int index,
            int count,
            IEqualityComparer<IValue> equalityComparer
        )
        {
            return Value.LastIndexOf(item, index, count, equalityComparer);
        }

        IImmutableList<IValue> IImmutableList<IValue>.Remove(
            IValue value,
            IEqualityComparer<IValue> equalityComparer
        )
        {
            return new List(Value.Remove(value, equalityComparer));
        }

        IImmutableList<IValue> IImmutableList<IValue>.RemoveAll(
            Predicate<IValue> match
        )
        {
            return new List(Value.RemoveAll(match));
        }

        IImmutableList<IValue> IImmutableList<IValue>.RemoveAt(int index)
        {
            return new List(Value.RemoveAt(index));
        }

        IImmutableList<IValue> IImmutableList<IValue>.RemoveRange(
            IEnumerable<IValue> items,
            IEqualityComparer<IValue> equalityComparer
        )
        {
            return new List(Value.RemoveRange(items, equalityComparer));
        }

        IImmutableList<IValue> IImmutableList<IValue>.RemoveRange(
            int index,
            int count
        )
        {
            return new List(Value.RemoveRange(index, count));
        }

        IImmutableList<IValue> IImmutableList<IValue>.Replace(
            IValue oldValue,
            IValue newValue,
            IEqualityComparer<IValue> equalityComparer
        )
        {
            return new List(
                Value.Replace(oldValue, newValue, equalityComparer)
            );
        }

        IImmutableList<IValue> IImmutableList<IValue>.SetItem(
            int index,
            IValue value
        )
        {
            return new List(Value.SetItem(index, value));
        }

        [Pure]
        public IEnumerable<byte[]> EncodeIntoChunks()
        {
            yield return new byte[1] { 0x6c }; // 'l'
            foreach (IValue element in this)
            {
                foreach (byte[] chunk in element.EncodeIntoChunks())
                {
                    yield return chunk;
                }
            }

            yield return new byte[1] { 0x65 };  // 'e'
        }

        [Pure]
        public override string ToString()
        {
            IEnumerable<string> elements = this.Select(v => v.ToString());
            string elementsString = string.Join(", ", elements);
            return $"[{elementsString}]";
        }
    }
}
