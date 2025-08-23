using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Noise
{
	public class SimplexNoise
	{
		public float ComputeNoise(float x, float y)
		{
			float F2 = 0.3660254f;
			float s = (x + y) * F2;
			int i = SimplexNoise.FastFloor(x + s);
			int j = SimplexNoise.FastFloor(y + s);
			float G2 = 0.21132487f;
			float t = (float)(i + j) * G2;
			float X0 = (float)i - t;
			float Y0 = (float)j - t;
			float x2 = x - X0;
			float y2 = y - Y0;
			int i2;
			int j2;
			if (x2 > y2)
			{
				i2 = 1;
				j2 = 0;
			}
			else
			{
				i2 = 0;
				j2 = 1;
			}
			float x3 = x2 - (float)i2 + G2;
			float y3 = y2 - (float)j2 + G2;
			float x4 = x2 - 1f + 2f * G2;
			float y4 = y2 - 1f + 2f * G2;
			int ii = i & 255;
			int jj = j & 255;
			int gi0 = this._permute[ii + this._permute[jj]] % 12;
			int gi = this._permute[ii + i2 + this._permute[jj + j2]] % 12;
			int gi2 = this._permute[ii + 1 + this._permute[jj + 1]] % 12;
			float t2 = 0.5f - x2 * x2 - y2 * y2;
			float n0;
			if (t2 < 0f)
			{
				n0 = 0f;
			}
			else
			{
				t2 *= t2;
				n0 = t2 * t2 * SimplexNoise.Dot(SimplexNoise.s_gradientVectors3[gi0], x2, y2);
			}
			float t3 = 0.5f - x3 * x3 - y3 * y3;
			float n;
			if (t3 < 0f)
			{
				n = 0f;
			}
			else
			{
				t3 *= t3;
				n = t3 * t3 * SimplexNoise.Dot(SimplexNoise.s_gradientVectors3[gi], x3, y3);
			}
			float t4 = 0.5f - x4 * x4 - y4 * y4;
			float n2;
			if (t4 < 0f)
			{
				n2 = 0f;
			}
			else
			{
				t4 *= t4;
				n2 = t4 * t4 * SimplexNoise.Dot(SimplexNoise.s_gradientVectors3[gi2], x4, y4);
			}
			return 70f * (n0 + n + n2);
		}

		public float ComputeNoise(float x, float y, float z)
		{
			float F3 = 0.33333334f;
			float s = (x + y + z) * F3;
			int i = SimplexNoise.FastFloor(x + s);
			int j = SimplexNoise.FastFloor(y + s);
			int k = SimplexNoise.FastFloor(z + s);
			float G3 = 0.16666667f;
			float t = (float)(i + j + k) * G3;
			float X0 = (float)i - t;
			float Y0 = (float)j - t;
			float Z0 = (float)k - t;
			float x2 = x - X0;
			float y2 = y - Y0;
			float z2 = z - Z0;
			int i2;
			int j2;
			int k2;
			int i3;
			int j3;
			int k3;
			if (x2 >= y2)
			{
				if (y2 >= z2)
				{
					i2 = 1;
					j2 = 0;
					k2 = 0;
					i3 = 1;
					j3 = 1;
					k3 = 0;
				}
				else if (x2 >= z2)
				{
					i2 = 1;
					j2 = 0;
					k2 = 0;
					i3 = 1;
					j3 = 0;
					k3 = 1;
				}
				else
				{
					i2 = 0;
					j2 = 0;
					k2 = 1;
					i3 = 1;
					j3 = 0;
					k3 = 1;
				}
			}
			else if (y2 < z2)
			{
				i2 = 0;
				j2 = 0;
				k2 = 1;
				i3 = 0;
				j3 = 1;
				k3 = 1;
			}
			else if (x2 < z2)
			{
				i2 = 0;
				j2 = 1;
				k2 = 0;
				i3 = 0;
				j3 = 1;
				k3 = 1;
			}
			else
			{
				i2 = 0;
				j2 = 1;
				k2 = 0;
				i3 = 1;
				j3 = 1;
				k3 = 0;
			}
			float x3 = x2 - (float)i2 + G3;
			float y3 = y2 - (float)j2 + G3;
			float z3 = z2 - (float)k2 + G3;
			float x4 = x2 - (float)i3 + 2f * G3;
			float y4 = y2 - (float)j3 + 2f * G3;
			float z4 = z2 - (float)k3 + 2f * G3;
			float x5 = x2 - 1f + 3f * G3;
			float y5 = y2 - 1f + 3f * G3;
			float z5 = z2 - 1f + 3f * G3;
			int ii = i & 255;
			int jj = j & 255;
			int kk = k & 255;
			int gi0 = this._permute[ii + this._permute[jj + this._permute[kk]]] % 12;
			int gi = this._permute[ii + i2 + this._permute[jj + j2 + this._permute[kk + k2]]] % 12;
			int gi2 = this._permute[ii + i3 + this._permute[jj + j3 + this._permute[kk + k3]]] % 12;
			int gi3 = this._permute[ii + 1 + this._permute[jj + 1 + this._permute[kk + 1]]] % 12;
			float t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
			float n0;
			if (t2 < 0f)
			{
				n0 = 0f;
			}
			else
			{
				t2 *= t2;
				n0 = t2 * t2 * SimplexNoise.Dot(SimplexNoise.s_gradientVectors3[gi0], x2, y2, z2);
			}
			float t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
			float n;
			if (t3 < 0f)
			{
				n = 0f;
			}
			else
			{
				t3 *= t3;
				n = t3 * t3 * SimplexNoise.Dot(SimplexNoise.s_gradientVectors3[gi], x3, y3, z3);
			}
			float t4 = 0.6f - x4 * x4 - y4 * y4 - z4 * z4;
			float n2;
			if (t4 < 0f)
			{
				n2 = 0f;
			}
			else
			{
				t4 *= t4;
				n2 = t4 * t4 * SimplexNoise.Dot(SimplexNoise.s_gradientVectors3[gi2], x4, y4, z4);
			}
			float t5 = 0.6f - x5 * x5 - y5 * y5 - z5 * z5;
			float n3;
			if (t5 < 0f)
			{
				n3 = 0f;
			}
			else
			{
				t5 *= t5;
				n3 = t5 * t5 * SimplexNoise.Dot(SimplexNoise.s_gradientVectors3[gi3], x5, y5, z5);
			}
			return 32f * (n0 + n + n2 + n3);
		}

		public float ComputeNoise(float x, float y, float z, float w)
		{
			float F4 = 0.309017f;
			float G4 = 0.1381966f;
			float s = (x + y + z + w) * F4;
			int i = SimplexNoise.FastFloor(x + s);
			int j = SimplexNoise.FastFloor(y + s);
			int k = SimplexNoise.FastFloor(z + s);
			int l = SimplexNoise.FastFloor(w + s);
			float t = (float)(i + j + k + l) * G4;
			float X0 = (float)i - t;
			float Y0 = (float)j - t;
			float Z0 = (float)k - t;
			float W0 = (float)l - t;
			float x2 = x - X0;
			float y2 = y - Y0;
			float z2 = z - Z0;
			float w2 = w - W0;
			int c = ((x2 > y2) ? 32 : 0);
			int c2 = ((x2 > z2) ? 16 : 0);
			int c3 = ((y2 > z2) ? 8 : 0);
			int c4 = ((x2 > w2) ? 4 : 0);
			int c5 = ((y2 > w2) ? 2 : 0);
			int c6 = ((z2 > w2) ? 1 : 0);
			int c7 = c + c2 + c3 + c4 + c5 + c6;
			int i2 = ((SimplexNoise.s_simplex[c7][0] >= 3) ? 1 : 0);
			int j2 = ((SimplexNoise.s_simplex[c7][1] >= 3) ? 1 : 0);
			int k2 = ((SimplexNoise.s_simplex[c7][2] >= 3) ? 1 : 0);
			int l2 = ((SimplexNoise.s_simplex[c7][3] >= 3) ? 1 : 0);
			int i3 = ((SimplexNoise.s_simplex[c7][0] >= 2) ? 1 : 0);
			int j3 = ((SimplexNoise.s_simplex[c7][1] >= 2) ? 1 : 0);
			int k3 = ((SimplexNoise.s_simplex[c7][2] >= 2) ? 1 : 0);
			int l3 = ((SimplexNoise.s_simplex[c7][3] >= 2) ? 1 : 0);
			int i4 = ((SimplexNoise.s_simplex[c7][0] >= 1) ? 1 : 0);
			int j4 = ((SimplexNoise.s_simplex[c7][1] >= 1) ? 1 : 0);
			int k4 = ((SimplexNoise.s_simplex[c7][2] >= 1) ? 1 : 0);
			int l4 = ((SimplexNoise.s_simplex[c7][3] >= 1) ? 1 : 0);
			float x3 = x2 - (float)i2 + G4;
			float y3 = y2 - (float)j2 + G4;
			float z3 = z2 - (float)k2 + G4;
			float w3 = w2 - (float)l2 + G4;
			float x4 = x2 - (float)i3 + 2f * G4;
			float y4 = y2 - (float)j3 + 2f * G4;
			float z4 = z2 - (float)k3 + 2f * G4;
			float w4 = w2 - (float)l3 + 2f * G4;
			float x5 = x2 - (float)i4 + 3f * G4;
			float y5 = y2 - (float)j4 + 3f * G4;
			float z5 = z2 - (float)k4 + 3f * G4;
			float w5 = w2 - (float)l4 + 3f * G4;
			float x6 = x2 - 1f + 4f * G4;
			float y6 = y2 - 1f + 4f * G4;
			float z6 = z2 - 1f + 4f * G4;
			float w6 = w2 - 1f + 4f * G4;
			int ii = i & 255;
			int jj = j & 255;
			int kk = k & 255;
			int ll = l & 255;
			int gi0 = this._permute[ii + this._permute[jj + this._permute[kk + this._permute[ll]]]] % 32;
			int gi = this._permute[ii + i2 + this._permute[jj + j2 + this._permute[kk + k2 + this._permute[ll + l2]]]] % 32;
			int gi2 = this._permute[ii + i3 + this._permute[jj + j3 + this._permute[kk + k3 + this._permute[ll + l3]]]] % 32;
			int gi3 = this._permute[ii + i4 + this._permute[jj + j4 + this._permute[kk + k4 + this._permute[ll + l4]]]] % 32;
			int gi4 = this._permute[ii + 1 + this._permute[jj + 1 + this._permute[kk + 1 + this._permute[ll + 1]]]] % 32;
			float t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2 - w2 * w2;
			float n0;
			if (t2 < 0f)
			{
				n0 = 0f;
			}
			else
			{
				t2 *= t2;
				n0 = t2 * t2 * SimplexNoise.Dot(SimplexNoise.s_gradientVectors4[gi0], x2, y2, z2, w2);
			}
			float t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3 - w3 * w3;
			float n;
			if (t3 < 0f)
			{
				n = 0f;
			}
			else
			{
				t3 *= t3;
				n = t3 * t3 * SimplexNoise.Dot(SimplexNoise.s_gradientVectors4[gi], x3, y3, z3, w3);
			}
			float t4 = 0.6f - x4 * x4 - y4 * y4 - z4 * z4 - w4 * w4;
			float n2;
			if (t4 < 0f)
			{
				n2 = 0f;
			}
			else
			{
				t4 *= t4;
				n2 = t4 * t4 * SimplexNoise.Dot(SimplexNoise.s_gradientVectors4[gi2], x4, y4, z4, w4);
			}
			float t5 = 0.6f - x5 * x5 - y5 * y5 - z5 * z5 - w5 * w5;
			float n3;
			if (t5 < 0f)
			{
				n3 = 0f;
			}
			else
			{
				t5 *= t5;
				n3 = t5 * t5 * SimplexNoise.Dot(SimplexNoise.s_gradientVectors4[gi3], x5, y5, z5, w5);
			}
			float t6 = 0.6f - x6 * x6 - y6 * y6 - z6 * z6 - w6 * w6;
			float n4;
			if (t6 < 0f)
			{
				n4 = 0f;
			}
			else
			{
				t6 *= t6;
				n4 = t6 * t6 * SimplexNoise.Dot(SimplexNoise.s_gradientVectors4[gi4], x6, y6, z6, w6);
			}
			return 27f * (n0 + n + n2 + n3 + n4);
		}

		public float ComputeNoise(Vector2 v)
		{
			return this.ComputeNoise(v.X, v.Y);
		}

		public float ComputeNoise(Vector3 v)
		{
			return this.ComputeNoise(v.X, v.Y, v.Z);
		}

		public float ComputeNoise(Vector4 v)
		{
			return this.ComputeNoise(v.X, v.Y, v.Z, v.W);
		}

		private static int FastFloor(float x)
		{
			if (x <= 0f)
			{
				return (int)x - 1;
			}
			return (int)x;
		}

		private static float Dot(int[] g, float x, float y)
		{
			return (float)g[0] * x + (float)g[1] * y;
		}

		private static float Dot(int[] g, float x, float y, float z)
		{
			return (float)g[0] * x + (float)g[1] * y + (float)g[2] * z;
		}

		private static float Dot(int[] g, float x, float y, float z, float w)
		{
			return (float)g[0] * x + (float)g[1] * y + (float)g[2] * z + (float)g[3] * w;
		}

		private void Initalize(Random r)
		{
			for (int i = 0; i < 256; i++)
			{
				this._permute[256 + i] = (this._permute[i] = r.Next(256));
			}
		}

		public SimplexNoise()
		{
			this.Initalize(new Random());
		}

		public SimplexNoise(Random r)
		{
			this.Initalize(r);
		}

		// Note: this type is marked as 'beforefieldinit'.
		static SimplexNoise()
		{
			int[][] array = new int[12][];
			int[][] array2 = array;
			int num = 0;
			int[] array3 = new int[3];
			array3[0] = 1;
			array3[1] = 1;
			array2[num] = array3;
			int[][] array4 = array;
			int num2 = 1;
			int[] array5 = new int[3];
			array5[0] = -1;
			array5[1] = 1;
			array4[num2] = array5;
			int[][] array6 = array;
			int num3 = 2;
			int[] array7 = new int[3];
			array7[0] = 1;
			array7[1] = -1;
			array6[num3] = array7;
			int[][] array8 = array;
			int num4 = 3;
			int[] array9 = new int[3];
			array9[0] = -1;
			array9[1] = -1;
			array8[num4] = array9;
			array[4] = new int[] { 1, 0, 1 };
			array[5] = new int[] { -1, 0, 1 };
			array[6] = new int[] { 1, 0, -1 };
			array[7] = new int[] { -1, 0, -1 };
			array[8] = new int[] { 0, 1, 1 };
			array[9] = new int[] { 0, -1, 1 };
			array[10] = new int[] { 0, 1, -1 };
			array[11] = new int[] { 0, -1, -1 };
			SimplexNoise.s_gradientVectors3 = array;
			SimplexNoise.s_gradientVectors4 = new int[][]
			{
				new int[] { 0, 1, 1, 1 },
				new int[] { 0, 1, 1, -1 },
				new int[] { 0, 1, -1, 1 },
				new int[] { 0, 1, -1, -1 },
				new int[] { 0, -1, 1, 1 },
				new int[] { 0, -1, 1, -1 },
				new int[] { 0, -1, -1, 1 },
				new int[] { 0, -1, -1, -1 },
				new int[] { 1, 0, 1, 1 },
				new int[] { 1, 0, 1, -1 },
				new int[] { 1, 0, -1, 1 },
				new int[] { 1, 0, -1, -1 },
				new int[] { -1, 0, 1, 1 },
				new int[] { -1, 0, 1, -1 },
				new int[] { -1, 0, -1, 1 },
				new int[] { -1, 0, -1, -1 },
				new int[] { 1, 1, 0, 1 },
				new int[] { 1, 1, 0, -1 },
				new int[] { 1, -1, 0, 1 },
				new int[] { 1, -1, 0, -1 },
				new int[] { -1, 1, 0, 1 },
				new int[] { -1, 1, 0, -1 },
				new int[] { -1, -1, 0, 1 },
				new int[] { -1, -1, 0, -1 },
				new int[] { 1, 1, 1, 0 },
				new int[] { 1, 1, -1, 0 },
				new int[] { 1, -1, 1, 0 },
				new int[] { 1, -1, -1, 0 },
				new int[] { -1, 1, 1, 0 },
				new int[] { -1, 1, -1, 0 },
				new int[] { -1, -1, 1, 0 },
				new int[] { -1, -1, -1, 0 }
			};
			int[][] array10 = new int[64][];
			array10[0] = new int[] { 0, 1, 2, 3 };
			array10[1] = new int[] { 0, 1, 3, 2 };
			int[][] array11 = array10;
			int num5 = 2;
			int[] array12 = new int[4];
			array11[num5] = array12;
			array10[3] = new int[] { 0, 2, 3, 1 };
			int[][] array13 = array10;
			int num6 = 4;
			int[] array14 = new int[4];
			array13[num6] = array14;
			int[][] array15 = array10;
			int num7 = 5;
			int[] array16 = new int[4];
			array15[num7] = array16;
			int[][] array17 = array10;
			int num8 = 6;
			int[] array18 = new int[4];
			array17[num8] = array18;
			array10[7] = new int[] { 1, 2, 3, 0 };
			array10[8] = new int[] { 0, 2, 1, 3 };
			int[][] array19 = array10;
			int num9 = 9;
			int[] array20 = new int[4];
			array19[num9] = array20;
			array10[10] = new int[] { 0, 3, 1, 2 };
			array10[11] = new int[] { 0, 3, 2, 1 };
			int[][] array21 = array10;
			int num10 = 12;
			int[] array22 = new int[4];
			array21[num10] = array22;
			int[][] array23 = array10;
			int num11 = 13;
			int[] array24 = new int[4];
			array23[num11] = array24;
			int[][] array25 = array10;
			int num12 = 14;
			int[] array26 = new int[4];
			array25[num12] = array26;
			array10[15] = new int[] { 1, 3, 2, 0 };
			int[][] array27 = array10;
			int num13 = 16;
			int[] array28 = new int[4];
			array27[num13] = array28;
			int[][] array29 = array10;
			int num14 = 17;
			int[] array30 = new int[4];
			array29[num14] = array30;
			int[][] array31 = array10;
			int num15 = 18;
			int[] array32 = new int[4];
			array31[num15] = array32;
			int[][] array33 = array10;
			int num16 = 19;
			int[] array34 = new int[4];
			array33[num16] = array34;
			int[][] array35 = array10;
			int num17 = 20;
			int[] array36 = new int[4];
			array35[num17] = array36;
			int[][] array37 = array10;
			int num18 = 21;
			int[] array38 = new int[4];
			array37[num18] = array38;
			int[][] array39 = array10;
			int num19 = 22;
			int[] array40 = new int[4];
			array39[num19] = array40;
			int[][] array41 = array10;
			int num20 = 23;
			int[] array42 = new int[4];
			array41[num20] = array42;
			array10[24] = new int[] { 1, 2, 0, 3 };
			int[][] array43 = array10;
			int num21 = 25;
			int[] array44 = new int[4];
			array43[num21] = array44;
			array10[26] = new int[] { 1, 3, 0, 2 };
			int[][] array45 = array10;
			int num22 = 27;
			int[] array46 = new int[4];
			array45[num22] = array46;
			int[][] array47 = array10;
			int num23 = 28;
			int[] array48 = new int[4];
			array47[num23] = array48;
			int[][] array49 = array10;
			int num24 = 29;
			int[] array50 = new int[4];
			array49[num24] = array50;
			array10[30] = new int[] { 2, 3, 0, 1 };
			array10[31] = new int[] { 2, 3, 1, 0 };
			array10[32] = new int[] { 1, 0, 2, 3 };
			array10[33] = new int[] { 1, 0, 3, 2 };
			int[][] array51 = array10;
			int num25 = 34;
			int[] array52 = new int[4];
			array51[num25] = array52;
			int[][] array53 = array10;
			int num26 = 35;
			int[] array54 = new int[4];
			array53[num26] = array54;
			int[][] array55 = array10;
			int num27 = 36;
			int[] array56 = new int[4];
			array55[num27] = array56;
			array10[37] = new int[] { 2, 0, 3, 1 };
			int[][] array57 = array10;
			int num28 = 38;
			int[] array58 = new int[4];
			array57[num28] = array58;
			array10[39] = new int[] { 2, 1, 3, 0 };
			int[][] array59 = array10;
			int num29 = 40;
			int[] array60 = new int[4];
			array59[num29] = array60;
			int[][] array61 = array10;
			int num30 = 41;
			int[] array62 = new int[4];
			array61[num30] = array62;
			int[][] array63 = array10;
			int num31 = 42;
			int[] array64 = new int[4];
			array63[num31] = array64;
			int[][] array65 = array10;
			int num32 = 43;
			int[] array66 = new int[4];
			array65[num32] = array66;
			int[][] array67 = array10;
			int num33 = 44;
			int[] array68 = new int[4];
			array67[num33] = array68;
			int[][] array69 = array10;
			int num34 = 45;
			int[] array70 = new int[4];
			array69[num34] = array70;
			int[][] array71 = array10;
			int num35 = 46;
			int[] array72 = new int[4];
			array71[num35] = array72;
			int[][] array73 = array10;
			int num36 = 47;
			int[] array74 = new int[4];
			array73[num36] = array74;
			array10[48] = new int[] { 2, 0, 1, 3 };
			int[][] array75 = array10;
			int num37 = 49;
			int[] array76 = new int[4];
			array75[num37] = array76;
			int[][] array77 = array10;
			int num38 = 50;
			int[] array78 = new int[4];
			array77[num38] = array78;
			int[][] array79 = array10;
			int num39 = 51;
			int[] array80 = new int[4];
			array79[num39] = array80;
			array10[52] = new int[] { 3, 0, 1, 2 };
			array10[53] = new int[] { 3, 0, 2, 1 };
			int[][] array81 = array10;
			int num40 = 54;
			int[] array82 = new int[4];
			array81[num40] = array82;
			array10[55] = new int[] { 3, 1, 2, 0 };
			array10[56] = new int[] { 2, 1, 0, 3 };
			int[][] array83 = array10;
			int num41 = 57;
			int[] array84 = new int[4];
			array83[num41] = array84;
			int[][] array85 = array10;
			int num42 = 58;
			int[] array86 = new int[4];
			array85[num42] = array86;
			int[][] array87 = array10;
			int num43 = 59;
			int[] array88 = new int[4];
			array87[num43] = array88;
			array10[60] = new int[] { 3, 1, 0, 2 };
			int[][] array89 = array10;
			int num44 = 61;
			int[] array90 = new int[4];
			array89[num44] = array90;
			array10[62] = new int[] { 3, 2, 0, 1 };
			array10[63] = new int[] { 3, 2, 1, 0 };
			SimplexNoise.s_simplex = array10;
		}

		private const float Sqrt3 = 1.7320508f;

		private const float Sqrt5 = 2.236068f;

		private static readonly int[][] s_gradientVectors3;

		private static readonly int[][] s_gradientVectors4;

		private int[] _permute = new int[512];

		private static readonly int[][] s_simplex;
	}
}
