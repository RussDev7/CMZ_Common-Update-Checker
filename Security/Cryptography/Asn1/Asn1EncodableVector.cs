using System;
using System.Collections;
using System.Collections.Generic;

namespace DNA.Security.Cryptography.Asn1
{
	public class Asn1EncodableVector : IEnumerable
	{
		public static Asn1EncodableVector FromEnumerable(IEnumerable e)
		{
			Asn1EncodableVector v = new Asn1EncodableVector(new Asn1Encodable[0]);
			foreach (object obj2 in e)
			{
				Asn1Encodable obj = (Asn1Encodable)obj2;
				v.Add(new Asn1Encodable[] { obj });
			}
			return v;
		}

		public Asn1EncodableVector(params Asn1Encodable[] v)
		{
			this.Add(v);
		}

		public void Add(params Asn1Encodable[] objs)
		{
			foreach (Asn1Encodable obj in objs)
			{
				this.v.Add(obj);
			}
		}

		public Asn1Encodable this[int index]
		{
			get
			{
				return this.v[index];
			}
		}

		[Obsolete("Use 'object[index]' syntax instead")]
		public Asn1Encodable Get(int index)
		{
			return this[index];
		}

		[Obsolete("Use 'Count' property instead")]
		public int Size
		{
			get
			{
				return this.v.Count;
			}
		}

		public int Count
		{
			get
			{
				return this.v.Count;
			}
		}

		public IEnumerator GetEnumerator()
		{
			return this.v.GetEnumerator();
		}

		private List<Asn1Encodable> v = new List<Asn1Encodable>();
	}
}
