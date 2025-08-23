using System;
using System.Collections;
using DNA.Security.Cryptography.Utilities;

namespace DNA.Security.Cryptography.Asn1
{
	public class BerTaggedObject : DerTaggedObject
	{
		public BerTaggedObject(int tagNo, Asn1Encodable obj)
			: base(tagNo, obj)
		{
		}

		public BerTaggedObject(bool explicitly, int tagNo, Asn1Encodable obj)
			: base(explicitly, tagNo, obj)
		{
		}

		public BerTaggedObject(int tagNo)
			: base(false, tagNo, BerSequence.Empty)
		{
		}

		internal override void Encode(DerOutputStream derOut)
		{
			if (derOut is Asn1OutputStream || derOut is BerOutputStream)
			{
				derOut.WriteTag(160, this.tagNo);
				derOut.WriteByte(128);
				if (!base.IsEmpty())
				{
					if (!this.explicitly)
					{
						IEnumerable eObj;
						if (this.obj is Asn1OctetString)
						{
							if (this.obj is BerOctetString)
							{
								eObj = (BerOctetString)this.obj;
							}
							else
							{
								Asn1OctetString octs = (Asn1OctetString)this.obj;
								eObj = new BerOctetString(octs.GetOctets());
							}
						}
						else if (this.obj is Asn1Sequence)
						{
							eObj = (Asn1Sequence)this.obj;
						}
						else
						{
							if (!(this.obj is Asn1Set))
							{
								throw Platform.CreateNotImplementedException(this.obj.GetType().Name);
							}
							eObj = (Asn1Set)this.obj;
						}
						using (IEnumerator enumerator = eObj.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								object obj = enumerator.Current;
								Asn1Encodable o = (Asn1Encodable)obj;
								derOut.WriteObject(o);
							}
							goto IL_0119;
						}
					}
					derOut.WriteObject(this.obj);
				}
				IL_0119:
				derOut.WriteByte(0);
				derOut.WriteByte(0);
				return;
			}
			base.Encode(derOut);
		}
	}
}
