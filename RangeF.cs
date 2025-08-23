using System;

namespace DNA
{
	public struct RangeF
	{
		public bool Degenerate
		{
			get
			{
				return this._min == this._max;
			}
		}

		public float Min
		{
			get
			{
				return this._min;
			}
		}

		public float Max
		{
			get
			{
				return this._max;
			}
		}

		public float Span
		{
			get
			{
				return this.Max - this.Min;
			}
		}

		public float ToSpan(float val)
		{
			return (val - this.Min) / this.Span;
		}

		public bool InRange(float t)
		{
			return t >= this.Min && t <= this.Max;
		}

		public bool Overlaps(RangeF r)
		{
			return r.Min <= this.Max && r.Max >= this.Min;
		}

		public RangeF(float min, float max)
		{
			this._min = min;
			this._max = max;
			if (this._max < this._min)
			{
				throw new ArgumentException("Max must be Greator than Min");
			}
		}

		public override int GetHashCode()
		{
			return this._min.GetHashCode() ^ this._max.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			RangeF oval = (RangeF)obj;
			return this._min == oval._min && this._max == oval._max;
		}

		public static bool operator ==(RangeF a, RangeF b)
		{
			return a._min == b._min && a._max == b._max;
		}

		public static bool operator !=(RangeF a, RangeF b)
		{
			return a._min != b._min || a._max != b._max;
		}

		private float _min;

		private float _max;
	}
}
