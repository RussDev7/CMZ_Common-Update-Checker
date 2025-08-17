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
					Vector3 worldPosition = base.WorldPosition;
					Vector3 worldPosition2 = this.LookAtEntity.WorldPosition;
					LineF3D lineF3D = new LineF3D(worldPosition2, worldPosition);
					float? num = this._bsp.CollidesWith(lineF3D);
					if (num != null)
					{
						return Matrix.CreateLookAt(lineF3D.GetValue(num.Value * 0.9f), worldPosition2, Vector3.Up);
					}
				}
				return base.View;
			}
		}

		private CollisionMap _bsp;

		private int _percision = 4;
	}
}
