using System;
using DNA.Security.Cryptography.Crypto.Parameters;
using DNA.Security.Cryptography.Math;
using DNA.Security.Cryptography.Security;
using DNA.Security.Cryptography.Utilities;

namespace DNA.Security.Cryptography.Crypto.Engines
{
	public class RsaBlindedEngine : IAsymmetricBlockCipher
	{
		public string AlgorithmName
		{
			get
			{
				return "RSA";
			}
		}

		public void Init(bool forEncryption, ICipherParameters param)
		{
			this.core.Init(forEncryption, param);
			if (param is ParametersWithRandom)
			{
				ParametersWithRandom parametersWithRandom = (ParametersWithRandom)param;
				this.key = (RsaKeyParameters)parametersWithRandom.Parameters;
				this.random = parametersWithRandom.Random;
				return;
			}
			this.key = (RsaKeyParameters)param;
			this.random = new SecureRandom();
		}

		public int GetInputBlockSize()
		{
			return this.core.GetInputBlockSize();
		}

		public int GetOutputBlockSize()
		{
			return this.core.GetOutputBlockSize();
		}

		public byte[] ProcessBlock(byte[] inBuf, int inOff, int inLen)
		{
			if (this.key == null)
			{
				throw new InvalidOperationException("RSA engine not initialised");
			}
			BigInteger bigInteger = this.core.ConvertInput(inBuf, inOff, inLen);
			BigInteger bigInteger6;
			if (this.key is RsaPrivateCrtKeyParameters)
			{
				RsaPrivateCrtKeyParameters rsaPrivateCrtKeyParameters = (RsaPrivateCrtKeyParameters)this.key;
				BigInteger publicExponent = rsaPrivateCrtKeyParameters.PublicExponent;
				if (publicExponent != null)
				{
					BigInteger modulus = rsaPrivateCrtKeyParameters.Modulus;
					BigInteger bigInteger2 = BigIntegers.CreateRandomInRange(BigInteger.One, modulus.Subtract(BigInteger.One), this.random);
					BigInteger bigInteger3 = bigInteger2.ModPow(publicExponent, modulus).Multiply(bigInteger).Mod(modulus);
					BigInteger bigInteger4 = this.core.ProcessBlock(bigInteger3);
					BigInteger bigInteger5 = bigInteger2.ModInverse(modulus);
					bigInteger6 = bigInteger4.Multiply(bigInteger5).Mod(modulus);
				}
				else
				{
					bigInteger6 = this.core.ProcessBlock(bigInteger);
				}
			}
			else
			{
				bigInteger6 = this.core.ProcessBlock(bigInteger);
			}
			return this.core.ConvertOutput(bigInteger6);
		}

		private readonly RsaCoreEngine core = new RsaCoreEngine();

		private RsaKeyParameters key;

		private SecureRandom random;
	}
}
