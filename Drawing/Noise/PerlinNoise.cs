using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Noise
{
	public class PerlinNoise
	{
		public float ComputeNoise(float x)
		{
			int fx = PerlinNoise.FastFloor(x);
			int X = fx & 255;
			x -= (float)fx;
			float u = PerlinNoise.Fade(x);
			float x2 = (float)PerlinNoise.s_gradientVectors[PerlinNoise._permute[X] & 3][0] * x;
			float x3 = (float)PerlinNoise.s_gradientVectors[PerlinNoise._permute[X + 1] & 3][0] * (x - 1f);
			return PerlinNoise.Lerp(u, x2, x3);
		}

		public float ComputeNoise(Vector2 v)
		{
			return this.ComputeNoise(v.X, v.Y);
		}

		public float ComputeNoise(float x, float y)
		{
			int fx = PerlinNoise.FastFloor(x);
			int fy = PerlinNoise.FastFloor(y);
			int X = fx & 255;
			int Y = fy & 255;
			x -= (float)fx;
			y -= (float)fy;
			float u = PerlinNoise.Fade(x);
			float v = PerlinNoise.Fade(y);
			int A = PerlinNoise._permute[X] + Y;
			int B = PerlinNoise._permute[X + 1] + Y;
			float g0 = PerlinNoise.Grad(PerlinNoise._permute[A], x, y);
			float g = PerlinNoise.Grad(PerlinNoise._permute[B], x - 1f, y);
			float g2 = PerlinNoise.Grad(PerlinNoise._permute[A + 1], x, y - 1f);
			float g3 = PerlinNoise.Grad(PerlinNoise._permute[B + 1], x - 1f, y - 1f);
			float u2 = PerlinNoise.Lerp(u, g0, g);
			float u3 = PerlinNoise.Lerp(u, g2, g3);
			return PerlinNoise.Lerp(v, u2, u3);
		}

		public float ComputeNoise(Vector3 vect)
		{
			float x = vect.X;
			float y = vect.Y;
			float z = vect.Z;
			int fx = ((x > 0f) ? ((int)x) : ((int)x - 1));
			int fy = ((y > 0f) ? ((int)y) : ((int)y - 1));
			int fz = ((z > 0f) ? ((int)z) : ((int)z - 1));
			int X = fx & 255;
			int Y = fy & 255;
			int Z = fz & 255;
			x -= (float)fx;
			y -= (float)fy;
			z -= (float)fz;
			float u = x * x * x * (x * (x * 6f - 15f) + 10f);
			float v = y * y * y * (y * (y * 6f - 15f) + 10f);
			float w = z * z * z * (z * (z * 6f - 15f) + 10f);
			int A = PerlinNoise._permute[X] + Y;
			int AA = PerlinNoise._permute[A] + Z;
			int AB = PerlinNoise._permute[A + 1] + Z;
			int B = PerlinNoise._permute[X + 1] + Y;
			int BA = PerlinNoise._permute[B] + Z;
			int BB = PerlinNoise._permute[B + 1] + Z;
			float xm = x - 1f;
			float ym = y - 1f;
			float zm = z - 1f;
			int[] grad = PerlinNoise.s_gradientVectors[PerlinNoise._permute[AA] & 15];
			float g0 = x * (float)grad[0] + y * (float)grad[1] + z * (float)grad[2];
			grad = PerlinNoise.s_gradientVectors[PerlinNoise._permute[BA] & 15];
			float g = xm * (float)grad[0] + y * (float)grad[1] + z * (float)grad[2];
			grad = PerlinNoise.s_gradientVectors[PerlinNoise._permute[AB] & 15];
			float g2 = x * (float)grad[0] + ym * (float)grad[1] + z * (float)grad[2];
			grad = PerlinNoise.s_gradientVectors[PerlinNoise._permute[BB] & 15];
			float g3 = xm * (float)grad[0] + ym * (float)grad[1] + z * (float)grad[2];
			grad = PerlinNoise.s_gradientVectors[PerlinNoise._permute[AA + 1] & 15];
			float g4 = x * (float)grad[0] + y * (float)grad[1] + zm * (float)grad[2];
			grad = PerlinNoise.s_gradientVectors[PerlinNoise._permute[BA + 1] & 15];
			float g5 = xm * (float)grad[0] + y * (float)grad[1] + zm * (float)grad[2];
			grad = PerlinNoise.s_gradientVectors[PerlinNoise._permute[AB + 1] & 15];
			float g6 = x * (float)grad[0] + ym * (float)grad[1] + zm * (float)grad[2];
			grad = PerlinNoise.s_gradientVectors[PerlinNoise._permute[BB + 1] & 15];
			float g7 = xm * (float)grad[0] + ym * (float)grad[1] + zm * (float)grad[2];
			float u2 = g0 + u * (g - g0);
			float u3 = g2 + u * (g3 - g2);
			float u4 = g4 + u * (g5 - g4);
			float u5 = g6 + u * (g7 - g6);
			float v2 = u2 + v * (u3 - u2);
			float v3 = u4 + v * (u5 - u4);
			return v2 + w * (v3 - v2);
		}

		public float ComputeNoise(float x, float y, float z)
		{
			int fx = ((x > 0f) ? ((int)x) : ((int)x - 1));
			int fy = ((y > 0f) ? ((int)y) : ((int)y - 1));
			int fz = ((z > 0f) ? ((int)z) : ((int)z - 1));
			int X = fx & 255;
			int Y = fy & 255;
			int Z = fz & 255;
			x -= (float)fx;
			y -= (float)fy;
			z -= (float)fz;
			float u = x * x * x * (x * (x * 6f - 15f) + 10f);
			float v = y * y * y * (y * (y * 6f - 15f) + 10f);
			float w = z * z * z * (z * (z * 6f - 15f) + 10f);
			int A = PerlinNoise._permute[X] + Y;
			int AA = PerlinNoise._permute[A] + Z;
			int AB = PerlinNoise._permute[A + 1] + Z;
			int B = PerlinNoise._permute[X + 1] + Y;
			int BA = PerlinNoise._permute[B] + Z;
			int BB = PerlinNoise._permute[B + 1] + Z;
			float xm = x - 1f;
			float ym = y - 1f;
			float zm = z - 1f;
			int[] grad = PerlinNoise.s_gradientVectors[PerlinNoise._permute[AA] & 15];
			float g0 = x * (float)grad[0] + y * (float)grad[1] + z * (float)grad[2];
			grad = PerlinNoise.s_gradientVectors[PerlinNoise._permute[BA] & 15];
			float g = xm * (float)grad[0] + y * (float)grad[1] + z * (float)grad[2];
			grad = PerlinNoise.s_gradientVectors[PerlinNoise._permute[AB] & 15];
			float g2 = x * (float)grad[0] + ym * (float)grad[1] + z * (float)grad[2];
			grad = PerlinNoise.s_gradientVectors[PerlinNoise._permute[BB] & 15];
			float g3 = xm * (float)grad[0] + ym * (float)grad[1] + z * (float)grad[2];
			grad = PerlinNoise.s_gradientVectors[PerlinNoise._permute[AA + 1] & 15];
			float g4 = x * (float)grad[0] + y * (float)grad[1] + zm * (float)grad[2];
			grad = PerlinNoise.s_gradientVectors[PerlinNoise._permute[BA + 1] & 15];
			float g5 = xm * (float)grad[0] + y * (float)grad[1] + zm * (float)grad[2];
			grad = PerlinNoise.s_gradientVectors[PerlinNoise._permute[AB + 1] & 15];
			float g6 = x * (float)grad[0] + ym * (float)grad[1] + zm * (float)grad[2];
			grad = PerlinNoise.s_gradientVectors[PerlinNoise._permute[BB + 1] & 15];
			float g7 = xm * (float)grad[0] + ym * (float)grad[1] + zm * (float)grad[2];
			float u2 = g0 + u * (g - g0);
			float u3 = g2 + u * (g3 - g2);
			float u4 = g4 + u * (g5 - g4);
			float u5 = g6 + u * (g7 - g6);
			float v2 = u2 + v * (u3 - u2);
			float v3 = u4 + v * (u5 - u4);
			return v2 + w * (v3 - v2);
		}

		public float ComputeNoise(Vector4 v)
		{
			return this.ComputeNoise(v.X, v.Y, v.Z, v.W);
		}

		public float ComputeNoise(float x, float y, float z, float t)
		{
			int fx = PerlinNoise.FastFloor(x);
			int fy = PerlinNoise.FastFloor(y);
			int fz = PerlinNoise.FastFloor(z);
			int ft = PerlinNoise.FastFloor(t);
			int X = fx & 255;
			int Y = fy & 255;
			int Z = fz & 255;
			int T = ft & 255;
			x -= (float)fx;
			y -= (float)fy;
			z -= (float)fz;
			t -= (float)ft;
			float u = PerlinNoise.Fade(x);
			float v = PerlinNoise.Fade(y);
			float w = PerlinNoise.Fade(z);
			float s = PerlinNoise.Fade(t);
			int A = PerlinNoise._permute[X] + Y;
			int AA = PerlinNoise._permute[A] + Z;
			int AAA = PerlinNoise._permute[AA] + T;
			int AAB = PerlinNoise._permute[AA + 1] + T;
			int AB = PerlinNoise._permute[A + 1] + Z;
			int ABA = PerlinNoise._permute[AB] + T;
			int ABB = PerlinNoise._permute[AB + 1] + T;
			int B = PerlinNoise._permute[X + 1] + Y;
			int BA = PerlinNoise._permute[B] + Z;
			int BAA = PerlinNoise._permute[BA] + T;
			int BAB = PerlinNoise._permute[BA + 1] + T;
			int BB = PerlinNoise._permute[B + 1] + Z;
			int BBA = PerlinNoise._permute[BB] + T;
			int BBB = PerlinNoise._permute[BB + 1] + T;
			float g0 = PerlinNoise.Grad(PerlinNoise._permute[AAA], x, y, z, t);
			float g = PerlinNoise.Grad(PerlinNoise._permute[BAA], x - 1f, y, z, t);
			float g2 = PerlinNoise.Grad(PerlinNoise._permute[ABA], x, y - 1f, z, t);
			float g3 = PerlinNoise.Grad(PerlinNoise._permute[BBA], x - 1f, y - 1f, z, t);
			float g4 = PerlinNoise.Grad(PerlinNoise._permute[AAB], x, y, z - 1f, t);
			float g5 = PerlinNoise.Grad(PerlinNoise._permute[BAB], x - 1f, y, z - 1f, t);
			float g6 = PerlinNoise.Grad(PerlinNoise._permute[ABB], x, y - 1f, z - 1f, t);
			float g7 = PerlinNoise.Grad(PerlinNoise._permute[BBB], x - 1f, y - 1f, z - 1f, t);
			float g8 = PerlinNoise.Grad(PerlinNoise._permute[AAA + 1], x, y, z, t - 1f);
			float g9 = PerlinNoise.Grad(PerlinNoise._permute[BAA + 1], x - 1f, y, z, t - 1f);
			float g10 = PerlinNoise.Grad(PerlinNoise._permute[ABA + 1], x, y - 1f, z, t - 1f);
			float g11 = PerlinNoise.Grad(PerlinNoise._permute[BBA + 1], x - 1f, y - 1f, z, t - 1f);
			float g12 = PerlinNoise.Grad(PerlinNoise._permute[AAB + 1], x, y, z - 1f, t - 1f);
			float g13 = PerlinNoise.Grad(PerlinNoise._permute[BAB + 1], x - 1f, y, z - 1f, t - 1f);
			float g14 = PerlinNoise.Grad(PerlinNoise._permute[ABB + 1], x, y - 1f, z - 1f, t - 1f);
			float g15 = PerlinNoise.Grad(PerlinNoise._permute[BBB + 1], x - 1f, y - 1f, z - 1f, t - 1f);
			float g16 = PerlinNoise.Lerp(s, g7, g15);
			float g17 = PerlinNoise.Lerp(s, g3, g11);
			float g18 = PerlinNoise.Lerp(s, g5, g13);
			float g19 = PerlinNoise.Lerp(s, g, g9);
			float g20 = PerlinNoise.Lerp(s, g6, g14);
			float g21 = PerlinNoise.Lerp(s, g2, g10);
			float g22 = PerlinNoise.Lerp(s, g4, g12);
			float g23 = PerlinNoise.Lerp(s, g0, g8);
			float g24 = PerlinNoise.Lerp(w, g17, g16);
			float g25 = PerlinNoise.Lerp(w, g19, g18);
			float g26 = PerlinNoise.Lerp(w, g21, g20);
			float g27 = PerlinNoise.Lerp(w, g23, g22);
			float g28 = PerlinNoise.Lerp(v, g25, g24);
			float g29 = PerlinNoise.Lerp(v, g27, g26);
			return PerlinNoise.Lerp(u, g29, g28);
		}

		private static float Fade(float t)
		{
			return t * t * t * (t * (t * 6f - 15f) + 10f);
		}

		private static float Lerp(float t, float a, float b)
		{
			return a + t * (b - a);
		}

		private static int FastFloor(float x)
		{
			if (x <= 0f)
			{
				return (int)x - 1;
			}
			return (int)x;
		}

		private static float Grad(int hash, float x, float y, float z, float t)
		{
			int[] gradient = PerlinNoise.s_gradientVectors[hash & 31];
			return x * (float)gradient[0] + y * (float)gradient[1] + z * (float)gradient[2] + t * (float)gradient[3];
		}

		private static float Grad(int hash, float x, float y, float z)
		{
			int[] gradient = PerlinNoise.s_gradientVectors[hash & 15];
			return x * (float)gradient[0] + y * (float)gradient[1] + z * (float)gradient[2];
		}

		private static float Grad(int hash, float x, float y)
		{
			int[] gradient = PerlinNoise.s_gradientVectors[hash & 7];
			return x * (float)gradient[0] + y * (float)gradient[1];
		}

		private void Initalize(Random r)
		{
			for (int i = 0; i < 256; i++)
			{
				PerlinNoise._permute[256 + i] = (PerlinNoise._permute[i] = r.Next(256));
			}
			for (int j = 0; j < 512; j++)
			{
				PerlinNoise._permute[512 + j] = PerlinNoise._permute[j];
			}
		}

		public PerlinNoise()
		{
			this.Initalize(new Random());
		}

		public PerlinNoise(Random r)
		{
			this.Initalize(r);
		}

		private static readonly int[][] s_gradientVectors = new int[][]
		{
			new int[] { 0, -1, -1, -1 },
			new int[] { -1, 0, -1, -1 },
			new int[] { 1, 0, -1, -1 },
			new int[] { 0, 1, -1, -1 },
			new int[] { -1, -1, 0, -1 },
			new int[] { 1, -1, 0, -1 },
			new int[] { -1, 1, 0, -1 },
			new int[] { 1, 1, 0, -1 },
			new int[] { 0, -1, 1, -1 },
			new int[] { -1, 0, 1, -1 },
			new int[] { 1, 0, 1, -1 },
			new int[] { 0, 1, 1, -1 },
			new int[] { 1, 1, 0, 1 },
			new int[] { -1, 1, 0, 1 },
			new int[] { 0, -1, -1, 1 },
			new int[] { 0, -1, 1, 1 },
			new int[] { -1, -1, -1, 0 },
			new int[] { 1, -1, -1, 0 },
			new int[] { -1, 1, -1, 0 },
			new int[] { 1, 1, -1, 0 },
			new int[] { -1, -1, 1, 0 },
			new int[] { 1, -1, 1, 0 },
			new int[] { -1, 1, 1, 0 },
			new int[] { 1, 1, 1, 0 },
			new int[] { -1, 0, -1, 1 },
			new int[] { 1, 0, -1, 1 },
			new int[] { 0, 1, -1, 1 },
			new int[] { -1, -1, 0, 1 },
			new int[] { 1, -1, 0, 1 },
			new int[] { -1, 0, 1, 1 },
			new int[] { 1, 0, 1, 1 },
			new int[] { 0, 1, 1, 1 }
		};

		private static int[] _permute = new int[1024];
	}
}
