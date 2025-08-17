using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	[Serializable]
	public class Polygon
	{
		public Polygon(Vector2[] points)
		{
			this._points = points;
		}

		public Vector2[] Points
		{
			get
			{
				return this._points;
			}
		}

		public RectangleF Extents
		{
			get
			{
				return DrawingTools.GetBoundingRect(this._points);
			}
		}

		public bool Contains(Vector2 point)
		{
			bool flag = false;
			int i = 0;
			int num = this.Points.Length - 1;
			while (i < this.Points.Length)
			{
				float x = this.Points[i].X;
				float y = this.Points[i].Y;
				float x2 = this.Points[num].X;
				float y2 = this.Points[num].Y;
				if (((y <= point.Y && point.Y < y2) || (y2 <= point.Y && point.Y < y)) && point.X < (x2 - x) * (point.Y - y) / (y2 - y) + x)
				{
					flag = !flag;
				}
				num = i;
				i++;
			}
			return flag;
		}

		private Vector2[] _points;
	}
}
