using System.Diagnostics.Contracts;

namespace Bencodex.Types
{
    public interface IKey : IValue
    {
        [Pure]
        byte? KeyPrefix { get; }

        [Pure]
        byte[] EncodeAsByteArray();
    }
}
