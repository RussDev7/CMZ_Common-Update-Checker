using System;
using System.Collections;
using System.Collections.Generic;
using DNA.Security.Cryptography.Utilities.Collections;

namespace DNA.Security.Cryptography.Asn1
{
	public abstract class Asn1Set : Asn1Object, IEnumerable
	{
		public static Asn1Set GetInstance(object obj)
		{
			if (obj == null || obj is Asn1Set)
			{
				return (Asn1Set)obj;
			}
			throw new ArgumentException("Unknown object in GetInstance: " + obj.GetType().FullName, "obj");
		}

		public static Asn1Set GetInstance(Asn1TaggedObject obj, bool explicitly)
		{
			Asn1Object inner = obj.GetObject();
			if (explicitly)
			{
				if (!obj.IsExplicit())
				{
					throw new ArgumentException("object implicit - explicit expected.");
				}
				return (Asn1Set)inner;
			}
			else
			{
				if (obj.IsExplicit())
				{
					return new DerSet(inner);
				}
				if (inner is Asn1Set)
				{
					return (Asn1Set)inner;
				}
				if (inner is Asn1Sequence)
				{
					Asn1EncodableVector v = new Asn1EncodableVector(new Asn1Encodable[0]);
					Asn1Sequence s = (Asn1Sequence)inner;
					foreach (object obj2 in s)
					{
						Asn1Encodable ae = (Asn1Encodable)obj2;
						v.Add(new Asn1Encodable[] { ae });
					}
					return new DerSet(v, false);
				}
				throw new ArgumentException("Unknown object in GetInstance: " + obj.GetType().FullName, "obj");
			}
		}

		protected internal Asn1Set(int capacity)
		{
			this._set = new List<Asn1Encodable>(capacity);
		}

		public virtual IEnumerator GetEnumerator()
		{
			return this._set.GetEnumerator();
		}

		[Obsolete("Use GetEnumerator() instead")]
		public IEnumerator GetObjects()
		{
			return this.GetEnumerator();
		}

		public virtual Asn1Encodable this[int index]
		{
			get
			{
				return this._set[index];
			}
		}

		[Obsolete("Use 'object[index]' syntax instead")]
		public Asn1Encodable GetObjectAt(int index)
		{
			return this[index];
		}

		[Obsolete("Use 'Count' property instead")]
		public int Size
		{
			get
			{
				return this.Count;
			}
		}

		public virtual int Count
		{
			get
			{
				return this._set.Count;
			}
		}

		public Asn1SetParser Parser
		{
			get
			{
				return new Asn1Set.Asn1SetParserImpl(this);
			}
		}

		protected override int Asn1GetHashCode()
		{
			int hc = this.Count;
			foreach (object o in this)
			{
				hc *= 17;
				if (o != null)
				{
					hc ^= o.GetHashCode();
				}
			}
			return hc;
		}

		protected override bool Asn1Equals(Asn1Object asn1Object)
		{
			Asn1Set other = asn1Object as Asn1Set;
			if (other == null)
			{
				return false;
			}
			if (this.Count != other.Count)
			{
				return false;
			}
			IEnumerator s = this.GetEnumerator();
			IEnumerator s2 = other.GetEnumerator();
			while (s.MoveNext() && s2.MoveNext())
			{
				Asn1Object o = ((Asn1Encodable)s.Current).ToAsn1Object();
				if (!o.Equals(s2.Current))
				{
					return false;
				}
			}
			return true;
		}

		private bool LessThanOrEqual(byte[] a, byte[] b)
		{
			int cmpLen = Math.Min(a.Length, b.Length);
			for (int i = 0; i < cmpLen; i++)
			{
				byte j = a[i];
				byte r = b[i];
				if (j != r)
				{
					return r > j;
				}
			}
			return a.Length <= b.Length;
		}

		protected internal void Sort()
		{
			if (this._set.Count > 1)
			{
				bool swapped = true;
				int lastSwap = this._set.Count - 1;
				while (swapped)
				{
					int index = 0;
					int swapIndex = 0;
					byte[] a = this._set[0].GetEncoded();
					swapped = false;
					while (index != lastSwap)
					{
						byte[] b = this._set[index + 1].GetEncoded();
						if (this.LessThanOrEqual(a, b))
						{
							a = b;
						}
						else
						{
							Asn1Encodable o = this._set[index];
							this._set[index] = this._set[index + 1];
							this._set[index + 1] = o;
							swapped = true;
							swapIndex = index;
						}
						index++;
					}
					lastSwap = swapIndex;
				}
			}
		}

		protected internal void AddObject(Asn1Encodable obj)
		{
			this._set.Add(obj);
		}

		public override string ToString()
		{
			return CollectionUtilities.ToString(this._set);
		}

		private readonly List<Asn1Encodable> _set;

		private class Asn1SetParserImpl : Asn1SetParser, IAsn1Convertible
		{
			public Asn1SetParserImpl(Asn1Set outer)
			{
				this.outer = outer;
				this.max = outer.Count;
			}

			public IAsn1Convertible ReadObject()
			{
				if (this.index == this.max)
				{
					return null;
				}
				Asn1Encodable obj = this.outer[this.index++];
				if (obj is Asn1Sequence)
				{
					return ((Asn1Sequence)obj).Parser;
				}
				if (obj is Asn1Set)
				{
					return ((Asn1Set)obj).Parser;
				}
				return obj;
			}

			public virtual Asn1Object ToAsn1Object()
			{
				return this.outer;
			}

			private readonly Asn1Set outer;

			private readonly int max;

			private int index;
		}
	}
}
