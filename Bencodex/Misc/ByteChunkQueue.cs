using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace Bencodex.Misc
{
    /// <summary>A special-purpose queue for internal use, which accepts
    /// inserting a chunk of <c cref="System.Byte">Byte</c>s into and
    /// removing the number of <c cref="System.Byte">Byte</c>s from.
    /// <para>For example, if <c>foo</c>, <c>bar</c>, and <c>baz</c> were
    /// <c cref="Append">Push</c>ed into a queue, the first
    /// <c cref="Pop">Pop(5)</c> returns <c>fooba</c> and the second
    /// <c cref="Pop">Pop(5)</c> returns <c>rbaz</c>.</para>
    /// </summary>
    public class ByteChunkQueue
    {
        private readonly Queue<byte[]> _chunks;
        private long _subOffset;

        /// <summary>The total size of <c cref="System.Byte">Byte</c>s that
        /// the queue contains.</summary>
        public long ByteLength { get; private set; }

        /// <summary>Creates a new empty queue.</summary>
        public ByteChunkQueue()
        {
            _chunks = new Queue<byte[]>();
            _subOffset = 0;
            ByteLength = 0;
        }

        /// <summary>Whether the queue is empty or not.</summary>
        public bool Empty => ByteLength < 1;

        /// <summary>Insert an array of <c cref="System.Byte">Byte</c>s chunk
        /// into the end of the queue.</summary>
        /// <param name="chunk">A chunk of <c cref="System.Byte">Byte</c>s to
        /// insert.</param>
        public void Append(byte[] chunk)
        {
            var copied = new byte[chunk.LongLength];
            chunk.CopyTo(copied, 0);
            _chunks.Enqueue(copied);
            ByteLength += chunk.LongLength;
        }

        /// <summary>Fetches the specified size (<paramref name="byteSize"/>)
        /// of leading <c cref="System.Byte">Byte</c>s and removes them from
        /// the queue.</summary>
        /// <param name="byteSize">The length of <c cref="System.Byte">Byte</c>s
        /// to request.</param>
        /// <returns>An array of <c cref="System.Byte">Byte</c>s.  Its size is
        /// probably the requested <paramref name="byteSize"/>, but also may be
        /// less than that if there's not enough <c cref="System.Byte">Byte</c>s
        /// left.</returns>
        /// <exception cref="ArgumentException">Thrown when a requested
        /// <paramref name="byteSize"/> is negative or zero.</exception>
        public byte[] Pop(long byteSize)
        {
            if (byteSize < 1)
            {
                throw new ArgumentException(
                    "the byte size to request to pop must be longer than zero",
                    nameof(byteSize)
                );
            }
            byteSize = Math.Min(byteSize, ByteLength);
            var result = new byte[byteSize];
            long index = 0;
            long consumed = _subOffset;
            while (index < byteSize)
            {
                byte[] chunk = _chunks.Peek();
                long lengthToCopy = Math.Min(
                    chunk.LongLength - _subOffset,
                    byteSize - index
                );
                Array.Copy(
                    chunk,
                    _subOffset,
                    result,
                    index,
                    lengthToCopy
                );
                consumed = _subOffset + lengthToCopy;
                if (consumed >= chunk.LongLength)
                {
                    _chunks.Dequeue();
                    consumed = 0;
                }
                _subOffset = 0;
                index += lengthToCopy;
            }
            _subOffset = consumed;
            ByteLength -= index;
            return result;
        }

        /// <summary>Tests whether the queue shares the same leading
        /// <c cref="System.Byte">Byte</c>s with the given
        /// <paramref name="prefix"/>.</summary>
        /// <param name="prefix">An array of <c cref="System.Byte">Byte</c>s to
        /// test if it's appeared in the queue at first.</param>
        /// <returns>A <c>true</c> if the given <paramref name="prefix"/> is
        /// appeared in the queue at first, or <c>false</c>.</returns>
        [Pure]
        public bool StartsWith(byte[] prefix)
        {
            if (prefix.LongLength > ByteLength)
            {
                return false;
            }

            long subOffset = _subOffset;
            long prefixOffset = 0;
            foreach (byte[] chunk in _chunks)
            {
                long toRead = Math.Min(
                    chunk.LongLength - subOffset,
                    prefix.LongLength - prefixOffset
                );
                for (long i = 0; i < toRead; i++, prefixOffset++)
                {
                    if (chunk[subOffset + i] != prefix[prefixOffset])
                    {
                        return false;
                    }
                }

                subOffset = 0;
            }

            return true;
        }

        /// <summary>A first <c cref="System.Byte">Byte</c> in the queue,
        /// unless it's empty.  If the queue is empty, it is <c>null</c>.
        /// </summary>
        [Pure]
        public byte? FirstByte =>
            Empty ? (byte?) null : _chunks.Peek()[_subOffset];

        /// <summary>Determines the position of the given
        /// <paramref name="element"/> is appeared first in the queue.</summary>
        /// <param name="element">A <c cref="System.Byte">Byte</c> to look
        /// up.</param>
        /// <returns>A zero-indexed offset the given <paramref name="element"/>
        /// is appeared in the queue.  It could be <c>-1</c> if
        /// <paramref name="element"/> does not exist in the queue.</returns>
        [Pure]
        public long IndexOf(byte element)
        {
            long subOffset = _subOffset;
            long i = 0;
            foreach (byte[] chunk in _chunks)
            {
                for (long j = subOffset; j < chunk.Length; i++, j++)
                {
                    if (chunk[j] == element)
                    {
                        return i;
                    }
                }
                subOffset = 0;
            }

            return -1;
        }

        public int ReadFrom(Stream input, int count)
        {
            int step = Math.Min(count, 1024);
            var buffer = new byte[step];
            int read = 0;
            int chunkSize = 0;
            for (int i = 0; i < count; i += step)
            {
                chunkSize = input.Read(buffer, 0, step);
                read += chunkSize;
                if (chunkSize == buffer.Length)
                {
                    Append(buffer);
                }
                else
                {
                    var chunk = new byte[chunkSize];
                    Array.Copy(buffer, 0, chunk, 0, chunkSize);
                    Append(chunk);
                }
            }

            return read;
        }

        public override string ToString()
        {
            var chunkStrings = new string[_chunks.Count];
            int i = 0;
            foreach (byte[] chunk in _chunks)
            {
                string chunkString = BitConverter.ToString(chunk);
                if (i == 0 && _subOffset > 0)
                {
                    int pos = (int) (_subOffset * 2 + (_subOffset - 1));
                    chunkString = chunkString.Remove(pos) +
                        "|" + chunkString.Substring(pos + 1);
                }
                chunkStrings[i++] = chunkString;
            }
            return $"{base.ToString()} [{string.Join(" ", chunkStrings)}]";
        }
    }
}
