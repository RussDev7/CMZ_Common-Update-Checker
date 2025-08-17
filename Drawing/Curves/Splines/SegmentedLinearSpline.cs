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
			float num = 0f;
			for (int i = 0; i < this._points.Length - 1; i++)
			{
				num += Vector3.Distance(this._points[i], this._points[i + 1]);
				this._weights[i] = num;
			}
			for (int j = 0; j < this._weights.Length; j++)
			{
				this._weights[j] /= num;
			}
		}

		public override Vector3 ComputeValue(float t)
		{
			this.ComputeWeights();
			int num = 0;
			while (t > this._weights[num] && num < this._weights.Length)
			{
				num++;
			}
			float num2 = this._weights[num];
			float num3 = this._weights[num - 1];
			t = (t - num2) / (num3 - num2);
			Vector3 vector = this._points[num - 1];
			Vector3 vector2 = this._points[num];
			return vector + (vector2 - vector) * t;
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
