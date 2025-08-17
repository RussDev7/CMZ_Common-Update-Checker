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
			int controlPointIndex = Spline.GetControlPointIndex(this._controlPoints.Count, ref t);
			return QuadraticBezierCurve.ComputeValue(t, this.ControlPoints[controlPointIndex], this.ControlPoints[controlPointIndex + 1]);
		}

		private static Vector3 ComputeValue(float t, QuadraticBezierCurve.ControlPoint cp1, QuadraticBezierCurve.ControlPoint cp2)
		{
			Vector3 location = cp1.Location;
			Vector3 handle = cp1.Handle;
			Vector3 location2 = cp2.Location;
			float num = 1f - t;
			float num2 = num * num;
			float num3 = 2f * t * num;
			float num4 = t * t;
			float num5 = num2 * location.X + num3 * handle.X + num4 * location2.X;
			float num6 = num2 * location.Y + num3 * handle.Y + num4 * location2.Y;
			float num7 = num2 * location.Z + num3 * handle.Z + num4 * location2.Z;
			return new Vector3(num5, num6, num7);
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
			QuadraticBezierCurve quadraticBezierCurve = obj as QuadraticBezierCurve;
			return !(quadraticBezierCurve == null) && this == quadraticBezierCurve;
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
