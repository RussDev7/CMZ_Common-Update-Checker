using System;

namespace DNA.Drawing.Curves
{
	public class CubicFunction : ISlopeFunction, IFunction
	{
		public float StartValue
		{
			get
			{
				return this._startValue;
			}
			set
			{
				this.SetCurve(value, this._startSlope, this.EndValue, this.EndSlope);
			}
		}

		public float StartSlope
		{
			get
			{
				return this._startSlope;
			}
			set
			{
				this.SetCurve(this.StartValue, value, this.EndValue, this.EndSlope);
			}
		}

		public float EndValue
		{
			get
			{
				return this.GetValue(1f);
			}
			set
			{
				this.SetCurve(this.StartValue, this.StartSlope, value, this.EndSlope);
			}
		}

		public float EndSlope
		{
			get
			{
				return this.GetSlope(1f);
			}
			set
			{
				this.SetCurve(this.StartValue, this.StartSlope, this.EndValue, value);
			}
		}

		public CubicFunction()
		{
		}

		public CubicFunction(float startValue, float startSlope, float endValue, float endSlope)
		{
			this.SetCurve(startValue, startSlope, endValue, endSlope);
		}

		public void SetCurve(float startValue, float startSlope, float endValue, float endSlope)
		{
			this._a = endSlope + startSlope + 2f * (startValue - endValue);
			this._b = (endSlope - startSlope - 3f * this._a) * 0.5f;
			this._startSlope = startSlope;
			this._startValue = startValue;
		}

		public float GetSlope(float x)
		{
			return (3f * this._a * x + 2f * this._b) * x + this._startSlope;
		}

		public RangeF SlopeRange
		{
			get
			{
				if (this.StartSlope < this.EndSlope)
				{
					return new RangeF(this.StartSlope, this.EndSlope);
				}
				return new RangeF(this.EndSlope, this.StartSlope);
			}
		}

		public float GetValue(float x)
		{
			return ((this._a * x + this._b) * x + this._startSlope) * x + this._startValue;
		}

		public RangeF Range
		{
			get
			{
				float hi;
				float low = (hi = this.StartValue);
				float t = this.EndValue;
				if (t > hi)
				{
					hi = t;
				}
				else if (t < low)
				{
					low = t;
				}
				if (this._a == 0f)
				{
					if (this._b != 0f)
					{
						t = -0.5f * this._startSlope / this._b;
						if (t > 0f && t < 1f)
						{
							t = this.GetValue(t);
							if (t < low)
							{
								low = t;
							}
							else if (t > hi)
							{
								hi = t;
							}
						}
					}
				}
				else
				{
					float da = 3f * this._a;
					float db = 2f * this._b;
					float range = db * db - 4f * da * this._startSlope;
					if (range >= 0f)
					{
						da = 0.5f / da;
						db = -db * da;
						range = (float)(Math.Sqrt((double)range) * (double)da);
						t = db - range;
						if (t > 0f && t < 1f)
						{
							t = this.GetValue(t);
							if (t < low)
							{
								low = t;
							}
							else if (t > hi)
							{
								hi = t;
							}
						}
						t = db + range;
						if (t > 0f && t < 1f)
						{
							t = this.GetValue(t);
							if (t < low)
							{
								low = t;
							}
							else if (t > hi)
							{
								hi = t;
							}
						}
					}
				}
				return new RangeF(low, hi);
			}
		}

		private float _startValue;

		private float _startSlope;

		private float _a;

		private float _b;
	}
}
