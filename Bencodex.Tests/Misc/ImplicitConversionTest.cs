using System.Collections.Generic;
using System.Collections.Immutable;
using Bencodex.Types;
using Xunit;
using Boolean = Bencodex.Types.Boolean;

namespace Bencodex.Tests.Misc
{
    public class ImplicitConversionTest
    {
        [Fact]
        public void Integer()
        {
            Integer integer = 0x9c;

            short s = integer;
            ushort us = integer;
            int i = integer;
            uint ui = integer;
            long l = integer;
            ulong ul = integer;
            Assert.Equal(0x9c, s);
            Assert.Equal(0x9c, us);
            Assert.Equal(0x9c, i);
            Assert.Equal(0x9cU, ui);
            Assert.Equal(0x9c, l);
            Assert.Equal(0x9cUL, ul);
        }

        [Fact]
        public void Text()
        {
            var text = new Text("BENCODEX");

            string s = text;
            Assert.Equal(s, text.Value);
        }

        [Fact]
        public void Binary()
        {
            var binary = new Binary(new byte[] { 0x01, 0x02, 0x03 });

            ImmutableArray<byte> immutable = (ImmutableArray<byte>)binary;
            Assert.Equal(immutable, binary.ByteArray);

            byte[] mutable = (byte[])binary;
            Assert.Equal(mutable, binary.ToByteArray());
        }

        [Fact]
        public void Boolean()
        {
            var boolean = new Boolean(true);

            bool b = boolean;
            Assert.Equal(b, boolean.Value);
        }
    }
}
