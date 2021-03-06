using System;
using System.Collections.Generic;
using System.IO;

namespace MonitorCommon
{
    // from https://gist.github.com/bittercoder/3588074
    /// <summary>
    /// MemoryTributary is a re-implementation of MemoryStream that uses a dynamic list of byte arrays as a backing store, instead of a single byte array, the allocation
    /// of which will fail for relatively small streams as it requires contiguous memory.
    /// </summary>
    public class MemoryTributary : Stream       /* http://msdn.microsoft.com/en-us/library/system.io.stream.aspx */
    {
        #region Constructors

        public MemoryTributary()
        {
            Position = 0;
        }

        public MemoryTributary(int blockSize, int length)
        {
            this.blockSize = blockSize;

            Position = 0;

            if (length != 0)
            {
                SetLength(length);
                Position = length;
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                byte[] d = Block;   //access block to prompt the allocation of memory
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                Position = 0;
            }
        }

        public MemoryTributary(byte[] source)
        {
            this.Write(source, 0, source.Length);
            Position = 0;
        }

        /* length is ignored because capacity has no meaning unless we implement an artifical limit */
        public MemoryTributary(int length)
        {
            SetLength(length);
            Position = length;
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            byte[] d = Block;   //access block to prompt the allocation of memory
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            Position = 0;
        }

        #endregion

        #region Status Properties

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        #endregion

        #region Public Properties

        public override long Length
        {
            get { return length; }
        }

        public override long Position { get; set; }

        #endregion

        #region Members

        protected long length = 0;

        protected long blockSize = 65536;

        protected List<byte[]> blocks = new();

        #endregion

        #region Internal Properties

        /* Use these properties to gain access to the appropriate block of memory for the current Position */

        /// <summary>
        /// The block of memory currently addressed by Position
        /// </summary>
        protected byte[] Block
        {
            get
            {
                while (blocks.Count <= BlockId)
                    blocks.Add(new byte[blockSize]);
                return blocks[(int)BlockId];
            }
        }
        /// <summary>
        /// The id of the block currently addressed by Position
        /// </summary>
        protected long BlockId => Position / blockSize;

        /// <summary>
        /// The offset of the byte currently addressed by Position, into the block that contains it
        /// </summary>
        protected long BlockOffset => Position % blockSize;

        #endregion

        #region Public Stream Methods

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long lcount = count;

            if (lcount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), lcount, "Number of bytes to copy cannot be negative.");
            }

            long remaining = (length - Position);
            if (lcount > remaining)
                lcount = remaining;

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer), "Buffer cannot be null.");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset),offset,"Destination offset cannot be negative.");
            }

            int read = 0;
            long copysize;
            do
	        {
                copysize = Math.Min(lcount, (blockSize - BlockOffset));
                Buffer.BlockCopy(Block, (int)BlockOffset, buffer, offset, (int)copysize);
                lcount -= copysize;
                offset += (int)copysize;

                read += (int)copysize;
                Position += copysize;

	        } while (lcount > 0);

            return read;
               
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            length = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            long initialPosition = Position;
            int copysize;
            try
            {
                do
                {
                    copysize = Math.Min(count, (int)(blockSize - BlockOffset));

                    EnsureCapacity(Position + copysize);

                    Buffer.BlockCopy(buffer, (int)offset, Block, (int)BlockOffset, copysize);
                    count -= copysize;
                    offset += copysize;

                    Position += copysize;

                } while (count > 0);
            }
            catch
            {
                Position = initialPosition;
                throw;
            }
        }

        public override int ReadByte()
        {
            if (Position >= length)
                return -1;

            byte b = Block[BlockOffset];
            Position++;

            return b;
        }

        public override void WriteByte(byte value)
        {
            EnsureCapacity(Position + 1);
            Block[BlockOffset] = value;
            Position++;
        }

        protected void EnsureCapacity(long intended_length)
        {
            if (intended_length > length)
                length = (intended_length);
        }

        #endregion

        #region IDispose

        /* http://msdn.microsoft.com/en-us/library/fs2xkftw.aspx */
        protected override void Dispose(bool disposing)
        {
            /* We do not currently use unmanaged resources */
            base.Dispose(disposing);
        }

        #endregion

        #region Public Additional Helper Methods

        /// <summary>
        /// Returns the entire content of the stream as a byte array. This is not safe because the call to new byte[] may 
        /// fail if the stream is large enough. Where possible use methods which operate on streams directly instead.
        /// </summary>
        /// <returns>A byte[] containing the current data in the stream</returns>
        public byte[] ToArray()
        {
            long firstposition = Position;
            Position = 0;
            byte[] destination = new byte[Length];
            Read(destination, 0, (int)Length);
            Position = firstposition;
            return destination;
        }

        /// <summary>
        /// Reads length bytes from source into the this instance at the current position.
        /// </summary>
        /// <param name="source">The stream containing the data to copy</param>
        /// <param name="length">The number of bytes to copy</param>
        public void ReadFrom(Stream source, long length)
        {
            byte[] buffer = new byte[4096];
            int read;
            do
            {
                read = source.Read(buffer, 0, (int)Math.Min(4096, length));
                length -= read;
                this.Write(buffer, 0, read);

            } while (length > 0);
        }

        /// <summary>
        /// Writes the entire stream into destination, regardless of Position, which remains unchanged.
        /// </summary>
        /// <param name="destination">The stream to write the content of this stream to</param>
        public void WriteTo(Stream destination)
        {
            long initialpos = Position;
            Position = 0;
            this.CopyTo(destination);
            Position = initialpos;
        }

        #endregion
    }
}
