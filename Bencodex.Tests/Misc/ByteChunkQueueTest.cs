using System;
using System.IO;
using Bencodex.Misc;
using Xunit;

namespace Bencodex.Tests.Misc
{
    public class ByteChunkQueueTest
    {
        [Fact]
        public void TestByteChunkQueue()
        {
            var q = new ByteChunkQueue();
            // {}
            Assert.Equal(0, q.ByteLength);
            Assert.True(q.Empty);
            Assert.Throws<ArgumentException>(() => q.Pop(0));
            Assert.Throws<ArgumentException>(() => q.Pop(-1));
            Assert.Empty(q.Pop(1));
            Assert.Empty(q.Pop(2));
            Assert.True(q.StartsWith(new byte[0]));
            Assert.False(q.StartsWith(new byte[] { 1, 2 }));
            Assert.Null(q.FirstByte);
            Assert.Equal(-1, q.IndexOf(1));
            Assert.EndsWith("[]", q.ToString());

            q.Append(new byte[] { 1, 2, 3, 4 });
            // {1 2 3 4}
            Assert.Equal(4, q.ByteLength);
            Assert.False(q.Empty);
            Assert.True(q.StartsWith(new byte[0]));
            Assert.True(q.StartsWith(new byte[] { 1, 2 }));
            Assert.True(q.StartsWith(new byte[] { 1, 2, 3, 4 }));
            Assert.False(q.StartsWith(new byte[] { 1, 2, 3, 4, 5 }));
            Assert.False(q.StartsWith(new byte[] { 1, 3 }));
            Assert.Equal((byte) 1, q.FirstByte);
            Assert.Equal(0, q.IndexOf(1));
            Assert.Equal(2, q.IndexOf(3));
            Assert.Equal(-1, q.IndexOf(5));
            Assert.EndsWith("[01-02-03-04]", q.ToString());

            byte[] popped = q.Pop(2);
            // {1 2 > 3 4}
            Assert.Equal(new byte[] { 1, 2 }, popped);
            Assert.Equal(2, q.ByteLength);
            Assert.False(q.Empty);
            Assert.True(q.StartsWith(new byte[0]));
            Assert.True(q.StartsWith(new byte[] { 3 }));
            Assert.True(q.StartsWith(new byte[] { 3, 4 }));
            Assert.False(q.StartsWith(new byte[] { 3, 4, 5 }));
            Assert.False(q.StartsWith(new byte[] { 1, 2 }));
            Assert.Equal((byte) 3, q.FirstByte);
            Assert.Equal(-1, q.IndexOf(1));
            Assert.Equal(0, q.IndexOf(3));
            Assert.Equal(1, q.IndexOf(4));
            Assert.Equal(-1, q.IndexOf(5));
            Assert.EndsWith("[01-02|03-04]", q.ToString());

            var input = new MemoryStream(
                new byte[] { 5, 6, 7, 8, 9, 10, 11, 12 }
            );
            q.ReadFrom(input, 4);
            // {1 2 > 3 4, 5 6 7 8}
            Assert.Equal(6, q.ByteLength);
            Assert.False(q.Empty);
            Assert.True(q.StartsWith(new byte[0]));
            Assert.True(q.StartsWith(new byte[] { 3 }));
            Assert.True(q.StartsWith(new byte[] { 3, 4 }));
            Assert.True(q.StartsWith(new byte[] { 3, 4, 5 }));
            Assert.True(q.StartsWith(new byte[] { 3, 4, 5, 6, 7, 8 }));
            Assert.False(q.StartsWith(new byte[] { 3, 4, 5, 6, 7, 8, 9 }));
            Assert.False(q.StartsWith(new byte[] { 1, 2 }));
            Assert.Equal((byte) 3, q.FirstByte);
            Assert.Equal(-1, q.IndexOf(1));
            Assert.Equal(0, q.IndexOf(3));
            Assert.Equal(4, q.IndexOf(7));
            Assert.Equal(-1, q.IndexOf(9));
            Assert.EndsWith("[01-02|03-04 05-06-07-08]", q.ToString());

            popped = q.Pop(4);
            // {5 6 > 7 8}
            Assert.Equal(new byte[] { 3, 4, 5, 6 }, popped);
            Assert.Equal(2, q.ByteLength);
            Assert.False(q.Empty);
            Assert.True(q.StartsWith(new byte[0]));
            Assert.True(q.StartsWith(new byte[] { 7 }));
            Assert.True(q.StartsWith(new byte[] { 7, 8 }));
            Assert.False(q.StartsWith(new byte[] { 7, 8, 9 }));
            Assert.False(q.StartsWith(new byte[] { 1, 2 }));
            Assert.False(q.StartsWith(new byte[] { 3 }));
            Assert.Equal((byte) 7, q.FirstByte);
            Assert.Equal(-1, q.IndexOf(3));
            Assert.Equal(0, q.IndexOf(7));
            Assert.Equal(1, q.IndexOf(8));
            Assert.Equal(-1, q.IndexOf(9));
            Assert.EndsWith("[05-06|07-08]", q.ToString());

            q.ReadFrom(input, 3);
            // {5 6 > 7 8, 9 10 11}
            Assert.Equal(5, q.ByteLength);
            Assert.False(q.Empty);
            Assert.True(q.StartsWith(new byte[0]));
            Assert.True(q.StartsWith(new byte[] { 7 }));
            Assert.True(q.StartsWith(new byte[] { 7, 8 }));
            Assert.True(q.StartsWith(new byte[] { 7, 8, 9 }));
            Assert.True(q.StartsWith(new byte[] { 7, 8, 9, 10, 11 }));
            Assert.False(q.StartsWith(new byte[] { 7, 8, 9, 10, 11, 12 }));
            Assert.False(q.StartsWith(new byte[] { 1, 2 }));
            Assert.False(q.StartsWith(new byte[] { 3 }));
            Assert.Equal((byte) 7, q.FirstByte);
            Assert.Equal(-1, q.IndexOf(3));
            Assert.Equal(-1, q.IndexOf(6));
            Assert.Equal(0, q.IndexOf(7));
            Assert.Equal(2, q.IndexOf(9));
            Assert.Equal(4, q.IndexOf(11));
            Assert.Equal(-1, q.IndexOf(12));
            Assert.EndsWith("[05-06|07-08 09-0A-0B]", q.ToString());

            q.ReadFrom(input, 10);
            // {5 6 > 7 8, 9 10 11, 12}
            Assert.Equal(6, q.ByteLength);
            Assert.False(q.Empty);
            Assert.True(q.StartsWith(new byte[0]));
            Assert.True(q.StartsWith(new byte[] { 7 }));
            Assert.True(q.StartsWith(new byte[] { 7, 8, 9 }));
            Assert.True(q.StartsWith(new byte[] { 7, 8, 9, 10, 11 }));
            Assert.True(q.StartsWith(new byte[] { 7, 8, 9, 10, 11, 12 }));
            Assert.False(q.StartsWith(new byte[] { 7, 8, 9, 10, 11, 12, 13 }));
            Assert.False(q.StartsWith(new byte[] { 1, 2 }));
            Assert.False(q.StartsWith(new byte[] { 3 }));
            Assert.Equal((byte) 7, q.FirstByte);
            Assert.Equal(-1, q.IndexOf(6));
            Assert.Equal(0, q.IndexOf(7));
            Assert.Equal(5, q.IndexOf(12));
            Assert.Equal(-1, q.IndexOf(13));
            Assert.EndsWith("[05-06|07-08 09-0A-0B 0C]", q.ToString());

            q.Append(new byte[] { 13, 14 });
            // {5 6 > 7 8, 9 10 11, 12, 13 14}
            Assert.Equal(8, q.ByteLength);
            Assert.False(q.Empty);
            Assert.True(q.StartsWith(new byte[0]));
            Assert.True(q.StartsWith(new byte[] { 7 }));
            Assert.True(q.StartsWith(new byte[] { 7, 8, 9 }));
            Assert.True(q.StartsWith(new byte[] { 7, 8, 9, 10, 11, 12 }));
            Assert.True(q.StartsWith(new byte[] { 7, 8, 9, 10, 11, 12, 13 }));
            Assert.False(
                q.StartsWith(new byte[] { 7, 8, 9, 10, 11, 12, 13, 14, 15 })
            );
            Assert.False(q.StartsWith(new byte[] { 1, 2 }));
            Assert.False(q.StartsWith(new byte[] { 3 }));
            Assert.Equal((byte) 7, q.FirstByte);
            Assert.Equal(-1, q.IndexOf(6));
            Assert.Equal(0, q.IndexOf(7));
            Assert.Equal(7, q.IndexOf(14));
            Assert.Equal(-1, q.IndexOf(15));
            Assert.EndsWith("[05-06|07-08 09-0A-0B 0C 0D-0E]", q.ToString());

            q.Append(new byte[] { 15 });
            // {5 6 > 7 8, 9 10 11, 12, 13 14, 15}
            Assert.Equal(9, q.ByteLength);
            Assert.False(q.Empty);
            Assert.True(q.StartsWith(new byte[0]));
            Assert.True(q.StartsWith(new byte[] { 7 }));
            Assert.True(q.StartsWith(new byte[] { 7, 8, 9 }));
            Assert.True(
                q.StartsWith(new byte[] { 7, 8, 9, 10, 11, 12, 13, 14, 15 })
            );
            Assert.False(
                q.StartsWith(new byte[] { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 })
            );
            Assert.False(q.StartsWith(new byte[] { 1, 2 }));
            Assert.False(q.StartsWith(new byte[] { 3 }));
            Assert.Equal((byte) 7, q.FirstByte);
            Assert.Equal(-1, q.IndexOf(6));
            Assert.Equal(0, q.IndexOf(7));
            Assert.Equal(8, q.IndexOf(15));
            Assert.Equal(-1, q.IndexOf(16));
            Assert.EndsWith("[05-06|07-08 09-0A-0B 0C 0D-0E 0F]", q.ToString());

            popped = q.Pop(7);
            // {13 > 14, 15}
            Assert.Equal(new byte[] { 7, 8, 9, 10, 11, 12, 13 }, popped);
            Assert.Equal(2, q.ByteLength);
            Assert.False(q.Empty);
            Assert.True(q.StartsWith(new byte[0]));
            Assert.True(q.StartsWith(new byte[] { 14 }));
            Assert.True(q.StartsWith(new byte[] { 14, 15 }));
            Assert.False(q.StartsWith(new byte[] { 14, 15, 16 }));
            Assert.False(q.StartsWith(new byte[] { 1, 2 }));
            Assert.False(q.StartsWith(new byte[] { 7 }));
            Assert.False(q.StartsWith(new byte[] { 7, 8, 9 }));
            Assert.Equal((byte) 14, q.FirstByte);
            Assert.Equal(-1, q.IndexOf(7));
            Assert.Equal(-1, q.IndexOf(13));
            Assert.Equal(0, q.IndexOf(14));
            Assert.Equal(1, q.IndexOf(15));
            Assert.Equal(-1, q.IndexOf(16));
            Assert.EndsWith("[0D|0E 0F]", q.ToString());

            popped = q.Pop(10);
            // {}
            Assert.Equal(new byte[] { 14, 15 }, popped);
            Assert.Equal(0, q.ByteLength);
            Assert.True(q.Empty);
            Assert.True(q.StartsWith(new byte[0]));
            Assert.False(q.StartsWith(new byte[] { 1, 2 }));
            Assert.False(q.StartsWith(new byte[] { 14 }));
            Assert.False(q.StartsWith(new byte[] { 14, 15 }));
            Assert.False(q.StartsWith(new byte[] { 14, 15, 16 }));
            Assert.Null(q.FirstByte);
            Assert.Equal(-1, q.IndexOf(13));
            Assert.Equal(-1, q.IndexOf(15));
            Assert.Equal(-1, q.IndexOf(16));
            Assert.EndsWith("[]", q.ToString());
        }

        [Fact]
        public void EdgeCase1()
        {
            var q = new ByteChunkQueue();
            q.Append(new byte[] { 1, 2, 3, 4 });
            byte[] popped = q.Pop(4);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, popped);
            Assert.EndsWith("[]", q.ToString());
        }

        [Fact]
        public void EdgeCase2()
        {
            var q = new ByteChunkQueue();
            q.Append(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
            byte[] popped = q.Pop(7);
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, popped);
            Assert.EndsWith("[01-02-03-04-05-06-07|08]", q.ToString());
            q.Append(new byte[] { 9, 0xa, 0xb, 0xc, 0xd, 0xe, 0xf, 0x10 });
            Assert.EndsWith("[01-02-03-04-05-06-07|08 09-0A-0B-0C-0D-0E-0F-10]", q.ToString());

            popped = q.Pop(1);
            Assert.Equal(new byte[] { 8 }, popped);
            Assert.EndsWith("[09-0A-0B-0C-0D-0E-0F-10]", q.ToString());

            popped = q.Pop(1);
            Assert.Equal(new byte[] { 9 }, popped);
            Assert.EndsWith("[09|0A-0B-0C-0D-0E-0F-10]", q.ToString());
        }
    }
}
