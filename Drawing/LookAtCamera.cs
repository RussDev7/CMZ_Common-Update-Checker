using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	public class LookAtCamera : PerspectiveCamera
	{
		public override Matrix View
		{
			get
			{
				if (this.LookAtEntity != null)
				{
					Matrix cameraRollMatrix = Matrix.CreateFromAxisAngle(Vector3.Forward, this.Roll.Radians);
					Matrix cameraTransformMatrix = cameraRollMatrix * base.LocalToWorld;
					Vector3 up = Vector3.TransformNormal(Vector3.Up, cameraTransformMatrix);
					Vector3 pos = base.WorldPosition;
					Vector3 lookAt = this.LookAtEntity.WorldPosition;
					return Matrix.CreateLookAt(pos, lookAt, up);
				}
				return base.View;
			}
		}

		public Entity LookAtEntity;

		public Angle Roll = Angle.Zero;
	}
}
