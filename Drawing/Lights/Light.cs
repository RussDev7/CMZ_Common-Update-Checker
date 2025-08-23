using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Lights
{
	public abstract class Light : Entity
	{
		public float OuterRadius
		{
			get
			{
				return this._outerRadius;
			}
			set
			{
				this._outerRadius = value;
				this._outerRadiusSquared = this._outerRadius * this._outerRadius;
			}
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			this._lastLocation = base.WorldPosition;
			base.Update(game, gameTime);
		}

		public virtual float GetInfluence(Vector3 worldLocation)
		{
			if (!this.Visible)
			{
				return 0f;
			}
			switch (this.FallOff)
			{
			case FallOffType.None:
				return 1f;
			case FallOffType.Linear:
			{
				float distanceSqu = Vector3.DistanceSquared(worldLocation, this._lastLocation);
				if (distanceSqu > this._outerRadiusSquared)
				{
					return 0f;
				}
				float distance = (float)Math.Sqrt((double)distanceSqu);
				if (distance < this.InnerRadius)
				{
					return 1f;
				}
				float edgeRadius = this.OuterRadius - this.InnerRadius;
				float distToInnner = distance - this.InnerRadius;
				float blend = 1f - distToInnner / edgeRadius;
				return Math.Max(blend, 0f);
			}
			case FallOffType.Squared:
			{
				float distanceSqu2 = Vector3.DistanceSquared(worldLocation, this._lastLocation);
				if (distanceSqu2 > this._outerRadiusSquared)
				{
					return 0f;
				}
				float distance2 = (float)Math.Sqrt((double)distanceSqu2);
				if (distance2 < this.InnerRadius)
				{
					return 1f;
				}
				float edgeRadius2 = this.OuterRadius - this.InnerRadius;
				float distToInnner2 = distance2 - this.InnerRadius;
				float blend2 = 1f - distToInnner2 / edgeRadius2;
				blend2 *= blend2;
				return Math.Max(blend2, 0f);
			}
			default:
				return 1f;
			}
		}

		public float InnerRadius = 1f;

		private float _outerRadius = 2f;

		private float _outerRadiusSquared = 4f;

		public FallOffType FallOff;

		public Color LightColor;

		private Vector3 _lastLocation;
	}
}
