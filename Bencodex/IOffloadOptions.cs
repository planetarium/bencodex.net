using Bencodex.Types;

namespace Bencodex
{
    /// <summary>
    /// Options specifying how to offload heavy values of lists and dictionaries.
    /// </summary>
    public interface IOffloadOptions
    {
        /// <summary>
        /// Determines whether a given <paramref name="indirectValue"/> should be embedded or
        /// offloaded.
        /// </summary>
        /// <param name="indirectValue">A value to determine whether to embed or offload.</param>
        /// <returns><see langword="true"/> if <paramref name="indirectValue"/> should be embedded;
        /// <see langword="false"/> if it should be offloaded.</returns>
        public bool Embeds(in IndirectValue indirectValue);

        /// <summary>
        /// Stores an offloaded value in a separate place.
        /// </summary>
        /// <param name="indirectValue">An offloaded value.</param>
        /// <param name="loader">An optional loader used for loading
        /// the <paramref name="indirectValue"/>.  This can be <see langword="null"/> if
        /// the <paramref name="indirectValue"/> is already loaded.</param>
        public void Offload(in IndirectValue indirectValue, IndirectValue.Loader? loader);
    }
}
