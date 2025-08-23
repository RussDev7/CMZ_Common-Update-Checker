using System;
using System.IO;
using DNA.Security.Cryptography.Asn1.Utilities;

namespace DNA.Security.Cryptography.Asn1
{
	public class Asn1InputStream : FilterStream
	{
		public Asn1InputStream(Stream inputStream)
			: this(inputStream, int.MaxValue)
		{
		}

		public Asn1InputStream(Stream inputStream, int limit)
			: base(inputStream)
		{
			this.limit = limit;
		}

		public Asn1InputStream(byte[] input)
			: this(new MemoryStream(input, false), input.Length)
		{
		}

		internal Asn1EncodableVector BuildEncodableVector()
		{
			Asn1EncodableVector v = new Asn1EncodableVector(new Asn1Encodable[0]);
			Asn1Object o;
			while ((o = this.ReadObject()) != null)
			{
				v.Add(new Asn1Encodable[] { o });
			}
			return v;
		}

		internal virtual Asn1EncodableVector BuildDerEncodableVector(DefiniteLengthInputStream dIn)
		{
			return new Asn1InputStream(dIn).BuildEncodableVector();
		}

		internal virtual DerSequence CreateDerSequence(DefiniteLengthInputStream dIn)
		{
			return DerSequence.FromVector(this.BuildDerEncodableVector(dIn));
		}

		internal virtual DerSet CreateDerSet(DefiniteLengthInputStream dIn)
		{
			return DerSet.FromVector(this.BuildDerEncodableVector(dIn), false);
		}

		public Asn1Object ReadObject()
		{
			int tag = this.ReadByte();
			if (tag <= 0)
			{
				if (tag == 0)
				{
					throw new IOException("unexpected end-of-contents marker");
				}
				return null;
			}
			else
			{
				int tagNo = Asn1InputStream.ReadTagNumber(this, tag);
				bool isConstructed = (tag & 32) != 0;
				int length = Asn1InputStream.ReadLength(this, this.limit);
				if (length < 0)
				{
					if (!isConstructed)
					{
						throw new IOException("indefinite length primitive encoding encountered");
					}
					IndefiniteLengthInputStream indIn = new IndefiniteLengthInputStream(this);
					if ((tag & 64) != 0)
					{
						Asn1StreamParser sp2 = new Asn1StreamParser(indIn);
						return new BerApplicationSpecificParser(tagNo, sp2).ToAsn1Object();
					}
					if ((tag & 128) != 0)
					{
						return new BerTaggedObjectParser(tag, tagNo, indIn).ToAsn1Object();
					}
					Asn1StreamParser sp3 = new Asn1StreamParser(indIn);
					int num = tagNo;
					if (num == 4)
					{
						return new BerOctetStringParser(sp3).ToAsn1Object();
					}
					switch (num)
					{
					case 16:
						return new BerSequenceParser(sp3).ToAsn1Object();
					case 17:
						return new BerSetParser(sp3).ToAsn1Object();
					default:
						throw new IOException("unknown BER object encountered");
					}
				}
				else
				{
					DefiniteLengthInputStream defIn = new DefiniteLengthInputStream(this, length);
					if ((tag & 64) != 0)
					{
						return new DerApplicationSpecific(isConstructed, tagNo, defIn.ToArray());
					}
					if ((tag & 128) != 0)
					{
						return new BerTaggedObjectParser(tag, tagNo, defIn).ToAsn1Object();
					}
					if (!isConstructed)
					{
						return Asn1InputStream.CreatePrimitiveDerObject(tagNo, defIn.ToArray());
					}
					int num2 = tagNo;
					if (num2 == 4)
					{
						return new BerOctetString(this.BuildDerEncodableVector(defIn));
					}
					switch (num2)
					{
					case 16:
						return this.CreateDerSequence(defIn);
					case 17:
						return this.CreateDerSet(defIn);
					default:
						return new DerUnknownTag(true, tagNo, defIn.ToArray());
					}
				}
			}
		}

		internal static int ReadTagNumber(Stream s, int tag)
		{
			int tagNo = tag & 31;
			if (tagNo == 31)
			{
				tagNo = 0;
				int b = s.ReadByte();
				if ((b & 127) == 0)
				{
					throw new IOException("corrupted stream - invalid high tag number found");
				}
				while (b >= 0 && (b & 128) != 0)
				{
					tagNo |= b & 127;
					tagNo <<= 7;
					b = s.ReadByte();
				}
				if (b < 0)
				{
					throw new EndOfStreamException("EOF found inside tag value.");
				}
				tagNo |= b & 127;
			}
			return tagNo;
		}

		internal static int ReadLength(Stream s, int limit)
		{
			int length = s.ReadByte();
			if (length < 0)
			{
				throw new EndOfStreamException("EOF found when length expected");
			}
			if (length == 128)
			{
				return -1;
			}
			if (length > 127)
			{
				int size = length & 127;
				if (size > 4)
				{
					throw new IOException("DER length more than 4 bytes");
				}
				length = 0;
				for (int i = 0; i < size; i++)
				{
					int next = s.ReadByte();
					if (next < 0)
					{
						throw new EndOfStreamException("EOF found reading length");
					}
					length = (length << 8) + next;
				}
				if (length < 0)
				{
					throw new IOException("Corrupted stream - negative length found");
				}
				if (length >= limit)
				{
					throw new IOException("Corrupted stream - out of bounds length found");
				}
			}
			return length;
		}

		internal static Asn1Object CreatePrimitiveDerObject(int tagNo, byte[] bytes)
		{
			switch (tagNo)
			{
			case 1:
				return new DerBoolean(bytes);
			case 2:
				return new DerInteger(bytes);
			case 3:
			{
				int padBits = (int)bytes[0];
				byte[] data = new byte[bytes.Length - 1];
				Array.Copy(bytes, 1, data, 0, bytes.Length - 1);
				return new DerBitString(data, padBits);
			}
			case 4:
				return new DerOctetString(bytes);
			case 5:
				return DerNull.Instance;
			case 6:
				return new DerObjectIdentifier(bytes);
			case 10:
				return new DerEnumerated(bytes);
			case 12:
				return new DerUtf8String(bytes);
			case 18:
				return new DerNumericString(bytes);
			case 19:
				return new DerPrintableString(bytes);
			case 20:
				return new DerT61String(bytes);
			case 22:
				return new DerIA5String(bytes);
			case 23:
				return new DerUtcTime(bytes);
			case 24:
				return new DerGeneralizedTime(bytes);
			case 26:
				return new DerVisibleString(bytes);
			case 27:
				return new DerGeneralString(bytes);
			case 28:
				return new DerUniversalString(bytes);
			case 30:
				return new DerBmpString(bytes);
			}
			return new DerUnknownTag(false, tagNo, bytes);
		}

		private readonly int limit;
	}
}
