using System;
using System.Text;
using DNA.Security.Cryptography.Utilities;

namespace DNA.Security.Cryptography.Asn1
{
	public class DerUniversalString : DerStringBase
	{
		public static DerUniversalString GetInstance(object obj)
		{
			if (obj == null || obj is DerUniversalString)
			{
				return (DerUniversalString)obj;
			}
			if (obj is Asn1OctetString)
			{
				return new DerUniversalString(((Asn1OctetString)obj).GetOctets());
			}
			throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name);
		}

		public static DerUniversalString GetInstance(Asn1TaggedObject obj, bool explicitly)
		{
			return DerUniversalString.GetInstance(obj.GetObject());
		}

		public DerUniversalString(byte[] str)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			this.str = str;
		}

		public override string GetString()
		{
			StringBuilder buffer = new StringBuilder("#");
			byte[] enc = base.GetDerEncoded();
			for (int i = 0; i != enc.Length; i++)
			{
				uint ubyte = (uint)enc[i];
				buffer.Append(DerUniversalString.table[(int)((UIntPtr)((ubyte >> 4) & 15U))]);
				buffer.Append(DerUniversalString.table[(int)(enc[i] & 15)]);
			}
			return buffer.ToString();
		}

		public byte[] GetOctets()
		{
			return (byte[])this.str.Clone();
		}

		internal override void Encode(DerOutputStream derOut)
		{
			derOut.WriteEncoded(28, this.str);
		}

		protected override bool Asn1Equals(Asn1Object asn1Object)
		{
			DerUniversalString other = asn1Object as DerUniversalString;
			return other != null && Arrays.AreEqual(this.str, other.str);
		}

		private static readonly char[] table = new char[]
		{
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
			'A', 'B', 'C', 'D', 'E', 'F'
		};

		private readonly byte[] str;
	}
}
