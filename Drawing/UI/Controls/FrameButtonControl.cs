using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI.Controls
{
	public class FrameButtonControl : ButtonControl
	{
		public ScalableFrame Frame { get; set; }

		public SpriteFont Font { get; set; }

		public string Text { get; set; }

		public override Size Size
		{
			get
			{
				return new Size((int)((float)this._size.Width * this.Scale), (int)((float)this._size.Height * this.Scale));
			}
			set
			{
				this._size = value;
			}
		}

		public Color ButtonColor { get; set; }

		public Color ButtonHoverColor { get; set; }

		public Color ButtonPressedColor { get; set; }

		public Color TextColor { get; set; }

		public Color TextHoverColor { get; set; }

		public Color TextPressedColor { get; set; }

		public Color ButtonDisabledColor { get; set; }

		public Color TextDisabledColor { get; set; }

		public FrameButtonControl()
		{
			this.ButtonColor = Color.White;
			this.ButtonHoverColor = Color.Gray;
			this.ButtonPressedColor = Color.Black;
			this.ButtonDisabledColor = Color.Gray;
			this.TextColor = Color.Black;
			this.TextHoverColor = Color.Black;
			this.TextPressedColor = Color.White;
			this.TextDisabledColor = Color.DimGray;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle rectangle = new Rectangle(base.ScreenBounds.X, base.ScreenBounds.Y, base.ScreenBounds.Width, base.ScreenBounds.Height);
			Vector2 vector = new Vector2((float)rectangle.Center.X, (float)rectangle.Center.Y);
			Vector2 vector2 = this.Font.MeasureString(this.Text) * this.Scale;
			vector2.Y = (float)this.Font.LineSpacing * this.Scale;
			Vector2 vector3 = Vector2.Zero;
			switch (this.TextAlignment)
			{
			case FrameButtonControl.Alignment.Left:
				vector3.X = (float)(rectangle.Left + 5);
				vector3.Y = vector.Y - vector2.Y / 2f;
				break;
			case FrameButtonControl.Alignment.Right:
				vector3.X = (float)rectangle.Right - vector2.X * this.Scale - 5f;
				vector3.Y = vector.Y - vector2.Y / 2f;
				break;
			case FrameButtonControl.Alignment.Center:
				vector3 = vector - vector2 / 2f;
				break;
			}
			if (!base.Enabled)
			{
				this.Frame.Draw(spriteBatch, rectangle, this.ButtonDisabledColor);
				spriteBatch.DrawString(this.Font, this.Text, vector3, this.TextDisabledColor, 0f, Vector2.Zero, this.Scale, SpriteEffects.None, 0f);
				return;
			}
			if (base.CaptureInput)
			{
				this.Frame.Draw(spriteBatch, rectangle, this.ButtonPressedColor);
				spriteBatch.DrawString(this.Font, this.Text, vector3, this.TextPressedColor, 0f, Vector2.Zero, this.Scale, SpriteEffects.None, 0f);
				return;
			}
			if (base.Hovering)
			{
				this.Frame.Draw(spriteBatch, rectangle, this.ButtonHoverColor);
				spriteBatch.DrawString(this.Font, this.Text, vector3, this.TextHoverColor, 0f, Vector2.Zero, this.Scale, SpriteEffects.None, 0f);
				return;
			}
			this.Frame.Draw(spriteBatch, rectangle, this.ButtonColor);
			spriteBatch.DrawString(this.Font, this.Text, vector3, this.TextColor, 0f, Vector2.Zero, this.Scale, SpriteEffects.None, 0f);
		}

		public FrameButtonControl.Alignment TextAlignment = FrameButtonControl.Alignment.Center;

		private Size _size = new Size(100, 100);

		public enum Alignment
		{
			Left,
			Right,
			Center
		}
	}
}
