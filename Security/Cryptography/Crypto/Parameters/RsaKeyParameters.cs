using System;
using DNA.Security.Cryptography.Math;

namespace DNA.Security.Cryptography.Crypto.Parameters
{
	public class RsaKeyParameters : AsymmetricKeyParameter
	{
		public RsaKeyParameters(bool isPrivate, BigInteger modulus, BigInteger exponent)
			: base(isPrivate)
		{
			this.modulus = modulus;
			this.exponent = exponent;
		}

		public BigInteger Modulus
		{
			get
			{
				return this.modulus;
			}
		}

		public BigInteger Exponent
		{
			get
			{
				return this.exponent;
			}
		}

		public override bool Equals(object obj)
		{
			RsaKeyParameters kp = obj as RsaKeyParameters;
			return kp != null && (kp.IsPrivate == base.IsPrivate && kp.Modulus.Equals(this.modulus)) && kp.Exponent.Equals(this.exponent);
		}

		public override int GetHashCode()
		{
			return this.modulus.GetHashCode() ^ this.exponent.GetHashCode() ^ base.IsPrivate.GetHashCode();
		}

		private readonly BigInteger modulus;

		private readonly BigInteger exponent;
	}
}
