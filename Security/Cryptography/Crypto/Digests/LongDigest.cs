using System;

namespace DNA.Security.Cryptography.Crypto.Digests
{
	public abstract class LongDigest : IDigest
	{
		internal LongDigest()
		{
			this.xBuf = new byte[8];
			this.Reset();
		}

		internal LongDigest(LongDigest t)
		{
			this.xBuf = new byte[t.xBuf.Length];
			Array.Copy(t.xBuf, 0, this.xBuf, 0, t.xBuf.Length);
			this.xBufOff = t.xBufOff;
			this.byteCount1 = t.byteCount1;
			this.byteCount2 = t.byteCount2;
			this.H1 = t.H1;
			this.H2 = t.H2;
			this.H3 = t.H3;
			this.H4 = t.H4;
			this.H5 = t.H5;
			this.H6 = t.H6;
			this.H7 = t.H7;
			this.H8 = t.H8;
			Array.Copy(t.W, 0, this.W, 0, t.W.Length);
			this.wOff = t.wOff;
		}

		public void Update(byte input)
		{
			this.xBuf[this.xBufOff++] = input;
			if (this.xBufOff == this.xBuf.Length)
			{
				this.ProcessWord(this.xBuf, 0);
				this.xBufOff = 0;
			}
			this.byteCount1 += 1L;
		}

		public void BlockUpdate(byte[] input, int inOff, int length)
		{
			while (this.xBufOff != 0)
			{
				if (length <= 0)
				{
					break;
				}
				this.Update(input[inOff]);
				inOff++;
				length--;
			}
			while (length > this.xBuf.Length)
			{
				this.ProcessWord(input, inOff);
				inOff += this.xBuf.Length;
				length -= this.xBuf.Length;
				this.byteCount1 += (long)this.xBuf.Length;
			}
			while (length > 0)
			{
				this.Update(input[inOff]);
				inOff++;
				length--;
			}
		}

		public void Finish()
		{
			this.AdjustByteCounts();
			long lowBitLength = this.byteCount1 << 3;
			long hiBitLength = this.byteCount2;
			this.Update(128);
			while (this.xBufOff != 0)
			{
				this.Update(0);
			}
			this.ProcessLength(lowBitLength, hiBitLength);
			this.ProcessBlock();
		}

		public virtual void Reset()
		{
			this.byteCount1 = 0L;
			this.byteCount2 = 0L;
			this.xBufOff = 0;
			for (int i = 0; i < this.xBuf.Length; i++)
			{
				this.xBuf[i] = 0;
			}
			this.wOff = 0;
			for (int j = 0; j != this.W.Length; j++)
			{
				this.W[j] = 0L;
			}
		}

		internal void ProcessWord(byte[] input, int inOff)
		{
			this.W[this.wOff++] = ((long)(input[inOff] & byte.MaxValue) << 56) | ((long)(input[inOff + 1] & byte.MaxValue) << 48) | ((long)(input[inOff + 2] & byte.MaxValue) << 40) | ((long)(input[inOff + 3] & byte.MaxValue) << 32) | ((long)(input[inOff + 4] & byte.MaxValue) << 24) | ((long)(input[inOff + 5] & byte.MaxValue) << 16) | ((long)(input[inOff + 6] & byte.MaxValue) << 8) | (long)((ulong)(input[inOff + 7] & byte.MaxValue));
			if (this.wOff == 16)
			{
				this.ProcessBlock();
			}
		}

		internal static void UnpackWord(long word, byte[] outBytes, int outOff)
		{
			outBytes[outOff] = (byte)((ulong)word >> 56);
			outBytes[outOff + 1] = (byte)((ulong)word >> 48);
			outBytes[outOff + 2] = (byte)((ulong)word >> 40);
			outBytes[outOff + 3] = (byte)((ulong)word >> 32);
			outBytes[outOff + 4] = (byte)((ulong)word >> 24);
			outBytes[outOff + 5] = (byte)((ulong)word >> 16);
			outBytes[outOff + 6] = (byte)((ulong)word >> 8);
			outBytes[outOff + 7] = (byte)word;
		}

		private void AdjustByteCounts()
		{
			if (this.byteCount1 > 2305843009213693951L)
			{
				this.byteCount2 += (long)((ulong)this.byteCount1 >> 61);
				this.byteCount1 &= 2305843009213693951L;
			}
		}

		internal void ProcessLength(long lowW, long hiW)
		{
			if (this.wOff > 14)
			{
				this.ProcessBlock();
			}
			this.W[14] = hiW;
			this.W[15] = lowW;
		}

