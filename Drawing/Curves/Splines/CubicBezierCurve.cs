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
			int controlPointIndex = Spline.GetControlPointIndex(this._controlPoints.Count, ref t);
			return CubicBezierCurve.ComputeValue(t, this.ControlPoints[controlPointIndex], this.ControlPoints[controlPointIndex + 1]);
		}

		private static Vector3 ComputeValue(float t, CubicBezierCurve.ControlPoint cp1, CubicBezierCurve.ControlPoint cp2)
		{
			Vector3 location = cp1.Location;
			Vector3 @out = cp1.Out;
			Vector3 @in = cp2.In;
			Vector3 location2 = cp2.Location;
			float num = 1f - t;
			float num2 = num * num * num;
			float num3 = 3f * t * num * num;
			float num4 = 3f * t * t * num;
			float num5 = t * t * t;
			float num6 = num2 * location.X + num3 * @out.X + num4 * @in.X + num5 * location2.X;
			float num7 = num2 * location.Y + num3 * @out.Y + num4 * @in.Y + num5 * location2.Y;
			float num8 = num2 * location.Z + num3 * @out.Z + num4 * @in.Z + num5 * location2.Z;
			return new Vector3(num6, num7, num8);
		}

		public override Vector3 ComputeVelocity(float t)
		{
			int controlPointIndex = Spline.GetControlPointIndex(this._controlPoints.Count, ref t);
			return CubicBezierCurve.ComputeVelocity(t, this.ControlPoints[controlPointIndex], this.ControlPoints[controlPointIndex + 1]);
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
				Vector3 vector = this.In - this.Location;
				this.Out = this.Location - vector;
			}

			public void ReflectOutHandle()
			{
				Vector3 vector = this.Out - this.Location;
				this.In = this.Location - vector;
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
