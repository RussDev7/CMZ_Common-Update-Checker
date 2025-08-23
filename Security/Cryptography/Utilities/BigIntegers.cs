using System;
using DNA.Security.Cryptography.Math;
using DNA.Security.Cryptography.Security;

namespace DNA.Security.Cryptography.Utilities
{
	public sealed class BigIntegers
	{
		private BigIntegers()
		{
		}

		public static byte[] AsUnsignedByteArray(BigInteger n)
		{
			return n.ToByteArrayUnsigned();
		}

		public static BigInteger CreateRandomInRange(BigInteger min, BigInteger max, SecureRandom random)
		{
			int cmp = min.CompareTo(max);
			if (cmp >= 0)
			{
				if (cmp > 0)
				{
					throw new ArgumentException("'min' may not be greater than 'max'");
				}
				return min;
			}
			else
			{
				if (min.BitLength > max.BitLength / 2)
				{
					return BigIntegers.CreateRandomInRange(BigInteger.Zero, max.Subtract(min), random).Add(min);
				}
				for (int i = 0; i < 1000; i++)
				{
					BigInteger x = new BigInteger(max.BitLength, random);
					if (x.CompareTo(min) >= 0 && x.CompareTo(max) <= 0)
					{
						return x;
					}
				}
				return new BigInteger(max.Subtract(min).BitLength - 1, random).Add(min);
			}
		}

		private const int MaxIterations = 1000;
	}
}
