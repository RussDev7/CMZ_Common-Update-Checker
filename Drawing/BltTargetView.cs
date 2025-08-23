using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class BltTargetView : View
	{
		public RenderTarget2D OffScreenTarget
		{
			get
			{
				return this._offScreenBuffer;
			}
		}

		private void SetDestinationTargetInternal(RenderTarget2D destinationTarget)
		{
			int width;
			int height;
			SurfaceFormat format;
			DepthFormat dFormat;
			int msac;
			if (destinationTarget == null)
			{
				PresentationParameters pp = base.Game.GraphicsDevice.PresentationParameters;
				width = pp.BackBufferWidth;
				height = pp.BackBufferHeight;
				format = pp.BackBufferFormat;
				dFormat = pp.DepthStencilFormat;
				msac = pp.MultiSampleCount;
			}
			else
			{
				width = destinationTarget.Width;
				height = destinationTarget.Height;
				format = destinationTarget.Format;
				dFormat = destinationTarget.DepthStencilFormat;
				msac = destinationTarget.MultiSampleCount;
			}
			if (this._offScreenBuffer != null && !this._offScreenBuffer.IsDisposed)
			{
				this._offScreenBuffer.Dispose();
			}
			this._offScreenBuffer = new RenderTarget2D(base.Game.GraphicsDevice, width / this._downsample, height / this._downsample, this._mipMap, format, dFormat, msac, RenderTargetUsage.DiscardContents);
		}

		public override void SetDestinationTarget(RenderTarget2D destinationTarget)
		{
			base.SetDestinationTarget(destinationTarget);
			this.SetDestinationTargetInternal(destinationTarget);
		}

		public BltTargetView(Game game, RenderTarget2D destinationTarget, int downsample, bool mipmap)
			: base(game, destinationTarget)
		{
			this._mipMap = mipmap;
			this._downsample = downsample;
			this.SetDestinationTargetInternal(destinationTarget);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Viewport viewport = device.Viewport;
			base.SetRenderTargetToDevice(device);
			this.DrawFullscreenQuad(spriteBatch, this.OffScreenTarget, viewport.Width, viewport.Height, null);
		}

		protected void DrawFullscreenQuad(SpriteBatch spriteBatch, Texture2D texture, RenderTarget2D renderTarget, Effect effect)
		{
			spriteBatch.GraphicsDevice.SetRenderTarget(renderTarget);
			this.DrawFullscreenQuad(spriteBatch, texture, renderTarget.Width, renderTarget.Height, effect);
		}

		protected void DrawFullscreenQuad(SpriteBatch spriteBatch, Texture2D texture, int width, int height, Effect effect)
		{
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, effect);
			spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
			spriteBatch.End();
		}

		private RenderTarget2D _offScreenBuffer;

		private int _downsample;

		private bool _mipMap;
	}
}
