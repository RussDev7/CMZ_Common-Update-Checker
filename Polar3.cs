using System;
using Microsoft.Xna.Framework;

namespace DNA
{
	public struct Polar3
	{
		public float Radius
		{
			get
			{
				return this._radius;
			}
		}

		public Angle Zenith
		{
			get
			{
				return this._zenith;
			}
		}

		public Angle Azimuth
		{
			get
			{
				return this._azimuth;
			}
		}

		public Vector3 GetCartesian()
		{
			float x = (float)((double)this.Radius * Math.Sin((double)this.Zenith.Radians) * Math.Cos((double)this.Azimuth.Radians));
			float y = (float)((double)this.Radius * Math.Sin((double)this.Zenith.Radians) * Math.Sin((double)this.Azimuth.Radians));
			float z = (float)((double)this.Radius * Math.Cos((double)this.Zenith.Radians));
			return new Vector3(x, y, z);
		}

		public float DistanceTo(Polar3 p)
		{
			Vector3 p2 = this.GetCartesian();
			Vector3 p3 = p.GetCartesian();
			return Vector3.Distance(p2, p3);
		}

		public float ArcLength(Polar3 p3)
		{
			Vector3 v = this.GetCartesian();
			Vector3 v2 = p3.GetCartesian();
			Angle angle = v.AngleBetween(v2);
			float circum = (float)((double)(2f * p3.Radius) * 3.141592653589793);
			return angle.Revolutions * circum;
		}

		public Polar3(Angle zenith, Angle azimuth, float radius)
		{
			this._radius = radius;
			this._zenith = zenith;
			this._azimuth = azimuth;
		}

		public override bool Equals(object obj)
		{
			return obj.GetType() == typeof(Polar3) && this == (Polar3)obj;
		}

		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		public static bool operator ==(Polar3 a, Polar3 b)
		{
			throw new NotImplementedException();
		}

		public static bool operator !=(Polar3 a, Polar3 b)
		{
			throw new NotImplementedException();
		}

		private float _radius;

		private Angle _zenith;

		private Angle _azimuth;
	}
}
