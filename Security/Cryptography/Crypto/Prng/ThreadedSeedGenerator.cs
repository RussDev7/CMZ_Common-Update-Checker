using System;
using System.Threading;

namespace DNA.Security.Cryptography.Crypto.Prng
{
	public class ThreadedSeedGenerator
	{
		public byte[] GenerateSeed(int numBytes, bool fast)
		{
			return new ThreadedSeedGenerator.SeedGenerator().GenerateSeed(numBytes, fast);
		}

		private class SeedGenerator
		{
			private void Run(object ignored)
			{
				while (!this.stop)
				{
					this.counter++;
				}
			}

			public byte[] GenerateSeed(int numBytes, bool fast)
			{
				this.counter = 0;
				this.stop = false;
				byte[] result = new byte[numBytes];
				int last = 0;
				int end = (fast ? numBytes : (numBytes * 8));
				ThreadPool.QueueUserWorkItem(new WaitCallback(this.Run));
				for (int i = 0; i < end; i++)
				{
					while (this.counter == last)
					{
						try
						{
							Thread.Sleep(1);
						}
						catch (Exception)
						{
						}
					}
					last = this.counter;
					if (fast)
					{
						result[i] = (byte)last;
					}
					else
					{
						int bytepos = i / 8;
						result[bytepos] = (byte)(((int)result[bytepos] << 1) | (last & 1));
					}
				}
				this.stop = true;
				return result;
			}

			private volatile int counter;

			private volatile bool stop;
		}
	}
}
