using System;
using System.IO;

namespace DNA.Security.Cryptography.Asn1
{
	public abstract class Asn1Encodable : IAsn1Convertible
	{
		public byte[] GetEncoded()
		{
			MemoryStream bOut = new MemoryStream();
			Asn1OutputStream aOut = new Asn1OutputStream(bOut);
			aOut.WriteObject(this);
			return bOut.ToArray();
		}

		public byte[] GetEncoded(string encoding)
		{
			if (encoding.Equals("DER"))
			{
				MemoryStream bOut = new MemoryStream();
				DerOutputStream dOut = new DerOutputStream(bOut);
				dOut.WriteObject(this);
				return bOut.ToArray();
			}
			return this.GetEncoded();
		}

		public byte[] GetDerEncoded()
		{
			byte[] array;
			try
			{
				array = this.GetEncoded("DER");
			}
			catch (IOException)
			{
				array = null;
			}
			return array;
		}

		public sealed override int GetHashCode()
		{
			return this.ToAsn1Object().CallAsn1GetHashCode();
		}

		public sealed override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}
			IAsn1Convertible other = obj as IAsn1Convertible;
			if (other == null)
			{
				return false;
			}
			Asn1Object o = this.ToAsn1Object();
			Asn1Object o2 = other.ToAsn1Object();
			return o == o2 || o.CallAsn1Equals(o2);
		}

		public abstract Asn1Object ToAsn1Object();

		public const string Der = "DER";

		public const string Ber = "BER";
	}
}
