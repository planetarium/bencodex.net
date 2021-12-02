using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Bencodex.Types;
using Xunit;
using Xunit.Abstractions;
using static Bencodex.Tests.TestUtils;

namespace Bencodex.Tests
{
    public class CodecTest
    {
        private readonly ITestOutputHelper _output;
        private readonly Encoding _utf8;

        public CodecTest(ITestOutputHelper output)
        {
            _output = output;
            _utf8 = Encoding.GetEncoding(
                "UTF-8",
                new EncoderReplacementFallback(),
                new DecoderReplacementFallback()
            );
        }

        [Theory]
        [ClassData(typeof(SpecTheoryData))]
        public void SpecTestSuite(Spec spec)
        {
            _output.WriteLine("YAML: {0}", spec.SemanticsPath);
            _output.WriteLine("Data: {0}", spec.EncodingPath);
            Codec codec = new Codec();
            IValue decoded = codec.Decode(spec.Encoding);
            _output.WriteLine("Value: {0}", decoded.Inspect(false));
            Assert.Equal(spec.Semantics, decoded);
            Assert.Equal(spec.Encoding.LongLength, decoded.EncodingLength);
            Assert.Equal(spec.Semantics.EncodingLength, decoded.EncodingLength);
            Assert.Equal(spec.Semantics.Fingerprint, decoded.Fingerprint);

            byte[] encoded = codec.Encode(spec.Semantics);
            AssertEqual(spec.Encoding, encoded);

            var random = new Random();
            var toOffload = new ConcurrentDictionary<Fingerprint, bool>();
            var offloaded = new ConcurrentDictionary<Fingerprint, IValue>();
            var offloadOptions = new OffloadOptions(
                iv => toOffload.TryGetValue(iv.Fingerprint, out bool v)
                    ? v
                    : toOffload[iv.Fingerprint] = random.Next() % 2 == 0,
                (iv, loader) => offloaded[iv.Fingerprint] = iv.GetValue(loader)
            );
            byte[] encodingWithOffload = codec.Encode(spec.Semantics, offloadOptions);
            _output.WriteLine(
                "Encoding with offload ({0}): {1}",
                encodingWithOffload.LongLength,
                _utf8.GetString(encodingWithOffload)
            );
            _output.WriteLine(
                "Encoding with offload (hex): {0}",
                BitConverter.ToString(encodingWithOffload)
            );
            _output.WriteLine("Offloaded values:");
            foreach (KeyValuePair<Fingerprint, IValue> pair in offloaded)
            {
                _output.WriteLine("- {0}", pair.Key);
            }

            IValue partiallyDecoded = codec.Decode(
                encodingWithOffload,
                fp => offloaded[fp]
            );
            Assert.Equal(spec.Semantics.Fingerprint, partiallyDecoded.Fingerprint);
            Assert.Equal(spec.Semantics, partiallyDecoded);
            Assert.Equal(spec.Semantics.Inspect(true), partiallyDecoded.Inspect(true));
        }
    }
}
