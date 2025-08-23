using System;
using System.IO;

namespace DNA.Security.Cryptography.Asn1
{
	public class DerSequence : Asn1Sequence
	{
		public static DerSequence FromVector(Asn1EncodableVector v)
		{
			if (v.Count >= 1)
			{
				return new DerSequence(v);
			}
			return DerSequence.Empty;
		}

		public DerSequence()
			: base(0)
		{
		}

		public DerSequence(Asn1Encodable obj)
			: base(1)
		{
			base.AddObject(obj);
		}

		public DerSequence(params Asn1Encodable[] v)
			: base(v.Length)
		{
			foreach (Asn1Encodable ae in v)
			{
				base.AddObject(ae);
			}
		}

		public DerSequence(Asn1EncodableVector v)
			: base(v.Count)
		{
			foreach (object obj in v)
			{
				Asn1Encodable ae = (Asn1Encodable)obj;
				base.AddObject(ae);
			}
		}

		internal override void Encode(DerOutputStream derOut)
		{
			MemoryStream bOut = new MemoryStream();
			DerOutputStream dOut = new DerOutputStream(bOut);
			foreach (object obj2 in this)
			{
				Asn1Encodable obj = (Asn1Encodable)obj2;
				dOut.WriteObject(obj);
			}
			dOut.Close();
			byte[] bytes = bOut.ToArray();
			derOut.WriteEncoded(48, bytes);
		}

		public static readonly DerSequence Empty = new DerSequence();
	}
}
