using System;
using System.Text;
using DNA.Security.Cryptography.Utilities;

namespace DNA.Security.Cryptography.Asn1
{
	public class DerBitString : DerStringBase
	{
		internal static int GetPadBits(int bitString)
		{
			int val = 0;
			for (int i = 3; i >= 0; i--)
			{
				if (i != 0)
				{
					if (bitString >> i * 8 != 0)
					{
						val = (bitString >> i * 8) & 255;
						break;
					}
				}
				else if (bitString != 0)
				{
					val = bitString & 255;
					break;
				}
			}
			if (val == 0)
			{
				return 7;
			}
			int bits = 1;
			while (((val <<= 1) & 255) != 0)
			{
				bits++;
			}
			return 8 - bits;
		}

		internal static byte[] GetBytes(int bitString)
		{
			int bytes = 4;
			int i = 3;
			while (i >= 1 && (bitString & (255 << i * 8)) == 0)
			{
				bytes--;
				i--;
			}
			byte[] result = new byte[bytes];
			for (int j = 0; j < bytes; j++)
			{
				result[j] = (byte)((bitString >> j * 8) & 255);
			}
			return result;
		}

		public static DerBitString GetInstance(object obj)
		{
			if (obj == null || obj is DerBitString)
			{
				return (DerBitString)obj;
			}
			if (obj is Asn1OctetString)
			{
				byte[] bytes = ((Asn1OctetString)obj).GetOctets();
				int padBits = (int)bytes[0];
				byte[] data = new byte[bytes.Length - 1];
				Array.Copy(bytes, 1, data, 0, bytes.Length - 1);
				return new DerBitString(data, padBits);
			}
			if (obj is Asn1TaggedObject)
			{
				return DerBitString.GetInstance(((Asn1TaggedObject)obj).GetObject());
			}
			throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name);
		}

		public static DerBitString GetInstance(Asn1TaggedObject obj, bool explicitly)
		{
			return DerBitString.GetInstance(obj.GetObject());
		}

		internal DerBitString(byte data, int padBits)
		{
			this.data = new byte[] { data };
			this.padBits = padBits;
		}

		public DerBitString(byte[] data, int padBits)
		{
			this.data = data;
			this.padBits = padBits;
		}

		public DerBitString(byte[] data)
		{
			this.data = data;
		}

		public DerBitString(Asn1Encodable obj)
		{
			this.data = obj.GetDerEncoded();
		}

		public byte[] GetBytes()
		{
			return this.data;
		}

		public int PadBits
		{
			get
			{
				return this.padBits;
			}
		}

		public int IntValue
		{
			get
			{
				int value = 0;
				int i = 0;
				while (i != this.data.Length && i != 4)
				{
					value |= (int)(this.data[i] & byte.MaxValue) << 8 * i;
					i++;
				}
				return value;
			}
		}

		internal override void Encode(DerOutputStream derOut)
		{
			byte[] bytes = new byte[this.GetBytes().Length + 1];
			bytes[0] = (byte)this.PadBits;
			Array.Copy(this.GetBytes(), 0, bytes, 1, bytes.Length - 1);
			derOut.WriteEncoded(3, bytes);
		}

		protected override int Asn1GetHashCode()
		{
			return this.padBits.GetHashCode() ^ Arrays.GetHashCode(this.data);
		}

		protected override bool Asn1Equals(Asn1Object asn1Object)
		{
			DerBitString other = asn1Object as DerBitString;
			return other != null && this.padBits == other.padBits && Arrays.AreEqual(this.data, other.data);
		}

		public override string GetString()
		{
			StringBuilder buffer = new StringBuilder("#");
			byte[] str = base.GetDerEncoded();
			for (int i = 0; i != str.Length; i++)
			{
				uint ubyte = (uint)str[i];
				buffer.Append(DerBitString.table[(int)((UIntPtr)((ubyte >> 4) & 15U))]);
				buffer.Append(DerBitString.table[(int)(str[i] & 15)]);
			}
			return buffer.ToString();
		}

		private static readonly char[] table = new char[]
		{
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
			'A', 'B', 'C', 'D', 'E', 'F'
		};

		private readonly byte[] data;

		private readonly int padBits;
	}
}
