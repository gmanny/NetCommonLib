using System;
using System.Collections.Generic;
using System.IO;

namespace MonitorCommon
{
    /// <summary>
    /// A stream that caches a number of bytes at the start of the stream for times when it's needed for debug output.
    /// </summary>
    public class SnippetCachingStream : Stream
    {
        private readonly Stream parent;
        private readonly int snippetLength;
        private readonly List<byte> snippet;

        public SnippetCachingStream(Stream parent, int snippetLength = 256)
        {
            this.parent = parent;
            this.snippetLength = snippetLength;

            snippet = new List<byte>();
        }

        private static IEnumerable<T> GetRange<T>(T[] arr, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return arr[offset + i];
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int result = parent.Read(buffer, offset, count);

            if (result > 0 && snippet.Count < snippetLength)
            {
                int cnt = Math.Min(result, snippetLength - snippet.Count);
                snippet.AddRange(GetRange(buffer, offset, cnt));
            }

            return result;
        }

        public List<byte> Snippet => snippet;

        public override void Flush() => throw new System.NotImplementedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new System.NotImplementedException();
        public override void SetLength(long value) => throw new System.NotImplementedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new System.NotImplementedException();

        public override bool CanRead => parent.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => parent.Length;
        public override long Position
        {
            get => parent.Position;
            set => parent.Position = value;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            parent.Dispose();
        }
    }
}