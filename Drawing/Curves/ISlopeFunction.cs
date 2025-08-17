using System;

namespace DNA.Drawing.Curves
{
	public interface ISlopeFunction : IFunction
	{
		float GetSlope(float x);

		RangeF SlopeRange { get; }
	}
}
