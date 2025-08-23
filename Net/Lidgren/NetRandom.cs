using System;
using System.Threading;

namespace DNA.Net.Lidgren
{
	public class NetRandom
	{
		public NetRandom()
		{
			this.Reinitialise(this.GetSeed(this));
		}

		public NetRandom(int seed)
		{
			this.Reinitialise(seed);
		}

		public int GetSeed(object forObject)
		{
			int seed = Environment.TickCount;
			seed ^= forObject.GetHashCode();
			int extraSeed = Interlocked.Increment(ref NetRandom.s_extraSeed);
			return seed + extraSeed;
		}

		public void Reinitialise(int seed)
		{
			this.x = (uint)seed;
			this.y = 842502087U;
			this.z = 3579807591U;
			this.w = 273326509U;
		}

		public int Next()
		{
			uint t = this.x ^ (this.x << 11);
			this.x = this.y;
			this.y = this.z;
			this.z = this.w;
			this.w = this.w ^ (this.w >> 19) ^ (t ^ (t >> 8));
			uint rtn = this.w & 2147483647U;
			if (rtn == 2147483647U)
			{
				return this.Next();
			}
			return (int)rtn;
		}

		public int Next(int upperBound)
		{
			if (upperBound < 0)
			{
				throw new ArgumentOutOfRangeException("upperBound", upperBound, "upperBound must be >=0");
			}
			uint t = this.x ^ (this.x << 11);
			this.x = this.y;
			this.y = this.z;
			this.z = this.w;
			return (int)(4.656612873077393E-10 * (double)(2147483647U & (this.w = this.w ^ (this.w >> 19) ^ (t ^ (t >> 8)))) * (double)upperBound);
		}

		public int Next(int lowerBound, int upperBound)
		{
			if (lowerBound > upperBound)
			{
				throw new ArgumentOutOfRangeException("upperBound", upperBound, "upperBound must be >=lowerBound");
			}
			uint t = this.x ^ (this.x << 11);
			this.x = this.y;
			this.y = this.z;
			this.z = this.w;
			int range = upperBound - lowerBound;
			if (range < 0)
			{
				return lowerBound + (int)(2.3283064365386963E-10 * (this.w = this.w ^ (this.w >> 19) ^ (t ^ (t >> 8))) * (double)((long)upperBound - (long)lowerBound));
			}
			return lowerBound + (int)(4.656612873077393E-10 * (double)(2147483647U & (this.w = this.w ^ (this.w >> 19) ^ (t ^ (t >> 8)))) * (double)range);
		}

		public double NextDouble()
		{
			uint t = this.x ^ (this.x << 11);
			this.x = this.y;
			this.y = this.z;
			this.z = this.w;
			return 4.656612873077393E-10 * (double)(2147483647U & (this.w = this.w ^ (this.w >> 19) ^ (t ^ (t >> 8))));
		}

		public float NextSingle()
		{
			return (float)this.NextDouble();
		}

		public void NextBytes(byte[] buffer)
		{
			uint x = this.x;
			uint y = this.y;
			uint z = this.z;
			uint w = this.w;
			int i = 0;
			int bound = buffer.Length - 3;
			while (i < bound)
			{
				uint t = x ^ (x << 11);
				x = y;
				y = z;
				z = w;
				w = w ^ (w >> 19) ^ (t ^ (t >> 8));
				buffer[i++] = (byte)w;
				buffer[i++] = (byte)(w >> 8);
				buffer[i++] = (byte)(w >> 16);
				buffer[i++] = (byte)(w >> 24);
			}
			if (i < buffer.Length)
			{
				uint t = x ^ (x << 11);
				x = y;
				y = z;
				z = w;
				w = w ^ (w >> 19) ^ (t ^ (t >> 8));
				buffer[i++] = (byte)w;
				if (i < buffer.Length)
				{
					buffer[i++] = (byte)(w >> 8);
					if (i < buffer.Length)
					{
						buffer[i++] = (byte)(w >> 16);
						if (i < buffer.Length)
						{
							buffer[i] = (byte)(w >> 24);
						}
					}
				}
			}
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		[CLSCompliant(false)]
		public uint NextUInt()
		{
			uint t = this.x ^ (this.x << 11);
			this.x = this.y;
			this.y = this.z;
			this.z = this.w;
			return this.w = this.w ^ (this.w >> 19) ^ (t ^ (t >> 8));
		}

		public int NextInt()
		{
			uint t = this.x ^ (this.x << 11);
			this.x = this.y;
			this.y = this.z;
			this.z = this.w;
			return (int)(2147483647U & (this.w = this.w ^ (this.w >> 19) ^ (t ^ (t >> 8))));
		}

		public bool NextBool()
		{
			if (this.bitMask == 1U)
			{
				uint t = this.x ^ (this.x << 11);
				this.x = this.y;
				this.y = this.z;
				this.z = this.w;
				this.bitBuffer = (this.w = this.w ^ (this.w >> 19) ^ (t ^ (t >> 8)));
				this.bitMask = 2147483648U;
				return (this.bitBuffer & this.bitMask) == 0U;
			}
			return (this.bitBuffer & (this.bitMask >>= 1)) == 0U;
		}

		private const double REAL_UNIT_INT = 4.656612873077393E-10;

		private const double REAL_UNIT_UINT = 2.3283064365386963E-10;

		private const uint Y = 842502087U;

		private const uint Z = 3579807591U;

		private const uint W = 273326509U;

		public static readonly NetRandom Instance = new NetRandom();

		private static int s_extraSeed = 42;

		private uint x;

		private uint y;

		private uint z;

		private uint w;

		private uint bitBuffer;

		private uint bitMask = 1U;
	}
}
