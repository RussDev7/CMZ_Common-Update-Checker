using System;

namespace DNA.Security.Cryptography.Crypto.Digests
{
	public class Sha1Digest : GeneralDigest
	{
		public Sha1Digest()
		{
			this.Reset();
		}

		public Sha1Digest(Sha1Digest t)
			: base(t)
		{
			this.H1 = t.H1;
			this.H2 = t.H2;
			this.H3 = t.H3;
			this.H4 = t.H4;
			this.H5 = t.H5;
			Array.Copy(t.X, 0, this.X, 0, t.X.Length);
			this.xOff = t.xOff;
		}

		public override string AlgorithmName
		{
			get
			{
				return "SHA-1";
			}
		}

		public override int GetDigestSize()
		{
			return 20;
		}

		internal override void ProcessWord(byte[] input, int inOff)
		{
			this.X[this.xOff++] = ((int)(input[inOff] & byte.MaxValue) << 24) | ((int)(input[inOff + 1] & byte.MaxValue) << 16) | ((int)(input[inOff + 2] & byte.MaxValue) << 8) | (int)(input[inOff + 3] & byte.MaxValue);
			if (this.xOff == 16)
			{
				this.ProcessBlock();
			}
		}

		private static void UnpackWord(int word, byte[] outBytes, int outOff)
		{
			outBytes[outOff++] = (byte)((uint)word >> 24);
			outBytes[outOff++] = (byte)((uint)word >> 16);
			outBytes[outOff++] = (byte)((uint)word >> 8);
			outBytes[outOff++] = (byte)word;
		}

		internal override void ProcessLength(long bitLength)
		{
			if (this.xOff > 14)
			{
				this.ProcessBlock();
			}
			this.X[14] = (int)((ulong)bitLength >> 32);
			this.X[15] = (int)(bitLength & (long)((ulong)(-1)));
		}

		public override int DoFinal(byte[] output, int outOff)
		{
			base.Finish();
			Sha1Digest.UnpackWord(this.H1, output, outOff);
			Sha1Digest.UnpackWord(this.H2, output, outOff + 4);
			Sha1Digest.UnpackWord(this.H3, output, outOff + 8);
			Sha1Digest.UnpackWord(this.H4, output, outOff + 12);
			Sha1Digest.UnpackWord(this.H5, output, outOff + 16);
			this.Reset();
			return 20;
		}

		public override void Reset()
		{
			base.Reset();
			this.H1 = 1732584193;
			this.H2 = -271733879;
			this.H3 = -1732584194;
			this.H4 = 271733878;
			this.H5 = -1009589776;
			this.xOff = 0;
			for (int i = 0; i != this.X.Length; i++)
			{
				this.X[i] = 0;
			}
		}

		private static int F(int u, int v, int w)
		{
			return (u & v) | (~u & w);
		}

		private static int H(int u, int v, int w)
		{
			return u ^ v ^ w;
		}

		private static int G(int u, int v, int w)
		{
			return (u & v) | (u & w) | (v & w);
		}

