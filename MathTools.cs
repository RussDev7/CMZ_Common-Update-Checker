using System;
using DNA.Collections;
using Microsoft.Xna.Framework;

namespace DNA
{
	public static class MathTools
	{
		public static float Mod(float value, float modulus)
		{
			return (value % modulus + modulus) % modulus;
		}

		public static int Mod(int value, int modulus)
		{
			return (value % modulus + modulus) % modulus;
		}

		public static double Mod(double value, double modulus)
		{
			return (value % modulus + modulus) % modulus;
		}

		public static Angle AngleBetween(this Quaternion q1, Quaternion q2)
		{
			return Angle.ACos((double)Quaternion.Dot(q1, q2)) * 2f;
		}

		public static Vector3 Floor(this Vector3 v)
		{
			return new Vector3((float)Math.Floor((double)v.X), (float)Math.Floor((double)v.Y), (float)Math.Floor((double)v.Z));
		}

		public static Angle AngleBetween(this Vector3 v1, Vector3 v2)
		{
			v1.Normalize();
			v2.Normalize();
			float dot = Vector3.Dot(v1, v2);
			dot = Math.Min(dot, 1f);
			dot = Math.Max(dot, -1f);
			float rads = (float)Math.Acos((double)dot);
			return Angle.FromRadians(rads);
		}

		public static bool Coincident(this Vector3 v1, Vector3 v2)
		{
			return Vector3.DistanceSquared(v1, v2) < 1E-05f;
		}

		public static bool Coincident(this Vector3 v1, Vector3 v2, float tolerance)
		{
			return Vector3.DistanceSquared(v1, v2) < tolerance;
		}

		public static Quaternion RotationBetween(this Vector3 v1, Vector3 v2)
		{
			v1.Normalize();
			v2.Normalize();
			Vector3 cross = Vector3.Cross(v1, v2);
			return new Quaternion(cross.X, cross.Y, cross.Z, 1f + Vector3.Dot(v1, v2));
		}

		public static Vector3 GetValidUpVector(Vector3 forward)
		{
			if (Math.Abs(forward.X) + Math.Abs(forward.Z) > 0f)
			{
				return Vector3.Up;
			}
			return Vector3.Right;
		}

		public static Matrix CreateWorld(Vector3 position, Vector3 fwd)
		{
			return Matrix.CreateWorld(position, fwd, MathTools.GetValidUpVector(fwd));
		}

		public static double Distance(this Point p1, Point p2)
		{
			int dx = p2.X - p1.X;
			int dy = p2.Y - p1.Y;
			return Math.Sqrt((double)(dx * dx + dy * dy));
		}

		public static float Cross(this Vector2 v1, Vector2 v2)
		{
			return v1.X * v2.Y - v1.Y * v2.X;
		}

		public static int[] RandomArray(int length)
		{
			return MathTools.RandomArray(0, length);
		}

		public static int[] RandomArray(int start, int length)
		{
			return new Random().RandomArray(start, length);
		}

		public static int[] RandomArray(this Random rand, int length)
		{
			return rand.RandomArray(0, length);
		}

		public static bool Decide(this Random rand, float percentTrue)
		{
			return rand.NextDouble() > (double)percentTrue;
		}

		public static int[] RandomArray(this Random rand, int start, int length)
		{
			int[] nums = new int[length];
			for (int i = 0; i < length; i++)
			{
				nums[i] = start + i;
			}
			ArrayTools.Randomize<int>(nums, rand);
			return nums;
		}

		public static double RandomDouble(this Random rand, double min, double max)
		{
			if (max < min)
			{
				double temp = max;
				max = min;
				min = temp;
			}
			double delta = max - min;
			double scaledDelta = delta * rand.NextDouble();
			return min + scaledDelta;
		}

		public static float RandomFloat()
		{
			return (float)MathTools.Rnd.NextDouble();
		}

		public static float RandomFloat(float magnitude)
		{
			return (float)MathTools.Rnd.NextDouble() * magnitude;
		}

		public static float RandomFloat(float min, float max)
		{
			return MathHelper.Lerp(min, max, (float)MathTools.Rnd.NextDouble());
		}

		public static int RandomInt()
		{
			return MathTools.Rnd.Next();
		}

