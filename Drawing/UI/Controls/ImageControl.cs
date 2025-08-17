using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI.Controls
{
	public class ImageControl : UIControl
	{
		public override Size Size
		{
			get
			{
				return this._destinationSize;
			}
			set
			{
				this._destinationSize = value;
			}
		}

		public ImageControl(Sprite image, Rectangle destinationRectangle)
		{
			this._unselectedSprite = image;
			base.LocalPosition = new Point(destinationRectangle.X, destinationRectangle.Y);
			this._destinationSize = new Size(destinationRectangle.Width, destinationRectangle.Height);
		}

		public ImageControl(Sprite image, Point position)
		{
			this._unselectedSprite = image;
			base.LocalPosition = position;
		}

		public ImageControl(Sprite image)
		{
			this._unselectedSprite = image;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			this._unselectedSprite.Draw(spriteBatch, base.ScreenBounds, Color.White);
		}

		private Sprite _unselectedSprite;

		public Rectangle? SourceRect;

		public Size _destinationSize;
	}
}
