using System;
using System.Collections;
using System.Collections.Generic;
using DNA.Security.Cryptography.Utilities.Collections;

namespace DNA.Security.Cryptography.Asn1
{
	public abstract class Asn1Sequence : Asn1Object, IEnumerable
	{
		public static Asn1Sequence GetInstance(object obj)
		{
			if (obj == null || obj is Asn1Sequence)
			{
				return (Asn1Sequence)obj;
			}
			throw new ArgumentException("Unknown object in GetInstance: " + obj.GetType().FullName, "obj");
		}

		public static Asn1Sequence GetInstance(Asn1TaggedObject obj, bool explicitly)
		{
			Asn1Object inner = obj.GetObject();
			if (explicitly)
			{
				if (!obj.IsExplicit())
				{
					throw new ArgumentException("object implicit - explicit expected.");
				}
				return (Asn1Sequence)inner;
			}
			else if (obj.IsExplicit())
			{
				if (obj is BerTaggedObject)
				{
					return new BerSequence(inner);
				}
				return new DerSequence(inner);
			}
			else
			{
				if (inner is Asn1Sequence)
				{
					return (Asn1Sequence)inner;
				}
				throw new ArgumentException("Unknown object in GetInstance: " + obj.GetType().FullName, "obj");
			}
		}

		protected internal Asn1Sequence(int capacity)
		{
			this.seq = new List<Asn1Encodable>(capacity);
		}

		public virtual IEnumerator GetEnumerator()
		{
			return this.seq.GetEnumerator();
		}

		[Obsolete("Use GetEnumerator() instead")]
		public IEnumerator GetObjects()
		{
			return this.GetEnumerator();
		}

		public virtual Asn1SequenceParser Parser
		{
			get
			{
				return new Asn1Sequence.Asn1SequenceParserImpl(this);
			}
		}

		public virtual Asn1Encodable this[int index]
		{
			get
			{
				return this.seq[index];
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
				return this.seq.Count;
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
			Asn1Sequence other = asn1Object as Asn1Sequence;
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

		protected internal void AddObject(Asn1Encodable obj)
		{
			this.seq.Add(obj);
		}

		public override string ToString()
		{
			return CollectionUtilities.ToString(this.seq);
		}

		private readonly List<Asn1Encodable> seq;

		private class Asn1SequenceParserImpl : Asn1SequenceParser, IAsn1Convertible
		{
			public Asn1SequenceParserImpl(Asn1Sequence outer)
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

			public Asn1Object ToAsn1Object()
			{
				return this.outer;
			}

			private readonly Asn1Sequence outer;

			private readonly int max;

			private int index;
		}
	}
}
