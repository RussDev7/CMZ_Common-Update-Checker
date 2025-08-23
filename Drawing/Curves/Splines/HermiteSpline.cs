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
			int idx = Spline.GetControlPointIndex(this._controlPoints.Count, ref t);
			return HermiteSpline.ComputeValue(t, this.ControlPoints[idx], this.ControlPoints[idx + 1]);
		}

		private static Vector3 ComputeValue(float t, HermiteSpline.ControlPoint cp1, HermiteSpline.ControlPoint cp2)
		{
			Vector3 p = cp1.Location;
			Vector3 v = cp1.Out;
			Vector3 p2 = cp2.Location;
			Vector3 v2 = cp2.In;
			float t2 = t * t;
			float t3 = t2 * t;
			float a = 2f * t3 - 3f * t2 + 1f;
			float b = -2f * t3 + 3f * t2;
			float c = t3 - 2f * t2 + t;
			float d = t3 - t2;
			float x = a * p.X + b * p2.X + c * v.X + d * v2.X;
			float y = a * p.Y + b * p2.Y + c * v.Y + d * v2.Y;
			float z = a * p.Z + b * p2.Z + c * v.Z + d * v2.Z;
			return new Vector3(x, y, z);
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
			HermiteSpline val = obj as HermiteSpline;
			return !(val == null) && this == val;
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