		public static int RandomInt(int max)
		{
			return MathTools.Rnd.Next(0, max);
		}

		public static int RandomInt(int min, int max)
		{
			return MathTools.Rnd.Next(min, max);
		}

		public static bool RandomBool()
		{
			return MathTools.Rnd.Next(0, 2) == 0;
		}

		public static int IntDifference(float a, float b)
		{
			throw new NotImplementedException();
		}

		public static int IntDifference(double a, double b)
		{
			throw new NotImplementedException();
		}

		public static int IntRound(double d)
		{
			return (int)Math.Round(d);
		}

		public static int IntRound(float d)
		{
			return (int)Math.Round((double)d);
		}

		public static float MapAndLerp(float value, float map1, float map2, float lerp1, float lerp2)
		{
			float t = ((value - map1) / (map2 - map1)).Clamp(0f, 1f);
			return MathHelper.Lerp(lerp1, lerp2, t);
		}

		public static float GetTimeFixedLerpValue(float deltaTime, float timeToPercentage, float percentage)
		{
			if (timeToPercentage == 0f)
			{
				return 1f;
			}
			float d = deltaTime / timeToPercentage;
			return 1f - (float)Math.Pow((double)percentage, (double)d);
		}

		public static float GetTimeFixedLerpValue(float deltaTime, float timeToPercentage)
		{
			return MathTools.GetTimeFixedLerpValue(deltaTime, timeToPercentage, 0.5f);
		}

		public static float Clamp(this float val, float min, float max)
		{
			if (val < min)
			{
				return min;
			}
			if (val > max)
			{
				return max;
			}
			return val;
		}

		public static int Clamp(this int val, int min, int max)
		{
			if (val < min)
			{
				return min;
			}
			if (val > max)
			{
				return max;
			}
			return val;
		}

		public static bool IsPowerOf2(this int v)
		{
			return (v & (v - 1)) == 0 && v != 0;
		}

		public static bool IsPowerOf2(this uint v)
		{
			return (v & (v - 1U)) == 0U && v != 0U;
		}

		public static uint NextPowerOf2(this uint v)
		{
			v -= 1U;
			v |= v >> 1;
			v |= v >> 2;
			v |= v >> 4;
			v |= v >> 8;
			v |= v >> 16;
			v += 1U;
			return v;
		}

		public static int LogBase2(this uint v)
		{
			uint tt;
			if ((tt = v >> 16) != 0U)
			{
				uint t;
				if ((t = tt >> 8) == 0U)
				{
					return (int)(16 + MathTools.LogTable256[(int)((UIntPtr)tt)]);
				}
				return (int)(24 + MathTools.LogTable256[(int)((UIntPtr)t)]);
			}
			else
			{
				uint t;
				if ((t = v >> 8) == 0U)
				{
					return (int)MathTools.LogTable256[(int)((UIntPtr)v)];
				}
				return (int)(8 + MathTools.LogTable256[(int)((UIntPtr)t)]);
			}
		}

		public static Matrix QuickInvert(this Matrix mat)
		{
			Matrix result = Matrix.Identity;
			Vector3 w = mat.Translation;
			result.M11 = mat.M11;
			result.M12 = mat.M21;
			result.M13 = mat.M31;
			result.M21 = mat.M12;
			result.M22 = mat.M22;
			result.M23 = mat.M32;
			result.M31 = mat.M13;
			result.M32 = mat.M23;
			result.M33 = mat.M33;
			result.M41 = -Vector3.Dot(w, mat.Right);
			result.M42 = -Vector3.Dot(w, mat.Up);
			result.M43 = -Vector3.Dot(w, mat.Forward);
			return result;
		}

		public static int TrailingZeroBits(this uint v)
		{
			checked
			{
				return MathTools.MultiplyDeBruijnBitPosition[(int)((IntPtr)(unchecked(((ulong)v & -(ulong)v) * 125613361UL) >> 27))];
			}
		}

		public static int BitsSet(this uint v)
		{
			return (int)(MathTools.BitsSetTable256[(int)((UIntPtr)(v & 255U))] + MathTools.BitsSetTable256[(int)((UIntPtr)((v >> 8) & 255U))] + MathTools.BitsSetTable256[(int)((UIntPtr)((v >> 16) & 255U))] + MathTools.BitsSetTable256[(int)((UIntPtr)(v >> 24))]);
		}

