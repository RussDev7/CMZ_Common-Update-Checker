using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Curves.Splines
{
	public abstract class Spline : ISpline
	{
		public abstract Vector3 ComputeValue(float t);

		public abstract Vector3 ComputeVelocity(float t);

		public abstract Vector3 ComputeAcceleration(float t);

		protected static int GetControlPointIndex(int total, ref float t)
		{
			float dt = 1f / (float)(total - 1);
			float fidx = t / dt;
			int idx = (int)fidx;
			t = fidx - (float)idx;
			return idx;
		}
	}
}
