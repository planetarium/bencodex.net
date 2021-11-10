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
    public struct List :
        IValue,
        IImmutableList<IValue>,
        IEquatable<IImmutableList<IValue>>
    {
        /// <summary>
        /// The empty list.
        /// </summary>
        public static readonly List Empty = default;

        /// <summary>
        /// The singleton fingerprint for empty lists.
        /// </summary>
        public static readonly Fingerprint EmptyFingerprint =
            new Fingerprint(ValueType.List, 2);

        private static readonly byte[] _listPrefix = new byte[1] { 0x6c };  // 'l'

        private ImmutableArray<IValue> _value;
        private int? _encodingLength;
        private ImmutableArray<byte>? _hash;

        public List(IEnumerable<IValue> value)
        {
            _value = value?.ToImmutableArray() ?? ImmutableArray<IValue>.Empty;
            _encodingLength = null;
            _hash = null;
        }

        public ImmutableArray<IValue> Value =>
            _value.IsDefault ? (_value = ImmutableArray<IValue>.Empty) : _value;

        /// <inheritdoc cref="IValue.Type"/>
        [Pure]
        public ValueType Type => ValueType.List;

        /// <inheritdoc cref="IValue.Fingerprint"/>
        [Pure]
        public Fingerprint Fingerprint
        {
            get
            {
                if (!(_value is { } els) || els.IsDefaultOrEmpty)
                {
                    return EmptyFingerprint;
                }

                if (!(_hash is { } hash))
                {
                    int encLength = 2;
                    SHA1 sha1 = SHA1.Create();
                    sha1.Initialize();
                    foreach (IValue value in els)
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
        public int EncodingLength =>
            _encodingLength is { } l ? l : (
                _encodingLength = _listPrefix.Length
                    + Value.Sum(e => e.EncodingLength)
                    + CommonVariables.Suffix.Length
            ).Value;

        /// <inheritdoc cref="IValue.Inspection"/>
        [Pure]
        public string Inspection
        {
            get
            {
                switch (Value.Length)
                {
                    case 0:
                        return "[]";

                    case 1:
                        var el = this.First();
                        if (el is List || el is Dictionary)
                        {
                            goto default;
                        }

                        return $"[{el.Inspection}]";

                    default:
                        IEnumerable<string> elements = this.Select(v =>
                            $"  {v.Inspection.Replace("\n", "\n  ")},\n"
                        );
                        return $"[\n{string.Join(string.Empty, elements)}]";
                }
            }
        }

        public int Count => Value.Length;

        public IValue this[int index] => Value[index];

        bool IEquatable<IImmutableList<IValue>>.Equals(
            IImmutableList<IValue> other
        )
        {
            return Value.SequenceEqual(other);
        }

        bool IEquatable<IValue>.Equals(IValue other) =>
            other is List o &&
            ((IEquatable<IImmutableList<IValue>>)this).Equals(o);

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

        public List Add(IValue value)
        {
            return new List(Value.Add(value));
        }

        public List Add(string value)
        {
            return new List(Value.Add((Text)value));
        }

        public List Add(bool value)
        {
            return new List(Value.Add((Boolean)value));
        }

        public List Add(BigInteger value)
        {
            return new List(Value.Add((Integer)value));
        }

        public List Add(byte[] value)
        {
            return new List(Value.Add((Binary)value));
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
            foreach (IValue element in Value)
            {
                element.EncodeToStream(stream);
            }

            stream.WriteByte(CommonVariables.Suffix[0]);
            _encodingLength = (int?)((int)stream.Position - startPos);
        }

        [Pure]
        public override string ToString() =>
            $"{nameof(Bencodex)}.{nameof(Bencodex.Types)}.{nameof(List)} {Inspection}";
    }
}
