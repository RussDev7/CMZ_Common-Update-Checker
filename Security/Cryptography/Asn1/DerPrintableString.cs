using System;
using DNA.Text;

namespace DNA.Security.Cryptography.Asn1
{
	public class DerPrintableString : DerStringBase
	{
		public static DerPrintableString GetInstance(object obj)
		{
			if (obj == null || obj is DerPrintableString)
			{
				return (DerPrintableString)obj;
			}
			if (obj is Asn1OctetString)
			{
				return new DerPrintableString(((Asn1OctetString)obj).GetOctets());
			}
			if (obj is Asn1TaggedObject)
			{
				return DerPrintableString.GetInstance(((Asn1TaggedObject)obj).GetObject());
			}
			throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name);
		}

		public static DerPrintableString GetInstance(Asn1TaggedObject obj, bool explicitly)
		{
			return DerPrintableString.GetInstance(obj.GetObject());
		}

		public DerPrintableString(byte[] str)
			: this(ASCIIEncoder.GetString(str, 0, str.Length), false)
		{
		}

		public DerPrintableString(string str)
			: this(str, false)
		{
		}

		public DerPrintableString(string str, bool validate)
		{
			if (str == null)
			{
				throw new ArgumentNullException("str");
			}
			if (validate && !DerPrintableString.IsPrintableString(str))
			{
				throw new ArgumentException("string contains illegal characters", "str");
			}
			this.str = str;
		}

		public override string GetString()
		{
			return this.str;
		}

		public byte[] GetOctets()
		{
			return ASCIIEncoder.GetBytes(this.str);
		}

		internal override void Encode(DerOutputStream derOut)
		{
			derOut.WriteEncoded(19, this.GetOctets());
		}

		protected override bool Asn1Equals(Asn1Object asn1Object)
		{
			DerPrintableString other = asn1Object as DerPrintableString;
			return other != null && this.str.Equals(other.str);
		}

		public static bool IsPrintableString(string str)
		{
			int i = 0;
			while (i < str.Length)
			{
				char ch = str[i];
				if (ch <= '\u007f')
				{
					if (!char.IsLetterOrDigit(ch))
					{
						char c = ch;
						switch (c)
						{
						case ' ':
						case '\'':
						case '(':
						case ')':
						case '+':
						case ',':
						case '-':
						case '.':
						case '/':
							goto IL_0092;
						case '!':
						case '"':
						case '#':
						case '$':
						case '%':
						case '&':
						case '*':
							break;
						default:
							if (c == ':')
							{
								goto IL_0092;
							}
							switch (c)
							{
							case '=':
							case '?':
								goto IL_0092;
							}
							break;
						}
						return false;
					}
					IL_0092:
					i++;
					continue;
				}
				return false;
			}
			return true;
		}

		private readonly string str;
	}
}
