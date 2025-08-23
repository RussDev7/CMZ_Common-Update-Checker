using System;

namespace DNA.IO.Checksums
{
	public sealed class Adler32 : IChecksum<uint>
	{
		public uint Value
		{
			get
			{
				return this.checksum;
			}
		}

		public Adler32()
		{
			this.Reset();
		}

		public void Reset()
		{
			this.checksum = 1U;
		}

		public void Update(byte bval)
		{
			uint s = this.checksum & 65535U;
			uint s2 = this.checksum >> 16;
			s = (s + (uint)(bval & byte.MaxValue)) % Adler32.BASE;
			s2 = (s + s2) % Adler32.BASE;
			this.checksum = (s2 << 16) + s;
		}

		public void Update(byte[] buffer)
		{
			this.Update(buffer, 0, buffer.Length);
		}

		public void Update(byte[] buf, int off, int len)
		{
			if (buf == null)
			{
				throw new ArgumentNullException("buf");
			}
			if (off < 0 || len < 0 || off + len > buf.Length)
			{
				throw new ArgumentOutOfRangeException();
			}
			uint s = this.checksum & 65535U;
			uint s2 = this.checksum >> 16;
			while (len > 0)
			{
				int i = 3800;
				if (i > len)
				{
					i = len;
				}
				len -= i;
				while (--i >= 0)
				{
					s += (uint)(buf[off++] & byte.MaxValue);
					s2 += s;
				}
				s %= Adler32.BASE;
				s2 %= Adler32.BASE;
			}
			this.checksum = (s2 << 16) | s;
		}

		private static readonly uint BASE = 65521U;

		private uint checksum;
	}
}
