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
			Vector2 vector = new Vector2((float)base.LocalPosition.X, (float)base.LocalPosition.Y);
			Vector2 vector2 = new Vector2(vector.X + (float)(this.Image.Width / 2) - this.Font.MeasureString(this.Text).X / 2f, vector.Y + (float)(this.Image.Height / 2) - this.Font.MeasureString(this.Text).Y / 2f);
			if (base.CaptureInput)
			{
				spriteBatch.Draw(this.Image, vector, Color.White);
				spriteBatch.DrawString(this.Font, this.Text, vector2, Color.Black);
				return;
			}
			if (base.Hovering)
			{
				spriteBatch.Draw(this.Image, vector, Color.Gray);
				spriteBatch.DrawString(this.Font, this.Text, vector2, Color.White);
				return;
			}
			spriteBatch.Draw(this.Image, vector, this.ImageDefaultColor);
			spriteBatch.DrawString(this.Font, this.Text, vector2, Color.White);
		}

		public Sprite Image;

		public Color ImageDefaultColor = Color.Black;
	}
}
