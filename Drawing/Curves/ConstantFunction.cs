using System;

namespace DNA.Drawing.Curves
{
	public class ConstantFunction : ISlopeFunction, IFunction
	{
		public float Value
		{
			get
			{
				return this._value;
			}
			set
			{
				this._value = value;
			}
		}

		public float GetValue(float x)
		{
			return this._value;
		}

		public float GetSlope(float x)
		{
			return 0f;
		}

		public RangeF Range
		{
			get
			{
				return new RangeF(this.Value, this.Value);
			}
		}

		public RangeF SlopeRange
		{
			get
			{
				return new RangeF(0f, 0f);
			}
		}

		private float _value;
	}
}
