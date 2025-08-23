using System;
using System.IO;

namespace DNA.Security.Cryptography.Crypto.IO
{
	public class CipherStream : Stream
	{
		public CipherStream(Stream stream, IBufferedCipher readCipher, IBufferedCipher writeCipher)
		{
			this.stream = stream;
			if (readCipher != null)
			{
				this.inCipher = readCipher;
				this.mInBuf = null;
			}
			if (writeCipher != null)
			{
				this.outCipher = writeCipher;
			}
		}

		public IBufferedCipher ReadCipher
		{
			get
			{
				return this.inCipher;
			}
		}

		public IBufferedCipher WriteCipher
		{
			get
			{
				return this.outCipher;
			}
		}

		public override int ReadByte()
		{
			if (this.inCipher == null)
			{
				return this.stream.ReadByte();
			}
			if ((this.mInBuf == null || this.mInPos >= this.mInBuf.Length) && !this.FillInBuf())
			{
				return -1;
			}
			return (int)this.mInBuf[this.mInPos++];
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (this.inCipher == null)
			{
				return this.stream.Read(buffer, offset, count);
			}
			int num = 0;
			while (num < count && ((this.mInBuf != null && this.mInPos < this.mInBuf.Length) || this.FillInBuf()))
			{
				int numToCopy = Math.Min(count - num, this.mInBuf.Length - this.mInPos);
				Array.Copy(this.mInBuf, this.mInPos, buffer, offset + num, numToCopy);
				this.mInPos += numToCopy;
				num += numToCopy;
			}
			return num;
		}

		private bool FillInBuf()
		{
			if (this.inStreamEnded)
			{
				return false;
			}
			this.mInPos = 0;
			do
			{
				this.mInBuf = this.ReadAndProcessBlock();
			}
			while (!this.inStreamEnded && this.mInBuf == null);
			return this.mInBuf != null;
		}

		private byte[] ReadAndProcessBlock()
		{
			int blockSize = this.inCipher.GetBlockSize();
			int readSize = ((blockSize == 0) ? 256 : blockSize);
			byte[] block = new byte[readSize];
			int numRead = 0;
			for (;;)
			{
				int count = this.stream.Read(block, numRead, block.Length - numRead);
				if (count < 1)
				{
					break;
				}
				numRead += count;
				if (numRead >= block.Length)
				{
					goto IL_004E;
				}
			}
			this.inStreamEnded = true;
			IL_004E:
			byte[] bytes = (this.inStreamEnded ? this.inCipher.DoFinal(block, 0, numRead) : this.inCipher.ProcessBytes(block));
			if (bytes != null && bytes.Length == 0)
			{
				bytes = null;
			}
			return bytes;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (this.outCipher == null)
			{
				this.stream.Write(buffer, offset, count);
				return;
			}
			byte[] data = this.outCipher.ProcessBytes(buffer, offset, count);
			if (data != null)
			{
				this.stream.Write(data, 0, data.Length);
			}
		}

		public override void WriteByte(byte b)
		{
			if (this.outCipher == null)
			{
				this.stream.WriteByte(b);
				return;
			}
			byte[] data = this.outCipher.ProcessByte(b);
			if (data != null)
			{
				this.stream.Write(data, 0, data.Length);
			}
		}

		public override bool CanRead
		{
			get
			{
				return this.stream.CanRead && this.inCipher != null;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return this.stream.CanWrite && this.outCipher != null;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public sealed override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public sealed override long Position
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public override void Close()
		{
			if (this.outCipher != null)
			{
				byte[] data = this.outCipher.DoFinal();
				this.stream.Write(data, 0, data.Length);
				this.stream.Flush();
			}
			this.stream.Close();
		}

		public override void Flush()
		{
			this.stream.Flush();
		}

		public sealed override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public sealed override void SetLength(long length)
		{
			throw new NotSupportedException();
		}

		internal Stream stream;

		internal IBufferedCipher inCipher;

		internal IBufferedCipher outCipher;

		private byte[] mInBuf;

		private int mInPos;

		private bool inStreamEnded;
	}
}
