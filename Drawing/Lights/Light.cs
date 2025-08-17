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
				float num = Vector3.DistanceSquared(worldLocation, this._lastLocation);
				if (num > this._outerRadiusSquared)
				{
					return 0f;
				}
				float num2 = (float)Math.Sqrt((double)num);
				if (num2 < this.InnerRadius)
				{
					return 1f;
				}
				float num3 = this.OuterRadius - this.InnerRadius;
				float num4 = num2 - this.InnerRadius;
				float num5 = 1f - num4 / num3;
				return Math.Max(num5, 0f);
			}
			case FallOffType.Squared:
			{
				float num6 = Vector3.DistanceSquared(worldLocation, this._lastLocation);
				if (num6 > this._outerRadiusSquared)
				{
					return 0f;
				}
				float num7 = (float)Math.Sqrt((double)num6);
				if (num7 < this.InnerRadius)
				{
					return 1f;
				}
				float num8 = this.OuterRadius - this.InnerRadius;
				float num9 = num7 - this.InnerRadius;
				float num10 = 1f - num9 / num8;
				num10 *= num10;
				return Math.Max(num10, 0f);
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
