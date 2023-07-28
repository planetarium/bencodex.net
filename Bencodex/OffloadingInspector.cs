using System.Collections.Generic;
using Bencodex.Types;

namespace Bencodex
{
    /// <summary>
    /// Inspects the internal states of how a Bencodex container load and offload its elements.
    /// </summary>
    public static class OffloadingInspector
    {
        /// <summary>
        /// Inspects the internal states of how a Bencodex <paramref name="dictionary"/> load and
        /// offload its values.
        /// </summary>
        /// <param name="dictionary">The Bencodex dictionary to inspect.</param>
        /// <param name="loader">The loader that the <paramref name="dictionary"/> internally holds.
        /// It can be <see langword="null"/> if the <paramref name="dictionary"/> has no offloaded
        /// values.</param>
        /// <returns>An enumerable of pairs of key and associated <see cref="IndirectValue"/>.
        /// The order of the returned pairs is guaranteed to follow the order according to
        /// the Bencodex specification, i.e., lexicographical order of the keys and every
        /// binary key is prior to every text key.</returns>
        public static IEnumerable<KeyValuePair<IKey, IndirectValue>> EnumerableIndirectPairs(
            this Dictionary dictionary,
            out IndirectValue.Loader? loader
        )
        {
            loader = dictionary.Loader;
            return dictionary.EnumerateIndirectPairs();
        }
    }
}
