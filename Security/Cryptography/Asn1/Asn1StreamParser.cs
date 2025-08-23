using System;
using System.IO;

namespace DNA.Security.Cryptography.Asn1
{
	public class Asn1StreamParser
	{
		public Asn1StreamParser(Stream inStream)
			: this(inStream, int.MaxValue)
		{
		}

		public Asn1StreamParser(Stream inStream, int limit)
		{
			if (!inStream.CanRead)
			{
				throw new ArgumentException("Expected stream to be readable", "inStream");
			}
			this._in = inStream;
			this._limit = limit;
		}

		public Asn1StreamParser(byte[] encoding)
			: this(new MemoryStream(encoding, false), encoding.Length)
		{
		}

		public virtual IAsn1Convertible ReadObject()
		{
			int tag = this._in.ReadByte();
			if (tag == -1)
			{
				return null;
			}
			this.Set00Check(false);
			int tagNo = Asn1InputStream.ReadTagNumber(this._in, tag);
			bool isConstructed = (tag & 32) != 0;
			int length = Asn1InputStream.ReadLength(this._in, this._limit);
			if (length < 0)
			{
				if (!isConstructed)
				{
					throw new IOException("indefinite length primitive encoding encountered");
				}
				IndefiniteLengthInputStream indIn = new IndefiniteLengthInputStream(this._in);
				if ((tag & 64) != 0)
				{
					Asn1StreamParser sp2 = new Asn1StreamParser(indIn);
					return new BerApplicationSpecificParser(tagNo, sp2);
				}
				if ((tag & 128) != 0)
				{
					return new BerTaggedObjectParser(tag, tagNo, indIn);
				}
				Asn1StreamParser sp3 = new Asn1StreamParser(indIn);
				int num = tagNo;
				if (num == 4)
				{
					return new BerOctetStringParser(sp3);
				}
				switch (num)
				{
				case 16:
					return new BerSequenceParser(sp3);
				case 17:
					return new BerSetParser(sp3);
				default:
					throw new IOException("unknown BER object encountered");
				}
			}
			else
			{
				DefiniteLengthInputStream defIn = new DefiniteLengthInputStream(this._in, length);
				if ((tag & 64) != 0)
				{
					return new DerApplicationSpecific(isConstructed, tagNo, defIn.ToArray());
				}
				if ((tag & 128) != 0)
				{
					return new BerTaggedObjectParser(tag, tagNo, defIn);
				}
				if (isConstructed)
				{
					int num2 = tagNo;
					if (num2 == 4)
					{
						return new BerOctetStringParser(new Asn1StreamParser(defIn));
					}
					switch (num2)
					{
					case 16:
						return new DerSequenceParser(new Asn1StreamParser(defIn));
					case 17:
						return new DerSetParser(new Asn1StreamParser(defIn));
					default:
						return new DerUnknownTag(true, tagNo, defIn.ToArray());
					}
				}
				else
				{
					int num3 = tagNo;
					if (num3 == 4)
					{
						return new DerOctetStringParser(defIn);
					}
					return Asn1InputStream.CreatePrimitiveDerObject(tagNo, defIn.ToArray());
				}
			}
		}

		private void Set00Check(bool enabled)
		{
			if (this._in is IndefiniteLengthInputStream)
			{
				((IndefiniteLengthInputStream)this._in).SetEofOn00(enabled);
			}
		}

		internal Asn1EncodableVector ReadVector()
		{
			Asn1EncodableVector v = new Asn1EncodableVector(new Asn1Encodable[0]);
			IAsn1Convertible obj;
			while ((obj = this.ReadObject()) != null)
			{
				v.Add(new Asn1Encodable[] { obj.ToAsn1Object() });
			}
			return v;
		}

		private readonly Stream _in;

		private readonly int _limit;
	}
}