		public static uint Max(uint a, uint b, uint c)
		{
			if (a <= b)
			{
				if (b <= c)
				{
					return c;
				}
				return b;
			}
			else
			{
				if (a <= c)
				{
					return c;
				}
				return a;
			}
		}

		public static int Max(int a, int b, int c)
		{
			if (a <= b)
			{
				if (b <= c)
				{
					return c;
				}
				return b;
			}
			else
			{
				if (a <= c)
				{
					return c;
				}
				return a;
			}
		}

		public static float Max(float a, float b, float c)
		{
			if (a <= b)
			{
				if (b <= c)
				{
					return c;
				}
				return b;
			}
			else
			{
				if (a <= c)
				{
					return c;
				}
				return a;
			}
		}

		public static decimal Max(decimal a, decimal b, decimal c)
		{
			if (!(a > b))
			{
				if (!(b > c))
				{
					return c;
				}
				return b;
			}
			else
			{
				if (!(a > c))
				{
					return c;
				}
				return a;
			}
		}

		public static short Max(short a, short b, short c)
		{
			if (a <= b)
			{
				if (b <= c)
				{
					return c;
				}
				return b;
			}
			else
			{
				if (a <= c)
				{
					return c;
				}
				return a;
			}
		}

		public static char Max(char a, char b, char c)
		{
			if (a <= b)
			{
				if (b <= c)
				{
					return c;
				}
				return b;
			}
			else
			{
				if (a <= c)
				{
					return c;
				}
				return a;
			}
		}

		public static byte Max(byte a, byte b, byte c)
		{
			if (a <= b)
			{
				if (b <= c)
				{
					return c;
				}
				return b;
			}
			else
			{
				if (a <= c)
				{
					return c;
				}
				return a;
			}
		}

		public static uint Min(uint a, uint b, uint c)
		{
			if (a >= b)
			{
				if (b >= c)
				{
					return c;
				}
				return b;
			}
			else
			{
				if (a >= c)
				{
					return c;
				}
				return a;
			}
		}

		public static int Min(int a, int b, int c)
		{
			if (a >= b)
			{
				if (b >= c)
				{
					return c;
				}
				return b;
			}
			else
			{
				if (a >= c)
				{
					return c;
				}
				return a;
			}
		}

		public static float Min(float a, float b, float c)
		{
			if (a >= b)
			{
				if (b >= c)
				{
					return c;
				}
				return b;
			}
			else
			{
				if (a >= c)
				{
					return c;
				}
				return a;
			}
		}

		public static decimal Min(decimal a, decimal b, decimal c)
		{
			if (!(a < b))
			{
				if (!(b < c))
				{
					return c;
				}
				return b;
			}
			else
			{
				if (!(a < c))
				{
					return c;
				}
				return a;
			}
		}

		public static short Min(short a, short b, short c)
		{
			if (a >= b)
			{
				if (b >= c)
				{
					return c;
				}
				return b;
			}
			else
			{
				if (a >= c)
				{
					return c;
				}
				return a;
			}
		}

		public static char Min(char a, char b, char c)
		{
			if (a >= b)
			{
				if (b >= c)
				{
					return c;
				}
				return b;
			}
			else
			{
				if (a >= c)
				{
					return c;
				}
				return a;
			}
		}

		public static byte Min(byte a, byte b, byte c)
		{
			if (a >= b)
			{
				if (b >= c)
				{
					return c;
				}
				return b;
			}
			else
			{
				if (a >= c)
				{
					return c;
				}
				return a;
			}
		}

		public static int DecimalToPow2Fraction(double val, out int denom, int maxDenom)
		{
			double lfp = 1.0;
			int ldenom = maxDenom;
			for (int i = 1; i < maxDenom; i <<= 1)
			{
				double num = val * (double)i;
				double fp = num - Math.Floor(num);
				if (fp == 0.0)
				{
					denom = i;
					return (int)num;
				}
				if (fp < lfp)
				{
					lfp = fp;
					ldenom = i;
				}
			}
			denom = ldenom;
			return (int)val * ldenom;
		}

