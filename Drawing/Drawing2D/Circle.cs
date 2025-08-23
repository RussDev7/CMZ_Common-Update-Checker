using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Drawing2D
{
	public struct Circle : IShape2D, ICloneable
	{
		public Circle(Vector2 center, float radius)
		{
			this._center = center;
			this.Radius = radius;
		}

		public RectangleF BoundingBox
		{
			get
			{
				return new RectangleF(this._center.X - this.Radius, this._center.Y - this.Radius, this.Radius * 2f, this.Radius * 2f);
			}
		}

		public Vector2 Center
		{
			get
			{
				return this._center;
			}
			set
			{
				this._center = value;
			}
		}

		public float Area
		{
			get
			{
				return (float)(3.141592653589793 * (double)this.Radius * (double)this.Radius);
			}
		}

		public OrientedBoundingRect GetOrientedBoundingRect()
		{
			throw new NotImplementedException();
		}

		public void Transform(Matrix mat)
		{
			this._center = Vector2.Transform(this._center, mat);
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
			return (p - this._center).Length() <= this.Radius;
		}

		public int Contains(IList<Vector2> points)
		{
			int count = 0;
			for (int i = 0; i < points.Count; i++)
			{
				Vector2 vector = points[i];
				if (!this.Contains(points[i]))
				{
					count++;
				}
			}
			return count;
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
			return new Circle(this._center, this.Radius);
		}

		public Vector2 _center;

		public float Radius;
	}
}
