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
				Vector2 vector = new Vector2(this._axis.Y, -this._axis.X);
				return new Vector2(this._location.X + this._axis.X * this._size.Y / 2f + vector.X * this._size.X / 2f, this._location.Y + this._axis.Y * this._size.Y / 2f + vector.Y * this._size.X / 2f);
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
			Vector2[] convexHull = DrawingTools.GetConvexHull(opoints);
			if (convexHull.Length <= 0)
			{
				return OrientedBoundingRect.Empty;
			}
			RectangleF rectangleF = DrawingTools.FindBounds(convexHull);
			Vector2 vector = new Vector2(rectangleF.Left + rectangleF.Width / 2f, rectangleF.Top + rectangleF.Height / 2f);
			Vector2 vector2 = new Vector2(0f, 0f);
			foreach (Vector2 vector3 in convexHull)
			{
				Vector2 vector4 = new Vector2(vector3.X - vector.X, vector3.Y - vector.Y);
				if (vector4.X < 0f)
				{
					vector4 = -vector4;
				}
				vector2 += vector4;
			}
			Vector2 vector5 = new Vector2(0f, 0f);
			foreach (Vector2 vector6 in convexHull)
			{
				Vector2 vector7 = new Vector2(vector6.X - vector.X, vector6.Y - vector.Y);
				if (vector7.Y < 0f)
				{
					vector7 = -vector7;
				}
				vector5 += vector7;
			}
			if (vector5.Length() > vector2.Length())
			{
				vector2 = vector5;
			}
			vector2.Normalize();
			Vector2 vector8 = new Vector2(vector2.Y, -vector2.X);
			float num = float.MaxValue;
			float num2 = float.MaxValue;
			float num3 = float.MinValue;
			float num4 = float.MinValue;
			foreach (Vector2 vector9 in convexHull)
			{
				Vector2 vector10 = new Vector2(vector.X - vector9.X, vector.Y - vector9.Y);
				float num5 = vector2.X * vector10.Y - vector10.X * vector2.Y;
				float num6 = -(vector8.X * vector10.Y - vector10.X * vector8.Y);
				num = Math.Min(num, num5);
				num2 = Math.Min(num2, num6);
				num3 = Math.Max(num3, num5);
				num4 = Math.Max(num4, num6);
			}
			float num7 = num3 - num;
			float num8 = num4 - num2;
			Vector2 vector11 = num * vector8;
			Vector2 vector12 = num2 * vector2;
			vector.X = vector.X + vector11.X + vector12.X;
			vector.Y = vector.Y + vector11.Y + vector12.Y;
			return new OrientedBoundingRect(vector, new Vector2(num7, num8), vector2);
		}

		public static OrientedBoundingRect FromPoints(IList<Point> opoints)
		{
			Vector2[] array = new Vector2[opoints.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new Vector2((float)opoints[i].X, (float)opoints[i].Y);
			}
			return OrientedBoundingRect.FromPoints(array);
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
