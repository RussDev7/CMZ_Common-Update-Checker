using System;

namespace DNA.Drawing.Curves
{
	public interface IFunction
	{
		float GetValue(float x);

		RangeF Range { get; }
	}
}
