using System;

namespace DNA.Security.Cryptography.Asn1
{
	public class DerBmpString : DerStringBase
	{
		public static DerBmpString GetInstance(object obj)
		{
			if (obj == null || obj is DerBmpString)
			{
				return (DerBmpString)obj;
			}
			if (obj is Asn1OctetString)
			{
				return new DerBmpString(((Asn1OctetString)obj).GetOctets());
			}
			if (obj is Asn1TaggedObject)
			{
				return DerBmpString.GetInstance(((Asn1TaggedObject)obj).GetObject());
			}
			throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name);
		}

		public static DerBmpString GetInstance(Asn1TaggedObject obj, bool explicitly)
		{
			return DerBmpString.GetInstance(obj.GetObject());
		}

		public DerBmpString(byte[] str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			char[] cs = new char[str.Length / 2];
			for (int i = 0; i != cs.Length; i++)
			{
				cs[i] = (char)(((int)str[2 * i] << 8) | (int)(str[2 * i + 1] & byte.MaxValue));
			}
			this.str = new string(cs);
		}

		public DerBmpString(string str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			this.str = str;
		}

		public override string GetString()
		{
			return this.str;
		}

		protected override bool Asn1Equals(Asn1Object asn1Object)
		{
			DerBmpString other = asn1Object as DerBmpString;
			return other != null && this.str.Equals(other.str);
		}

		internal override void Encode(DerOutputStream derOut)
		{
			char[] c = this.str.ToCharArray();
			byte[] b = new byte[c.Length * 2];
			for (int i = 0; i != c.Length; i++)
			{
				b[2 * i] = (byte)(c[i] >> 8);
				b[2 * i + 1] = (byte)c[i];
			}
			derOut.WriteEncoded(30, b);
		}

		private readonly string str;
	}
}
