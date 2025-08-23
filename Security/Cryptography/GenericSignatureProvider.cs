using System;
using System.IO;
using DNA.IO;
using DNA.Security.Cryptography.Crypto;

namespace DNA.Security.Cryptography
{
	public abstract class GenericSignatureProvider : ISignatureProvider
	{
		protected ISigner Signer
		{
			get
			{
				return this._signer;
			}
		}

		public abstract Signature Parse(string s);

		public abstract Signature FromByteArray(byte[] data);

		public GenericSignatureProvider(ISigner signer)
		{
			this._signer = signer;
		}

		public virtual Signature GetFileSignature(string path)
		{
			MemoryStream memStream = new MemoryStream();
			FileInfo info = new FileInfo(path);
			using (FileStream fstream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				memStream.CopyStream(fstream, info.Length);
			}
			return this.Sign(memStream.GetBuffer(), info.Length);
		}

		public Signature Sign(byte[] data)
		{
			return this.Sign(data, 0L, (long)data.Length);
		}

		public Signature Sign(byte[] data, long length)
		{
			return this.Sign(data, 0L, length);
		}

		public Signature Sign(byte[] data, long start, long length)
		{
			this._signer.BlockUpdate(data, (int)start, (int)length);
			return this.FromByteArray(this._signer.GenerateSignature());
		}

		private ISigner _signer;
	}
}