		public static int Factorial(int n)
		{
			int res = 1;
			for (int i = n; i > 1; i--)
			{
				res *= i;
			}
			return res;
		}

		public static int Permutations(int n, int r)
		{
			return MathTools.Factorial(n) / MathTools.Factorial(n - r);
		}

		public static int Combinations(int n, int r)
		{
			return MathTools.Permutations(n, r) / MathTools.Factorial(r);
		}

		public static int DecimalToFraction(double val, out int denom, int maxDenom)
		{
			double lfp = 1.0;
			int ldenom = maxDenom;
			for (int i = 1; i < maxDenom; i++)
			{
				double num = val * (double)i;
				double fp = num - Math.Floor(num);
				if (fp == 0.0)
				{
					denom = i;
					return (int)num;
				}
				if (fp < lfp)
				{
					lfp = fp;
					ldenom = i;
				}
			}
			denom = ldenom;
			return (int)val * ldenom;
		}

		public static float Square(float x)
		{
			return x * x;
		}

		public static int Square(int x)
		{
			return x * x;
		}

		public static bool CalculateInitialBallisticVector(Vector3 pt1, Vector3 pt2, float vel, out Vector3 res, float gravity)
		{
			float gravitysq = gravity * gravity;
			float Height = pt2.Y - pt1.Y;
			float Dist = (float)Math.Sqrt((double)(MathTools.Square(pt1.X - pt2.X) + MathTools.Square(pt1.Z - pt2.Z)));
			res = Vector3.Zero;
			if (Dist == 0f)
			{
				if (Height >= 0f)
				{
					res = new Vector3(0f, vel, 0f);
				}
				else
				{
					res = new Vector3(0f, -vel, 0f);
				}
				return true;
			}
			float HeightSq = Height * Height;
			float DistSq = Dist * Dist;
			float sub4 = 2f * HeightSq + 2f * DistSq;
			if (sub4 == 0f)
			{
				return false;
			}
			float velsq = vel * vel;
			float sub5 = 2f * gravity * Height * velsq + velsq * velsq - gravitysq * DistSq;
			if (sub5 < 0f)
			{
				return false;
			}
			sub5 = (float)Math.Sqrt((double)sub5);
			float sub6 = gravity * Height + velsq;
			float suba = sub6 - sub5;
			float subb = sub6 + sub5;
			float sub7 = DistSq + HeightSq;
			suba *= sub7;
			subb *= sub7;
			sub4 = 1.414213f * Dist / sub4;
			bool gotit = false;
			if (suba >= 0f)
			{
				suba = (float)Math.Sqrt((double)suba) * sub4;
				gotit = true;
			}
			if (subb >= 0f)
			{
				subb = (float)Math.Sqrt((double)subb) * sub4;
				if (!gotit || subb > suba)
				{
					gotit = true;
					suba = subb;
				}
			}
			if (gotit)
			{
				if (suba <= 0f)
				{
					return false;
				}
				float suba1sq = suba * suba;
				if (suba1sq > velsq)
				{
					return false;
				}
				float t = Dist / suba;
				float h = (float)Math.Sqrt((double)(velsq - suba1sq));
				if (Math.Abs(Height - (h * t + 0.5f * gravity * t * t)) > Math.Abs(Height - (-h * t + 0.5f * gravity * t * t)))
				{
					h = -h;
				}
				Vector2 nm = new Vector2(pt2.X - pt1.X, pt2.Z - pt1.Z);
				nm.Normalize();
				res.X = suba * nm.X;
				res.Y = h;
				res.Z = suba * nm.Y;
			}
			return gotit;
		}

		public static float MoveTowardTarget(float current, float target, float rate, float dt)
		{
			float result;
			if (target != current)
			{
				float dv = target - current;
				float absdv = Math.Abs(dv);
				if (absdv > rate)
				{
					dv *= rate / absdv;
				}
				result = current + dv * dt;
				if (Math.Abs(result - target) < 0.01f)
				{
					result = target;
				}
			}
			else
			{
				result = current;
			}
			return result;
		}

