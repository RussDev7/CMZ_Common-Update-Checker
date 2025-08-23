using System;
using DNA.Security.Cryptography.Math;
using DNA.Security.Cryptography.Utilities;

namespace DNA.Security.Cryptography.Asn1
{
	public class DerInteger : Asn1Object
	{
		public static DerInteger GetInstance(object obj)
		{
			if (obj == null)
			{
				return null;
			}
			DerInteger i = obj as DerInteger;
			if (i != null)
			{
				return i;
			}
			Asn1OctetString octs = obj as Asn1OctetString;
			if (octs != null)
			{
				return new DerInteger(octs.GetOctets());
			}
			Asn1TaggedObject tagged = obj as Asn1TaggedObject;
			if (tagged != null)
			{
				return DerInteger.GetInstance(tagged.GetObject());
			}
			throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name);
		}

		public static DerInteger GetInstance(Asn1TaggedObject obj, bool explicitly)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			return DerInteger.GetInstance(obj.GetObject());
		}

		public DerInteger(int value)
		{
			this.bytes = BigInteger.ValueOf((long)value).ToByteArray();
		}

		public DerInteger(BigInteger value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			this.bytes = value.ToByteArray();
		}

		public DerInteger(byte[] bytes)
		{
			this.bytes = bytes;
		}

		public BigInteger Value
		{
			get
			{
				return new BigInteger(this.bytes);
			}
		}

		public BigInteger PositiveValue
		{
			get
			{
				return new BigInteger(1, this.bytes);
			}
		}

		internal override void Encode(DerOutputStream derOut)
		{
			derOut.WriteEncoded(2, this.bytes);
		}

		protected override int Asn1GetHashCode()
		{
			return Arrays.GetHashCode(this.bytes);
		}

		protected override bool Asn1Equals(Asn1Object asn1Object)
		{
			DerInteger other = asn1Object as DerInteger;
			return other != null && Arrays.AreEqual(this.bytes, other.bytes);
		}

		public override string ToString()
		{
			return this.Value.ToString();
		}

		private readonly byte[] bytes;
	}
}
