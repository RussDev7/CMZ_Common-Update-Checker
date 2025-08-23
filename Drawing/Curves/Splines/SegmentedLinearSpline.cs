using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Curves.Splines
{
	public class SegmentedLinearSpline : Spline
	{
		public Vector3[] Points
		{
			get
			{
				return this._points;
			}
			set
			{
				this._points = value;
			}
		}

		private void ComputeWeights()
		{
			this._weights = new float[this._points.Length - 1];
			float total = 0f;
			for (int i = 0; i < this._points.Length - 1; i++)
			{
				total += Vector3.Distance(this._points[i], this._points[i + 1]);
				this._weights[i] = total;
			}
			for (int j = 0; j < this._weights.Length; j++)
			{
				this._weights[j] /= total;
			}
		}

		public override Vector3 ComputeValue(float t)
		{
			this.ComputeWeights();
			int index = 0;
			while (t > this._weights[index] && index < this._weights.Length)
			{
				index++;
			}
			float start = this._weights[index];
			float end = this._weights[index - 1];
			t = (t - start) / (end - start);
			Vector3 p = this._points[index - 1];
			Vector3 p2 = this._points[index];
			return p + (p2 - p) * t;
		}

		public override Vector3 ComputeVelocity(float t)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override Vector3 ComputeAcceleration(float t)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		private Vector3[] _points;

		private float[] _weights;
	}
}
