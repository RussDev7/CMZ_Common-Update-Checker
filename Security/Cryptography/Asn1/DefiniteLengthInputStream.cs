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
			int num = this._in.ReadByte();
			if (num < 0)
			{
				throw new EndOfStreamException(string.Concat(new object[] { "DEF length ", this._originalLength, " object truncated by ", this._remaining }));
			}
			if (--this._remaining == 0)
			{
				this.SetParentEofDetect(true);
			}
			return num;
		}

		public override int Read(byte[] buf, int off, int len)
		{
			if (this._remaining == 0)
			{
				return 0;
			}
			int num = Math.Min(len, this._remaining);
			int num2 = this._in.Read(buf, off, num);
			if (num2 < 1)
			{
				throw new EndOfStreamException(string.Concat(new object[] { "DEF length ", this._originalLength, " object truncated by ", this._remaining }));
			}
			if ((this._remaining -= num2) == 0)
			{
				this.SetParentEofDetect(true);
			}
			return num2;
		}

		internal byte[] ToArray()
		{
			if (this._remaining == 0)
			{
				return DefiniteLengthInputStream.EmptyBytes;
			}
			byte[] array = new byte[this._remaining];
			if ((this._remaining -= Streams.ReadFully(this._in, array)) != 0)
			{
				throw new EndOfStreamException(string.Concat(new object[] { "DEF length ", this._originalLength, " object truncated by ", this._remaining }));
			}
			this.SetParentEofDetect(true);
			return array;
		}

		private static readonly byte[] EmptyBytes = new byte[0];

		private readonly int _originalLength;

		private int _remaining;
	}
}
