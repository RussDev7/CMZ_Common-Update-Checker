using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Drawing2D
{
	public struct OrientedBoundingRect
	{
		public Vector2 Size
		{
			get
			{
				return this._size;
			}
		}

		public Vector2 Location
		{
			get
			{
				return this._location;
			}
		}

		public Angle Rotation
		{
			get
			{
				return Angle.FromDegrees(90f) + Angle.ATan2((double)this._axis.Y, (double)this._axis.X);
			}
		}

		public Vector2 Axis
		{
			get
			{
				return this._axis;
			}
		}

		public Vector2 Center
		{
			get
			{
				Vector2 axis2 = new Vector2(this._axis.Y, -this._axis.X);
				return new Vector2(this._location.X + this._axis.X * this._size.Y / 2f + axis2.X * this._size.X / 2f, this._location.Y + this._axis.Y * this._size.Y / 2f + axis2.Y * this._size.X / 2f);
			}
		}

		public Matrix Orientation
		{
			get
			{
				return Matrix.CreateScale(this._size.X, this._size.Y, 1f) * Matrix.CreateRotationZ(this.Rotation.Radians) * Matrix.CreateTranslation(this._location.X, this._location.Y, 0f);
			}
		}

		private OrientedBoundingRect(Vector2 location, Vector2 size, Vector2 axis)
		{
			this._size = size;
			this._location = location;
			this._axis = axis;
		}

		public static OrientedBoundingRect FromPoints(IList<Vector2> opoints)
		{
			Vector2[] points = DrawingTools.GetConvexHull(opoints);
			if (points.Length <= 0)
			{
				return OrientedBoundingRect.Empty;
			}
			RectangleF bounds = DrawingTools.FindBounds(points);
			Vector2 pivot = new Vector2(bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f);
			Vector2 axis = new Vector2(0f, 0f);
			foreach (Vector2 p in points)
			{
				Vector2 v = new Vector2(p.X - pivot.X, p.Y - pivot.Y);
				if (v.X < 0f)
				{
					v = -v;
				}
				axis += v;
			}
			Vector2 axisB = new Vector2(0f, 0f);
			foreach (Vector2 p2 in points)
			{
				Vector2 v2 = new Vector2(p2.X - pivot.X, p2.Y - pivot.Y);
				if (v2.Y < 0f)
				{
					v2 = -v2;
				}
				axisB += v2;
			}
			if (axisB.Length() > axis.Length())
			{
				axis = axisB;
			}
			axis.Normalize();
			Vector2 axis2 = new Vector2(axis.Y, -axis.X);
			float minWidth = float.MaxValue;
			float minHeight = float.MaxValue;
			float maxWidth = float.MinValue;
			float maxHeight = float.MinValue;
			foreach (Vector2 p3 in points)
			{
				Vector2 v3 = new Vector2(pivot.X - p3.X, pivot.Y - p3.Y);
				float dist = axis.X * v3.Y - v3.X * axis.Y;
				float dist2 = -(axis2.X * v3.Y - v3.X * axis2.Y);
				minWidth = Math.Min(minWidth, dist);
				minHeight = Math.Min(minHeight, dist2);
				maxWidth = Math.Max(maxWidth, dist);
				maxHeight = Math.Max(maxHeight, dist2);
			}
			float width = maxWidth - minWidth;
			float height = maxHeight - minHeight;
			Vector2 shift = minWidth * axis2;
			Vector2 shift2 = minHeight * axis;
			pivot.X = pivot.X + shift.X + shift2.X;
			pivot.Y = pivot.Y + shift.Y + shift2.Y;
			return new OrientedBoundingRect(pivot, new Vector2(width, height), axis);
		}

		public static OrientedBoundingRect FromPoints(IList<Point> opoints)
		{
			Vector2[] points = new Vector2[opoints.Count];
			for (int i = 0; i < points.Length; i++)
			{
				points[i] = new Vector2((float)opoints[i].X, (float)opoints[i].Y);
			}
			return OrientedBoundingRect.FromPoints(points);
		}

		public static OrientedBoundingRect FromPoints(IList<Vector3> points)
		{
			throw new Exception();
		}

		public static readonly OrientedBoundingRect Empty = new OrientedBoundingRect(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));

		private Vector2 _size;

		private Vector2 _location;

		private Vector2 _axis;
	}
}
