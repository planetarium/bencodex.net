using System.Diagnostics.Contracts;

namespace Bencodex.Types
{
    /// <summary>
    /// Represents Bencodex values which can be keys of a Bencodex <see cref="Dictionary"/>.
    /// </summary>
    /// <seealso cref="Binary"/>
    /// <seealso cref="Text"/>
    public interface IKey : IValue
    {
        [Pure]
        byte[] EncodeAsByteArray();
    }
}
