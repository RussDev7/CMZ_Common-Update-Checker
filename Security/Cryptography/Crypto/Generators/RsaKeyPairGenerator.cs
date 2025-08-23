using System;
using DNA.Security.Cryptography.Crypto.Parameters;
using DNA.Security.Cryptography.Math;

namespace DNA.Security.Cryptography.Crypto.Generators
{
	public class RsaKeyPairGenerator : IAsymmetricCipherKeyPairGenerator
	{
		public void Init(KeyGenerationParameters parameters)
		{
			if (parameters is RsaKeyGenerationParameters)
			{
				this.param = (RsaKeyGenerationParameters)parameters;
				return;
			}
			this.param = new RsaKeyGenerationParameters(RsaKeyPairGenerator.DefaultPublicExponent, parameters.Random, parameters.Strength, 12);
		}

		public AsymmetricCipherKeyPair GenerateKeyPair()
		{
			int strength = this.param.Strength;
			int pbitlength = (strength + 1) / 2;
			int qbitlength = strength - pbitlength;
			int mindiffbits = strength / 3;
			BigInteger e = this.param.PublicExponent;
			BigInteger p;
			do
			{
				p = new BigInteger(pbitlength, 1, this.param.Random);
			}
			while (p.Mod(e).Equals(BigInteger.One) || !p.IsProbablePrime(this.param.Certainty) || !e.Gcd(p.Subtract(BigInteger.One)).Equals(BigInteger.One));
			BigInteger q;
			BigInteger i;
			for (;;)
			{
				q = new BigInteger(qbitlength, 1, this.param.Random);
				if (q.Subtract(p).Abs().BitLength >= mindiffbits && !q.Mod(e).Equals(BigInteger.One) && q.IsProbablePrime(this.param.Certainty) && e.Gcd(q.Subtract(BigInteger.One)).Equals(BigInteger.One))
				{
					i = p.Multiply(q);
					if (i.BitLength == this.param.Strength)
					{
						break;
					}
					p = p.Max(q);
				}
			}
			BigInteger phi;
			if (p.CompareTo(q) < 0)
			{
				phi = p;
				p = q;
				q = phi;
			}
			BigInteger pSub = p.Subtract(BigInteger.One);
			BigInteger qSub = q.Subtract(BigInteger.One);
			phi = pSub.Multiply(qSub);
			BigInteger d = e.ModInverse(phi);
			BigInteger dP = d.Remainder(pSub);
			BigInteger dQ = d.Remainder(qSub);
			BigInteger qInv = q.ModInverse(p);
			return new AsymmetricCipherKeyPair(new RsaKeyParameters(false, i, e), new RsaPrivateCrtKeyParameters(i, e, d, p, q, dP, dQ, qInv));
		}

		private const int DefaultTests = 12;

		private static readonly BigInteger DefaultPublicExponent = BigInteger.ValueOf(65537L);

		private RsaKeyGenerationParameters param;
	}
}
