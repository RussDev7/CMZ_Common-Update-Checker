using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Drawing2D
{
	[Serializable]
	public class Polygon2D : Shape2D, IPointShape2D, IShape2D, ICloneable
	{
		public override object Clone()
		{
			return new Polygon2D(this._points);
		}

		public IList<Vector2> Points
		{
			get
			{
				return this._points;
			}
		}

		public override RectangleF BoundingBox
		{
			get
			{
				return this._boundingBox;
			}
		}

		public override Vector2 Center
		{
			get
			{
				return this._center;
			}
		}

		public override float Area
		{
			get
			{
				return this._area;
			}
		}

		public override OrientedBoundingRect GetOrientedBoundingRect()
		{
			return OrientedBoundingRect.FromPoints(this._points);
		}

		public IList<LineF2D> GetLineSegments()
		{
			LineF2D[] lines = new LineF2D[this._points.Length];
			for (int i = 0; i < this._points.Length - 1; i++)
			{
				lines[i] = new LineF2D(this._points[i], this._points[i + 1]);
			}
			lines[this._points.Length - 1] = new LineF2D(this._points[this._points.Length - 1], this._points[0]);
			return lines;
		}

		public override void Transform(Matrix mat)
		{
			for (int i = 0; i < this._points.Length; i++)
			{
				this._points[i] = Vector2.Transform(this._points[i], mat);
			}
			this.ComputeBoundingBox();
		}

		public Polygon2D(IList<Vector2> points)
		{
			int newLen = points.Count;
			if (points[0] != points[points.Count - 1])
			{
				newLen++;
			}
			this._points = new Vector2[newLen];
			for (int i = 0; i < points.Count; i++)
			{
				Vector2 p = points[i];
				if (this._points.Length == 0 || this._points[this._points.Length - 1] != p)
				{
					this._points[i] = p;
				}
			}
			if (points[0] != points[points.Count - 1])
			{
				this._points[points.Count] = points[0];
			}
			this.ComputeBoundingBox();
			this.ComputeArea();
		}

		public Rectangle ComputeTransformedBoundingBox(Matrix mat)
		{
			float lowX = float.MaxValue;
			float lowY = float.MaxValue;
			float highX = float.MinValue;
			float highY = float.MinValue;
			for (int i = 0; i < this._points.Length; i++)
			{
				Vector2 p = this._points[i];
				Vector3 vect = Vector3.Transform(new Vector3(p.X, p.Y, 0f), mat);
				lowX = Math.Min(lowX, vect.X);
				lowY = Math.Min(lowY, vect.Y);
				highX = Math.Max(highX, vect.X);
				highY = Math.Max(highY, vect.Y);
			}
			return new Rectangle((int)lowX, (int)lowY, (int)(highX - lowX), (int)(highY - lowY));
		}

		private void ComputeBoundingBox()
		{
			float lowX = 2.1474836E+09f;
			float lowY = 2.1474836E+09f;
			float highX = -2.1474836E+09f;
			float highY = -2.1474836E+09f;
			for (int i = 0; i < this._points.Length; i++)
			{
				Vector2 p = this._points[i];
				lowX = Math.Min(lowX, p.X);
				lowY = Math.Min(lowY, p.Y);
				highX = Math.Max(highX, p.X);
				highY = Math.Max(highY, p.Y);
			}
			this._boundingBox = new RectangleF(lowX, lowY, highX - lowX + 1f, highY - lowY + 1f);
			this._center = new Vector2(this._boundingBox.Left + this._boundingBox.Width / 2f, this._boundingBox.Top + this._boundingBox.Height / 2f);
		}

		private void ComputeArea()
		{
			this._area = Math.Abs(Polygon2D.GetPolygonArea(this._points));
			this._area += (float)this._points.Length;
		}

		public bool PortionBelow(Vector2 p)
		{
			return !this.PortionAbove(p);
		}

		public bool PortionAbove(Vector2 p)
		{
			for (int i = 0; i < this._points.Length; i++)
			{
				if (this._points[i].Y < p.Y)
				{
					return true;
				}
			}
			return false;
		}

		public float PointsInside(IShape2D region)
		{
			if (region == this)
			{
				return 0f;
			}
			if (!base.BoundsIntersect(region))
			{
				return 0f;
			}
			int incount = 0;
			for (int i = 0; i < this._points.Length; i++)
			{
				if (region.Contains(this._points[i]))
				{
					incount++;
				}
			}
			return (float)incount / (float)this._points.Length;
		}

		private bool Touches(Polygon2D s)
		{
			return false;
		}

		public override bool Touches(IShape2D s)
		{
			if (base.BoundsIntersect(s))
			{
				for (int i = 0; i < this._points.Length; i++)
				{
					Vector2 p = this._points[i];
					if (s.Contains(p))
					{
						return true;
					}
				}
				if (!(s is IPointShape2D))
				{
					throw new NotImplementedException();
				}
				IPointShape2D poly = (IPointShape2D)s;
				IList<Vector2> points = poly.Points;
				for (int j = 0; j < points.Count; j++)
				{
					Vector2 p2 = points[j];
					if (this.Contains(p2))
					{
						return true;
					}
				}
				IList<LineF2D> lines = this.GetLineSegments();
				IList<LineF2D> lines2 = poly.GetLineSegments();
				for (int k = 0; k < lines.Count; k++)
				{
					for (int l = 0; l < lines2.Count; l++)
					{
						if (lines[k].Intersects(lines2[l]))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public override bool IsAbove(Vector2 p)
		{
			if (p.X < this._boundingBox.Left || p.X > this._boundingBox.Right || p.Y > this._boundingBox.Bottom)
			{
				return false;
			}
			Vector2 p2 = this._points[0];
			float x = p.X + 0.5f;
			for (int i = 1; i < this._points.Length; i++)
			{
				Vector2 p3 = this._points[i];
				float x2 = p3.X - x;
				float x3 = p2.X - x;
				if ((x2 > 0f && x3 < 0f) || (x2 < 0f && x3 > 0f))
				{
					float t = (x - p2.X) / (p3.X - p2.X);
					float y = p2.Y + t * (p3.Y - p2.Y);
					if (y < p.Y)
					{
						return false;
					}
				}
				p2 = p3;
			}
			return true;
		}

		public bool LowestParametricIntersection(LineF2D targetLine, out float lowestT)
		{
			lowestT = float.MaxValue;
			for (int i = 0; i < this._points.Length - 1; i++)
			{
				LineF2D line = new LineF2D(this._points[i], this._points[i + 1]);
				float intersection;
				bool coincident;
				if (line.Intersects(targetLine, out intersection, out coincident) && !coincident && lowestT > intersection)
				{
					lowestT = intersection;
				}
			}
			return lowestT != float.MaxValue;
		}

		public void ParametricIntersections(LineF2D targetLine, List<Polygon2D.IntersectionData> intersections)
		{
			for (int i = 0; i < this._points.Length - 1; i++)
			{
				LineF2D line = new LineF2D(this._points[i], this._points[i + 1]);
				float intersection;
				bool coincident;
				if (line.Intersects(targetLine, out intersection, out coincident) && !coincident)
				{
					intersections.Add(new Polygon2D.IntersectionData(intersection, line, coincident));
				}
			}
		}

		public IList<float> ParametricIntersections(LineF2D targetLine)
		{
			List<float> ints = new List<float>();
			for (int i = 0; i < this._points.Length - 1; i++)
			{
				LineF2D line = new LineF2D(this._points[i], this._points[i + 1]);
				float intersection;
				bool coincident;
				if (line.Intersects(targetLine, out intersection, out coincident) && !coincident)
				{
					ints.Add(intersection);
				}
			}
			return ints;
		}

		public IList<Vector2> Intersections(LineF2D targetLine)
		{
			List<Vector2> ints = new List<Vector2>();
			for (int i = 0; i < this._points.Length - 1; i++)
			{
				LineF2D line = new LineF2D(this._points[i], this._points[i + 1]);
				Vector2 intersection;
				bool coincident;
				if (line.Intersects(targetLine, out intersection, out coincident) && !coincident)
				{
					ints.Add(intersection);
				}
			}
			return ints;
		}

		public Vector2 ShortestVectorTo(Vector2 point)
		{
			LineF2D line = new LineF2D(this._points[0], this._points[1]);
			Vector2 shortestVector = line.ShortestVectorTo(point);
			float lsq = shortestVector.LengthSquared();
			Vector2 lastPoint = this._points[1];
			for (int i = 2; i < this._points.Length; i++)
			{
				line = new LineF2D(lastPoint, this._points[i]);
				Vector2 vt = line.ShortestVectorTo(point);
				float vlsq = vt.LengthSquared();
				if (vlsq < lsq)
				{
					lsq = vlsq;
					shortestVector = vt;
				}
				lastPoint = this._points[i];
			}
			return shortestVector;
		}

		public override float DistanceSquaredTo(IShape2D shape)
		{
			if (shape is IPointShape2D)
			{
				IPointShape2D poly = (IPointShape2D)shape;
				float min = float.MaxValue;
				IList<Vector2> p2s = poly.Points;
				for (int i = 0; i < this._points.Length; i++)
				{
					for (int j = 0; j < p2s.Count; j++)
					{
						min = Math.Min((float)this._points[i].DistanceSquared(p2s[j]), min);
					}
				}
				return min;
			}
			throw new NotImplementedException();
		}

		public override bool IsBelow(Vector2 p)
		{
			if (p.X < this._boundingBox.Left || p.X > this._boundingBox.Right || p.Y < this._boundingBox.Top)
			{
				return false;
			}
			Vector2 p2 = this._points[0];
			float x = p.X + 0.5f;
			for (int i = 1; i < this._points.Length; i++)
			{
				Vector2 p3 = this._points[i];
				float x2 = p3.X - x;
				float x3 = p2.X - x;
				if ((x2 > 0f && x3 < 0f) || (x2 < 0f && x3 > 0f))
				{
					float t = (x - p2.X) / (p3.X - p2.X);
					float y = p2.Y + t * (p3.Y - p2.Y);
					if (y > p.Y)
					{
						return false;
					}
				}
				p2 = p3;
			}
			return true;
		}

		public override bool IsLeftOf(Vector2 p)
		{
			if (p.Y < this._boundingBox.Top || p.Y > this._boundingBox.Bottom || p.X < this._boundingBox.Left)
			{
				return false;
			}
			Vector2 p2 = this._points[0];
			float y = p.Y + 0.5f;
			for (int i = 1; i < this._points.Length; i++)
			{
				Vector2 p3 = this._points[i];
				float y2 = p3.Y - y;
				float y3 = p2.Y - y;
				if ((y2 > 0f && y3 < 0f) || (y2 < 0f && y3 > 0f))
				{
					float t = (y - p2.Y) / (p3.Y - p2.Y);
					float x = p2.X + t * (p3.X - p2.X);
					if (x < p.X)
					{
						return false;
					}
				}
				p2 = p3;
			}
			return true;
		}

		public override bool IsRightOf(Vector2 p)
		{
			if (p.Y < this._boundingBox.Top || p.Y > this._boundingBox.Bottom || p.X > this._boundingBox.Right)
			{
				return false;
			}
			Vector2 p2 = this._points[0];
			float y = p.Y + 0.5f;
			for (int i = 1; i < this._points.Length; i++)
			{
				Vector2 p3 = this._points[i];
				float y2 = p3.Y - y;
				float y3 = p2.Y - y;
				if ((y2 > 0f && y3 < 0f) || (y2 < 0f && y3 > 0f))
				{
					float t = (y - p2.Y) / (p3.Y - p2.Y);
					float x = p2.X + t * (p3.X - p2.X);
					if (x > p.X)
					{
						return false;
					}
				}
				p2 = p3;
			}
			return true;
		}

		public override bool CompletelyContains(IShape2D shape)
		{
			if (shape is IPointShape2D)
			{
				IPointShape2D poly = (IPointShape2D)shape;
				return poly.PointsInside(this) == 1f;
			}
			throw new NotImplementedException();
		}

		public override bool Contains(Vector2 p)
		{
			if (!this.BoundingBox.Contains(p))
			{
				return false;
			}
			Vector2 p2 = this._points[0];
			float y = p.Y;
			int crosses = 0;
			for (int i = 1; i < this._points.Length; i++)
			{
				Vector2 p3 = this._points[i];
				float y2 = p3.Y - y;
				float y3 = p2.Y - y;
				if ((y2 >= 0f && y3 < 0f) || (y2 < 0f && y3 >= 0f))
				{
					float t = (y - p2.Y) / (p3.Y - p2.Y);
					float x = p2.X + t * (p3.X - p2.X);
					if (x < p.X)
					{
						crosses++;
					}
				}
				p2 = p3;
			}
			return (crosses & 1) != 0;
		}

		public override IShape2D Intersection(IShape2D s)
		{
			throw new NotImplementedException();
		}

		public bool BoundsIntersect(Polygon2D shape)
		{
			return this._boundingBox.Left < shape._boundingBox.Right && this._boundingBox.Right > shape._boundingBox.Left && this._boundingBox.Top < shape._boundingBox.Bottom && this._boundingBox.Bottom > shape._boundingBox.Top;
		}

		public override float Contains(IShape2D shape)
		{
			if (shape == this)
			{
				return 0f;
			}
			float maxArea = shape.Area;
			if (shape.Area == 0f)
			{
				return 0f;
			}
			if (!base.BoundsIntersect(shape))
			{
				return 0f;
			}
			RectangleF intersect = RectangleF.Intersect(this._boundingBox, shape.BoundingBox);
			float iArea = intersect.Width * intersect.Height;
			return iArea / maxArea;
		}

		private static float GetPolygonArea(IList<Vector2> points)
		{
			float area = 0f;
			Vector2 p = points[0];
			for (int i = 0; i < points.Count; i++)
			{
				Vector2 p2 = points[i];
				area += (p.X + p2.X) * (p.Y - p2.Y);
				p = p2;
			}
			return area * 0.5f;
		}

		private static float GetTriangleArea(Vector2 p1, Vector2 p2, Vector2 p3)
		{
			return (p1.X + p2.X) * (p1.Y - p2.Y) + (p2.X + p3.X) * (p2.Y - p3.Y) + (p3.X + p1.X) * (p3.Y - p1.Y);
		}

		private static bool IsEar(IList<Vector2> points, LinkedListNode<int> currentNode)
		{
			int index = currentNode.Value;
			int pindex;
			if (currentNode.Previous == null)
			{
				pindex = currentNode.List.Last.Value;
			}
			else
			{
				pindex = currentNode.Previous.Value;
			}
			int nindex;
			if (currentNode.Next == null)
			{
				nindex = currentNode.List.First.Value;
			}
			else
			{
				nindex = currentNode.Next.Value;
			}
			Vector2 pj = points[pindex];
			Vector2 pi = points[index];
			Vector2 pk = points[nindex];
			if (Polygon2D.GetTriangleArea(pj, pi, pk) < 0f)
			{
				foreach (int i in currentNode.List)
				{
					if (i != pindex && i != nindex && i != index && DrawingTools.PointInTriangle(pj, pi, pk, points[i]))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		private Vector2[] _points = new Vector2[0];

		private RectangleF _boundingBox;

		private Vector2 _center;

		private float _area;

		public struct IntersectionData
		{
			public IntersectionData(float t, LineF2D line, bool coincident)
			{
				this.TargetT = t;
				this.Line = line;
				this.Coincident = coincident;
			}

			public float TargetT;

			public LineF2D Line;

			public bool Coincident;
		}
	}
}
