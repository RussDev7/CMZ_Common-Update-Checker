using System;

namespace DNA.Drawing.Curves
{
	public class LinearFunction : ISlopeFunction, IFunction
	{
		public float Start
		{
			get
			{
				return this._intercept;
			}
			set
			{
				this._intercept = value;
			}
		}

		public float End
		{
			get
			{
				return this.GetValue(1f);
			}
			set
			{
				this.SetFunction(this._intercept, value);
			}
		}

		private void SetFunction(float start, float end)
		{
			this._intercept = start;
			this._slope = end - start;
		}

		public float GetValue(float x)
		{
			return this._slope * x + this._intercept;
		}

		public float GetSlope(float x)
		{
			return this._slope;
		}

		public RangeF Range
		{
			get
			{
				return new RangeF(this.Start, this.End);
			}
		}

		public RangeF SlopeRange
		{
			get
			{
				return new RangeF(this._slope, this._slope);
			}
		}

		private float _intercept;

		private float _slope;
	}
}
