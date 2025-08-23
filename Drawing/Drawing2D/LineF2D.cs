using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Drawing2D
{
	public struct LineF2D : IShape2D, ICloneable
	{
		public Vector2 Start
		{
			get
			{
				return this._start;
			}
			set
			{
				this._start = value;
			}
		}

		public Vector2 End
		{
			get
			{
				return this._end;
			}
			set
			{
				this._end = value;
			}
		}

		public bool IsVertical
		{
			get
			{
				return this._end.X == this._start.X;
			}
		}

		public bool IsHorizonal
		{
			get
			{
				return this._end.Y == this._start.Y;
			}
		}

		public double LengthSq
		{
			get
			{
				return (double)(this.End.X * this.End.X + this.Start.X * this.Start.X);
			}
		}

		public double Length
		{
			get
			{
				return (double)Vector2.Distance(this.Start, this.End);
			}
		}

		public RectangleF BoundingBox
		{
			get
			{
				return this.ComputeBoundingBox();
			}
		}

		private RectangleF ComputeBoundingBox()
		{
			float minx;
			float maxx;
			if (this._start.X < this._end.X)
			{
				minx = this._start.X;
				maxx = this._end.X;
			}
			else
			{
				minx = this._end.X;
				maxx = this._start.X;
			}
			float miny;
			float maxy;
			if (this._start.Y < this._end.Y)
			{
				miny = this._start.Y;
				maxy = this._end.Y;
			}
			else
			{
				miny = this._end.Y;
				maxy = this._start.Y;
			}
			return new RectangleF(minx, miny, maxx - minx, maxy - miny);
		}

		public LineF2D(float sx, float sy, float ex, float ey)
		{
			this._start = new Vector2(sx, sy);
			this._end = new Vector2(ex, ey);
		}

		public LineF2D(Vector2 start, Vector2 end)
		{
			this._start = start;
			this._end = end;
		}

		public Vector2 GetValue(float t)
		{
			float x = (this._end.X - this._start.X) * t + this._start.X;
			float y = (this._end.Y - this._start.Y) * t + this._start.Y;
			return new Vector2(x, y);
		}

		public float DistanceSquare()
		{
			float a = this.End.Y - this.Start.Y;
			float b = this.End.X - this.Start.X;
			return a * a + b * b;
		}

		public float Intersect(Vector2 pointOnLine)
		{
			if (this.IsVertical)
			{
				float span = this._end.Y - this._start.Y;
				return (pointOnLine.Y - this._start.Y) / span;
			}
			float span2 = this._end.X - this._start.X;
			return (pointOnLine.X - this._start.X) / span2;
		}

		public bool HorizontalIntersect(float x, out float y)
		{
			float deltaX = this._end.X - this._start.X;
			if (deltaX == 0f)
			{
				y = float.NaN;
				return false;
			}
			float t = (x - this._start.X) / deltaX;
			y = this._start.Y + t * (this._end.Y - this._start.Y);
			return t >= 0f && t <= 1f;
		}

		public bool VerticalIntersect(float y, out float x)
		{
			float deltaY = this._end.Y - this._start.Y;
			if (deltaY == 0f)
			{
				x = float.NaN;
				return false;
			}
			float t = (y - this._start.Y) / deltaY;
			x = this._start.X + t * (this._end.X - this._start.X);
			return t >= 0f && t <= 1f;
		}

		public bool Intersects(LineF2D line)
		{
			Vector2 intr;
			bool coincident;
			return this.Intersects(line, out intr, out coincident);
		}

		public bool Intersects(Circle circle, out float t1, out float t2)
		{
			Vector2 center = circle.Center;
			float r = circle.Radius;
			Vector2 dist = this._end - this._start;
			Vector2 v2 = this._start - center;
			double a = (double)dist.LengthSquared();
			double b = (double)(2f * Vector2.Dot(dist, v2));
			double c = (double)(center.LengthSquared() + this._start.LengthSquared() - 2f * Vector2.Dot(center, this._start) - r * r);
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

		public bool Intersects(Vector2 start, Vector2 end, out float t, out bool coincident)
		{
			coincident = false;
			Vector2 a = new Vector2(this._end.X - this._start.X, this._end.Y - this._start.Y);
			Vector2 b = new Vector2(end.X - start.X, end.Y - start.Y);
			if (a.LengthSquared() == 0f || b.LengthSquared() == 0f)
			{
				t = -1f;
				return false;
			}
			Vector2 c = new Vector2(this._start.X - start.X, this._start.Y - start.Y);
			float UaNum = b.Cross(c);
			float UDen = a.Cross(b);
			float UbNum = a.Cross(c);
			if (UDen == 0f)
			{
				t = -1f;
				if (UaNum == 0f && UbNum == 0f)
				{
					coincident = true;
					return true;
				}
				return false;
			}
			else
			{
				float Ua = UaNum / UDen;
				float Ub = UbNum / UDen;
				if (Ua >= 0f && Ua <= 1f && Ub >= 0f && Ub <= 1f)
				{
					t = Ua;
					return true;
				}
				t = -1f;
				return false;
			}
		}

		public bool Intersects(LineF2D line, out float t, out bool coincident)
		{
			coincident = false;
			Vector2 a = new Vector2(this._end.X - this._start.X, this._end.Y - this._start.Y);
			Vector2 b = new Vector2(line._end.X - line._start.X, line._end.Y - line._start.Y);
			if (a.LengthSquared() == 0f || b.LengthSquared() == 0f)
			{
				t = -1f;
				return false;
			}
			Vector2 c = new Vector2(this._start.X - line._start.X, this._start.Y - line._start.Y);
			float UaNum = b.Cross(c);
			float UDen = a.Cross(b);
			float UbNum = a.Cross(c);
			if (UDen == 0f)
			{
				t = -1f;
				if (UaNum == 0f && UbNum == 0f)
				{
					coincident = true;
					return true;
				}
				return false;
			}
			else
			{
				float Ua = UaNum / UDen;
				float Ub = UbNum / UDen;
				if (Ua >= 0f && Ua <= 1f && Ub >= 0f && Ub <= 1f)
				{
					t = Ua;
					return true;
				}
				t = -1f;
				return false;
			}
		}

		public bool Intersects(LineF2D line, out Vector2 intersection, out bool coincident)
		{
			coincident = false;
			Vector2 a = new Vector2(this._end.X - this._start.X, this._end.Y - this._start.Y);
			Vector2 b = new Vector2(line._end.X - line._start.X, line._end.Y - line._start.Y);
			if (a.LengthSquared() == 0f || b.LengthSquared() == 0f)
			{
				intersection = Vector2.Zero;
				return false;
			}
			Vector2 c = new Vector2(this._start.X - line._start.X, this._start.Y - line._start.Y);
			float UaNum = b.Cross(c);
			float UDen = a.Cross(b);
			float UbNum = a.Cross(c);
			if (UDen == 0f)
			{
				intersection = Vector2.Zero;
				if (UaNum == 0f && UbNum == 0f)
				{
					coincident = true;
					return true;
				}
				return false;
			}
			else
			{
				float Ua = UaNum / UDen;
				float Ub = UbNum / UDen;
				if (Ua >= 0f && Ua <= 1f && Ub >= 0f && Ub <= 1f)
				{
					intersection = new Vector2(this._start.X + Ua * a.X, this._start.Y + Ua * a.Y);
					return true;
				}
				intersection = Vector2.Zero;
				return false;
			}
		}

		public Vector2 ShortestVectorTo(Vector2 pnt)
		{
			double lengthSq = (double)Vector2.DistanceSquared(this._start, this._end);
			double u;
			if (lengthSq != 0.0)
			{
				u = (double)((pnt.X - this._start.X) * (this._end.X - this._start.X) + (pnt.Y - this._start.Y) * (this._end.Y - this._start.Y)) / lengthSq;
			}
			else
			{
				u = 0.0;
			}
			if (u < 0.0)
			{
				return this._start - pnt;
			}
			if (u > 1.0)
			{
				return this._end - pnt;
			}
			double newx = (double)this._start.X + u * (double)(this._end.X - this._start.X);
			double newy = (double)this._start.Y + u * (double)(this._end.Y - this._start.Y);
			double xd = newx - (double)pnt.X;
			double yd = newy - (double)pnt.Y;
			return new Vector2((float)xd, (float)yd);
		}

		public double DistanceTo(Vector2 pnt)
		{
			return (double)this.ShortestVectorTo(pnt).Length();
		}

		public override string ToString()
		{
			return this.Start.ToString() + "-" + this.End.ToString();
		}

		public Vector2 Center
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public float Area
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public OrientedBoundingRect GetOrientedBoundingRect()
		{
			throw new NotImplementedException();
		}

		public void Transform(Matrix mat)
		{
			throw new NotImplementedException();
		}

		public bool Touches(IShape2D s)
		{
			throw new NotImplementedException();
		}

		public bool IsAbove(Vector2 p)
		{
			throw new NotImplementedException();
		}

		public float DistanceTo(IShape2D poly)
		{
			throw new NotImplementedException();
		}

		public float DistanceSquaredTo(IShape2D poly)
		{
			throw new NotImplementedException();
		}

		public bool IsBelow(Vector2 p)
		{
			throw new NotImplementedException();
		}

		public bool IsLeftOf(Vector2 p)
		{
			throw new NotImplementedException();
		}

		public bool IsRightOf(Vector2 p)
		{
			throw new NotImplementedException();
		}

		public bool CompletelyContains(IShape2D region)
		{
			throw new NotImplementedException();
		}

		public bool Contains(Vector2 p)
		{
			throw new NotImplementedException();
		}

		public int Contains(IList<Vector2> points)
		{
			throw new NotImplementedException();
		}

		public IShape2D Intersection(IShape2D s)
		{
			throw new NotImplementedException();
		}

		public bool BoundsIntersect(IShape2D shape)
		{
			throw new NotImplementedException();
		}

		public float Contains(IShape2D shape)
		{
			throw new NotImplementedException();
		}

		public object Clone()
		{
			throw new NotImplementedException();
		}

		public Vector2 _start;

		public Vector2 _end;
	}
}
