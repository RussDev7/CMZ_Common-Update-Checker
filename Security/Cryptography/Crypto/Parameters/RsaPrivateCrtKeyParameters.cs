using System;
using DNA.Security.Cryptography.Math;

namespace DNA.Security.Cryptography.Crypto.Parameters
{
	public class RsaPrivateCrtKeyParameters : RsaKeyParameters
	{
		public RsaPrivateCrtKeyParameters(BigInteger modulus, BigInteger publicExponent, BigInteger privateExponent, BigInteger p, BigInteger q, BigInteger dP, BigInteger dQ, BigInteger qInv)
			: base(true, modulus, privateExponent)
		{
			this.e = publicExponent;
			this.p = p;
			this.q = q;
			this.dP = dP;
			this.dQ = dQ;
			this.qInv = qInv;
		}

		public BigInteger PublicExponent
		{
			get
			{
				return this.e;
			}
		}

		public BigInteger P
		{
			get
			{
				return this.p;
			}
		}

		public BigInteger Q
		{
			get
			{
				return this.q;
			}
		}

		public BigInteger DP
		{
			get
			{
				return this.dP;
			}
		}

		public BigInteger DQ
		{
			get
			{
				return this.dQ;
			}
		}

		public BigInteger QInv
		{
			get
			{
				return this.qInv;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}
			RsaPrivateCrtKeyParameters kp = obj as RsaPrivateCrtKeyParameters;
			return kp != null && (kp.DP.Equals(this.dP) && kp.DQ.Equals(this.dQ) && kp.Exponent.Equals(base.Exponent) && kp.Modulus.Equals(base.Modulus) && kp.P.Equals(this.p) && kp.Q.Equals(this.q) && kp.PublicExponent.Equals(this.e)) && kp.QInv.Equals(this.qInv);
		}

		public override int GetHashCode()
		{
			return this.DP.GetHashCode() ^ this.DQ.GetHashCode() ^ base.Exponent.GetHashCode() ^ base.Modulus.GetHashCode() ^ this.P.GetHashCode() ^ this.Q.GetHashCode() ^ this.PublicExponent.GetHashCode() ^ this.QInv.GetHashCode();
		}

		private readonly BigInteger e;

		private readonly BigInteger p;

		private readonly BigInteger q;

		private readonly BigInteger dP;

		private readonly BigInteger dQ;

		private readonly BigInteger qInv;
	}
}
