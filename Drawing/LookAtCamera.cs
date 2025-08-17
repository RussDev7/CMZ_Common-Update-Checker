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
					Matrix matrix = Matrix.CreateFromAxisAngle(Vector3.Forward, this.Roll.Radians);
					Matrix matrix2 = matrix * base.LocalToWorld;
					Vector3 vector = Vector3.TransformNormal(Vector3.Up, matrix2);
					Vector3 worldPosition = base.WorldPosition;
					Vector3 worldPosition2 = this.LookAtEntity.WorldPosition;
					return Matrix.CreateLookAt(worldPosition, worldPosition2, vector);
				}
				return base.View;
			}
		}

		public Entity LookAtEntity;

		public Angle Roll = Angle.Zero;
	}
}
