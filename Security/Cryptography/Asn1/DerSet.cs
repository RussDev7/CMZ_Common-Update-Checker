using System;
using System.IO;

namespace DNA.Security.Cryptography.Asn1
{
	public class DerSet : Asn1Set
	{
		public static DerSet FromVector(Asn1EncodableVector v)
		{
			if (v.Count >= 1)
			{
				return new DerSet(v);
			}
			return DerSet.Empty;
		}

		internal static DerSet FromVector(Asn1EncodableVector v, bool needsSorting)
		{
			if (v.Count >= 1)
			{
				return new DerSet(v, needsSorting);
			}
			return DerSet.Empty;
		}

		public DerSet()
			: base(0)
		{
		}

		public DerSet(Asn1Encodable obj)
			: base(1)
		{
			base.AddObject(obj);
		}

		public DerSet(params Asn1Encodable[] v)
			: base(v.Length)
		{
			foreach (Asn1Encodable o in v)
			{
				base.AddObject(o);
			}
			base.Sort();
		}

		public DerSet(Asn1EncodableVector v)
			: this(v, true)
		{
		}

		internal DerSet(Asn1EncodableVector v, bool needsSorting)
			: base(v.Count)
		{
			foreach (object obj in v)
			{
				Asn1Encodable o = (Asn1Encodable)obj;
				base.AddObject(o);
			}
			if (needsSorting)
			{
				base.Sort();
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
			derOut.WriteEncoded(49, bytes);
		}

		public static readonly DerSet Empty = new DerSet();
	}
}