		internal void ProcessBlock()
		{
			this.AdjustByteCounts();
			for (int ti = 16; ti <= 79; ti++)
			{
				this.W[ti] = LongDigest.Sigma1(this.W[ti - 2]) + this.W[ti - 7] + LongDigest.Sigma0(this.W[ti - 15]) + this.W[ti - 16];
			}
			long a = this.H1;
			long b = this.H2;
			long c = this.H3;
			long d = this.H4;
			long e = this.H5;
			long f = this.H6;
			long g = this.H7;
			long h = this.H8;
			int t = 0;
			for (int i = 0; i < 10; i++)
			{
				h += LongDigest.Sum1(e) + LongDigest.Ch(e, f, g) + LongDigest.K[t] + this.W[t++];
				d += h;
				h += LongDigest.Sum0(a) + LongDigest.Maj(a, b, c);
				g += LongDigest.Sum1(d) + LongDigest.Ch(d, e, f) + LongDigest.K[t] + this.W[t++];
				c += g;
				g += LongDigest.Sum0(h) + LongDigest.Maj(h, a, b);
				f += LongDigest.Sum1(c) + LongDigest.Ch(c, d, e) + LongDigest.K[t] + this.W[t++];
				b += f;
				f += LongDigest.Sum0(g) + LongDigest.Maj(g, h, a);
				e += LongDigest.Sum1(b) + LongDigest.Ch(b, c, d) + LongDigest.K[t] + this.W[t++];
				a += e;
				e += LongDigest.Sum0(f) + LongDigest.Maj(f, g, h);
				d += LongDigest.Sum1(a) + LongDigest.Ch(a, b, c) + LongDigest.K[t] + this.W[t++];
				h += d;
				d += LongDigest.Sum0(e) + LongDigest.Maj(e, f, g);
				c += LongDigest.Sum1(h) + LongDigest.Ch(h, a, b) + LongDigest.K[t] + this.W[t++];
				g += c;
				c += LongDigest.Sum0(d) + LongDigest.Maj(d, e, f);
				b += LongDigest.Sum1(g) + LongDigest.Ch(g, h, a) + LongDigest.K[t] + this.W[t++];
				f += b;
				b += LongDigest.Sum0(c) + LongDigest.Maj(c, d, e);
				a += LongDigest.Sum1(f) + LongDigest.Ch(f, g, h) + LongDigest.K[t] + this.W[t++];
				e += a;
				a += LongDigest.Sum0(b) + LongDigest.Maj(b, c, d);
			}
			this.H1 += a;
			this.H2 += b;
			this.H3 += c;
			this.H4 += d;
			this.H5 += e;
			this.H6 += f;
			this.H7 += g;
			this.H8 += h;
			this.wOff = 0;
			Array.Clear(this.W, 0, 16);
		}

		private static long Ch(long x, long y, long z)
		{
			return (x & y) ^ (~x & z);
		}

		private static long Maj(long x, long y, long z)
		{
			return (x & y) ^ (x & z) ^ (y & z);
		}

		private static long Sum0(long x)
		{
			return ((x << 36) | (long)((ulong)x >> 28)) ^ ((x << 30) | (long)((ulong)x >> 34)) ^ ((x << 25) | (long)((ulong)x >> 39));
		}

		private static long Sum1(long x)
		{
			return ((x << 50) | (long)((ulong)x >> 14)) ^ ((x << 46) | (long)((ulong)x >> 18)) ^ ((x << 23) | (long)((ulong)x >> 41));
		}

		private static long Sigma0(long x)
		{
			return ((x << 63) | (long)((ulong)x >> 1)) ^ ((x << 56) | (long)((ulong)x >> 8)) ^ (long)((ulong)x >> 7);
		}

		private static long Sigma1(long x)
		{
			return ((x << 45) | (long)((ulong)x >> 19)) ^ ((x << 3) | (long)((ulong)x >> 61)) ^ (long)((ulong)x >> 6);
		}

		public int GetByteLength()
		{
			return this.MyByteLength;
		}

		public abstract string AlgorithmName { get; }

		public abstract int GetDigestSize();

		public abstract int DoFinal(byte[] output, int outOff);

		private int MyByteLength = 128;

		private byte[] xBuf;

		private int xBufOff;

		private long byteCount1;

		private long byteCount2;

		internal long H1;

		internal long H2;

		internal long H3;

		internal long H4;

		internal long H5;

		internal long H6;

		internal long H7;

		internal long H8;

		private long[] W = new long[80];

		private int wOff;

		internal static readonly long[] K = new long[]
		{
			4794697086780616226L, 8158064640168781261L, -5349999486874862801L, -1606136188198331460L, 4131703408338449720L, 6480981068601479193L, -7908458776815382629L, -6116909921290321640L, -2880145864133508542L, 1334009975649890238L,
			2608012711638119052L, 6128411473006802146L, 8268148722764581231L, -9160688886553864527L, -7215885187991268811L, -4495734319001033068L, -1973867731355612462L, -1171420211273849373L, 1135362057144423861L, 2597628984639134821L,
			3308224258029322869L, 5365058923640841347L, 6679025012923562964L, 8573033837759648693L, -7476448914759557205L, -6327057829258317296L, -5763719355590565569L, -4658551843659510044L, -4116276920077217854L, -3051310485924567259L,
			489312712824947311L, 1452737877330783856L, 2861767655752347644L, 3322285676063803686L, 5560940570517711597L, 5996557281743188959L, 7280758554555802590L, 8532644243296465576L, -9096487096722542874L, -7894198246740708037L,
			-6719396339535248540L, -6333637450476146687L, -4446306890439682159L, -4076793802049405392L, -3345356375505022440L, -2983346525034927856L, -860691631967231958L, 1182934255886127544L, 1847814050463011016L, 2177327727835720531L,
			2830643537854262169L, 3796741975233480872L, 4115178125766777443L, 5681478168544905931L, 6601373596472566643L, 7507060721942968483L, 8399075790359081724L, 8693463985226723168L, -8878714635349349518L, -8302665154208450068L,
			-8016688836872298968L, -6606660893046293015L, -4685533653050689259L, -4147400797238176981L, -3880063495543823972L, -3348786107499101689L, -1523767162380948706L, -757361751448694408L, 500013540394364858L, 748580250866718886L,
			1242879168328830382L, 1977374033974150939L, 2944078676154940804L, 3659926193048069267L, 4368137639120453308L, 4836135668995329356L, 5532061633213252278L, 6448918945643986474L, 6902733635092675308L, 7801388544844847127L
		};
	}
}
