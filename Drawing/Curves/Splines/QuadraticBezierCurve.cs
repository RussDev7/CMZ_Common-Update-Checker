using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Curves.Splines
{
	public class QuadraticBezierCurve : Spline
	{
		public List<QuadraticBezierCurve.ControlPoint> ControlPoints
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
			return QuadraticBezierCurve.ComputeValue(t, this.ControlPoints[idx], this.ControlPoints[idx + 1]);
		}

		private static Vector3 ComputeValue(float t, QuadraticBezierCurve.ControlPoint cp1, QuadraticBezierCurve.ControlPoint cp2)
		{
			Vector3 p = cp1.Location;
			Vector3 p2 = cp1.Handle;
			Vector3 p3 = cp2.Location;
			float omt = 1f - t;
			float a = omt * omt;
			float b = 2f * t * omt;
			float c = t * t;
			float x = a * p.X + b * p2.X + c * p3.X;
			float y = a * p.Y + b * p2.Y + c * p3.Y;
			float z = a * p.Z + b * p2.Z + c * p3.Z;
			return new Vector3(x, y, z);
		}

		public override Vector3 ComputeVelocity(float t)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public override Vector3 ComputeAcceleration(float t)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public override bool Equals(object obj)
		{
			QuadraticBezierCurve val = obj as QuadraticBezierCurve;
			return !(val == null) && this == val;
		}

		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		public static bool operator ==(QuadraticBezierCurve a, QuadraticBezierCurve b)
		{
			throw new NotImplementedException();
		}

		public static bool operator !=(QuadraticBezierCurve a, QuadraticBezierCurve b)
		{
			throw new NotImplementedException();
		}

		private List<QuadraticBezierCurve.ControlPoint> _controlPoints = new List<QuadraticBezierCurve.ControlPoint>();

		public struct ControlPoint
		{
			public ControlPoint(Vector3 location, Vector3 handle)
			{
				this._location = location;
				this._handle = handle;
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

			public Vector3 Handle
			{
				get
				{
					return this._handle;
				}
				set
				{
					this._handle = value;
				}
			}

			public override int GetHashCode()
			{
				throw new NotImplementedException();
			}

			public bool Equals(QuadraticBezierCurve.ControlPoint other)
			{
				throw new NotImplementedException();
			}

			public override bool Equals(object obj)
			{
				return obj.GetType() == typeof(QuadraticBezierCurve.ControlPoint) && this.Equals((QuadraticBezierCurve.ControlPoint)obj);
			}

			public static bool operator ==(QuadraticBezierCurve.ControlPoint a, QuadraticBezierCurve.ControlPoint b)
			{
				return a.Equals(b);
			}

			public static bool operator !=(QuadraticBezierCurve.ControlPoint a, QuadraticBezierCurve.ControlPoint b)
			{
				return !a.Equals(b);
			}

			private Vector3 _handle;

			private Vector3 _location;
		}
	}
}
