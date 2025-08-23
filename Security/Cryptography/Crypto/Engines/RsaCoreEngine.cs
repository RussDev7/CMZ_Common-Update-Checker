using System;
using DNA.Security.Cryptography.Crypto.Parameters;
using DNA.Security.Cryptography.Math;
using DNA.Security.Cryptography.Security;

namespace DNA.Security.Cryptography.Crypto.Engines
{
	internal class RsaCoreEngine
	{
		public void Init(bool forEncryption, ICipherParameters parameters)
		{
			if (parameters is ParametersWithRandom)
			{
				parameters = ((ParametersWithRandom)parameters).Parameters;
			}
			if (!(parameters is RsaKeyParameters))
			{
				throw new InvalidKeyException("Not an RSA key");
			}
			this.key = (RsaKeyParameters)parameters;
			this.forEncryption = forEncryption;
			this.bitSize = this.key.Modulus.BitLength;
		}

		public int GetInputBlockSize()
		{
			if (this.forEncryption)
			{
				return (this.bitSize - 1) / 8;
			}
			return (this.bitSize + 7) / 8;
		}

		public int GetOutputBlockSize()
		{
			if (this.forEncryption)
			{
				return (this.bitSize + 7) / 8;
			}
			return (this.bitSize - 1) / 8;
		}

		public BigInteger ConvertInput(byte[] inBuf, int inOff, int inLen)
		{
			int maxLength = (this.bitSize + 7) / 8;
			if (inLen > maxLength)
			{
				throw new DataLengthException("input too large for RSA cipher.");
			}
			BigInteger input = new BigInteger(1, inBuf, inOff, inLen);
			if (input.CompareTo(this.key.Modulus) >= 0)
			{
				throw new DataLengthException("input too large for RSA cipher.");
			}
			return input;
		}

		public byte[] ConvertOutput(BigInteger result)
		{
			byte[] output = result.ToByteArrayUnsigned();
			if (this.forEncryption)
			{
				int outSize = this.GetOutputBlockSize();
				if (output.Length < outSize)
				{
					byte[] tmp = new byte[outSize];
					output.CopyTo(tmp, tmp.Length - output.Length);
					output = tmp;
				}
			}
			return output;
		}

		public BigInteger ProcessBlock(BigInteger input)
		{
			if (this.key is RsaPrivateCrtKeyParameters)
			{
				RsaPrivateCrtKeyParameters crtKey = (RsaPrivateCrtKeyParameters)this.key;
				BigInteger p = crtKey.P;
				BigInteger q = crtKey.Q;
				BigInteger dP = crtKey.DP;
				BigInteger dQ = crtKey.DQ;
				BigInteger qInv = crtKey.QInv;
				BigInteger mP = input.Remainder(p).ModPow(dP, p);
				BigInteger mQ = input.Remainder(q).ModPow(dQ, q);
				BigInteger h = mP.Subtract(mQ);
				h = h.Multiply(qInv);
				h = h.Mod(p);
				BigInteger i = h.Multiply(q);
				return i.Add(mQ);
			}
			return input.ModPow(this.key.Exponent, this.key.Modulus);
		}

		private RsaKeyParameters key;

		private bool forEncryption;

		private int bitSize;
	}
}
