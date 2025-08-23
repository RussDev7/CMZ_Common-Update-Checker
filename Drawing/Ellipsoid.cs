using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	public struct Ellipsoid
	{
		public BoundingSphere GetBoundingSphere()
		{
			return new BoundingSphere(this.Center, Math.Max(this.Radius.X, Math.Max(this.Radius.Y, this.Radius.Z)));
		}

		public Ellipsoid(Vector3 center, Vector3 scale, Quaternion orientation)
		{
			this.Center = center;
			this.Radius = scale;
			this.ReciprocalRadius = new Vector3(1f / scale.X, 1f / scale.Y, 1f / scale.Z);
		}

		public Vector3 TransformWorldSpacePointToUnitSphereSpace(Vector3 point)
		{
			return Vector3.Multiply(point - this.Center, this.ReciprocalRadius);
		}

		public Vector3 TransformUnitSphereSpacePointToWorldSpace(Vector3 point)
		{
			return Vector3.Multiply(point, this.Radius) + this.Center;
		}

		public Vector3 TransformWorldSpaceVectorToUnitSphereSpace(Vector3 vector)
		{
			Vector3 result = Vector3.Multiply(vector, this.Radius);
			result.Normalize();
			return result;
		}

		public Vector3 TransformUnitSphereSpaceVectorToWorldSpace(Vector3 vector)
		{
			Vector3 result = Vector3.Multiply(vector, this.ReciprocalRadius);
			result.Normalize();
			return result;
		}

		public Plane TransformWorldSpacePlaneToUnitSphereSpace(Plane plane)
		{
			Plane result = default(Plane);
			result.D = plane.D + Vector3.Dot(plane.Normal, this.Center);
			result.Normal = Vector3.Multiply(plane.Normal, this.Radius);
			float recipDist = 1f / result.Normal.Length();
			result.Normal *= recipDist;
			result.D *= recipDist;
			return result;
		}

		public Triangle3D TransformWorldSpaceTriToUnitSphereSpace(Triangle3D tri, ref Triangle3D result)
		{
			result.A = this.TransformWorldSpacePointToUnitSphereSpace(tri.A);
			result.B = this.TransformWorldSpacePointToUnitSphereSpace(tri.B);
			result.C = this.TransformWorldSpacePointToUnitSphereSpace(tri.C);
			return result;
		}

		public float CalculateWorldSpacePenetration(Vector3 point)
		{
			Vector3 edgePoint = this.TransformUnitSphereSpacePointToWorldSpace(Vector3.Normalize(point));
			Vector3 touchPoint = this.TransformUnitSphereSpacePointToWorldSpace(point);
			return Vector3.Distance(edgePoint, touchPoint);
		}

		public Vector3 Center;

		public Vector3 Radius;

		public Vector3 ReciprocalRadius;
	}
}
