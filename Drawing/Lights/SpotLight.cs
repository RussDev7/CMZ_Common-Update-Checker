using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Lights
{
	public class SpotLight : DirectionalLight
	{
		public override float GetInfluence(Vector3 worldLocation)
		{
			float influence = base.GetInfluence(worldLocation);
			if (influence > 0f)
			{
				switch (this.FallOff)
				{
				case FallOffType.Linear:
				{
					Vector3 direction = worldLocation - base.WorldPosition;
					Angle angle = direction.AngleBetween(base.LightDirection);
					if (angle > this.InnerSpotAngle)
					{
						float blend = 1f - angle / this.OuterSpotAngle;
						blend = Math.Max(blend, 0f);
						influence *= blend;
					}
					break;
				}
				case FallOffType.Squared:
				{
					Vector3 direction2 = worldLocation - base.WorldPosition;
					Angle angle2 = direction2.AngleBetween(base.LightDirection);
					if (angle2 > this.InnerSpotAngle)
					{
						float blend2 = 1f - angle2 / this.OuterSpotAngle;
						blend2 *= blend2;
						blend2 = Math.Max(blend2, 0f);
						influence *= blend2;
					}
					break;
				}
				}
			}
			return influence;
		}

		public Angle InnerSpotAngle = Angle.FromDegrees(10f);

		public Angle OuterSpotAngle = Angle.FromDegrees(30f);

		public FallOffType ConeFalloff = FallOffType.Linear;
	}
}
