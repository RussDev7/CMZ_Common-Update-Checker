using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	public struct Line
	{
		public Line(Point start, Point end)
		{
			this._start = start;
			this._end = end;
		}

		public Line(int x1, int y1, int x2, int y2)
		{
			this._start = new Point(x1, y1);
			this._end = new Point(x2, y2);
		}

		public Point Start
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

		public Point End
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

		public double Length
		{
			get
			{
				return this.Start.Distance(this.End);
			}
		}

		public double DistanceTo(Point pnt)
		{
			double length = this.Length;
			double u;
			if (length != 0.0)
			{
				u = (double)((pnt.X - this._start.X) * (this._end.X - this._start.X) + (pnt.Y - this._start.Y) * (this._end.Y - this._start.Y)) / (length * length);
			}
			else
			{
				u = 0.0;
			}
			if (u >= 0.0 && u <= 1.0)
			{
				double newx = (double)this._start.X + u * (double)(this._end.X - this._start.X);
				double newy = (double)this._start.Y + u * (double)(this._end.Y - this._start.Y);
				double xd = (double)pnt.X - newx;
				double yd = (double)pnt.Y - newy;
				return Math.Sqrt(xd * xd + yd * yd);
			}
			double d = pnt.Distance(this._start);
			double d2 = pnt.Distance(this._end);
			if (d >= d2)
			{
				return d2;
			}
			return d;
		}

		private Point _start;

		private Point _end;
	}
}
