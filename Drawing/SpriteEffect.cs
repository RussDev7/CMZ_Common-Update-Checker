using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class SpriteEffect : Effect
	{
		public SpriteEffect(Effect effect)
			: base(effect)
		{
			this.CacheEffectParameters();
		}

		protected SpriteEffect(SpriteEffect cloneSource)
			: base(cloneSource)
		{
			this.CacheEffectParameters();
		}

		public override Effect Clone()
		{
			return new SpriteEffect(this);
		}

		private void CacheEffectParameters()
		{
			this.matrixParam = base.Parameters["MatrixTransform"];
		}

		protected override void OnApply()
		{
			Viewport viewport = base.GraphicsDevice.Viewport;
			Matrix matrix = Matrix.CreateOrthographicOffCenter(0f, (float)viewport.Width, (float)viewport.Height, 0f, 0f, 1f);
			Matrix matrix2 = Matrix.CreateTranslation(-0.5f, -0.5f, 0f);
			this.matrixParam.SetValue(matrix2 * matrix);
		}

		private EffectParameter matrixParam;
	}
}
