using System;
using System.IO;
using DNA.Security.Cryptography.Utilities;

namespace DNA.Security.Cryptography.Asn1
{
	public class BerTaggedObjectParser : Asn1TaggedObjectParser, IAsn1Convertible
	{
		internal BerTaggedObjectParser(int baseTag, int tagNumber, Stream contentStream)
		{
			if (!contentStream.CanRead)
			{
				throw new ArgumentException("Expected stream to be readable", "contentStream");
			}
			this._baseTag = baseTag;
			this._tagNumber = tagNumber;
			this._contentStream = contentStream;
			this._indefiniteLength = contentStream is IndefiniteLengthInputStream;
		}

		public bool IsConstructed
		{
			get
			{
				return (this._baseTag & 32) != 0;
			}
		}

		public int TagNo
		{
			get
			{
				return this._tagNumber;
			}
		}

		public IAsn1Convertible GetObjectParser(int tag, bool isExplicit)
		{
			if (isExplicit)
			{
				return new Asn1StreamParser(this._contentStream).ReadObject();
			}
			if (tag != 4)
			{
				switch (tag)
				{
				case 16:
					if (this._indefiniteLength)
					{
						return new BerSequenceParser(new Asn1StreamParser(this._contentStream));
					}
					return new DerSequenceParser(new Asn1StreamParser(this._contentStream));
				case 17:
					if (this._indefiniteLength)
					{
						return new BerSetParser(new Asn1StreamParser(this._contentStream));
					}
					return new DerSetParser(new Asn1StreamParser(this._contentStream));
				default:
					throw Platform.CreateNotImplementedException("implicit tagging");
				}
			}
			else
			{
				if (this._indefiniteLength || this.IsConstructed)
				{
					return new BerOctetStringParser(new Asn1StreamParser(this._contentStream));
				}
				return new DerOctetStringParser((DefiniteLengthInputStream)this._contentStream);
			}
		}

		private Asn1EncodableVector rLoadVector(Stream inStream)
		{
			Asn1EncodableVector asn1EncodableVector;
			try
			{
				asn1EncodableVector = new Asn1StreamParser(inStream).ReadVector();
			}
			catch (IOException e)
			{
				throw new InvalidOperationException(e.Message, e);
			}
			return asn1EncodableVector;
		}

		public Asn1Object ToAsn1Object()
		{
			if (this._indefiniteLength)
			{
				Asn1EncodableVector v = this.rLoadVector(this._contentStream);
				if (v.Count != 1)
				{
					return new BerTaggedObject(false, this._tagNumber, BerSequence.FromVector(v));
				}
				return new BerTaggedObject(true, this._tagNumber, v[0]);
			}
			else
			{
				if (!this.IsConstructed)
				{
					Asn1Object asn1Object;
					try
					{
						DefiniteLengthInputStream defIn = (DefiniteLengthInputStream)this._contentStream;
						asn1Object = new DerTaggedObject(false, this._tagNumber, new DerOctetString(defIn.ToArray()));
					}
					catch (IOException e)
					{
						throw new InvalidOperationException(e.Message, e);
					}
					return asn1Object;
				}
				Asn1EncodableVector v2 = this.rLoadVector(this._contentStream);
				if (v2.Count != 1)
				{
					return new DerTaggedObject(false, this._tagNumber, DerSequence.FromVector(v2));
				}
				return new DerTaggedObject(true, this._tagNumber, v2[0]);
			}
		}

		private int _baseTag;

		private int _tagNumber;

		private Stream _contentStream;

		private bool _indefiniteLength;
	}
}
