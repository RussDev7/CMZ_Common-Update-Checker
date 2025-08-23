using System;
using System.IO;
using DNA.IO.Checksums;

namespace DNA.IO
{
	public class ChecksumStream<T> : Stream
	{
		public Stream BaseStream
		{
			get
			{
				return this._stream;
			}
		}

		public T ChecksumValue
		{
			get
			{
				return this._checkSum.Value;
			}
		}

		public void Reset()
		{
			this._checkSum.Reset();
		}

		public ChecksumStream(Stream stream, IChecksum<T> checksum)
		{
			this._stream = stream;
			this._checkSum = checksum;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int byteCount = this._stream.Read(buffer, offset, count);
			this._checkSum.Update(buffer, offset, count);
			return byteCount;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this._stream.Write(buffer, offset, count);
			this._checkSum.Update(buffer, offset, count);
		}

		public override int ReadByte()
		{
			int data = this._stream.ReadByte();
			this._checkSum.Update((byte)data);
			return data;
		}

		public override void WriteByte(byte value)
		{
			this._stream.WriteByte(value);
			this._checkSum.Update(value);
		}

		public override bool CanRead
		{
			get
			{
				return this._stream.CanRead;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return this._stream.CanWrite;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override void Flush()
		{
			this._stream.Flush();
		}

		public override long Length
		{
			get
			{
				return this._stream.Length;
			}
		}

		public override long Position
		{
			get
			{
				return this._stream.Position;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			this._stream.SetLength(value);
		}

		private IChecksum<T> _checkSum;

		private Stream _stream;
	}
}
