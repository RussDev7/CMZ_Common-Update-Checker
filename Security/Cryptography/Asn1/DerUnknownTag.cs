using System;
using DNA.Security.Cryptography.Utilities;

namespace DNA.Security.Cryptography.Asn1
{
	public class DerUnknownTag : Asn1Object
	{
		public DerUnknownTag(int tag, byte[] data)
			: this(false, tag, data)
		{
		}

		public DerUnknownTag(bool isConstructed, int tag, byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			this.isConstructed = isConstructed;
			this.tag = tag;
			this.data = data;
		}

		public bool IsConstructed
		{
			get
			{
				return this.isConstructed;
			}
		}

		public int Tag
		{
			get
			{
				return this.tag;
			}
		}

		public byte[] GetData()
		{
			return this.data;
		}

		internal override void Encode(DerOutputStream derOut)
		{
			derOut.WriteEncoded(this.isConstructed ? 32 : 0, this.tag, this.data);
		}

		protected override bool Asn1Equals(Asn1Object asn1Object)
		{
			DerUnknownTag other = asn1Object as DerUnknownTag;
			return other != null && (this.isConstructed == other.isConstructed && this.tag == other.tag) && Arrays.AreEqual(this.data, other.data);
		}

		protected override int Asn1GetHashCode()
		{
			return this.isConstructed.GetHashCode() ^ this.tag.GetHashCode() ^ Arrays.GetHashCode(this.data);
		}

		private readonly bool isConstructed;

		private readonly int tag;

		private readonly byte[] data;
	}
}
