using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI.Controls
{
	public class ImageButtonControl : ButtonControl
	{
		public SpriteFont Font { get; set; }

		public string Text { get; set; }

		public override Size Size
		{
			get
			{
				return new Size(this.Image.Width, this.Image.Height);
			}
			set
			{
			}
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Vector2 imgPos = new Vector2((float)base.LocalPosition.X, (float)base.LocalPosition.Y);
			Vector2 strPos = new Vector2(imgPos.X + (float)(this.Image.Width / 2) - this.Font.MeasureString(this.Text).X / 2f, imgPos.Y + (float)(this.Image.Height / 2) - this.Font.MeasureString(this.Text).Y / 2f);
			if (base.CaptureInput)
			{
				spriteBatch.Draw(this.Image, imgPos, Color.White);
				spriteBatch.DrawString(this.Font, this.Text, strPos, Color.Black);
				return;
			}
			if (base.Hovering)
			{
				spriteBatch.Draw(this.Image, imgPos, Color.Gray);
				spriteBatch.DrawString(this.Font, this.Text, strPos, Color.White);
				return;
			}
			spriteBatch.Draw(this.Image, imgPos, this.ImageDefaultColor);
			spriteBatch.DrawString(this.Font, this.Text, strPos, Color.White);
		}

		public Sprite Image;

		public Color ImageDefaultColor = Color.Black;
	}
}