		public static float MoveTowardTargetAngle(float current, float target, float rate, float dt)
		{
			float result;
			if (target != current)
			{
				float dv = MathHelper.WrapAngle(target - current);
				float absdv = Math.Abs(dv);
				if (absdv > rate)
				{
					dv *= rate / absdv;
				}
				result = MathHelper.WrapAngle(current + dv * dt);
				if (Math.Abs(result - target) < 0.01f)
				{
					result = target;
				}
			}
			else
			{
				result = current;
			}
			return result;
		}

		public static Vector3 Hermite1stDerivative(Vector3 Point1, Vector3 Tangent1, Vector3 Point2, Vector3 Tangent2, float t)
		{
			float tsq = t * t;
			Vector3 result = Point1 * (6f * (tsq - t));
			result += Tangent1 * (3f * tsq - 4f * t + 1f);
			result += Point2 * (6f * (t - tsq));
			return result + Tangent2 * (3f * tsq - 2f * t);
		}

		public static Vector3 Hermite2ndDerivative(Vector3 Point1, Vector3 Tangent1, Vector3 Point2, Vector3 Tangent2, float t)
		{
			Vector3 result = Point1 * (12f * t - 6f);
			result += Tangent1 * (6f * t - 4f);
			result += Point2 * (6f - 12f * t);
			return result + Tangent2 * (6f * t - 2f);
		}

		public const float Sqrt2 = 1.414213f;

		private static readonly byte[] BitsSetTable256 = new byte[]
		{
			0, 1, 1, 2, 1, 2, 2, 3, 1, 2,
			2, 3, 2, 3, 3, 4, 1, 2, 2, 3,
			2, 3, 3, 4, 2, 3, 3, 4, 3, 4,
			4, 5, 1, 2, 2, 3, 2, 3, 3, 4,
			2, 3, 3, 4, 3, 4, 4, 5, 2, 3,
			3, 4, 3, 4, 4, 5, 3, 4, 4, 5,
			4, 5, 5, 6, 1, 2, 2, 3, 2, 3,
			3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
			2, 3, 3, 4, 3, 4, 4, 5, 3, 4,
			4, 5, 4, 5, 5, 6, 2, 3, 3, 4,
			3, 4, 4, 5, 3, 4, 4, 5, 4, 5,
			5, 6, 3, 4, 4, 5, 4, 5, 5, 6,
			4, 5, 5, 6, 5, 6, 6, 7, 1, 2,
			2, 3, 2, 3, 3, 4, 2, 3, 3, 4,
			3, 4, 4, 5, 2, 3, 3, 4, 3, 4,
			4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
			2, 3, 3, 4, 3, 4, 4, 5, 3, 4,
			4, 5, 4, 5, 5, 6, 3, 4, 4, 5,
			4, 5, 5, 6, 4, 5, 5, 6, 5, 6,
			6, 7, 2, 3, 3, 4, 3, 4, 4, 5,
			3, 4, 4, 5, 4, 5, 5, 6, 3, 4,
			4, 5, 4, 5, 5, 6, 4, 5, 5, 6,
			5, 6, 6, 7, 3, 4, 4, 5, 4, 5,
			5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
			4, 5, 5, 6, 5, 6, 6, 7, 5, 6,
			6, 7, 6, 7, 7, 8
		};

		private static readonly byte[] LogTable256 = new byte[]
		{
			0, 0, 1, 1, 2, 2, 2, 2, 3, 3,
			3, 3, 3, 3, 3, 3, 4, 4, 4, 4,
			4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
			4, 4, 5, 5, 5, 5, 5, 5, 5, 5,
			5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
			5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
			5, 5, 5, 5, 6, 6, 6, 6, 6, 6,
			6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
			6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
			6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
			6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
			6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
			6, 6, 6, 6, 6, 6, 6, 6, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
			7, 7, 7, 7, 7, 7
		};

		private static readonly int[] MultiplyDeBruijnBitPosition = new int[]
		{
			0, 1, 28, 2, 29, 14, 24, 3, 30, 22,
			20, 15, 25, 17, 4, 8, 31, 27, 13, 23,
			21, 19, 16, 7, 26, 12, 18, 6, 11, 5,
			10, 9
		};

		public static Random Rnd = new Random();
	}
}
