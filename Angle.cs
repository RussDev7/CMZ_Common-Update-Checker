using System;
using Microsoft.Xna.Framework;

namespace DNA
{
	public struct Angle
	{
		public float Radians
		{
			get
			{
				return this._radians;
			}
			set
			{
				this._radians = value;
			}
		}

		public float Degrees
		{
			get
			{
				return this._radians * 180f / 3.1415927f;
			}
			set
			{
				this._radians = value * 3.1415927f / 180f;
			}
		}

		public double Sin
		{
			get
			{
				return Math.Sin((double)this._radians);
			}
		}

		public double Cos
		{
			get
			{
				return Math.Cos((double)this._radians);
			}
		}

		public double Tan
		{
			get
			{
				return Math.Tan((double)this._radians);
			}
		}

		public static Angle ASin(double value)
		{
			return new Angle((float)Math.Asin(value));
		}

		public static Angle ACos(double value)
		{
			return new Angle((float)Math.Acos(value));
		}

		public static Angle ATan(double value)
		{
			return new Angle((float)Math.Atan(value));
		}

		public static Angle ATan2(double y, double x)
		{
			return new Angle((float)Math.Atan2(y, x));
		}

		public static Angle Lerp(Angle a, Angle b, float t)
		{
			return Angle.FromRadians((b.Radians - a.Radians) * t + a.Radians);
		}

		public float Revolutions
		{
			get
			{
				return this._radians / 6.2831855f;
			}
			set
			{
				this._radians = value * 6.2831855f;
			}
		}

		public void Normalize()
		{
			this._radians = MathTools.Mod(this._radians, 6.2831855f);
		}

		public static double DegreesToRadians(double degs)
		{
			return degs * 3.141592653589793 / 180.0;
		}

		public static float DegreesToRadians(float degs)
		{
			return degs * 3.1415927f / 180f;
		}

		public static float RadiansToDegrees(float rads)
		{
			return rads * 180f / 3.1415927f;
		}

		public static Angle FromLocations(Point pivot, Point point)
		{
			int deltax = point.X - pivot.X;
			int deltay = point.Y - pivot.Y;
			float oa = (float)deltay / (float)deltax;
			float rads;
			if (deltax == 0)
			{
				if (deltay > 0)
				{
					rads = 1.5707964f;
				}
				else if (deltay < 0)
				{
					rads = -1.5707964f;
				}
				else
				{
					rads = 0f;
				}
			}
			else
			{
				rads = (float)Math.Atan((double)oa);
				if (deltax < 0)
				{
					rads += 3.1415927f;
				}
			}
			Angle ret = Angle.FromRadians(rads);
			ret.Normalize();
			return ret;
		}

		public static Angle FromRadians(double rads)
		{
			return new Angle((float)rads);
		}

		public static Angle FromDegrees(double degs)
		{
			double rads = Angle.DegreesToRadians(degs);
			return new Angle(rads);
		}

		public static Angle FromRadians(float rads)
		{
			return new Angle(rads);
		}

		public static Angle FromDegrees(float degs)
		{
			return new Angle(Angle.DegreesToRadians(degs));
		}

		public static Angle FromRevolutions(float revs)
		{
			return new Angle(revs * 6.2831855f);
		}

		public static Angle operator +(Angle a, Angle b)
		{
			return new Angle(a._radians + b._radians);
		}

		public static Angle operator -(Angle a)
		{
			return new Angle(-a._radians);
		}

		public static Angle operator -(Angle a, Angle b)
		{
			return new Angle(a._radians - b._radians);
		}

		public static Angle operator /(Angle a, float b)
		{
			return new Angle(a._radians / b);
		}

		public static Angle operator /(Angle a, double b)
		{
			return new Angle((double)a._radians / b);
		}

		public static float operator /(Angle a, Angle b)
		{
			return a._radians / b._radians;
		}

		public static bool operator <(Angle a, Angle b)
		{
			return a._radians < b._radians;
		}

		public static bool operator >(Angle a, Angle b)
		{
			return a._radians > b._radians;
		}

		public static Angle operator *(float b, Angle a)
		{
			return new Angle(a._radians * b);
		}

		public static Angle operator *(double b, Angle a)
		{
			return new Angle((double)a._radians * b);
		}

		public static Angle operator *(Angle a, float b)
		{
			return new Angle(a._radians * b);
		}

		public static Angle operator *(Angle a, double b)
		{
			return new Angle((double)a._radians * b);
		}

		private Angle(double radians)
		{
			this._radians = (float)radians;
		}

		private Angle(float radians)
		{
			this._radians = radians;
		}

		public override string ToString()
		{
			return this.Degrees.ToString();
		}

		public static Angle Parse(string s)
		{
			return Angle.FromDegrees(float.Parse(s));
		}

		public override int GetHashCode()
		{
			return this._radians.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			Angle b = (Angle)obj;
			return this._radians == b._radians;
		}

		public static bool operator ==(Angle a, Angle b)
		{
			return a._radians == b._radians;
		}

		public static bool operator !=(Angle a, Angle b)
		{
			return a._radians != b._radians;
		}

		public static readonly Angle Zero = Angle.FromRadians(0f);

		public static readonly Angle UnitCircle = Angle.FromRevolutions(1f);

		private float _radians;
	}
}
