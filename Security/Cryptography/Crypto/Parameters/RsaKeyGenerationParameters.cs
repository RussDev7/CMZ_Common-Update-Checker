using System;
using DNA.Security.Cryptography.Math;
using DNA.Security.Cryptography.Security;

namespace DNA.Security.Cryptography.Crypto.Parameters
{
	public class RsaKeyGenerationParameters : KeyGenerationParameters
	{
		public RsaKeyGenerationParameters(BigInteger publicExponent, SecureRandom random, int strength, int certainty)
			: base(random, strength)
		{
			this.publicExponent = publicExponent;
			this.certainty = certainty;
		}

		public BigInteger PublicExponent
		{
			get
			{
				return this.publicExponent;
			}
		}

		public int Certainty
		{
			get
			{
				return this.certainty;
			}
		}

		public override bool Equals(object obj)
		{
			RsaKeyGenerationParameters other = obj as RsaKeyGenerationParameters;
			return other != null && this.certainty == other.certainty && this.publicExponent.Equals(other.publicExponent);
		}

		public override int GetHashCode()
		{
			return this.certainty.GetHashCode() ^ this.publicExponent.GetHashCode();
		}

		private readonly BigInteger publicExponent;

		private readonly int certainty;
	}
}
