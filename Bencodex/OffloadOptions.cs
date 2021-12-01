using System;
using Bencodex.Types;

namespace Bencodex
{
    /// <summary>
    /// Options specifying how to offload heavy values of lists and dictionaries.
    /// </summary>
    public sealed class OffloadOptions : IOffloadOptions
    {
        private readonly Predicate<IndirectValue> _embedPredicate;
        private readonly Action<IndirectValue, IndirectValue.Loader?> _offloadAction;

        /// <summary>
        /// Creates a new instance of <see cref="OffloadOptions"/>.
        /// </summary>
        /// <param name="embedPredicate">A predicate to implement <see cref="Embeds"/> method.
        /// </param>
        /// <param name="offloadAction">An action to implement <see cref="Offload"/> method.</param>
        public OffloadOptions(
            Predicate<IndirectValue> embedPredicate,
            Action<IndirectValue, IndirectValue.Loader?> offloadAction
        )
        {
            _embedPredicate = embedPredicate;
            _offloadAction = offloadAction;
        }

        /// <inheritdoc cref="IOffloadOptions.Embeds"/>
        public bool Embeds(in IndirectValue indirectValue) =>
            _embedPredicate(indirectValue);

        /// <inheritdoc cref="IOffloadOptions.Offload"/>
        public void Offload(in IndirectValue indirectValue, IndirectValue.Loader? loader) =>
            _offloadAction(indirectValue, loader);
    }
}
