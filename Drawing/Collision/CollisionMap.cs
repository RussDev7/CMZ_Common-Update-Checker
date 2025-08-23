using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Collision
{
	public abstract class CollisionMap
	{
		public float? CollidesWith(LineF3D line)
		{
			Triangle3D triangle;
			return this.CollidesWith(line, out triangle);
		}

		public float? CollidesWith(Ray ray)
		{
			Triangle3D triangle;
			return this.CollidesWith(ray, out triangle);
		}

		public float? CollidesWith(Ray ray, out Triangle3D triangle)
		{
			CollisionMap.RayQueryResult? result = this.CollidesWith(ray, 0f, float.MaxValue);
			if (result == null)
			{
				triangle = default(Triangle3D);
				return null;
			}
			triangle = result.Value.Triangle;
			return new float?(result.Value.T);
		}

		public float? CollidesWith(LineF3D line, out Triangle3D triangle)
		{
			Ray ray = new Ray(line.Start, line.End - line.Start);
			CollisionMap.RayQueryResult? result = this.CollidesWith(ray, 0f, 1f);
			if (result == null)
			{
				triangle = default(Triangle3D);
				return null;
			}
			triangle = result.Value.Triangle;
			return new float?(result.Value.T);
		}

		protected abstract CollisionMap.RayQueryResult? CollidesWith(Ray ray, float min, float max);

		public abstract void GetTriangles(List<Triangle3D> tris);

		public abstract void FindCollisions(Ellipsoid ellipsoid, List<CollisionMap.ContactPoint> contacts);

		public abstract void FindCollisions(BoundingSphere sphere, List<CollisionMap.ContactPoint> contacts);

		public abstract void Load(BinaryReader reader);

		public abstract void Save(BinaryWriter reader);

		public struct ContactPoint
		{
			public ContactPoint(Vector3 dir, Triangle3D tri)
			{
				this.PenetrationDirection = dir;
				this.Triangle = tri;
				this.PenetrationDepth = 0f;
			}

			public ContactPoint(Vector3 dir, Triangle3D tri, float p)
			{
				this.PenetrationDirection = dir;
				this.Triangle = tri;
				this.PenetrationDepth = p;
			}

			public Vector3 PenetrationDirection;

			public Triangle3D Triangle;

			public float PenetrationDepth;
		}

		public struct RayQueryResult
		{
			public RayQueryResult(float t, Triangle3D tri)
			{
				this.T = t;
				this.Triangle = tri;
			}

			public Triangle3D Triangle;

			public float T;
		}
	}
}
