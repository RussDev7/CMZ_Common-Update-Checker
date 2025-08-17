﻿using System;

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
			char[] array = new char[str.Length / 2];
			for (int num = 0; num != array.Length; num++)
			{
				array[num] = (char)(((int)str[2 * num] << 8) | (int)(str[2 * num + 1] & byte.MaxValue));
			}
			this.str = new string(array);
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
			DerBmpString derBmpString = asn1Object as DerBmpString;
			return derBmpString != null && this.str.Equals(derBmpString.str);
		}

		internal override void Encode(DerOutputStream derOut)
		{
			char[] array = this.str.ToCharArray();
			byte[] array2 = new byte[array.Length * 2];
			for (int num = 0; num != array.Length; num++)
			{
				array2[2 * num] = (byte)(array[num] >> 8);
				array2[2 * num + 1] = (byte)array[num];
			}
			derOut.WriteEncoded(30, array2);
		}

		private readonly string str;
	}
}
