using System;
using DNA.Security.Cryptography.Utilities;

namespace DNA.Security.Cryptography.Asn1
{
	public abstract class Asn1TaggedObject : Asn1Object, Asn1TaggedObjectParser, IAsn1Convertible
	{
		public static Asn1TaggedObject GetInstance(Asn1TaggedObject obj, bool explicitly)
		{
			if (explicitly)
			{
				return (Asn1TaggedObject)obj.GetObject();
			}
			throw new ArgumentException("implicitly tagged tagged object");
		}

		public static Asn1TaggedObject GetInstance(object obj)
		{
			if (obj == null || obj is Asn1TaggedObject)
			{
				return (Asn1TaggedObject)obj;
			}
			throw new ArgumentException("Unknown object in GetInstance: " + obj.GetType().FullName, "obj");
		}

		protected Asn1TaggedObject(int tagNo, Asn1Encodable obj)
		{
			this.explicitly = true;
			this.tagNo = tagNo;
			this.obj = obj;
		}

		protected Asn1TaggedObject(bool explicitly, int tagNo, Asn1Encodable obj)
		{
			this.explicitly = explicitly || obj is IAsn1Choice;
			this.tagNo = tagNo;
			this.obj = obj;
		}

		protected override bool Asn1Equals(Asn1Object asn1Object)
		{
			Asn1TaggedObject other = asn1Object as Asn1TaggedObject;
			return other != null && (this.tagNo == other.tagNo && this.explicitly == other.explicitly) && object.Equals(this.GetObject(), other.GetObject());
		}

		protected override int Asn1GetHashCode()
		{
			int code = this.tagNo.GetHashCode();
			if (this.obj != null)
			{
				code ^= this.obj.GetHashCode();
			}
			return code;
		}

		public int TagNo
		{
			get
			{
				return this.tagNo;
			}
		}

		public bool IsExplicit()
		{
			return this.explicitly;
		}

		public bool IsEmpty()
		{
			return false;
		}

		public Asn1Object GetObject()
		{
			if (this.obj != null)
			{
				return this.obj.ToAsn1Object();
			}
			return null;
		}

		public IAsn1Convertible GetObjectParser(int tag, bool isExplicit)
		{
			if (tag == 4)
			{
				return Asn1OctetString.GetInstance(this, isExplicit).Parser;
			}
			switch (tag)
			{
			case 16:
				return Asn1Sequence.GetInstance(this, isExplicit).Parser;
			case 17:
				return Asn1Set.GetInstance(this, isExplicit).Parser;
			default:
				if (isExplicit)
				{
					return this.GetObject();
				}
				throw Platform.CreateNotImplementedException("implicit tagging for tag: " + tag);
			}
		}

		public override string ToString()
		{
			return string.Concat(new object[] { "[", this.tagNo, "]", this.obj });
		}

		internal int tagNo;

		internal bool explicitly = true;

		internal Asn1Encodable obj;
	}
}
