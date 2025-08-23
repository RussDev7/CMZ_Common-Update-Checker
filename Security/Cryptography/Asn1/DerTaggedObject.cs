using System;

namespace DNA.Security.Cryptography.Asn1
{
	public class DerTaggedObject : Asn1TaggedObject
	{
		public DerTaggedObject(int tagNo, Asn1Encodable obj)
			: base(tagNo, obj)
		{
		}

		public DerTaggedObject(bool explicitly, int tagNo, Asn1Encodable obj)
			: base(explicitly, tagNo, obj)
		{
		}

		public DerTaggedObject(int tagNo)
			: base(false, tagNo, DerSequence.Empty)
		{
		}

		internal override void Encode(DerOutputStream derOut)
		{
			if (base.IsEmpty())
			{
				derOut.WriteEncoded(160, this.tagNo, new byte[0]);
				return;
			}
			byte[] bytes = this.obj.GetDerEncoded();
			if (this.explicitly)
			{
				derOut.WriteEncoded(160, this.tagNo, bytes);
				return;
			}
			int flags = (int)((bytes[0] & 32) | 128);
			derOut.WriteTag(flags, this.tagNo);
			derOut.Write(bytes, 1, bytes.Length - 1);
		}
	}
}
