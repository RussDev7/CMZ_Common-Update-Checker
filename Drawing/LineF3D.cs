using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	public struct LineF3D
	{
		public float LengthSquared
		{
			get
			{
				return (this.End - this.Start).LengthSquared();
			}
		}

		public Vector3 Center
		{
			get
			{
				return 0.5f * (this.Start + this.End);
			}
		}

		public Vector3 Direction
		{
			get
			{
				Vector3 vect = this.End - this.Start;
				vect.Normalize();
				return vect;
			}
		}

		public float Length
		{
			get
			{
				return (this.End - this.Start).Length();
			}
		}

		public LineF3D(Vector3 start, Vector3 end)
		{
			this.Start = start;
			this.End = end;
		}

		public Vector3 GetValue(float t)
		{
			return (this.End - this.Start) * t + this.Start;
		}

		public float DistanceTo(Vector3 point)
		{
			Vector3 closestPoint = this.ClosetPointTo(point);
			return Vector3.Distance(closestPoint, point);
		}

		public Vector3 ClosetPointTo(Vector3 point)
		{
			Vector3 a = this.End - this.Start;
			Vector3 b = point - this.Start;
			float ab = Vector3.Dot(a, b);
			if (ab <= 0f)
			{
				return this.Start;
			}
			float bb = Vector3.Dot(b, b);
			if (bb >= ab)
			{
				return this.End;
			}
			float t = bb / ab;
			return this.Start + a * t;
		}

		public float? Intersects(Plane plane)
		{
			float t = plane.DotCoordinate(this.Start);
			float denom = t - plane.DotCoordinate(this.End);
			if (denom != 0f)
			{
				float t2 = t / denom;
				if ((double)t2 >= 0.0 && t2 <= 1f)
				{
					return new float?(t2);
				}
			}
			return null;
		}

		public bool Intersects(Plane plane, out float t, out bool parallel, int precisionDigits)
		{
			double t2 = (double)plane.DotCoordinate(this.Start);
			double denom = t2 - (double)plane.DotCoordinate(this.End);
			if (Math.Round(denom, precisionDigits) == 0.0)
			{
				t = float.NaN;
				parallel = true;
				return t2 == 0.0;
			}
			parallel = false;
			double dt = t2 / denom;
			t = (float)Math.Round(dt, precisionDigits);
			return (double)t >= 0.0 && t <= 1f;
		}

		public bool Intersects(Triangle3D triangle, out float t, out bool parallel)
		{
			Vector3 u = triangle.B - triangle.A;
			Vector3 v = triangle.C - triangle.A;
			Vector3 i = Vector3.Cross(u, v);
			Vector3 dir = this.End - this.Start;
			Vector3 w0 = this.Start - triangle.A;
			float a = -Vector3.Dot(i, w0);
			float b = Vector3.Dot(i, dir);
			if (b == 0f)
			{
				parallel = true;
				if (a == 0f)
				{
					t = 0f;
					return true;
				}
				t = float.NaN;
				return false;
			}
			else
			{
				parallel = false;
				t = a / b;
				if (t < 0f || t > 1f)
				{
					return false;
				}
				Vector3 I = this.Start + t * dir;
				float uu = Vector3.Dot(u, u);
				float uv = Vector3.Dot(u, v);
				float vv = Vector3.Dot(v, v);
				Vector3 w = I - triangle.A;
				float wu = Vector3.Dot(w, u);
				float wv = Vector3.Dot(w, v);
				float D = uv * uv - uu * vv;
				float sI = (uv * wv - vv * wu) / D;
				if ((double)sI < 0.0 || (double)sI > 1.0)
				{
					return false;
				}
				float tI = (uv * wu - uu * wv) / D;
				return (double)tI >= 0.0 && (double)(sI + tI) <= 1.0;
			}
		}

		public int Intersects(Capsule capsule, out float? t1, out float? t2)
		{
			Ray ray = new Ray(this.Start, this.End - this.Start);
			int intersections = ray.Intersects(capsule, out t1, out t2);
			if (t1 != null)
			{
				float? num = t1;
				if (num.GetValueOrDefault() < 0f && num != null)
				{
					goto IL_0074;
				}
			}
			float? num2 = t1;
			if (num2.GetValueOrDefault() <= 1f || num2 == null)
			{
				goto IL_007F;
			}
			IL_0074:
			intersections--;
			t1 = null;
			IL_007F:
			if (t2 != null)
			{
				float? num3 = t2;
				if (num3.GetValueOrDefault() < 0f && num3 != null)
				{
					goto IL_00CD;
				}
			}
			float? num4 = t2;
			if (num4.GetValueOrDefault() <= 1f || num4 == null)
			{
				goto IL_00D8;
			}
			IL_00CD:
			intersections--;
			t2 = null;
			IL_00D8:
			if (t1 == null && t2 != null)
			{
				t1 = t2;
				t2 = null;
			}
			return intersections;
		}

		public bool Intersects(BoundingSphere sphere)
		{
			float t;
			float t2;
			return this.Intersects(sphere, out t, out t2);
		}

		public bool Intersects(BoundingSphere sphere, out float t1, out float t2)
		{
			Vector3 center = sphere.Center;
			float r = sphere.Radius;
			Vector3 dist = this.End - this.Start;
			Vector3 v2 = this.Start - center;
			double a = (double)dist.LengthSquared();
			double b = (double)(2f * Vector3.Dot(dist, v2));
			double c = (double)(center.LengthSquared() + this.Start.LengthSquared() - 2f * Vector3.Dot(center, this.Start) - r * r);
			double bb4ac = b * b - 4.0 * a * c;
			if (a == 0.0 || bb4ac < 0.0)
			{
				t1 = 0f;
				t2 = 0f;
				return false;
			}
			double sqbb4ac = Math.Sqrt(bb4ac);
			t1 = (float)((-(float)b + sqbb4ac) / (2.0 * a));
			t2 = (float)((-(float)b - sqbb4ac) / (2.0 * a));
			if (t1 >= 0f && t1 <= 1f)
			{
				return true;
			}
			if (t2 >= 0f && t2 <= 1f)
			{
				t1 = t2;
				return true;
			}
			return false;
		}

		public bool Intersects(Plane plane, out Vector3 pos, out bool parallel, int precisionDigits)
		{
			float t;
			bool res = this.Intersects(plane, out t, out parallel, precisionDigits);
			if (parallel)
			{
				pos = new Vector3(float.NaN, float.NaN, float.NaN);
				return res;
			}
			pos = this.GetValue(t);
			return res;
		}

		public bool Intersects(LineF3D line, out float intersection)
		{
			throw new NotImplementedException();
		}

		public bool Intersects(LineF3D line, out Vector3 intersection)
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"(",
				this.Start.X,
				",",
				this.Start.Y,
				",",
				this.Start.Z,
				")-(",
				this.End.X,
				",",
				this.End.Y,
				",",
				this.End.Z,
				")"
			});
		}

		public Vector3 Start;

		public Vector3 End;
	}
}
