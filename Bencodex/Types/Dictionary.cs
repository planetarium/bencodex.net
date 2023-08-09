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
    /// Represents Bencodex dictionaries.
    /// </summary>
    [Pure]
    public sealed class Dictionary :
        IValue,
        IEquatable<Dictionary>,
        IImmutableDictionary<IKey, IValue>
    {
        /// <summary>
        /// The empty dictionary.
        /// </summary>
        public static readonly Dictionary Empty =
            new Dictionary(Enumerable.Empty<KeyValuePair<IKey, IValue>>())
            {
                EncodingLength = 2L,
            };

        /// <summary>
        /// The singleton fingerprint for empty dictionaries.
        /// </summary>
        public static readonly Fingerprint EmptyFingerprint =
            new Fingerprint(ValueKind.Dictionary, 2);

        private readonly ImmutableSortedDictionary<IKey, IValue> _dict;
        private ImmutableArray<byte>? _hash;
        private long _encodingLength = -1L;

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<IKey, IValue>> pairs)
            : this(
                pairs.ToImmutableSortedDictionary(
                    kv => kv.Key,
                    kv => kv.Value,
                    KeyComparer.Instance))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Text, IValue>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Text, Boolean>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Text, Integer>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Text, Binary>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Text, Text>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Text, List>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Text, Dictionary>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Text, bool>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Boolean(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Text, short>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Text, ushort>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Text, int>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Text, uint>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Text, long>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Text, ulong>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Text, byte[]>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Binary(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Text, ImmutableArray<byte>>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Binary(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Text, string>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Text(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Binary, IValue>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Binary, Boolean>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Binary, Integer>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Binary, Binary>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Binary, Text>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Binary, List>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Binary, Dictionary>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Binary, bool>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Boolean(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Binary, short>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Binary, ushort>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Binary, int>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Binary, uint>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Binary, long>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Binary, ulong>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Binary, byte[]>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Binary(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Binary, ImmutableArray<byte>>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Binary(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<Binary, string>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(p.Key, new Text(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<string, IValue>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Text(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<string, Boolean>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Text(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<string, Integer>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Text(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<string, Binary>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Text(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<string, Text>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Text(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<string, List>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Text(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<string, Dictionary>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Text(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<string, bool>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Text(p.Key), new Boolean(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<string, short>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Text(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<string, ushort>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Text(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<string, int>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Text(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<string, uint>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Text(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<string, long>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Text(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<string, ulong>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Text(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<string, byte[]>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Text(p.Key), new Binary(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<string, ImmutableArray<byte>>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Text(p.Key), new Binary(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<string, string>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Text(p.Key), new Text(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<byte[], IValue>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<byte[], Boolean>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<byte[], Integer>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<byte[], Binary>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<byte[], Text>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<byte[], List>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<byte[], Dictionary>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<byte[], bool>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Boolean(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<byte[], short>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<byte[], ushort>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<byte[], int>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<byte[], uint>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<byte[], long>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<byte[], ulong>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<byte[], byte[]>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Binary(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<byte[], ImmutableArray<byte>>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Binary(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<byte[], string>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Text(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<ImmutableArray<byte>, IValue>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<ImmutableArray<byte>, Boolean>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<ImmutableArray<byte>, Integer>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<ImmutableArray<byte>, Binary>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<ImmutableArray<byte>, Text>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<ImmutableArray<byte>, List>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<ImmutableArray<byte>, Dictionary>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), p.Value)))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<ImmutableArray<byte>, bool>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Boolean(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<ImmutableArray<byte>, short>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<ImmutableArray<byte>, ushort>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<ImmutableArray<byte>, int>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<ImmutableArray<byte>, uint>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<ImmutableArray<byte>, long>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<ImmutableArray<byte>, ulong>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Integer(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<ImmutableArray<byte>, byte[]>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Binary(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<ImmutableArray<byte>, ImmutableArray<byte>>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Binary(p.Value))))
        {
        }

        /// <summary>
        /// Creates a <see cref="Dictionary"/> instance with key-value <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">Key-value pairs to include.  If there are duplicated keys,
        /// later pairs overwrite earlier ones.</param>
        public Dictionary(IEnumerable<KeyValuePair<ImmutableArray<byte>, string>> pairs)
            : this(pairs.Select(p => new KeyValuePair<IKey, IValue>(new Binary(p.Key), new Text(p.Value))))
        {
        }

        internal Dictionary(in ImmutableSortedDictionary<IKey, IValue> dict)
        {
            if (!KeyComparer.Instance.Equals(dict.KeyComparer))
            {
                throw new ArgumentException(
                    $"Given {nameof(dict)} has an invalid key comparer.");
            }

            _dict = dict;
        }

        /// <inheritdoc cref="IReadOnlyCollection{T}.Count"/>
        public int Count => _dict.Count;

        /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.Keys"/>
        public IEnumerable<IKey> Keys => _dict.Keys;

        /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.Values"/>
        [Obsolete("This operation immediately loads all unloaded values on the memory.")]
        public IEnumerable<IValue> Values
        {
            get
            {
                foreach (IValue v in _dict.Values)
                {
                    yield return v;
                }
            }
        }

        /// <inheritdoc cref="IValue.Kind"/>
        public ValueKind Kind => ValueKind.Dictionary;

        /// <inheritdoc cref="IValue.Fingerprint"/>
        public Fingerprint Fingerprint
        {
            get
            {
                if (_dict.Count < 1)
                {
                    return EmptyFingerprint;
                }

                if (!(_hash is { } hash))
                {
                    long encLength = 2L;
                    SHA1 sha1 = SHA1.Create();
                    sha1.Initialize();
                    foreach (KeyValuePair<IKey, IValue> pair in _dict)
                    {
                        byte[] fp = pair.Key.Fingerprint.Serialize();
                        sha1.TransformBlock(fp, 0, fp.Length, null, 0);
                        fp = pair.Value.Fingerprint.Serialize();
                        sha1.TransformBlock(fp, 0, fp.Length, null, 0);
                        encLength += pair.Key.EncodingLength +
                                     pair.Value.EncodingLength;
                    }

                    sha1.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                    hash = ImmutableArray.Create(sha1.Hash);
                    _hash = hash;
                    if (_encodingLength < 0)
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
            get =>
                _encodingLength < 2L
                    ? _encodingLength = 1L
                        + _dict.Sum(kv =>
                            kv.Key.EncodingLength + kv.Value.EncodingLength)
                        + CommonVariables.Suffix.LongLength
                    : _encodingLength;
            internal set => _encodingLength = value;
        }

        /// <inheritdoc cref="IValue.Inspection"/>
        [Obsolete("Deprecated in favour of " + nameof(Inspect) + "() method.")]
        public string Inspection => Inspect(true);

        /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.this[TKey]"/>
        public IValue this[IKey key] => _dict[key];

        /// <summary>
        /// Gets the element that has the specified text key in the read-only dictionary.
        /// </summary>
        /// <param name="key">The text key to locate.</param>
        /// <returns>The element that has the specified key in the read-only dictionary.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="key" />
        /// is not found.</exception>
        public IValue this[Text key] => this[(IKey)key];

        /// <summary>
        /// Gets the element that has the specified string key in the read-only dictionary.
        /// </summary>
        /// <param name="key">The string key to locate.  This key is automatically turned into
        /// a <see cref="Text"/> instance.</param>
        /// <returns>The element that has the specified key in the read-only dictionary.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="key" />
        /// is not found.</exception>
        public IValue this[string key] => this[new Text(key)];

        /// <summary>
        /// Gets the element that has the specified binary key in the read-only dictionary.
        /// </summary>
        /// <param name="key">The binary key to locate.</param>
        /// <returns>The element that has the specified key in the read-only dictionary.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="key" />
        /// is not found.</exception>
        public IValue this[Binary key] => this[(IKey)key];

        /// <summary>
        /// Gets the element that has the specified bytes key in the read-only dictionary.
        /// </summary>
        /// <param name="key">The bytes key to locate.  This key is automatically turned into
        /// a <see cref="Binary"/> instance.</param>
        /// <returns>The element that has the specified key in the read-only dictionary.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="key" />
        /// is not found.</exception>
        public IValue this[ImmutableArray<byte> key] => this[new Binary(key)];

        /// <summary>
        /// Gets the element that has the specified bytes key in the read-only dictionary.
        /// </summary>
        /// <param name="key">The bytes key to locate.  This key is automatically turned into
        /// a <see cref="Binary"/> instance.</param>
        /// <returns>The element that has the specified key in the read-only dictionary.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="key" />
        /// is not found.</exception>
        public IValue this[byte[] key] => this[new Binary(key)];

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator()"/>
        public IEnumerator<KeyValuePair<IKey, IValue>> GetEnumerator()
        {
            foreach (KeyValuePair<IKey, IValue> kv in _dict)
            {
                yield return kv;
            }
        }

        /// <inheritdoc cref="IEnumerable.GetEnumerator()"/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.ContainsKey(TKey)"/>
        public bool ContainsKey(IKey key) => _dict.ContainsKey(key);

        /// <summary>Determines whether the dictionary contains the specified text key.</summary>
        /// <param name="key">The text key to locate.</param>
        /// <returns><see langword="true" /> if the dictionary contains the specified key;
        /// otherwise, <see langword="false" />.</returns>
        public bool ContainsKey(Text key) => ContainsKey((IKey)key);

        /// <summary>Determines whether the dictionary contains the specified string key.</summary>
        /// <param name="key">The string key to locate.</param>
        /// <returns><see langword="true" /> if the dictionary contains the specified key;
        /// otherwise, <see langword="false" />.</returns>
        public bool ContainsKey(string key) => ContainsKey(new Text(key));

        /// <summary>Determines whether the dictionary contains the specified binary key.</summary>
        /// <param name="key">The binary key to locate.</param>
        /// <returns><see langword="true" /> if the dictionary contains the specified key;
        /// otherwise, <see langword="false" />.</returns>
        public bool ContainsKey(Binary key) => ContainsKey((IKey)key);

        /// <summary>Determines whether the dictionary contains the specified bytes key.</summary>
        /// <param name="key">The bytes key to locate.</param>
        /// <returns><see langword="true" /> if the dictionary contains the specified key;
        /// otherwise, <see langword="false" />.</returns>
        public bool ContainsKey(ImmutableArray<byte> key) => ContainsKey(new Binary(key));

        /// <summary>Determines whether the dictionary contains the specified bytes key.</summary>
        /// <param name="key">The bytes key to locate.</param>
        /// <returns><see langword="true" /> if the dictionary contains the specified key;
        /// otherwise, <see langword="false" />.</returns>
        public bool ContainsKey(byte[] key) => ContainsKey(new Binary(key));

        /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.TryGetValue(TKey, out TValue)"/>
        public bool TryGetValue(IKey key, out IValue value)
        {
            if (_dict.TryGetValue(key, out IValue v))
            {
                value = v;
                return true;
            }

            value = null!;
            return false;
        }

        /// <inheritdoc cref="IImmutableDictionary{TKey,TValue}.Add(TKey, TValue)"/>
        public IImmutableDictionary<IKey, IValue> Add(IKey key, IValue value) =>
            new Dictionary(_dict.Add(key, value));

        public Dictionary Add(Text key, IValue value) =>
            (Dictionary)Add((IKey)key, value);

        public Dictionary Add(Text key, Boolean value) =>
            Add(key, (IValue)value);

        public Dictionary Add(Text key, Integer value) =>
            Add(key, (IValue)value);

        public Dictionary Add(Text key, Binary value) =>
            Add(key, (IValue)value);

        public Dictionary Add(Text key, Text value) =>
            Add(key, (IValue)value);

        public Dictionary Add(Text key, List value) =>
            Add(key, (IValue)value);

        public Dictionary Add(Text key, Dictionary value) =>
            Add(key, (IValue)value);

        public Dictionary Add(Text key, bool value) =>
            Add(key, new Boolean(value));

        public Dictionary Add(Text key, short value) =>
            Add(key, new Integer(value));

        public Dictionary Add(Text key, ushort value) =>
            Add(key, new Integer(value));

        public Dictionary Add(Text key, int value) =>
            Add(key, new Integer(value));

        public Dictionary Add(Text key, uint value) =>
            Add(key, new Integer(value));

        public Dictionary Add(Text key, long value) =>
            Add(key, new Integer(value));

        public Dictionary Add(Text key, ulong value) =>
            Add(key, new Integer(value));

        public Dictionary Add(Text key, BigInteger value) =>
            Add(key, new Integer(value));

        public Dictionary Add(Text key, byte[] value) =>
            Add(key, new Binary(value));

        public Dictionary Add(Text key, ImmutableArray<byte> value) =>
            Add(key, new Binary(value));

        public Dictionary Add(Text key, string value) =>
            Add(key, new Text(value));

        public Dictionary Add(Binary key, IValue value) =>
            (Dictionary)Add((IKey)key, value);

        public Dictionary Add(Binary key, Boolean value) =>
            Add(key, (IValue)value);

        public Dictionary Add(Binary key, Integer value) =>
            Add(key, (IValue)value);

        public Dictionary Add(Binary key, Binary value) =>
            Add(key, (IValue)value);

        public Dictionary Add(Binary key, Text value) =>
            Add(key, (IValue)value);

        public Dictionary Add(Binary key, List value) =>
            Add(key, (IValue)value);

        public Dictionary Add(Binary key, Dictionary value) =>
            Add(key, (IValue)value);

        public Dictionary Add(Binary key, bool value) =>
            Add(key, new Boolean(value));

        public Dictionary Add(Binary key, short value) =>
            Add(key, new Integer(value));

        public Dictionary Add(Binary key, ushort value) =>
            Add(key, new Integer(value));

        public Dictionary Add(Binary key, int value) =>
            Add(key, new Integer(value));

        public Dictionary Add(Binary key, uint value) =>
            Add(key, new Integer(value));

        public Dictionary Add(Binary key, long value) =>
            Add(key, new Integer(value));

        public Dictionary Add(Binary key, ulong value) =>
            Add(key, new Integer(value));

        public Dictionary Add(Binary key, BigInteger value) =>
            Add(key, new Integer(value));

        public Dictionary Add(Binary key, byte[] value) =>
            Add(key, new Binary(value));

        public Dictionary Add(Binary key, ImmutableArray<byte> value) =>
            Add(key, new Binary(value));

        public Dictionary Add(Binary key, string value) =>
            Add(key, new Text(value));

        public Dictionary Add(string key, IValue value) =>
            Add(new Text(key), value);

        public Dictionary Add(string key, Boolean value) =>
            Add(new Text(key), value);

        public Dictionary Add(string key, Integer value) =>
            Add(new Text(key), value);

        public Dictionary Add(string key, Binary value) =>
            Add(new Text(key), value);

        public Dictionary Add(string key, Text value) =>
            Add(new Text(key), value);

        public Dictionary Add(string key, List value) =>
            Add(new Text(key), value);

        public Dictionary Add(string key, Dictionary value) =>
            Add(new Text(key), value);

        public Dictionary Add(string key, bool value) =>
            Add(key, new Boolean(value));

        public Dictionary Add(string key, short value) =>
            Add(key, new Integer(value));

        public Dictionary Add(string key, ushort value) =>
            Add(key, new Integer(value));

        public Dictionary Add(string key, int value) =>
            Add(key, new Integer(value));

        public Dictionary Add(string key, uint value) =>
            Add(key, new Integer(value));

        public Dictionary Add(string key, long value) =>
            Add(key, new Integer(value));

        public Dictionary Add(string key, ulong value) =>
            Add(key, new Integer(value));

        public Dictionary Add(string key, BigInteger value) =>
            Add(key, new Integer(value));

        public Dictionary Add(string key, byte[] value) =>
            Add(key, new Binary(value));

        public Dictionary Add(string key, ImmutableArray<byte> value) =>
            Add(key, new Binary(value));

        public Dictionary Add(string key, string value) =>
            Add(key, new Text(value));

        public Dictionary Add(ImmutableArray<byte> key, IValue value) =>
            Add(new Binary(key), value);

        public Dictionary Add(ImmutableArray<byte> key, Boolean value) =>
            Add(new Binary(key), value);

        public Dictionary Add(ImmutableArray<byte> key, Integer value) =>
            Add(new Binary(key), value);

        public Dictionary Add(ImmutableArray<byte> key, Binary value) =>
            Add(new Binary(key), value);

        public Dictionary Add(ImmutableArray<byte> key, Text value) =>
            Add(new Binary(key), value);

        public Dictionary Add(ImmutableArray<byte> key, List value) =>
            Add(new Binary(key), value);

        public Dictionary Add(ImmutableArray<byte> key, Dictionary value) =>
            Add(new Binary(key), value);

        public Dictionary Add(ImmutableArray<byte> key, bool value) =>
            Add(key, new Boolean(value));

        public Dictionary Add(ImmutableArray<byte> key, short value) =>
            Add(key, new Integer(value));

        public Dictionary Add(ImmutableArray<byte> key, ushort value) =>
            Add(key, new Integer(value));

        public Dictionary Add(ImmutableArray<byte> key, int value) =>
            Add(key, new Integer(value));

        public Dictionary Add(ImmutableArray<byte> key, uint value) =>
            Add(key, new Integer(value));

        public Dictionary Add(ImmutableArray<byte> key, long value) =>
            Add(key, new Integer(value));

        public Dictionary Add(ImmutableArray<byte> key, ulong value) =>
            Add(key, new Integer(value));

        public Dictionary Add(ImmutableArray<byte> key, BigInteger value) =>
            Add(key, new Integer(value));

        public Dictionary Add(ImmutableArray<byte> key, byte[] value) =>
            Add(key, new Binary(value));

        public Dictionary Add(ImmutableArray<byte> key, ImmutableArray<byte> value) =>
            Add(key, new Binary(value));

        public Dictionary Add(ImmutableArray<byte> key, string value) =>
            Add(key, new Text(value));

        public Dictionary Add(byte[] key, IValue value) =>
            Add(new Binary(key), value);

        public Dictionary Add(byte[] key, Boolean value) =>
            Add(new Binary(key), value);

        public Dictionary Add(byte[] key, Integer value) =>
            Add(new Binary(key), value);

        public Dictionary Add(byte[] key, Binary value) =>
            Add(new Binary(key), value);

        public Dictionary Add(byte[] key, Text value) =>
            Add(new Binary(key), value);

        public Dictionary Add(byte[] key, List value) =>
            Add(new Binary(key), value);

        public Dictionary Add(byte[] key, Dictionary value) =>
            Add(new Binary(key), value);

        public Dictionary Add(byte[] key, bool value) =>
            Add(key, new Boolean(value));

        public Dictionary Add(byte[] key, short value) =>
            Add(key, new Integer(value));

        public Dictionary Add(byte[] key, ushort value) =>
            Add(key, new Integer(value));

        public Dictionary Add(byte[] key, int value) =>
            Add(key, new Integer(value));

        public Dictionary Add(byte[] key, uint value) =>
            Add(key, new Integer(value));

        public Dictionary Add(byte[] key, long value) =>
            Add(key, new Integer(value));

        public Dictionary Add(byte[] key, ulong value) =>
            Add(key, new Integer(value));

        public Dictionary Add(byte[] key, BigInteger value) =>
            Add(key, new Integer(value));

        public Dictionary Add(byte[] key, byte[] value) =>
            Add(key, new Binary(value));

        public Dictionary Add(byte[] key, ImmutableArray<byte> value) =>
            Add(key, new Binary(value));

        public Dictionary Add(byte[] key, string value) =>
            Add(key, new Text(value));

        /// <inheritdoc cref="IImmutableDictionary{TKey,TValue}.AddRange"/>
        public IImmutableDictionary<IKey, IValue> AddRange(
            IEnumerable<KeyValuePair<IKey, IValue>> pairs
        ) =>
            new Dictionary(_dict.AddRange(pairs));

        /// <inheritdoc cref="IImmutableDictionary{TKey,TValue}.Clear()"/>
        public IImmutableDictionary<IKey, IValue> Clear() => Empty;

        /// <inheritdoc
        /// cref="IImmutableDictionary{TKey,TValue}.Contains(KeyValuePair{TKey, TValue})"/>
        public bool Contains(KeyValuePair<IKey, IValue> pair) =>
            _dict.Contains(pair);

        /// <inheritdoc cref="IImmutableDictionary{TKey,TValue}.Remove(TKey)"/>
        public IImmutableDictionary<IKey, IValue> Remove(IKey key) =>
            _dict.Count < 1 ? this : new Dictionary(_dict.Remove(key));

        /// <inheritdoc cref="IImmutableDictionary{TKey,TValue}.RemoveRange(IEnumerable{TKey})"/>
        public IImmutableDictionary<IKey, IValue> RemoveRange(IEnumerable<IKey> keys) =>
            _dict.Count < 1 ? this : new Dictionary(_dict.RemoveRange(keys));

        /// <inheritdoc cref="IImmutableDictionary{TKey,TValue}.SetItem(TKey,TValue)"/>
        public IImmutableDictionary<IKey, IValue> SetItem(IKey key, IValue value) =>
            new Dictionary(_dict.SetItem(key, value));

        public Dictionary SetItem(Text key, IValue value) =>
            (Dictionary)SetItem((IKey)key, value);

        public Dictionary SetItem(Text key, Boolean value) =>
            SetItem(key, (IValue)value);

        public Dictionary SetItem(Text key, Integer value) =>
            SetItem(key, (IValue)value);

        public Dictionary SetItem(Text key, Binary value) =>
            SetItem(key, (IValue)value);

        public Dictionary SetItem(Text key, Text value) =>
            SetItem(key, (IValue)value);

        public Dictionary SetItem(Text key, List value) =>
            SetItem(key, (IValue)value);

        public Dictionary SetItem(Text key, Dictionary value) =>
            SetItem(key, (IValue)value);

        public Dictionary SetItem(Text key, bool value) =>
            SetItem(key, new Boolean(value));

        public Dictionary SetItem(Text key, short value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(Text key, ushort value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(Text key, int value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(Text key, uint value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(Text key, long value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(Text key, ulong value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(Text key, BigInteger value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(Text key, byte[] value) =>
            SetItem(key, new Binary(value));

        public Dictionary SetItem(Text key, ImmutableArray<byte> value) =>
            SetItem(key, new Binary(value));

        public Dictionary SetItem(Text key, string value) =>
            SetItem(key, new Text(value));

        public Dictionary SetItem(Binary key, IValue value) =>
            (Dictionary)SetItem((IKey)key, value);

        public Dictionary SetItem(Binary key, Boolean value) =>
            SetItem(key, (IValue)value);

        public Dictionary SetItem(Binary key, Integer value) =>
            SetItem(key, (IValue)value);

        public Dictionary SetItem(Binary key, Binary value) =>
            SetItem(key, (IValue)value);

        public Dictionary SetItem(Binary key, Text value) =>
            SetItem(key, (IValue)value);

        public Dictionary SetItem(Binary key, List value) =>
            SetItem(key, (IValue)value);

        public Dictionary SetItem(Binary key, Dictionary value) =>
            SetItem(key, (IValue)value);

        public Dictionary SetItem(Binary key, bool value) =>
            SetItem(key, new Boolean(value));

        public Dictionary SetItem(Binary key, short value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(Binary key, ushort value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(Binary key, int value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(Binary key, uint value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(Binary key, long value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(Binary key, ulong value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(Binary key, BigInteger value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(Binary key, byte[] value) =>
            SetItem(key, new Binary(value));

        public Dictionary SetItem(Binary key, ImmutableArray<byte> value) =>
            SetItem(key, new Binary(value));

        public Dictionary SetItem(Binary key, string value) =>
            SetItem(key, new Text(value));

        public Dictionary SetItem(string key, IValue value) =>
            SetItem(new Text(key), value);

        public Dictionary SetItem(string key, Boolean value) =>
            SetItem(new Text(key), value);

        public Dictionary SetItem(string key, Integer value) =>
            SetItem(new Text(key), value);

        public Dictionary SetItem(string key, Binary value) =>
            SetItem(new Text(key), value);

        public Dictionary SetItem(string key, Text value) =>
            SetItem(new Text(key), value);

        public Dictionary SetItem(string key, List value) =>
            SetItem(new Text(key), value);

        public Dictionary SetItem(string key, Dictionary value) =>
            SetItem(new Text(key), value);

        public Dictionary SetItem(string key, bool value) =>
            SetItem(key, new Boolean(value));

        public Dictionary SetItem(string key, short value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(string key, ushort value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(string key, int value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(string key, uint value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(string key, long value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(string key, ulong value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(string key, BigInteger value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(string key, byte[] value) =>
            SetItem(key, new Binary(value));

        public Dictionary SetItem(string key, ImmutableArray<byte> value) =>
            SetItem(key, new Binary(value));

        public Dictionary SetItem(string key, string value) =>
            SetItem(key, new Text(value));

        public Dictionary SetItem(ImmutableArray<byte> key, IValue value) =>
            SetItem(new Binary(key), value);

        public Dictionary SetItem(ImmutableArray<byte> key, Boolean value) =>
            SetItem(new Binary(key), value);

        public Dictionary SetItem(ImmutableArray<byte> key, Integer value) =>
            SetItem(new Binary(key), value);

        public Dictionary SetItem(ImmutableArray<byte> key, Binary value) =>
            SetItem(new Binary(key), value);

        public Dictionary SetItem(ImmutableArray<byte> key, Text value) =>
            SetItem(new Binary(key), value);

        public Dictionary SetItem(ImmutableArray<byte> key, List value) =>
            SetItem(new Binary(key), value);

        public Dictionary SetItem(ImmutableArray<byte> key, Dictionary value) =>
            SetItem(new Binary(key), value);

        public Dictionary SetItem(ImmutableArray<byte> key, bool value) =>
            SetItem(key, new Boolean(value));

        public Dictionary SetItem(ImmutableArray<byte> key, short value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(ImmutableArray<byte> key, ushort value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(ImmutableArray<byte> key, int value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(ImmutableArray<byte> key, uint value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(ImmutableArray<byte> key, long value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(ImmutableArray<byte> key, ulong value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(ImmutableArray<byte> key, BigInteger value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(ImmutableArray<byte> key, byte[] value) =>
            SetItem(key, new Binary(value));

        public Dictionary SetItem(ImmutableArray<byte> key, ImmutableArray<byte> value) =>
            SetItem(key, new Binary(value));

        public Dictionary SetItem(ImmutableArray<byte> key, string value) =>
            SetItem(key, new Text(value));

        public Dictionary SetItem(byte[] key, IValue value) =>
            SetItem(new Binary(key), value);

        public Dictionary SetItem(byte[] key, Boolean value) =>
            SetItem(new Binary(key), value);

        public Dictionary SetItem(byte[] key, Integer value) =>
            SetItem(new Binary(key), value);

        public Dictionary SetItem(byte[] key, Binary value) =>
            SetItem(new Binary(key), value);

        public Dictionary SetItem(byte[] key, Text value) =>
            SetItem(new Binary(key), value);

        public Dictionary SetItem(byte[] key, List value) =>
            SetItem(new Binary(key), value);

        public Dictionary SetItem(byte[] key, Dictionary value) =>
            SetItem(new Binary(key), value);

        public Dictionary SetItem(byte[] key, bool value) =>
            SetItem(key, new Boolean(value));

        public Dictionary SetItem(byte[] key, short value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(byte[] key, ushort value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(byte[] key, int value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(byte[] key, uint value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(byte[] key, long value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(byte[] key, ulong value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(byte[] key, BigInteger value) =>
            SetItem(key, new Integer(value));

        public Dictionary SetItem(byte[] key, byte[] value) =>
            SetItem(key, new Binary(value));

        public Dictionary SetItem(byte[] key, ImmutableArray<byte> value) =>
            SetItem(key, new Binary(value));

        public Dictionary SetItem(byte[] key, string value) =>
            SetItem(key, new Text(value));

        /// <inheritdoc cref="IImmutableDictionary{TKey,TValue}.SetItems"/>
        public IImmutableDictionary<IKey, IValue> SetItems(
            IEnumerable<KeyValuePair<IKey, IValue>> items
        ) =>
            new Dictionary(_dict.SetItems(items));

        /// <inheritdoc cref="IImmutableDictionary{TKey,TValue}.TryGetKey(TKey, out TKey)"/>
        public bool TryGetKey(IKey equalKey, out IKey actualKey) =>
            _dict.TryGetKey(equalKey, out actualKey);

        public T GetValue<T>(string name)
            where T : IValue
        {
            return (T)this[name];
        }

        public T GetValue<T>(ImmutableArray<byte> name)
            where T : IValue
        =>
            (T)this[name];

        public T GetValue<T>(byte[] name)
            where T : IValue
        {
            return (T)this[name];
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public override bool Equals(object obj) =>
            obj switch
            {
                null => false,
                Dictionary d => Equals(d),
                _ => false,
            };

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public bool Equals(Dictionary other) =>
            Fingerprint.Equals(other.Fingerprint);

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        bool IEquatable<IValue>.Equals(IValue other) =>
            other is Dictionary o && ((IEquatable<Dictionary>)this).Equals(o);

        /// <inheritdoc cref="object.GetHashCode()"/>
        public override int GetHashCode()
            => unchecked(_dict.Aggregate(GetType().GetHashCode(), (accum, next)
                => (accum * 397) ^ ((next.Key.GetHashCode() * 397) ^ next.Value.GetHashCode())));

        /// <inheritdoc cref="IValue.Inspect(bool)"/>
        public string Inspect(bool loadAll)
        {
            if (_dict.Count < 1)
            {
                return "{}";
            }

            IEnumerable<string> pairs = this.Select(kv =>
                $"  {kv.Key.Inspect(loadAll)}: {kv.Value.Inspect(loadAll).Replace("\n", "\n  ")},\n"
            ).OrderBy(s => s);
            string pairsString = string.Join(string.Empty, pairs);
            return $"{{\n{pairsString}}}";
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString() =>
            $"{nameof(Bencodex)}.{nameof(Types)}.{nameof(Dictionary)} {Inspect(false)}";
    }
}
