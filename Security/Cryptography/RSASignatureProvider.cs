using System;
using System.Globalization;
using DNA.Security.Cryptography.Crypto.Signers;

namespace DNA.Security.Cryptography
{
	public class RSASignatureProvider : GenericSignatureProvider
	{
		private static RsaDigestSigner GetSigner(GenericHashProvider hashProvider, RSAKey key)
		{
			RsaDigestSigner signer = new RsaDigestSigner(hashProvider.GetHashAlgorythim());
			signer.Init(key.IsPrivate, key.Key);
			return signer;
		}

		public RSASignatureProvider(GenericHashProvider hashProvider, RSAKey key)
			: base(RSASignatureProvider.GetSigner(hashProvider, key))
		{
		}

		private RSASignatureProvider(RsaDigestSigner rsa)
			: base(rsa)
		{
		}

		public override Signature FromByteArray(byte[] data)
		{
			return new RSASignatureProvider.RSASignature(data);
		}

		public override Signature Parse(string s)
		{
			byte[] data = new byte[s.Length / 2];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = byte.Parse(s.Substring(i * 2, 2), NumberStyles.HexNumber);
			}
			return new RSASignatureProvider.RSASignature(data);
		}

		private class RSASignature : Signature
		{
			public RSASignature(byte[] data)
				: base(data)
			{
			}

			public override bool Verify(ISignatureProvider signer, byte[] data, long start, long length)
			{
				RSASignatureProvider pvr = (RSASignatureProvider)signer;
				RsaDigestSigner rsa = (RsaDigestSigner)pvr.Signer;
				rsa.BlockUpdate(data, (int)start, (int)length);
				return rsa.VerifySignature(base.Data);
			}
		}
	}
}
