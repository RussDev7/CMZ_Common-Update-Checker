using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class PostProcessView : BltTargetView
	{
		public RenderTarget2D Target1
		{
			get
			{
				return this._renderTarget1;
			}
		}

		public RenderTarget2D Target2
		{
			get
			{
				return this._renderTarget2;
			}
		}

		public Texture2D LastFrame
		{
			get
			{
				return this._lastFrame;
			}
		}

		private void SetDestinationTargetInternal(RenderTarget2D destinationTarget)
		{
			GraphicsDevice device = base.Game.GraphicsDevice;
			int width;
			int height;
			SurfaceFormat format;
			DepthFormat dFormat;
			if (destinationTarget == null)
			{
				PresentationParameters pp = device.PresentationParameters;
				width = pp.BackBufferWidth;
				height = pp.BackBufferHeight;
				format = pp.BackBufferFormat;
				dFormat = pp.DepthStencilFormat;
				int multiSampleCount = pp.MultiSampleCount;
			}
			else
			{
				width = destinationTarget.Width;
				height = destinationTarget.Height;
				format = destinationTarget.Format;
				dFormat = destinationTarget.DepthStencilFormat;
				int multiSampleCount2 = destinationTarget.MultiSampleCount;
			}
			if (this._renderTarget1 != null && !this._renderTarget1.IsDisposed)
			{
				this._renderTarget1.Dispose();
			}
			if (this._renderTarget2 != null && !this._renderTarget2.IsDisposed)
			{
				this._renderTarget1.Dispose();
			}
			if (this._lastFrame != null && !this._lastFrame.IsDisposed)
			{
				this._lastFrame.Dispose();
			}
			width /= 2;
			height /= 2;
			this._renderTarget1 = new RenderTarget2D(device, width, height, false, format, dFormat, 1, RenderTargetUsage.DiscardContents);
			this._renderTarget2 = new RenderTarget2D(device, width, height, false, format, DepthFormat.None, 1, RenderTargetUsage.DiscardContents);
			if (device.GraphicsProfile == GraphicsProfile.HiDef)
			{
				this._lastFrame = new RenderTarget2D(device, width, height, true, format, DepthFormat.None, 1, RenderTargetUsage.DiscardContents);
				return;
			}
			int pow2w = 1 << (int)(Math.Log((double)width) / Math.Log(2.0));
			int pow2h = 1 << (int)(Math.Log((double)height) / Math.Log(2.0));
			this._lastFrame = new RenderTarget2D(device, pow2w, pow2h, false, format, DepthFormat.None, 1, RenderTargetUsage.DiscardContents);
		}

		public override void SetDestinationTarget(RenderTarget2D destinationTarget)
		{
			base.SetDestinationTarget(destinationTarget);
			this.SetDestinationTargetInternal(destinationTarget);
		}

		public PostProcessView(Game game, RenderTarget2D destinationTarget)
			: base(game, destinationTarget, 1, false)
		{
			this.bloomExtractEffect = base.Game.Content.Load<Effect>("PostProcess\\BloomExtract");
			this.bloomCombineEffect = base.Game.Content.Load<Effect>("PostProcess\\BloomCombine");
			this.gaussianBlurEffect = base.Game.Content.Load<Effect>("PostProcess\\GaussianBlur");
			this.SetDestinationTargetInternal(destinationTarget);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			device.SamplerStates[1] = SamplerState.LinearClamp;
			this.bloomExtractEffect.Parameters["BloomThreshold"].SetValue(this.BloomSettings.BloomThreshold);
			base.DrawFullscreenQuad(spriteBatch, base.OffScreenTarget, this._renderTarget1, this.bloomExtractEffect);
			this.SetBlurEffectParameters(1f / (float)this._renderTarget1.Width, 0f);
			base.DrawFullscreenQuad(spriteBatch, this._renderTarget1, this._renderTarget2, this.gaussianBlurEffect);
			this.SetBlurEffectParameters(0f, 1f / (float)this._renderTarget1.Height);
			base.DrawFullscreenQuad(spriteBatch, this._renderTarget2, this._renderTarget1, this.gaussianBlurEffect);
			EffectParameterCollection parameters = this.bloomCombineEffect.Parameters;
			parameters["BloomIntensity"].SetValue(this.BloomSettings.BloomIntensity);
			parameters["BaseIntensity"].SetValue(this.BloomSettings.BaseIntensity);
			parameters["BloomSaturation"].SetValue(this.BloomSettings.BloomSaturation);
			parameters["BaseSaturation"].SetValue(this.BloomSettings.BaseSaturation);
			device.Textures[1] = base.OffScreenTarget;
			base.DrawFullscreenQuad(spriteBatch, this._renderTarget1, this._renderTarget2, this.bloomCombineEffect);
			device.SetRenderTarget(this._lastFrame);
			Viewport viewport = device.Viewport;
			base.DrawFullscreenQuad(spriteBatch, this._renderTarget1, viewport.Width, viewport.Height, this.bloomCombineEffect);
			base.SetRenderTargetToDevice(device);
			viewport = device.Viewport;
			base.DrawFullscreenQuad(spriteBatch, this._renderTarget1, viewport.Width, viewport.Height, this.bloomCombineEffect);
		}

		private void SetBlurEffectParameters(float dx, float dy)
		{
			EffectParameter weightsParameter = this.gaussianBlurEffect.Parameters["SampleWeights"];
			EffectParameter offsetsParameter = this.gaussianBlurEffect.Parameters["SampleOffsets"];
			int sampleCount = weightsParameter.Elements.Count;
			if (this.sampleWeights.Length != sampleCount)
			{
				this.sampleWeights = new float[sampleCount];
			}
			if (this.sampleOffsets.Length != sampleCount)
			{
				this.sampleOffsets = new Vector2[sampleCount];
			}
			this.sampleWeights[0] = this.ComputeGaussian(0f);
			this.sampleOffsets[0] = new Vector2(0f);
			float totalWeights = this.sampleWeights[0];
			for (int i = 0; i < sampleCount / 2; i++)
			{
				float weight = this.ComputeGaussian((float)(i + 1));
				this.sampleWeights[i * 2 + 1] = weight;
				this.sampleWeights[i * 2 + 2] = weight;
				totalWeights += weight * 2f;
				float sampleOffset = (float)(i * 2) + 1.5f;
				Vector2 delta = new Vector2(dx, dy) * sampleOffset;
				this.sampleOffsets[i * 2 + 1] = delta;
				this.sampleOffsets[i * 2 + 2] = -delta;
			}
			for (int j = 0; j < this.sampleWeights.Length; j++)
			{
				this.sampleWeights[j] /= totalWeights;
			}
			weightsParameter.SetValue(this.sampleWeights);
			offsetsParameter.SetValue(this.sampleOffsets);
		}

		private float ComputeGaussian(float n)
		{
			float theta = this.BloomSettings.BlurAmount;
			return (float)(1.0 / Math.Sqrt(6.283185307179586 * (double)theta) * Math.Exp((double)(-(double)(n * n) / (2f * theta * theta))));
		}

		private RenderTarget2D _renderTarget1;

		private RenderTarget2D _renderTarget2;

		private RenderTarget2D _lastFrame;

		private Effect bloomExtractEffect;

		private Effect bloomCombineEffect;

		private Effect gaussianBlurEffect;

		public BloomSettings BloomSettings = BloomSettings.Default;

		private float[] sampleWeights = new float[0];

		private Vector2[] sampleOffsets = new Vector2[0];
	}
}
