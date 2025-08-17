using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Noise
{
	public class PerlinNoise
	{
		public float ComputeNoise(float x)
		{
			int num = PerlinNoise.FastFloor(x);
			int num2 = num & 255;
			x -= (float)num;
			float num3 = PerlinNoise.Fade(x);
			float num4 = (float)PerlinNoise.s_gradientVectors[PerlinNoise._permute[num2] & 3][0] * x;
			float num5 = (float)PerlinNoise.s_gradientVectors[PerlinNoise._permute[num2 + 1] & 3][0] * (x - 1f);
			return PerlinNoise.Lerp(num3, num4, num5);
		}

		public float ComputeNoise(Vector2 v)
		{
			return this.ComputeNoise(v.X, v.Y);
		}

		public float ComputeNoise(float x, float y)
		{
			int num = PerlinNoise.FastFloor(x);
			int num2 = PerlinNoise.FastFloor(y);
			int num3 = num & 255;
			int num4 = num2 & 255;
			x -= (float)num;
			y -= (float)num2;
			float num5 = PerlinNoise.Fade(x);
			float num6 = PerlinNoise.Fade(y);
			int num7 = PerlinNoise._permute[num3] + num4;
			int num8 = PerlinNoise._permute[num3 + 1] + num4;
			float num9 = PerlinNoise.Grad(PerlinNoise._permute[num7], x, y);
			float num10 = PerlinNoise.Grad(PerlinNoise._permute[num8], x - 1f, y);
			float num11 = PerlinNoise.Grad(PerlinNoise._permute[num7 + 1], x, y - 1f);
			float num12 = PerlinNoise.Grad(PerlinNoise._permute[num8 + 1], x - 1f, y - 1f);
			float num13 = PerlinNoise.Lerp(num5, num9, num10);
			float num14 = PerlinNoise.Lerp(num5, num11, num12);
			return PerlinNoise.Lerp(num6, num13, num14);
		}

		public float ComputeNoise(Vector3 vect)
		{
			float num = vect.X;
			float num2 = vect.Y;
			float num3 = vect.Z;
			int num4 = ((num > 0f) ? ((int)num) : ((int)num - 1));
			int num5 = ((num2 > 0f) ? ((int)num2) : ((int)num2 - 1));
			int num6 = ((num3 > 0f) ? ((int)num3) : ((int)num3 - 1));
			int num7 = num4 & 255;
			int num8 = num5 & 255;
			int num9 = num6 & 255;
			num -= (float)num4;
			num2 -= (float)num5;
			num3 -= (float)num6;
			float num10 = num * num * num * (num * (num * 6f - 15f) + 10f);
			float num11 = num2 * num2 * num2 * (num2 * (num2 * 6f - 15f) + 10f);
			float num12 = num3 * num3 * num3 * (num3 * (num3 * 6f - 15f) + 10f);
			int num13 = PerlinNoise._permute[num7] + num8;
			int num14 = PerlinNoise._permute[num13] + num9;
			int num15 = PerlinNoise._permute[num13 + 1] + num9;
			int num16 = PerlinNoise._permute[num7 + 1] + num8;
			int num17 = PerlinNoise._permute[num16] + num9;
			int num18 = PerlinNoise._permute[num16 + 1] + num9;
			float num19 = num - 1f;
			float num20 = num2 - 1f;
			float num21 = num3 - 1f;
			int[] array = PerlinNoise.s_gradientVectors[PerlinNoise._permute[num14] & 15];
			float num22 = num * (float)array[0] + num2 * (float)array[1] + num3 * (float)array[2];
			array = PerlinNoise.s_gradientVectors[PerlinNoise._permute[num17] & 15];
			float num23 = num19 * (float)array[0] + num2 * (float)array[1] + num3 * (float)array[2];
			array = PerlinNoise.s_gradientVectors[PerlinNoise._permute[num15] & 15];
			float num24 = num * (float)array[0] + num20 * (float)array[1] + num3 * (float)array[2];
			array = PerlinNoise.s_gradientVectors[PerlinNoise._permute[num18] & 15];
			float num25 = num19 * (float)array[0] + num20 * (float)array[1] + num3 * (float)array[2];
			array = PerlinNoise.s_gradientVectors[PerlinNoise._permute[num14 + 1] & 15];
			float num26 = num * (float)array[0] + num2 * (float)array[1] + num21 * (float)array[2];
			array = PerlinNoise.s_gradientVectors[PerlinNoise._permute[num17 + 1] & 15];
			float num27 = num19 * (float)array[0] + num2 * (float)array[1] + num21 * (float)array[2];
			array = PerlinNoise.s_gradientVectors[PerlinNoise._permute[num15 + 1] & 15];
			float num28 = num * (float)array[0] + num20 * (float)array[1] + num21 * (float)array[2];
			array = PerlinNoise.s_gradientVectors[PerlinNoise._permute[num18 + 1] & 15];
			float num29 = num19 * (float)array[0] + num20 * (float)array[1] + num21 * (float)array[2];
			float num30 = num22 + num10 * (num23 - num22);
			float num31 = num24 + num10 * (num25 - num24);
			float num32 = num26 + num10 * (num27 - num26);
			float num33 = num28 + num10 * (num29 - num28);
			float num34 = num30 + num11 * (num31 - num30);
			float num35 = num32 + num11 * (num33 - num32);
			return num34 + num12 * (num35 - num34);
		}

		public float ComputeNoise(float x, float y, float z)
		{
			int num = ((x > 0f) ? ((int)x) : ((int)x - 1));
			int num2 = ((y > 0f) ? ((int)y) : ((int)y - 1));
			int num3 = ((z > 0f) ? ((int)z) : ((int)z - 1));
			int num4 = num & 255;
			int num5 = num2 & 255;
			int num6 = num3 & 255;
			x -= (float)num;
			y -= (float)num2;
			z -= (float)num3;
			float num7 = x * x * x * (x * (x * 6f - 15f) + 10f);
			float num8 = y * y * y * (y * (y * 6f - 15f) + 10f);
			float num9 = z * z * z * (z * (z * 6f - 15f) + 10f);
			int num10 = PerlinNoise._permute[num4] + num5;
			int num11 = PerlinNoise._permute[num10] + num6;
			int num12 = PerlinNoise._permute[num10 + 1] + num6;
			int num13 = PerlinNoise._permute[num4 + 1] + num5;
			int num14 = PerlinNoise._permute[num13] + num6;
			int num15 = PerlinNoise._permute[num13 + 1] + num6;
			float num16 = x - 1f;
			float num17 = y - 1f;
			float num18 = z - 1f;
			int[] array = PerlinNoise.s_gradientVectors[PerlinNoise._permute[num11] & 15];
			float num19 = x * (float)array[0] + y * (float)array[1] + z * (float)array[2];
			array = PerlinNoise.s_gradientVectors[PerlinNoise._permute[num14] & 15];
			float num20 = num16 * (float)array[0] + y * (float)array[1] + z * (float)array[2];
			array = PerlinNoise.s_gradientVectors[PerlinNoise._permute[num12] & 15];
			float num21 = x * (float)array[0] + num17 * (float)array[1] + z * (float)array[2];
			array = PerlinNoise.s_gradientVectors[PerlinNoise._permute[num15] & 15];
			float num22 = num16 * (float)array[0] + num17 * (float)array[1] + z * (float)array[2];
			array = PerlinNoise.s_gradientVectors[PerlinNoise._permute[num11 + 1] & 15];
			float num23 = x * (float)array[0] + y * (float)array[1] + num18 * (float)array[2];
			array = PerlinNoise.s_gradientVectors[PerlinNoise._permute[num14 + 1] & 15];
			float num24 = num16 * (float)array[0] + y * (float)array[1] + num18 * (float)array[2];
			array = PerlinNoise.s_gradientVectors[PerlinNoise._permute[num12 + 1] & 15];
			float num25 = x * (float)array[0] + num17 * (float)array[1] + num18 * (float)array[2];
			array = PerlinNoise.s_gradientVectors[PerlinNoise._permute[num15 + 1] & 15];
			float num26 = num16 * (float)array[0] + num17 * (float)array[1] + num18 * (float)array[2];
			float num27 = num19 + num7 * (num20 - num19);
			float num28 = num21 + num7 * (num22 - num21);
			float num29 = num23 + num7 * (num24 - num23);
			float num30 = num25 + num7 * (num26 - num25);
			float num31 = num27 + num8 * (num28 - num27);
			float num32 = num29 + num8 * (num30 - num29);
			return num31 + num9 * (num32 - num31);
		}

		public float ComputeNoise(Vector4 v)
		{
			return this.ComputeNoise(v.X, v.Y, v.Z, v.W);
		}

		public float ComputeNoise(float x, float y, float z, float t)
		{
			int num = PerlinNoise.FastFloor(x);
			int num2 = PerlinNoise.FastFloor(y);
			int num3 = PerlinNoise.FastFloor(z);
			int num4 = PerlinNoise.FastFloor(t);
			int num5 = num & 255;
			int num6 = num2 & 255;
			int num7 = num3 & 255;
			int num8 = num4 & 255;
			x -= (float)num;
			y -= (float)num2;
			z -= (float)num3;
			t -= (float)num4;
			float num9 = PerlinNoise.Fade(x);
			float num10 = PerlinNoise.Fade(y);
			float num11 = PerlinNoise.Fade(z);
			float num12 = PerlinNoise.Fade(t);
			int num13 = PerlinNoise._permute[num5] + num6;
			int num14 = PerlinNoise._permute[num13] + num7;
			int num15 = PerlinNoise._permute[num14] + num8;
			int num16 = PerlinNoise._permute[num14 + 1] + num8;
			int num17 = PerlinNoise._permute[num13 + 1] + num7;
			int num18 = PerlinNoise._permute[num17] + num8;
			int num19 = PerlinNoise._permute[num17 + 1] + num8;
			int num20 = PerlinNoise._permute[num5 + 1] + num6;
			int num21 = PerlinNoise._permute[num20] + num7;
			int num22 = PerlinNoise._permute[num21] + num8;
			int num23 = PerlinNoise._permute[num21 + 1] + num8;
			int num24 = PerlinNoise._permute[num20 + 1] + num7;
			int num25 = PerlinNoise._permute[num24] + num8;
			int num26 = PerlinNoise._permute[num24 + 1] + num8;
			float num27 = PerlinNoise.Grad(PerlinNoise._permute[num15], x, y, z, t);
			float num28 = PerlinNoise.Grad(PerlinNoise._permute[num22], x - 1f, y, z, t);
			float num29 = PerlinNoise.Grad(PerlinNoise._permute[num18], x, y - 1f, z, t);
			float num30 = PerlinNoise.Grad(PerlinNoise._permute[num25], x - 1f, y - 1f, z, t);
			float num31 = PerlinNoise.Grad(PerlinNoise._permute[num16], x, y, z - 1f, t);
			float num32 = PerlinNoise.Grad(PerlinNoise._permute[num23], x - 1f, y, z - 1f, t);
			float num33 = PerlinNoise.Grad(PerlinNoise._permute[num19], x, y - 1f, z - 1f, t);
			float num34 = PerlinNoise.Grad(PerlinNoise._permute[num26], x - 1f, y - 1f, z - 1f, t);
			float num35 = PerlinNoise.Grad(PerlinNoise._permute[num15 + 1], x, y, z, t - 1f);
			float num36 = PerlinNoise.Grad(PerlinNoise._permute[num22 + 1], x - 1f, y, z, t - 1f);
			float num37 = PerlinNoise.Grad(PerlinNoise._permute[num18 + 1], x, y - 1f, z, t - 1f);
			float num38 = PerlinNoise.Grad(PerlinNoise._permute[num25 + 1], x - 1f, y - 1f, z, t - 1f);
			float num39 = PerlinNoise.Grad(PerlinNoise._permute[num16 + 1], x, y, z - 1f, t - 1f);
			float num40 = PerlinNoise.Grad(PerlinNoise._permute[num23 + 1], x - 1f, y, z - 1f, t - 1f);
			float num41 = PerlinNoise.Grad(PerlinNoise._permute[num19 + 1], x, y - 1f, z - 1f, t - 1f);
			float num42 = PerlinNoise.Grad(PerlinNoise._permute[num26 + 1], x - 1f, y - 1f, z - 1f, t - 1f);
			float num43 = PerlinNoise.Lerp(num12, num34, num42);
			float num44 = PerlinNoise.Lerp(num12, num30, num38);
			float num45 = PerlinNoise.Lerp(num12, num32, num40);
			float num46 = PerlinNoise.Lerp(num12, num28, num36);
			float num47 = PerlinNoise.Lerp(num12, num33, num41);
			float num48 = PerlinNoise.Lerp(num12, num29, num37);
			float num49 = PerlinNoise.Lerp(num12, num31, num39);
			float num50 = PerlinNoise.Lerp(num12, num27, num35);
			float num51 = PerlinNoise.Lerp(num11, num44, num43);
			float num52 = PerlinNoise.Lerp(num11, num46, num45);
			float num53 = PerlinNoise.Lerp(num11, num48, num47);
			float num54 = PerlinNoise.Lerp(num11, num50, num49);
			float num55 = PerlinNoise.Lerp(num10, num52, num51);
			float num56 = PerlinNoise.Lerp(num10, num54, num53);
			return PerlinNoise.Lerp(num9, num56, num55);
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
			int[] array = PerlinNoise.s_gradientVectors[hash & 31];
			return x * (float)array[0] + y * (float)array[1] + z * (float)array[2] + t * (float)array[3];
		}

		private static float Grad(int hash, float x, float y, float z)
		{
			int[] array = PerlinNoise.s_gradientVectors[hash & 15];
			return x * (float)array[0] + y * (float)array[1] + z * (float)array[2];
		}

		private static float Grad(int hash, float x, float y)
		{
			int[] array = PerlinNoise.s_gradientVectors[hash & 7];
			return x * (float)array[0] + y * (float)array[1];
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
