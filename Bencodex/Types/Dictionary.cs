using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Bencodex.Misc;

namespace Bencodex.Types
{
    public struct Dictionary :
        IValue,
        IEquatable<Dictionary>,
        IImmutableDictionary<IKey, IValue>
    {
        private ImmutableDictionary<IKey, IValue> _value;

        private ImmutableDictionary<IKey, IValue> Value =>
            _value ?? (_value = ImmutableDictionary<IKey, IValue>.Empty);

        public Dictionary(IEnumerable<KeyValuePair<IKey, IValue>> value = null)
        {
            _value = value == null
                ? ImmutableDictionary<IKey, IValue>.Empty
                : value.ToImmutableDictionary();
        }

        public IEnumerator<KeyValuePair<IKey, IValue>> GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) Value).GetEnumerator();
        }

        public int Count => Value.Count;

        public bool ContainsKey(IKey key)
        {
            return Value.ContainsKey(key);
        }

        public bool TryGetValue(IKey key, out IValue value)
        {
            return Value.TryGetValue(key, out value);
        }

        public IValue this[IKey key] => Value[key];

        public IValue this[string key] => Value[(Text) key];

        public IValue this[byte[] key] => Value[(Binary) key];

        public IEnumerable<IKey> Keys => Value.Keys;

        public IEnumerable<IValue> Values => Value.Values;

        public IImmutableDictionary<IKey, IValue> Add(IKey key, IValue value)
        {
            return new Dictionary(Value.Add(key, value));
        }

        public Dictionary Add(string key, IValue value)
        {
            return (Dictionary)Add((IKey)(Text) key, value);
        }

        public Dictionary Add(string key, string value)
        {
            return Add(key, (IValue)(Text) value);
        }

        public Dictionary Add(string key, long value)
        {
            return Add(key, (IValue)(Integer) value);
        }

        public Dictionary Add(string key, ulong value)
        {
            return Add(key, (IValue)(Integer) value);
        }

        public Dictionary Add(string key, byte[] value)
        {
            return Add(key, (IValue)(Binary) value);
        }

        public Dictionary Add(string key, bool value)
        {
            return Add(key, (IValue)(Bencodex.Types.Boolean) value);
        }

        public Dictionary Add(string key, IEnumerable<IValue> value)
        {
            return Add(key, (IValue)new Bencodex.Types.List(value));
        }

        public Dictionary Add(byte[] key, IValue value)
        {
            return (Dictionary)Add((IKey)(Binary) key, value);
        }

        public Dictionary Add(byte[] key, string value)
        {
            return Add(key, (IValue)(Text) value);
        }

        public Dictionary Add(byte[] key, long value)
        {
            return Add(key, (IValue)(Integer) value);
        }

        public Dictionary Add(byte[] key, ulong value)
        {
            return Add(key, (IValue)(Integer) value);
        }

        public Dictionary Add(byte[] key, byte[] value)
        {
            return Add(key, (IValue)(Binary) value);
        }

        public Dictionary Add(byte[] key, bool value)
        {
            return Add(key, (IValue)(Bencodex.Types.Boolean) value);
        }

        public Dictionary Add(byte[] key, IEnumerable<IValue> value)
        {
            return Add(key, (IValue)(Bencodex.Types.List) value);
        }

        public IImmutableDictionary<IKey, IValue> AddRange(
            IEnumerable<KeyValuePair<IKey, IValue>> pairs
        )
        {
            return new Dictionary(Value.AddRange(pairs));
        }

        public IImmutableDictionary<IKey, IValue> Clear()
        {
            return new Dictionary(Value.Clear());
        }

        public bool Contains(KeyValuePair<IKey, IValue> pair)
        {
            return Value.Contains(pair);
        }

        public IImmutableDictionary<IKey, IValue> Remove(IKey key)
        {
            return new Dictionary(Value.Remove(key));
        }

        public IImmutableDictionary<IKey, IValue> RemoveRange(
            IEnumerable<IKey> keys
        )
        {
            return new Dictionary(Value.RemoveRange(keys));
        }

        public IImmutableDictionary<IKey, IValue> SetItem(
            IKey key,
            IValue value
        )
        {
            return new Dictionary(Value.SetItem(key, value));
        }

        public Dictionary SetItem(
            IKey key,
            string value
        )
        {
            return (Dictionary)SetItem(key, (IValue)(Text)value);
        }

        public Dictionary SetItem(
            IKey key,
            byte[] value
        )
        {
            return (Dictionary)SetItem(key, (IValue)(Binary)value);
        }

        public Dictionary SetItem(
            IKey key,
            long value
        )
        {
            return (Dictionary)SetItem(key, (IValue)(Integer)value);
        }

        public Dictionary SetItem(
            IKey key,
            ulong value
        )
        {
            return (Dictionary)SetItem(key, (IValue)(Integer)value);
        }

        public Dictionary SetItem(
            IKey key,
            bool value
        )
        {
            return (Dictionary)SetItem(key, (IValue)(Bencodex.Types.Boolean)value);
        }

        public Dictionary SetItem(
            IKey key,
            IEnumerable<IValue> value
        )
        {
            return (Dictionary)SetItem(key, (IValue)new Bencodex.Types.List(value));
        }

        public Dictionary SetItem(
            string key,
            IValue value
        )
        {
            return (Dictionary)SetItem((IKey)(Text) key, value);
        }

        public Dictionary SetItem(
            string key,
            string value
        )
        {
            return SetItem(key, (IValue)(Text)value);
        }

        public Dictionary SetItem(
            string key,
            byte[] value
        )
        {
            return SetItem(key, (IValue)(Binary)value);
        }

        public Dictionary SetItem(
            string key,
            long value
        )
        {
            return SetItem(key, (IValue)(Integer)value);
        }

        public Dictionary SetItem(
            string key,
            ulong value
        )
        {
            return SetItem(key, (IValue)(Integer)value);
        }

        public Dictionary SetItem(
            string key,
            bool value
        )
        {
            return SetItem(key, (IValue)(Bencodex.Types.Boolean)value);
        }

        public Dictionary SetItem(
            string key,
            IEnumerable<IValue> value
        )
        {
            return SetItem(key, (IValue)new Bencodex.Types.List(value));
        }

        public Dictionary SetItem(
            byte[] key,
            IValue value
        )
        {
            return (Dictionary) SetItem((IKey) (Binary) key, value);
        }

        public Dictionary SetItem(
            byte[] key,
            string value
        )
        {
            return SetItem(key, (IValue)(Text)value);
        }

        public Dictionary SetItem(
            byte[] key,
            byte[] value
        )
        {
            return SetItem(key, (IValue)(Binary)value);
        }

        public Dictionary SetItem(
            byte[] key,
            long value
        )
        {
            return SetItem(key, (IValue)(Integer)value);
        }

        public Dictionary SetItem(
            byte[] key,
            ulong value
        )
        {
            return SetItem(key, (IValue)(Integer)value);
        }

        public Dictionary SetItem(
            byte[] key,
            bool value
        )
        {
            return SetItem(key, (IValue)(Bencodex.Types.Boolean)value);
        }

        public Dictionary SetItem(
            byte[] key,
            IEnumerable<IValue> value
        )
        {
            return SetItem(key, (IValue)new Bencodex.Types.List(value));
        }

        public IImmutableDictionary<IKey, IValue> SetItems(
            IEnumerable<KeyValuePair<IKey, IValue>> items
        )
        {
            return new Dictionary(Value.SetItems(items));
        }

        public bool TryGetKey(IKey equalKey, out IKey actualKey)
        {
            return Value.TryGetKey(equalKey, out actualKey);
        }

        public T GetValue<T>(string name)
            where T : IValue
        {
            return (T)this[name];
        }

        public static Dictionary Empty => new Dictionary();

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null:
                    return false;
                case Dictionary d:
                    return (
                        (IEquatable<Dictionary>) this
                    ).Equals(d);
                default:
                    return false;
            }
        }

        bool IEquatable<Dictionary>.Equals(
            Dictionary other
        )
        {
            if (Value.LongCount() != other.Value.LongCount()) return false;
            foreach (KeyValuePair<IKey, IValue> kv in this)
            {
                if (!other.ContainsKey(kv.Key)) return false;
                if (!other[kv.Key].Equals(kv.Value)) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

        private static IComparer<ValueTuple<byte?, byte[]>> keyPairComparer =
            new CompositeComparer<byte?, byte[]>(
                Comparer<byte?>.Default,
                new ByteArrayComparer()
            );

        [Pure]
        public IEnumerable<byte[]> EncodeIntoChunks()
        {
            yield return new byte[1] { 0x64 }; // 'd'
            IEnumerable<ValueTuple<byte?, byte[], IValue>> rawPairs =
                Value.Select(pair => (pair.Key.KeyPrefix,
                    pair.Key.EncodeAsByteArray(), pair.Value));
            IEnumerable<ValueTuple<byte?, byte[], IValue>> orderedPairs =
                rawPairs.OrderBy(
                    (triple) => ValueTuple.Create(triple.Item1, triple.Item2),
                    keyPairComparer
                );
            foreach ((byte? keyPrefix, byte[] key, IValue value) in orderedPairs)
            {
                if (keyPrefix != null)
                {
                    yield return new byte[1] { 0x75 }; // 'u'
                }
                yield return Encoding.ASCII.GetBytes(
                    key.Length.ToString()
                );
                yield return new byte[1] { 0x3a }; // ':'
                yield return key;
                foreach (byte[] chunk in value.EncodeIntoChunks())
                {
                    yield return chunk;
                }
            }
            yield return new byte[1] { 0x65 }; // 'e'
        }

        [Pure]
        public override string ToString()
        {
            IEnumerable<string> pairs = Value.Select(
                kv => $"{kv.Key}: {kv.Value}"
            );
            string pairsString = string.Join(", ", pairs);
            return $"{{{pairsString}}}";
        }
    }
}
