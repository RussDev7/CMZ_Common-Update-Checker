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
			bool c = false;
			int i = 0;
			int j = this.Points.Length - 1;
			while (i < this.Points.Length)
			{
				float xi = this.Points[i].X;
				float yi = this.Points[i].Y;
				float xj = this.Points[j].X;
				float yj = this.Points[j].Y;
				if (((yi <= point.Y && point.Y < yj) || (yj <= point.Y && point.Y < yi)) && point.X < (xj - xi) * (point.Y - yi) / (yj - yi) + xi)
				{
					c = !c;
				}
				j = i;
				i++;
			}
			return c;
		}

		private Vector2[] _points;
	}
}
