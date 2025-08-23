using System;
using System.Collections.Generic;
using DNA.Security.Cryptography.Asn1;
using DNA.Security.Cryptography.Asn1.Nist;
using DNA.Security.Cryptography.Asn1.Pkcs;
using DNA.Security.Cryptography.Asn1.TeleTrust;
using DNA.Security.Cryptography.Asn1.X509;
using DNA.Security.Cryptography.Crypto.Encodings;
using DNA.Security.Cryptography.Crypto.Engines;
using DNA.Security.Cryptography.Crypto.Parameters;
using DNA.Security.Cryptography.Security;

namespace DNA.Security.Cryptography.Crypto.Signers
{
	public class RsaDigestSigner : ISigner
	{
		static RsaDigestSigner()
		{
			RsaDigestSigner.oidMap["RIPEMD128"] = TeleTrusTObjectIdentifiers.RipeMD128;
			RsaDigestSigner.oidMap["RIPEMD160"] = TeleTrusTObjectIdentifiers.RipeMD160;
			RsaDigestSigner.oidMap["RIPEMD256"] = TeleTrusTObjectIdentifiers.RipeMD256;
			RsaDigestSigner.oidMap["SHA-1"] = X509ObjectIdentifiers.IdSha1;
			RsaDigestSigner.oidMap["SHA-224"] = NistObjectIdentifiers.IdSha224;
			RsaDigestSigner.oidMap["SHA-256"] = NistObjectIdentifiers.IdSha256;
			RsaDigestSigner.oidMap["SHA-384"] = NistObjectIdentifiers.IdSha384;
			RsaDigestSigner.oidMap["SHA-512"] = NistObjectIdentifiers.IdSha512;
			RsaDigestSigner.oidMap["MD2"] = PkcsObjectIdentifiers.MD2;
			RsaDigestSigner.oidMap["MD4"] = PkcsObjectIdentifiers.MD4;
			RsaDigestSigner.oidMap["MD5"] = PkcsObjectIdentifiers.MD5;
		}

		public RsaDigestSigner(IDigest digest)
		{
			this.digest = digest;
			this.algId = new AlgorithmIdentifier(RsaDigestSigner.oidMap[digest.AlgorithmName], DerNull.Instance);
		}

		public string AlgorithmName
		{
			get
			{
				return this.digest.AlgorithmName + "withRSA";
			}
		}

		public void Init(bool forSigning, ICipherParameters parameters)
		{
			this.forSigning = forSigning;
			AsymmetricKeyParameter i;
			if (parameters is ParametersWithRandom)
			{
				i = (AsymmetricKeyParameter)((ParametersWithRandom)parameters).Parameters;
			}
			else
			{
				i = (AsymmetricKeyParameter)parameters;
			}
			if (forSigning && !i.IsPrivate)
			{
				throw new InvalidKeyException("Signing requires private key.");
			}
			if (!forSigning && i.IsPrivate)
			{
				throw new InvalidKeyException("Verification requires public key.");
			}
			this.Reset();
			this.rsaEngine.Init(forSigning, parameters);
		}

		public void Update(byte input)
		{
			this.digest.Update(input);
		}

		public void BlockUpdate(byte[] input, int inOff, int length)
		{
			this.digest.BlockUpdate(input, inOff, length);
		}

		public byte[] GenerateSignature()
		{
			if (!this.forSigning)
			{
				throw new InvalidOperationException("RsaDigestSigner not initialised for signature generation.");
			}
			byte[] hash = new byte[this.digest.GetDigestSize()];
			this.digest.DoFinal(hash, 0);
			byte[] data = this.DerEncode(hash);
			return this.rsaEngine.ProcessBlock(data, 0, data.Length);
		}

		public bool VerifySignature(byte[] signature)
		{
			if (this.forSigning)
			{
				throw new InvalidOperationException("RsaDigestSigner not initialised for verification");
			}
			byte[] hash = new byte[this.digest.GetDigestSize()];
			this.digest.DoFinal(hash, 0);
			byte[] sig;
			byte[] expected;
			try
			{
				sig = this.rsaEngine.ProcessBlock(signature, 0, signature.Length);
				expected = this.DerEncode(hash);
			}
			catch (Exception)
			{
				return false;
			}
			if (sig.Length == expected.Length)
			{
				for (int i = 0; i < sig.Length; i++)
				{
					if (sig[i] != expected[i])
					{
						return false;
					}
				}
			}
			else
			{
				if (sig.Length != expected.Length - 2)
				{
					return false;
				}
				int sigOffset = sig.Length - hash.Length - 2;
				int expectedOffset = expected.Length - hash.Length - 2;
				byte[] array = expected;
				int num = 1;
				array[num] -= 2;
				byte[] array2 = expected;
				int num2 = 3;
				array2[num2] -= 2;
				for (int j = 0; j < hash.Length; j++)
				{
					if (sig[sigOffset + j] != expected[expectedOffset + j])
					{
						return false;
					}
				}
				for (int k = 0; k < sigOffset; k++)
				{
					if (sig[k] != expected[k])
					{
						return false;
					}
				}
			}
			return true;
		}

		public void Reset()
		{
			this.digest.Reset();
		}

		private byte[] DerEncode(byte[] hash)
		{
			DigestInfo dInfo = new DigestInfo(this.algId, hash);
			return dInfo.GetDerEncoded();
		}

		private readonly IAsymmetricBlockCipher rsaEngine = new Pkcs1Encoding(new RsaBlindedEngine());

		private readonly AlgorithmIdentifier algId;

		private readonly IDigest digest;

		private bool forSigning;

		private static readonly Dictionary<string, DerObjectIdentifier> oidMap = new Dictionary<string, DerObjectIdentifier>();
	}
}
