using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Lights
{
	public class SpotLight : DirectionalLight
	{
		public override float GetInfluence(Vector3 worldLocation)
		{
			float num = base.GetInfluence(worldLocation);
			if (num > 0f)
			{
				switch (this.FallOff)
				{
				case FallOffType.Linear:
				{
					Vector3 vector = worldLocation - base.WorldPosition;
					Angle angle = vector.AngleBetween(base.LightDirection);
					if (angle > this.InnerSpotAngle)
					{
						float num2 = 1f - angle / this.OuterSpotAngle;
						num2 = Math.Max(num2, 0f);
						num *= num2;
					}
					break;
				}
				case FallOffType.Squared:
				{
					Vector3 vector2 = worldLocation - base.WorldPosition;
					Angle angle2 = vector2.AngleBetween(base.LightDirection);
					if (angle2 > this.InnerSpotAngle)
					{
						float num3 = 1f - angle2 / this.OuterSpotAngle;
						num3 *= num3;
						num3 = Math.Max(num3, 0f);
						num *= num3;
					}
					break;
				}
				}
			}
			return num;
		}

		public Angle InnerSpotAngle = Angle.FromDegrees(10f);

		public Angle OuterSpotAngle = Angle.FromDegrees(30f);

		public FallOffType ConeFalloff = FallOffType.Linear;
	}
}