		internal override void ProcessBlock()
		{
			for (int i = 16; i < 80; i++)
			{
				int t = this.X[i - 3] ^ this.X[i - 8] ^ this.X[i - 14] ^ this.X[i - 16];
				this.X[i] = (t << 1) | (int)((uint)t >> 31);
			}
			int A = this.H1;
			int B = this.H2;
			int C = this.H3;
			int D = this.H4;
			int E = this.H5;
			int idx = 0;
			for (int j = 0; j < 4; j++)
			{
				E += ((A << 5) | (int)((uint)A >> 27)) + Sha1Digest.F(B, C, D) + this.X[idx++] + 1518500249;
				B = (B << 30) | (int)((uint)B >> 2);
				D += ((E << 5) | (int)((uint)E >> 27)) + Sha1Digest.F(A, B, C) + this.X[idx++] + 1518500249;
				A = (A << 30) | (int)((uint)A >> 2);
				C += ((D << 5) | (int)((uint)D >> 27)) + Sha1Digest.F(E, A, B) + this.X[idx++] + 1518500249;
				E = (E << 30) | (int)((uint)E >> 2);
				B += ((C << 5) | (int)((uint)C >> 27)) + Sha1Digest.F(D, E, A) + this.X[idx++] + 1518500249;
				D = (D << 30) | (int)((uint)D >> 2);
				A += ((B << 5) | (int)((uint)B >> 27)) + Sha1Digest.F(C, D, E) + this.X[idx++] + 1518500249;
				C = (C << 30) | (int)((uint)C >> 2);
			}
			for (int k = 0; k < 4; k++)
			{
				E += ((A << 5) | (int)((uint)A >> 27)) + Sha1Digest.H(B, C, D) + this.X[idx++] + 1859775393;
				B = (B << 30) | (int)((uint)B >> 2);
				D += ((E << 5) | (int)((uint)E >> 27)) + Sha1Digest.H(A, B, C) + this.X[idx++] + 1859775393;
				A = (A << 30) | (int)((uint)A >> 2);
				C += ((D << 5) | (int)((uint)D >> 27)) + Sha1Digest.H(E, A, B) + this.X[idx++] + 1859775393;
				E = (E << 30) | (int)((uint)E >> 2);
				B += ((C << 5) | (int)((uint)C >> 27)) + Sha1Digest.H(D, E, A) + this.X[idx++] + 1859775393;
				D = (D << 30) | (int)((uint)D >> 2);
				A += ((B << 5) | (int)((uint)B >> 27)) + Sha1Digest.H(C, D, E) + this.X[idx++] + 1859775393;
				C = (C << 30) | (int)((uint)C >> 2);
			}
			for (int l = 0; l < 4; l++)
			{
				E += ((A << 5) | (int)((uint)A >> 27)) + Sha1Digest.G(B, C, D) + this.X[idx++] + -1894007588;
				B = (B << 30) | (int)((uint)B >> 2);
				D += ((E << 5) | (int)((uint)E >> 27)) + Sha1Digest.G(A, B, C) + this.X[idx++] + -1894007588;
				A = (A << 30) | (int)((uint)A >> 2);
				C += ((D << 5) | (int)((uint)D >> 27)) + Sha1Digest.G(E, A, B) + this.X[idx++] + -1894007588;
				E = (E << 30) | (int)((uint)E >> 2);
				B += ((C << 5) | (int)((uint)C >> 27)) + Sha1Digest.G(D, E, A) + this.X[idx++] + -1894007588;
				D = (D << 30) | (int)((uint)D >> 2);
				A += ((B << 5) | (int)((uint)B >> 27)) + Sha1Digest.G(C, D, E) + this.X[idx++] + -1894007588;
				C = (C << 30) | (int)((uint)C >> 2);
			}
			for (int m = 0; m <= 3; m++)
			{
				E += ((A << 5) | (int)((uint)A >> 27)) + Sha1Digest.H(B, C, D) + this.X[idx++] + -899497514;
				B = (B << 30) | (int)((uint)B >> 2);
				D += ((E << 5) | (int)((uint)E >> 27)) + Sha1Digest.H(A, B, C) + this.X[idx++] + -899497514;
				A = (A << 30) | (int)((uint)A >> 2);
				C += ((D << 5) | (int)((uint)D >> 27)) + Sha1Digest.H(E, A, B) + this.X[idx++] + -899497514;
				E = (E << 30) | (int)((uint)E >> 2);
				B += ((C << 5) | (int)((uint)C >> 27)) + Sha1Digest.H(D, E, A) + this.X[idx++] + -899497514;
				D = (D << 30) | (int)((uint)D >> 2);
				A += ((B << 5) | (int)((uint)B >> 27)) + Sha1Digest.H(C, D, E) + this.X[idx++] + -899497514;
				C = (C << 30) | (int)((uint)C >> 2);
			}
			this.H1 += A;
			this.H2 += B;
			this.H3 += C;
			this.H4 += D;
			this.H5 += E;
			this.xOff = 0;
			for (int n = 0; n < 16; n++)
			{
				this.X[n] = 0;
			}
		}

		private const int DigestLength = 20;

		private const int Y1 = 1518500249;

		private const int Y2 = 1859775393;

		private const int Y3 = -1894007588;

		private const int Y4 = -899497514;

		private int H1;

		private int H2;

		private int H3;

		private int H4;

		private int H5;

		private int[] X = new int[80];

		private int xOff;
	}
}
