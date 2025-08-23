using System;
using System.Globalization;
using DNA.Security.Cryptography.Crypto;
using DNA.Security.Cryptography.Crypto.Digests;
using DNA.Security.Cryptography.Crypto.Prng;

namespace DNA.Security.Cryptography.Security
{
	public class SecureRandom : Random
	{
		private static SecureRandom Master
		{
			get
			{
				if (SecureRandom.master[0] == null)
				{
					IRandomGenerator gen = new DigestRandomGenerator(new Sha256Digest());
					gen = new ReversedWindowGenerator(gen, 32);
					SecureRandom sr = (SecureRandom.master[0] = new SecureRandom(gen));
					sr.SetSeed(DateTime.Now.Ticks);
					sr.SetSeed(new ThreadedSeedGenerator().GenerateSeed(24, true));
					sr.GenerateSeed(1 + sr.Next(32));
				}
				return SecureRandom.master[0];
			}
		}

		public static SecureRandom GetInstance(string algorithm)
		{
			IDigest digest = null;
			string text;
			if ((text = algorithm.ToUpper(CultureInfo.InvariantCulture)) != null)
			{
				if (!(text == "SHA1PRNG"))
				{
					if (text == "SHA256PRNG")
					{
						digest = new Sha256Digest();
					}
				}
				else
				{
					digest = new Sha1Digest();
				}
			}
			if (digest != null)
			{
				return new SecureRandom(new DigestRandomGenerator(digest));
			}
			throw new ArgumentException("Unrecognised PRNG algorithm: " + algorithm, "algorithm");
		}

		public static byte[] GetSeed(int length)
		{
			return SecureRandom.Master.GenerateSeed(length);
		}

		public SecureRandom()
			: base(0)
		{
			this.generator = new DigestRandomGenerator(new Sha1Digest());
			this.SetSeed(SecureRandom.GetSeed(8));
		}

		public SecureRandom(byte[] inSeed)
			: base(0)
		{
			this.generator = new DigestRandomGenerator(new Sha1Digest());
			this.SetSeed(inSeed);
		}

		public SecureRandom(IRandomGenerator generator)
			: base(0)
		{
			this.generator = generator;
		}

		public virtual byte[] GenerateSeed(int length)
		{
			this.SetSeed(DateTime.Now.Ticks);
			byte[] rv = new byte[length];
			this.NextBytes(rv);
			return rv;
		}

		public virtual void SetSeed(byte[] inSeed)
		{
			this.generator.AddSeedMaterial(inSeed);
		}

		public virtual void SetSeed(long seed)
		{
			this.generator.AddSeedMaterial(seed);
		}

		public override int Next()
		{
			int i;
			do
			{
				i = this.NextInt() & int.MaxValue;
			}
			while (i == 2147483647);
			return i;
		}

		public override int Next(int maxValue)
		{
			if (maxValue < 2)
			{
				if (maxValue < 0)
				{
					throw new ArgumentOutOfRangeException("maxValue < 0");
				}
				return 0;
			}
			else
			{
				if ((maxValue & -maxValue) == maxValue)
				{
					int val = this.NextInt() & int.MaxValue;
					long lr = (long)maxValue * (long)val >> 31;
					return (int)lr;
				}
				int bits;
				int result;
				do
				{
					bits = this.NextInt() & int.MaxValue;
					result = bits % maxValue;
				}
				while (bits - result + (maxValue - 1) < 0);
				return result;
			}
		}

		public override int Next(int minValue, int maxValue)
		{
			if (maxValue <= minValue)
			{
				if (maxValue == minValue)
				{
					return minValue;
				}
				throw new ArgumentException("maxValue cannot be less than minValue");
			}
			else
			{
				int diff = maxValue - minValue;
				if (diff > 0)
				{
					return minValue + this.Next(diff);
				}
				int i;
				do
				{
					i = this.NextInt();
				}
				while (i < minValue || i >= maxValue);
				return i;
			}
		}

		public override void NextBytes(byte[] buffer)
		{
			this.generator.NextBytes(buffer);
		}

		public virtual void NextBytes(byte[] buffer, int start, int length)
		{
			this.generator.NextBytes(buffer, start, length);
		}

		public override double NextDouble()
		{
			return Convert.ToDouble((ulong)this.NextLong()) / SecureRandom.DoubleScale;
		}

		public virtual int NextInt()
		{
			byte[] intBytes = new byte[4];
			this.NextBytes(intBytes);
			int result = 0;
			for (int i = 0; i < 4; i++)
			{
				result = (result << 8) + (int)(intBytes[i] & byte.MaxValue);
			}
			return result;
		}

		public virtual long NextLong()
		{
			return (long)(((ulong)this.NextInt() << 32) | (ulong)this.NextInt());
		}

		// Note: this type is marked as 'beforefieldinit'.
		static SecureRandom()
		{
			SecureRandom[] array = new SecureRandom[1];
			SecureRandom.master = array;
			SecureRandom.DoubleScale = Math.Pow(2.0, 64.0);
		}

		private static readonly SecureRandom[] master;

		protected IRandomGenerator generator;

		private static readonly double DoubleScale;
	}
}
