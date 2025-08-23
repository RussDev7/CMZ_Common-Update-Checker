using System;
using System.IO;
using DNA.IO;
using DNA.Security.Cryptography.Crypto;

namespace DNA.Security.Cryptography
{
	public abstract class GenericHashProvider : IHashProvider
	{
		internal abstract IDigest GetHashAlgorythim();

		public abstract Hash CreateHash(byte[] data);

		public abstract Hash Parse(string s);

		public abstract HashProcess BeginHash();

		public GenericHashProvider()
		{
		}

		public int HashLength
		{
			get
			{
				return this.GetHashAlgorythim().GetDigestSize();
			}
		}

		public Hash Compute(byte[] data)
		{
			if (data.Length > 2147483647)
			{
				throw new Exception("Data over 4GB not supported yet");
			}
			IDigest hasher = this.GetHashAlgorythim();
			hasher.BlockUpdate(data, 0, data.Length);
			byte[] hashData = new byte[hasher.GetDigestSize()];
			hasher.DoFinal(hashData, 0);
			return this.CreateHash(hashData);
		}

		public Hash Compute(byte[] data, long length)
		{
			if (length > 2147483647L)
			{
				throw new Exception("Data over 4GB not supported yet");
			}
			IDigest hasher = this.GetHashAlgorythim();
			hasher.BlockUpdate(data, 0, (int)length);
			byte[] hashData = new byte[hasher.GetDigestSize()];
			hasher.DoFinal(hashData, 0);
			return this.CreateHash(hashData);
		}

		public Hash Compute(byte[] data, long start, long length)
		{
			if (length > 2147483647L)
			{
				throw new Exception("Data over 4GB not supported yet");
			}
			IDigest hasher = this.GetHashAlgorythim();
			hasher.BlockUpdate(data, (int)start, (int)length);
			byte[] hashData = new byte[hasher.GetDigestSize()];
			hasher.DoFinal(hashData, 0);
			return this.CreateHash(hashData);
		}

		public Hash Read(BinaryReader reader)
		{
			return this.CreateHash(reader.ReadBytes(this.HashLength));
		}

		public virtual Hash GetFileHash(string path)
		{
			MemoryStream memStream = new MemoryStream();
			FileInfo info = new FileInfo(path);
			using (FileStream fstream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				memStream.CopyStream(fstream, info.Length);
			}
			return this.Compute(memStream.GetBuffer(), info.Length);
		}
	}
}
