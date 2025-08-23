using System;
using System.IO;
using DNA.Security.Cryptography.Asn1.Utilities;

namespace DNA.Security.Cryptography.Asn1
{
	public class DerOutputStream : FilterStream
	{
		public DerOutputStream(Stream os)
			: base(os)
		{
		}

		private void WriteLength(int length)
		{
			if (length > 127)
			{
				int size = 1;
				uint val = (uint)length;
				while ((val >>= 8) != 0U)
				{
					size++;
				}
				this.WriteByte((byte)(size | 128));
				for (int i = (size - 1) * 8; i >= 0; i -= 8)
				{
					this.WriteByte((byte)(length >> i));
				}
				return;
			}
			this.WriteByte((byte)length);
		}

		internal void WriteEncoded(int tag, byte[] bytes)
		{
			this.WriteByte((byte)tag);
			this.WriteLength(bytes.Length);
			this.Write(bytes, 0, bytes.Length);
		}

		internal void WriteEncoded(int tag, byte[] bytes, int offset, int length)
		{
			this.WriteByte((byte)tag);
			this.WriteLength(length);
			this.Write(bytes, offset, length);
		}

		internal void WriteTag(int flags, int tagNo)
		{
			if (tagNo < 31)
			{
				this.WriteByte((byte)(flags | tagNo));
				return;
			}
			this.WriteByte((byte)(flags | 31));
			if (tagNo < 128)
			{
				this.WriteByte((byte)tagNo);
				return;
			}
			byte[] stack = new byte[5];
			int pos = stack.Length;
			stack[--pos] = (byte)(tagNo & 127);
			do
			{
				tagNo >>= 7;
				stack[--pos] = (byte)((tagNo & 127) | 128);
			}
			while (tagNo > 127);
			this.Write(stack, pos, stack.Length - pos);
		}

		internal void WriteEncoded(int flags, int tagNo, byte[] bytes)
		{
			this.WriteTag(flags, tagNo);
			this.WriteLength(bytes.Length);
			this.Write(bytes, 0, bytes.Length);
		}

		protected void WriteNull()
		{
			this.WriteByte(5);
			this.WriteByte(0);
		}

		[Obsolete("Use version taking an Asn1Encodable arg instead")]
		public virtual void WriteObject(object obj)
		{
			if (obj == null)
			{
				this.WriteNull();
				return;
			}
			if (obj is Asn1Object)
			{
				((Asn1Object)obj).Encode(this);
				return;
			}
			if (obj is Asn1Encodable)
			{
				((Asn1Encodable)obj).ToAsn1Object().Encode(this);
				return;
			}
			throw new IOException("object not Asn1Object");
		}

		public virtual void WriteObject(Asn1Encodable obj)
		{
			if (obj == null)
			{
				this.WriteNull();
				return;
			}
			obj.ToAsn1Object().Encode(this);
		}
	}
}
