using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;

namespace DNA.Security.Cryptography
{
	public class MD5HashProvider : IHashProvider
	{
		public int HashLength
		{
			get
			{
				return 16;
			}
		}

		public Hash Compute(byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			Hash hash;
			using (MD5 md5 = MD5.Create())
			{
				hash = new MD5HashProvider.MD5Hash(md5.ComputeHash(data));
			}
			return hash;
		}

		public Hash Compute(byte[] data, long length)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (length < 0L || length > data.LongLength)
			{
				throw new ArgumentOutOfRangeException("length");
			}
			int count = checked((int)length);
			Hash hash;
			using (MD5 md5 = MD5.Create())
			{
				hash = new MD5HashProvider.MD5Hash(md5.ComputeHash(data, 0, count));
			}
			return hash;
		}

		public Hash Compute(byte[] data, long start, long length)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (start < 0L || length < 0L || start > data.LongLength || start + length > data.LongLength)
			{
				throw new ArgumentOutOfRangeException("start/length");
			}
			checked
			{
				int offset = (int)start;
				int count = (int)length;
				Hash hash;
				using (MD5 md5 = MD5.Create())
				{
					hash = new MD5HashProvider.MD5Hash(md5.ComputeHash(data, offset, count));
				}
				return hash;
			}
		}

		public Hash GetFileHash(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			Hash hash;
			using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (MD5 md5 = MD5.Create())
				{
					hash = new MD5HashProvider.MD5Hash(md5.ComputeHash(fs));
				}
			}
			return hash;
		}

		public Hash Read(BinaryReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			byte[] bytes = reader.ReadBytes(this.HashLength);
			if (bytes.Length != this.HashLength)
			{
				throw new EndOfStreamException("Not enough bytes to read MD5 hash.");
			}
			return new MD5HashProvider.MD5Hash(bytes);
		}

		public Hash Parse(string s)
		{
			if (s == null)
			{
				throw new ArgumentNullException("s");
			}
			if ((s.Length & 1) != 0)
			{
				throw new FormatException("Hex string must have even length.");
			}
			int byteLen = s.Length / 2;
			if (byteLen != this.HashLength)
			{
				throw new FormatException("MD5 hex must be exactly 32 characters (16 bytes).");
			}
			byte[] data = new byte[byteLen];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = byte.Parse(s.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			}
			return new MD5HashProvider.MD5Hash(data);
		}

		public virtual Hash CreateHash(byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (data.Length != this.HashLength)
			{
				throw new ArgumentException("MD5 hash must be exactly 16 bytes.", "data");
			}
			return new MD5HashProvider.MD5Hash(data);
		}

		private class MD5Hash : Hash
		{
			public MD5Hash(byte[] data)
				: base(data)
			{
			}
		}
	}
}
