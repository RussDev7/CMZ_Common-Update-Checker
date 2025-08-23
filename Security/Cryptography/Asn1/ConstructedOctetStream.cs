using System;
using System.IO;
using DNA.Security.Cryptography.Utilities.IO;

namespace DNA.Security.Cryptography.Asn1
{
	internal class ConstructedOctetStream : BaseInputStream
	{
		internal ConstructedOctetStream(Asn1StreamParser parser)
		{
			this._parser = parser;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (this._currentStream == null)
			{
				if (!this._first)
				{
					return 0;
				}
				Asn1OctetStringParser s = (Asn1OctetStringParser)this._parser.ReadObject();
				if (s == null)
				{
					return 0;
				}
				this._first = false;
				this._currentStream = s.GetOctetStream();
			}
			int totalRead = 0;
			for (;;)
			{
				int numRead = this._currentStream.Read(buffer, offset + totalRead, count - totalRead);
				if (numRead > 0)
				{
					totalRead += numRead;
					if (totalRead == count)
					{
						break;
					}
				}
				else
				{
					Asn1OctetStringParser aos = (Asn1OctetStringParser)this._parser.ReadObject();
					if (aos == null)
					{
						goto Block_6;
					}
					this._currentStream = aos.GetOctetStream();
				}
			}
			return totalRead;
			Block_6:
			this._currentStream = null;
			return totalRead;
		}

		public override int ReadByte()
		{
			if (this._currentStream == null)
			{
				if (!this._first)
				{
					return 0;
				}
				Asn1OctetStringParser s = (Asn1OctetStringParser)this._parser.ReadObject();
				if (s == null)
				{
					return 0;
				}
				this._first = false;
				this._currentStream = s.GetOctetStream();
			}
			int b;
			for (;;)
			{
				b = this._currentStream.ReadByte();
				if (b >= 0)
				{
					break;
				}
				Asn1OctetStringParser aos = (Asn1OctetStringParser)this._parser.ReadObject();
				if (aos == null)
				{
					goto Block_5;
				}
				this._currentStream = aos.GetOctetStream();
			}
			return b;
			Block_5:
			this._currentStream = null;
			return -1;
		}

		private readonly Asn1StreamParser _parser;

		private bool _first = true;

		private Stream _currentStream;
	}
}
