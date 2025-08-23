using System;

namespace DNA.Security.Cryptography.Crypto.Prng
{
	public class DigestRandomGenerator : IRandomGenerator
	{
		public DigestRandomGenerator(IDigest digest)
		{
			this.digest = digest;
			this.seed = new byte[digest.GetDigestSize()];
			this.seedCounter = 1L;
			this.state = new byte[digest.GetDigestSize()];
			this.stateCounter = 1L;
		}

		public void AddSeedMaterial(byte[] inSeed)
		{
			lock (this)
			{
				this.DigestUpdate(inSeed);
				this.DigestUpdate(this.seed);
				this.DigestDoFinal(this.seed);
			}
		}

		public void AddSeedMaterial(long rSeed)
		{
			lock (this)
			{
				this.DigestAddCounter(rSeed);
				this.DigestUpdate(this.seed);
				this.DigestDoFinal(this.seed);
			}
		}

		public void NextBytes(byte[] bytes)
		{
			this.NextBytes(bytes, 0, bytes.Length);
		}

		public void NextBytes(byte[] bytes, int start, int len)
		{
			lock (this)
			{
				int stateOff = 0;
				this.GenerateState();
				int end = start + len;
				for (int i = start; i < end; i++)
				{
					if (stateOff == this.state.Length)
					{
						this.GenerateState();
						stateOff = 0;
					}
					bytes[i] = this.state[stateOff++];
				}
			}
		}

		private void CycleSeed()
		{
			this.DigestUpdate(this.seed);
			long num;
			this.seedCounter = (num = this.seedCounter) + 1L;
			this.DigestAddCounter(num);
			this.DigestDoFinal(this.seed);
		}

		private void GenerateState()
		{
			long num;
			this.stateCounter = (num = this.stateCounter) + 1L;
			this.DigestAddCounter(num);
			this.DigestUpdate(this.state);
			this.DigestUpdate(this.seed);
			this.DigestDoFinal(this.state);
			if (this.stateCounter % 10L == 0L)
			{
				this.CycleSeed();
			}
		}

		private void DigestAddCounter(long seedVal)
		{
			ulong seed = (ulong)seedVal;
			for (int i = 0; i != 8; i++)
			{
				this.digest.Update((byte)seed);
				seed >>= 8;
			}
		}

		private void DigestUpdate(byte[] inSeed)
		{
			this.digest.BlockUpdate(inSeed, 0, inSeed.Length);
		}

		private void DigestDoFinal(byte[] result)
		{
			this.digest.DoFinal(result, 0);
		}

		private const long CYCLE_COUNT = 10L;

		private long stateCounter;

		private long seedCounter;

		private IDigest digest;

		private byte[] state;

		private byte[] seed;
	}
}
