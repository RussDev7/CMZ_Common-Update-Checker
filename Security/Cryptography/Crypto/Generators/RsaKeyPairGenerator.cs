﻿using System;
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
			int num = (strength + 1) / 2;
			int num2 = strength - num;
			int num3 = strength / 3;
			BigInteger publicExponent = this.param.PublicExponent;
			BigInteger bigInteger;
			do
			{
				bigInteger = new BigInteger(num, 1, this.param.Random);
			}
			while (bigInteger.Mod(publicExponent).Equals(BigInteger.One) || !bigInteger.IsProbablePrime(this.param.Certainty) || !publicExponent.Gcd(bigInteger.Subtract(BigInteger.One)).Equals(BigInteger.One));
			BigInteger bigInteger2;
			BigInteger bigInteger3;
			for (;;)
			{
				bigInteger2 = new BigInteger(num2, 1, this.param.Random);
				if (bigInteger2.Subtract(bigInteger).Abs().BitLength >= num3 && !bigInteger2.Mod(publicExponent).Equals(BigInteger.One) && bigInteger2.IsProbablePrime(this.param.Certainty) && publicExponent.Gcd(bigInteger2.Subtract(BigInteger.One)).Equals(BigInteger.One))
				{
					bigInteger3 = bigInteger.Multiply(bigInteger2);
					if (bigInteger3.BitLength == this.param.Strength)
					{
						break;
					}
					bigInteger = bigInteger.Max(bigInteger2);
				}
			}
			BigInteger bigInteger4;
			if (bigInteger.CompareTo(bigInteger2) < 0)
			{
				bigInteger4 = bigInteger;
				bigInteger = bigInteger2;
				bigInteger2 = bigInteger4;
			}
			BigInteger bigInteger5 = bigInteger.Subtract(BigInteger.One);
			BigInteger bigInteger6 = bigInteger2.Subtract(BigInteger.One);
			bigInteger4 = bigInteger5.Multiply(bigInteger6);
			BigInteger bigInteger7 = publicExponent.ModInverse(bigInteger4);
			BigInteger bigInteger8 = bigInteger7.Remainder(bigInteger5);
			BigInteger bigInteger9 = bigInteger7.Remainder(bigInteger6);
			BigInteger bigInteger10 = bigInteger2.ModInverse(bigInteger);
			return new AsymmetricCipherKeyPair(new RsaKeyParameters(false, bigInteger3, publicExponent), new RsaPrivateCrtKeyParameters(bigInteger3, publicExponent, bigInteger7, bigInteger, bigInteger2, bigInteger8, bigInteger9, bigInteger10));
		}

		private const int DefaultTests = 12;

		private static readonly BigInteger DefaultPublicExponent = BigInteger.ValueOf(65537L);

		private RsaKeyGenerationParameters param;
	}
}
