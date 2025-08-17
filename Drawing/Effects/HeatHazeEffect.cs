using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.Effects
{
	public class HeatHazeEffect : DNAEffect
	{
		public float WaveMagnitude
		{
			get
			{
				return this._waveMagnitude;
			}
			set
			{
				this._waveMagnitude = value;
			}
		}

		public Texture2D DisplacementTexture
		{
			get
			{
				return this.displaceTextureParam.GetValueTexture2D();
			}
			set
			{
				this.displaceTextureParam.SetValue(value);
			}
		}

		public Texture2D ScreenTexture
		{
			get
			{
				return this.screenTextureParam.GetValueTexture2D();
			}
			set
			{
				this.screenTextureParam.SetValue(value);
			}
		}

		public HeatHazeEffect(Game game)
			: base(game.Content.Load<Effect>("HeatHaze"))
		{
			this.heatMap = game.Content.Load<Texture2D>("HeatNormal");
			this.CacheEffectParameters(null);
		}

		protected HeatHazeEffect(HeatHazeEffect cloneSource)
			: base(cloneSource)
		{
			this.CacheEffectParameters(cloneSource);
			this._waveMagnitude = cloneSource._waveMagnitude;
			this.heatMap = cloneSource.heatMap;
		}

		public override Effect Clone()
		{
			return new HeatHazeEffect(this);
		}

		private void CacheEffectParameters(HeatHazeEffect cloneSource)
		{
			this.displaceTextureParam = base.Parameters["DisplacementMap"];
			this.screenTextureParam = base.Parameters["ScreenMap"];
			this.waveMagnitudeParam = base.Parameters["WaveMagnitude"];
			this.displaceTextureParam.SetValue(this.heatMap);
		}

		protected override void OnApply()
		{
			this.waveMagnitudeParam.SetValue(this._waveMagnitude);
			base.OnApply();
		}

		private float _waveMagnitude = 0.2f;

		private EffectParameter displaceTextureParam;

		private EffectParameter screenTextureParam;

		private EffectParameter waveMagnitudeParam;

		private Texture2D heatMap;
	}
}
