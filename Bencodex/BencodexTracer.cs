using System.Diagnostics;

namespace Bencodex
{
    internal static class BencodexTracer
    {
        private static readonly ActivitySource _activitySource = new ActivitySource("Bencodex");

        public static Activity? StartActivity(string name) =>
            _activitySource.StartActivity(name);
    }
}
