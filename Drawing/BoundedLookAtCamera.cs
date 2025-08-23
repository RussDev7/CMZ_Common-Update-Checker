using System;
using DNA.Drawing.Collision;
using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	public class BoundedLookAtCamera : LookAtCamera
	{
		public BoundedLookAtCamera(CollisionMap bsp, int percision)
		{
			this._bsp = bsp;
			this._percision = percision;
		}

		public override Matrix View
		{
			get
			{
				if (this.LookAtEntity != null)
				{
					Vector3 pos = base.WorldPosition;
					Vector3 lookAt = this.LookAtEntity.WorldPosition;
					LineF3D cameraLine = new LineF3D(lookAt, pos);
					float? t = this._bsp.CollidesWith(cameraLine);
					if (t != null)
					{
						return Matrix.CreateLookAt(cameraLine.GetValue(t.Value * 0.9f), lookAt, Vector3.Up);
					}
				}
				return base.View;
			}
		}

		private CollisionMap _bsp;

		private int _percision = 4;
	}
}
