using System;

namespace Bencodex.Types
{
    /// <summary>
    /// An indirection to <see cref="IValue"/>, which can be loaded when it is needed.
    /// </summary>
    public struct IndirectValue
    {
        private Fingerprint? _fingerprint;
        private IValue? _loadedValue;

        /// <summary>
        /// Creates an <see cref="IndirectValue"/> with the <paramref name="loadedValue"/>.
        /// </summary>
        /// <param name="loadedValue">The value already loaded on the memory.</param>
        public IndirectValue(IValue loadedValue)
        {
            _fingerprint = null;
            _loadedValue = loadedValue;
        }

        /// <summary>
        /// Creates an <see cref="IndirectValue"/> with the <paramref name="fingerprint"/> of
        /// the value to load when it is needed.
        /// </summary>
        /// <param name="fingerprint">The <see cref="IValue.Fingerprint"/> of
        /// the <see cref="IValue"/> to load when it is needed.</param>
        public IndirectValue(in Fingerprint fingerprint)
        {
            _fingerprint = fingerprint;
            _loadedValue = null;
        }

        /// <summary>
        /// A delegate to load a value by its <paramref name="fingerprint"/>.
        /// </summary>
        /// <param name="fingerprint">The <see cref="IValue.Fingerprint"/> of
        /// the <see cref="IValue"/> to load.</param>
        /// <returns>The loaded <see cref="IValue"/>.</returns>
        public delegate IValue Loader(Fingerprint fingerprint);

        /// <summary>
        /// The <see cref="IValue.Fingerprint"/> of the <see cref="IValue"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the <see cref="IndirectValue"/>
        /// is an uninitialized default value.</exception>
        public Fingerprint Fingerprint => _loadedValue?.Fingerprint ?? _fingerprint ??
            throw new InvalidOperationException(
                $"No loaded value nor fingerprint.  Probably this {nameof(IndirectValue)} is an " +
                "uninitialized default value."
            );

        /// <summary>
        /// Whether the value is loaded on the memory or not.
        /// </summary>
        public bool Loaded => _loadedValue is { };

        /// <summary>
        /// Gets the value.
        /// <para>If it is not loaded on the memory yet, the value is loaded first using
        /// the <paramref name="loader"/>.</para>
        /// </summary>
        /// <param name="loader">The loading implementation.  If the value is already loaded
        /// on the memory, this delegate may not be invoked.  If it is <c>null</c>, the value must
        /// be already loaded (through <see cref="IndirectValue(IValue)"/> constructor).</param>
        /// <returns>The loaded value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the value is not loaded but no
        /// <paramref name="loader"/> is present.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the <see cref="IndirectValue"/>
        /// is an uninitialized default value.</exception>
        public IValue GetValue(in Loader? loader)
        {
            if (!(_loadedValue is { } v))
            {
                if (loader is { } load)
                {
                    Fingerprint f = Fingerprint;
                    v = load(f);
                    if (!v.Fingerprint.Equals(f))
                    {
                        throw new InvalidOperationException(
                            $"The expected fingerprint is {f}, but the fingerprint of the value " +
                            $"returned by the {nameof(loader)} is {v.Fingerprint}."
                        );
                    }

                    _loadedValue = v;
                }
                else
                {
                    throw new ArgumentNullException(
                        nameof(loader),
                        "The value is not loaded, but no loader is present."
                    );
                }
            }

            return v;
        }
    }
}
