using System;
using System.Linq;
using System.Text;
using Xunit;

namespace Bencodex.Tests
{
    public static class TestUtils
    {
        public static void AssertEqual(
            byte[] expected,
            byte[] actual,
            string message = null
        )
        {
            Encoding utf8 = Encoding.GetEncoding(
                "UTF-8",
                new EncoderReplacementFallback(),
                new DecoderReplacementFallback()
            );
            Assert.True(
                expected.SequenceEqual(actual),
                string.Format(
                    "{4}{5}" +
                    "Expected: {0}\nActual:   {1}\n" +
                    "Expected (hex): {2}\nActual (hex):   {3}",
                    utf8.GetString(expected),
                    utf8.GetString(actual),
                    BitConverter.ToString(expected),
                    BitConverter.ToString(actual),
                    message ?? string.Empty,
                    message == null ? string.Empty : "\n"
                )
            );
        }
    }
}