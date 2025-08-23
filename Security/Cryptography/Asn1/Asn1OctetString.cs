using System;
using System.Collections.Generic;
using System.IO;
using DNA.Security.Cryptography.Utilities;
using DNA.Security.Cryptography.Utilities.Encoders;
using DNA.Text;

namespace DNA.Security.Cryptography.Asn1
{
	public abstract class Asn1OctetString : Asn1Object, Asn1OctetStringParser, IAsn1Convertible
	{
		public static Asn1OctetString GetInstance(Asn1TaggedObject obj, bool explicitly)
		{
			return Asn1OctetString.GetInstance(obj.GetObject());
		}

		public static Asn1OctetString GetInstance(object obj)
		{
			if (obj == null || obj is Asn1OctetString)
			{
				return (Asn1OctetString)obj;
			}
			if (obj is Asn1TaggedObject)
			{
				return Asn1OctetString.GetInstance(((Asn1TaggedObject)obj).GetObject());
			}
			if (obj is Asn1Sequence)
			{
				List<object> v = new List<object>();
				foreach (object o in ((Asn1Sequence)obj))
				{
					v.Add(o);
				}
				return new BerOctetString(v);
			}
			throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name);
		}

		internal Asn1OctetString(byte[] str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			this.str = str;
		}

		internal Asn1OctetString(Asn1Encodable obj)
		{
			try
			{
				this.str = obj.GetDerEncoded();
			}
			catch (IOException e)
			{
				throw new ArgumentException("Error processing object : " + e.ToString());
			}
		}

		public Stream GetOctetStream()
		{
			return new MemoryStream(this.str, false);
		}

		public Asn1OctetStringParser Parser
		{
			get
			{
				return this;
			}
		}

		public virtual byte[] GetOctets()
		{
			return this.str;
		}

		protected override int Asn1GetHashCode()
		{
			return Arrays.GetHashCode(this.GetOctets());
		}

		protected override bool Asn1Equals(Asn1Object asn1Object)
		{
			DerOctetString other = asn1Object as DerOctetString;
			return other != null && Arrays.AreEqual(this.GetOctets(), other.GetOctets());
		}

		public override string ToString()
		{
			byte[] hex = Hex.Encode(this.str);
			return "#" + ASCIIEncoder.GetString(hex, 0, hex.Length);
		}

		internal byte[] str;
	}
}
