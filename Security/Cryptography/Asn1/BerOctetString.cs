using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DNA.Security.Cryptography.Asn1
{
	public class BerOctetString : DerOctetString, IEnumerable
	{
		private static byte[] ToBytes(IEnumerable octs)
		{
			MemoryStream bOut = new MemoryStream();
			foreach (object obj in octs)
			{
				DerOctetString o = (DerOctetString)obj;
				byte[] octets = o.GetOctets();
				bOut.Write(octets, 0, octets.Length);
			}
			return bOut.ToArray();
		}

		public BerOctetString(byte[] str)
			: base(str)
		{
		}

		public BerOctetString(IEnumerable octets)
			: base(BerOctetString.ToBytes(octets))
		{
			this.octs = octets;
		}

		public BerOctetString(Asn1Object obj)
			: base(obj)
		{
		}

		public BerOctetString(Asn1Encodable obj)
			: base(obj.ToAsn1Object())
		{
		}

		public override byte[] GetOctets()
		{
			return this.str;
		}

		public IEnumerator GetEnumerator()
		{
			if (this.octs == null)
			{
				return this.GenerateOcts().GetEnumerator();
			}
			return this.octs.GetEnumerator();
		}

		[Obsolete("Use GetEnumerator() instead")]
		public IEnumerator GetObjects()
		{
			return this.GetEnumerator();
		}

		private List<DerOctetString> GenerateOcts()
		{
			List<DerOctetString> vec = new List<DerOctetString>();
			for (int i = 0; i < this.str.Length; i += 1000)
			{
				int end = Math.Min(this.str.Length, i + 1000);
				byte[] nStr = new byte[end - i];
				Array.Copy(this.str, i, nStr, 0, nStr.Length);
				vec.Add(new DerOctetString(nStr));
			}
			return vec;
		}

		internal override void Encode(DerOutputStream derOut)
		{
			if (derOut is Asn1OutputStream || derOut is BerOutputStream)
			{
				derOut.WriteByte(36);
				derOut.WriteByte(128);
				foreach (object obj in this)
				{
					DerOctetString oct = (DerOctetString)obj;
					derOut.WriteObject(oct);
				}
				derOut.WriteByte(0);
				derOut.WriteByte(0);
				return;
			}
			base.Encode(derOut);
		}

		private const int MaxLength = 1000;

		private readonly IEnumerable octs;
	}
}
