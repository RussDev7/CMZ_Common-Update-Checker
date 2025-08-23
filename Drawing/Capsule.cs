using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	public struct Capsule
	{
		public Capsule(LineF3D segment, float radius)
		{
			this.Segment = segment;
			this.Radius = radius;
		}

		public bool Contains(Vector3 point)
		{
			Vector3 cpoint = this.Segment.ClosetPointTo(point);
			return Vector3.DistanceSquared(cpoint, point) < this.Radius * this.Radius;
		}

		public float Radius;

		public LineF3D Segment;
	}
}
