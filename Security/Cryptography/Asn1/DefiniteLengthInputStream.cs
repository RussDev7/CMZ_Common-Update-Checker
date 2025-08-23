using System;
using System.IO;
using DNA.Security.Cryptography.Utilities.IO;

namespace DNA.Security.Cryptography.Asn1
{
	internal class DefiniteLengthInputStream : LimitedInputStream
	{
		internal DefiniteLengthInputStream(Stream inStream, int length)
			: base(inStream)
		{
			if (length < 0)
			{
				throw new ArgumentException("negative lengths not allowed", "length");
			}
			this._originalLength = length;
			this._remaining = length;
			if (length == 0)
			{
				this.SetParentEofDetect(true);
			}
		}

		public override int ReadByte()
		{
			if (this._remaining == 0)
			{
				return -1;
			}
			int b = this._in.ReadByte();
			if (b < 0)
			{
				throw new EndOfStreamException(string.Concat(new object[] { "DEF length ", this._originalLength, " object truncated by ", this._remaining }));
			}
			if (--this._remaining == 0)
			{
				this.SetParentEofDetect(true);
			}
			return b;
		}

		public override int Read(byte[] buf, int off, int len)
		{
			if (this._remaining == 0)
			{
				return 0;
			}
			int toRead = Math.Min(len, this._remaining);
			int numRead = this._in.Read(buf, off, toRead);
			if (numRead < 1)
			{
				throw new EndOfStreamException(string.Concat(new object[] { "DEF length ", this._originalLength, " object truncated by ", this._remaining }));
			}
			if ((this._remaining -= numRead) == 0)
			{
				this.SetParentEofDetect(true);
			}
			return numRead;
		}

		internal byte[] ToArray()
		{
			if (this._remaining == 0)
			{
				return DefiniteLengthInputStream.EmptyBytes;
			}
			byte[] bytes = new byte[this._remaining];
			if ((this._remaining -= Streams.ReadFully(this._in, bytes)) != 0)
			{
				throw new EndOfStreamException(string.Concat(new object[] { "DEF length ", this._originalLength, " object truncated by ", this._remaining }));
			}
			this.SetParentEofDetect(true);
			return bytes;
		}

		private static readonly byte[] EmptyBytes = new byte[0];

		private readonly int _originalLength;

		private int _remaining;
	}
}
