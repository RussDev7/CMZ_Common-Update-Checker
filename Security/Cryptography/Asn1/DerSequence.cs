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
			foreach (Asn1Encodable asn1Encodable in v)
			{
				base.AddObject(asn1Encodable);
			}
		}

		public DerSequence(Asn1EncodableVector v)
			: base(v.Count)
		{
			foreach (object obj in v)
			{
				Asn1Encodable asn1Encodable = (Asn1Encodable)obj;
				base.AddObject(asn1Encodable);
			}
		}

		internal override void Encode(DerOutputStream derOut)
		{
			MemoryStream memoryStream = new MemoryStream();
			DerOutputStream derOutputStream = new DerOutputStream(memoryStream);
			foreach (object obj in this)
			{
				Asn1Encodable asn1Encodable = (Asn1Encodable)obj;
				derOutputStream.WriteObject(asn1Encodable);
			}
			derOutputStream.Close();
			byte[] array = memoryStream.ToArray();
			derOut.WriteEncoded(48, array);
		}

		public static readonly DerSequence Empty = new DerSequence();
	}
}
