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
			double num;
			if (length != 0.0)
			{
				num = (double)((pnt.X - this._start.X) * (this._end.X - this._start.X) + (pnt.Y - this._start.Y) * (this._end.Y - this._start.Y)) / (length * length);
			}
			else
			{
				num = 0.0;
			}
			if (num >= 0.0 && num <= 1.0)
			{
				double num2 = (double)this._start.X + num * (double)(this._end.X - this._start.X);
				double num3 = (double)this._start.Y + num * (double)(this._end.Y - this._start.Y);
				double num4 = (double)pnt.X - num2;
				double num5 = (double)pnt.Y - num3;
				return Math.Sqrt(num4 * num4 + num5 * num5);
			}
			double num6 = pnt.Distance(this._start);
			double num7 = pnt.Distance(this._end);
			if (num6 >= num7)
			{
				return num7;
			}
			return num6;
		}

		private Point _start;

		private Point _end;
	}
}
