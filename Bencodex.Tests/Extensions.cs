using System.Diagnostics.Contracts;

namespace Bencodex.Tests.Types
{
    public static class Extensions
    {
        [Pure]
        public static string NoCr(this string value) =>
            value.Replace("\r", string.Empty);
    }
}
