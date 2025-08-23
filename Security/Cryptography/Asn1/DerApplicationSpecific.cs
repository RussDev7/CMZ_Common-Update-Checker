using System;
using System.IO;
using DNA.Security.Cryptography.Utilities;

namespace DNA.Security.Cryptography.Asn1
{
	public class DerApplicationSpecific : Asn1Object
	{
		internal DerApplicationSpecific(bool isConstructed, int tag, byte[] octets)
		{
			this.isConstructed = isConstructed;
			this.tag = tag;
			this.octets = octets;
		}

		public DerApplicationSpecific(int tag, byte[] octets)
			: this(false, tag, octets)
		{
		}

		public DerApplicationSpecific(int tag, Asn1Encodable obj)
			: this(true, tag, obj)
		{
		}

		public DerApplicationSpecific(bool isExplicit, int tag, Asn1Encodable obj)
		{
			byte[] data = obj.GetDerEncoded();
			this.isConstructed = isExplicit;
			this.tag = tag;
			if (isExplicit)
			{
				this.octets = data;
				return;
			}
			int lenBytes = this.GetLengthOfLength(data);
			byte[] tmp = new byte[data.Length - lenBytes];
			Array.Copy(data, lenBytes, tmp, 0, tmp.Length);
			this.octets = tmp;
		}

		public DerApplicationSpecific(int tagNo, Asn1EncodableVector vec)
		{
			this.tag = tagNo;
			this.isConstructed = true;
			MemoryStream bOut = new MemoryStream();
			for (int i = 0; i != vec.Count; i++)
			{
				try
				{
					byte[] bs = vec[i].GetEncoded();
					bOut.Write(bs, 0, bs.Length);
				}
				catch (IOException e)
				{
					throw new InvalidOperationException("malformed object", e);
				}
			}
			this.octets = bOut.ToArray();
		}

		private int GetLengthOfLength(byte[] data)
		{
			int count = 2;
			while ((data[count - 1] & 128) != 0)
			{
				count++;
			}
			return count;
		}

		public bool IsConstructed()
		{
			return this.isConstructed;
		}

		public byte[] GetContents()
		{
			return this.octets;
		}

		public int ApplicationTag
		{
			get
			{
				return this.tag;
			}
		}

		public Asn1Object GetObject()
		{
			return Asn1Object.FromByteArray(this.GetContents());
		}

		public Asn1Object GetObject(int derTagNo)
		{
			if (derTagNo >= 31)
			{
				throw new IOException("unsupported tag number");
			}
			byte[] orig = base.GetEncoded();
			byte[] tmp = this.ReplaceTagNumber(derTagNo, orig);
			if ((orig[0] & 32) != 0)
			{
				byte[] array = tmp;
				int num = 0;
				array[num] |= 32;
			}
			return Asn1Object.FromByteArray(tmp);
		}

		internal override void Encode(DerOutputStream derOut)
		{
			int classBits = 64;
			if (this.isConstructed)
			{
				classBits |= 32;
			}
			derOut.WriteEncoded(classBits, this.tag, this.octets);
		}

		protected override bool Asn1Equals(Asn1Object asn1Object)
		{
			DerApplicationSpecific other = asn1Object as DerApplicationSpecific;
			return other != null && (this.isConstructed == other.isConstructed && this.tag == other.tag) && Arrays.AreEqual(this.octets, other.octets);
		}

		protected override int Asn1GetHashCode()
		{
			return this.isConstructed.GetHashCode() ^ this.tag.GetHashCode() ^ Arrays.GetHashCode(this.octets);
		}

		private byte[] ReplaceTagNumber(int newTag, byte[] input)
		{
			int tagNo = (int)(input[0] & 31);
			int index = 1;
			if (tagNo == 31)
			{
				tagNo = 0;
				int b = (int)(input[index++] & byte.MaxValue);
				if ((b & 127) == 0)
				{
					throw new InvalidOperationException("corrupted stream - invalid high tag number found");
				}
				while (b >= 0 && (b & 128) != 0)
				{
					tagNo |= b & 127;
					tagNo <<= 7;
					b = (int)(input[index++] & byte.MaxValue);
				}
				tagNo |= b & 127;
			}
			byte[] tmp = new byte[input.Length - index + 1];
			Array.Copy(input, index, tmp, 1, tmp.Length - 1);
			tmp[0] = (byte)newTag;
			return tmp;
		}

		private readonly bool isConstructed;

		private readonly int tag;

		private readonly byte[] octets;
	}
}
