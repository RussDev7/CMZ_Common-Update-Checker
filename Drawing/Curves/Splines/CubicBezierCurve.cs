using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Curves.Splines
{
	public class CubicBezierCurve : Spline
	{
		public List<CubicBezierCurve.ControlPoint> ControlPoints
		{
			get
			{
				return this._controlPoints;
			}
			set
			{
				this._controlPoints = value;
			}
		}

		public override Vector3 ComputeValue(float t)
		{
			int idx = Spline.GetControlPointIndex(this._controlPoints.Count, ref t);
			return CubicBezierCurve.ComputeValue(t, this.ControlPoints[idx], this.ControlPoints[idx + 1]);
		}

		private static Vector3 ComputeValue(float t, CubicBezierCurve.ControlPoint cp1, CubicBezierCurve.ControlPoint cp2)
		{
			Vector3 p = cp1.Location;
			Vector3 p2 = cp1.Out;
			Vector3 p3 = cp2.In;
			Vector3 p4 = cp2.Location;
			float omt = 1f - t;
			float a = omt * omt * omt;
			float b = 3f * t * omt * omt;
			float c = 3f * t * t * omt;
			float d = t * t * t;
			float x = a * p.X + b * p2.X + c * p3.X + d * p4.X;
			float y = a * p.Y + b * p2.Y + c * p3.Y + d * p4.Y;
			float z = a * p.Z + b * p2.Z + c * p3.Z + d * p4.Z;
			return new Vector3(x, y, z);
		}

		public override Vector3 ComputeVelocity(float t)
		{
			int idx = Spline.GetControlPointIndex(this._controlPoints.Count, ref t);
			return CubicBezierCurve.ComputeVelocity(t, this.ControlPoints[idx], this.ControlPoints[idx + 1]);
		}

		public static Vector3 ComputeVelocity(float t, CubicBezierCurve.ControlPoint cp1, CubicBezierCurve.ControlPoint cp2)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public override Vector3 ComputeAcceleration(float t)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		private List<CubicBezierCurve.ControlPoint> _controlPoints = new List<CubicBezierCurve.ControlPoint>();

		public struct ControlPoint
		{
			public ControlPoint(Vector3 inPoint, Vector3 location, Vector3 outPoint)
			{
				this._inHandle = inPoint;
				this._location = location;
				this._outHandle = outPoint;
			}

			public Vector3 In
			{
				get
				{
					return this._inHandle;
				}
				set
				{
					this._inHandle = value;
				}
			}

			public Vector3 Location
			{
				get
				{
					return this._location;
				}
				set
				{
					this._location = value;
				}
			}

			public Vector3 Out
			{
				get
				{
					return this._outHandle;
				}
				set
				{
					this._outHandle = value;
				}
			}

			public void ReflectInHandle()
			{
				Vector3 vect = this.In - this.Location;
				this.Out = this.Location - vect;
			}

			public void ReflectOutHandle()
			{
				Vector3 vect = this.Out - this.Location;
				this.In = this.Location - vect;
			}

			public override int GetHashCode()
			{
				throw new NotImplementedException();
			}

			public bool Equals(CubicBezierCurve.ControlPoint other)
			{
				throw new NotImplementedException();
			}

			public override bool Equals(object obj)
			{
				return obj.GetType() == typeof(CubicBezierCurve.ControlPoint) && this.Equals((CubicBezierCurve.ControlPoint)obj);
			}

			public static bool operator ==(CubicBezierCurve.ControlPoint a, CubicBezierCurve.ControlPoint b)
			{
				return a.Equals(b);
			}

			public static bool operator !=(CubicBezierCurve.ControlPoint a, CubicBezierCurve.ControlPoint b)
			{
				return !a.Equals(b);
			}

			private Vector3 _inHandle;

			private Vector3 _location;

			private Vector3 _outHandle;
		}
	}
}
