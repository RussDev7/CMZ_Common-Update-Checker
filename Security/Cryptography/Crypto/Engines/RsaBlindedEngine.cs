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
				ParametersWithRandom rParam = (ParametersWithRandom)param;
				this.key = (RsaKeyParameters)rParam.Parameters;
				this.random = rParam.Random;
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
			BigInteger input = this.core.ConvertInput(inBuf, inOff, inLen);
			BigInteger result;
			if (this.key is RsaPrivateCrtKeyParameters)
			{
				RsaPrivateCrtKeyParameters i = (RsaPrivateCrtKeyParameters)this.key;
				BigInteger e = i.PublicExponent;
				if (e != null)
				{
					BigInteger j = i.Modulus;
					BigInteger r = BigIntegers.CreateRandomInRange(BigInteger.One, j.Subtract(BigInteger.One), this.random);
					BigInteger blindedInput = r.ModPow(e, j).Multiply(input).Mod(j);
					BigInteger blindedResult = this.core.ProcessBlock(blindedInput);
					BigInteger rInv = r.ModInverse(j);
					result = blindedResult.Multiply(rInv).Mod(j);
				}
				else
				{
					result = this.core.ProcessBlock(input);
				}
			}
			else
			{
				result = this.core.ProcessBlock(input);
			}
			return this.core.ConvertOutput(result);
		}

		private readonly RsaCoreEngine core = new RsaCoreEngine();

		private RsaKeyParameters key;

		private SecureRandom random;
	}
}
