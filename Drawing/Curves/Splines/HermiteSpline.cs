using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Curves.Splines
{
	public class HermiteSpline : Spline
	{
		public List<HermiteSpline.ControlPoint> ControlPoints
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
			return HermiteSpline.ComputeValue(t, this.ControlPoints[controlPointIndex], this.ControlPoints[controlPointIndex + 1]);
		}

		private static Vector3 ComputeValue(float t, HermiteSpline.ControlPoint cp1, HermiteSpline.ControlPoint cp2)
		{
			Vector3 location = cp1.Location;
			Vector3 @out = cp1.Out;
			Vector3 location2 = cp2.Location;
			Vector3 @in = cp2.In;
			float num = t * t;
			float num2 = num * t;
			float num3 = 2f * num2 - 3f * num + 1f;
			float num4 = -2f * num2 + 3f * num;
			float num5 = num2 - 2f * num + t;
			float num6 = num2 - num;
			float num7 = num3 * location.X + num4 * location2.X + num5 * @out.X + num6 * @in.X;
			float num8 = num3 * location.Y + num4 * location2.Y + num5 * @out.Y + num6 * @in.Y;
			float num9 = num3 * location.Z + num4 * location2.Z + num5 * @out.Z + num6 * @in.Z;
			return new Vector3(num7, num8, num9);
		}

		public override Vector3 ComputeVelocity(float t)
		{
			throw new NotImplementedException();
		}

		public override Vector3 ComputeAcceleration(float t)
		{
			throw new NotImplementedException();
		}

		public override bool Equals(object obj)
		{
			HermiteSpline hermiteSpline = obj as HermiteSpline;
			return !(hermiteSpline == null) && this == hermiteSpline;
		}

		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		public static bool operator ==(HermiteSpline a, HermiteSpline b)
		{
			throw new NotImplementedException();
		}

		public static bool operator !=(HermiteSpline a, HermiteSpline b)
		{
			throw new NotImplementedException();
		}

		private List<HermiteSpline.ControlPoint> _controlPoints = new List<HermiteSpline.ControlPoint>();

		public struct ControlPoint
		{
			public ControlPoint(Vector3 inVect, Vector3 location, Vector3 outVect)
			{
				this._inHandle = inVect;
				this._location = location;
				this._outHandle = outVect;
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
				this.Out = -this.In;
			}

			public void ReflectOutHandle()
			{
				this.In = -this.Out;
			}

			public override int GetHashCode()
			{
				throw new NotImplementedException();
			}

			public bool Equals(HermiteSpline.ControlPoint other)
			{
				throw new NotImplementedException();
			}

			public override bool Equals(object obj)
			{
				return obj.GetType() == typeof(HermiteSpline.ControlPoint) && this.Equals((HermiteSpline.ControlPoint)obj);
			}

			public static bool operator ==(HermiteSpline.ControlPoint a, HermiteSpline.ControlPoint b)
			{
				return a.Equals(b);
			}

			public static bool operator !=(HermiteSpline.ControlPoint a, HermiteSpline.ControlPoint b)
			{
				return !a.Equals(b);
			}

			private Vector3 _location;

			private Vector3 _inHandle;

			private Vector3 _outHandle;
		}
	}
}
