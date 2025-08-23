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
			Rectangle dest = new Rectangle(base.ScreenBounds.X, base.ScreenBounds.Y, base.ScreenBounds.Width, base.ScreenBounds.Height);
			Vector2 center = new Vector2((float)dest.Center.X, (float)dest.Center.Y);
			Vector2 textSize = this.Font.MeasureString(this.Text) * this.Scale;
			textSize.Y = (float)this.Font.LineSpacing * this.Scale;
			Vector2 textPos = Vector2.Zero;
			switch (this.TextAlignment)
			{
			case FrameButtonControl.Alignment.Left:
				textPos.X = (float)(dest.Left + 5);
				textPos.Y = center.Y - textSize.Y / 2f;
				break;
			case FrameButtonControl.Alignment.Right:
				textPos.X = (float)dest.Right - textSize.X * this.Scale - 5f;
				textPos.Y = center.Y - textSize.Y / 2f;
				break;
			case FrameButtonControl.Alignment.Center:
				textPos = center - textSize / 2f;
				break;
			}
			if (!base.Enabled)
			{
				this.Frame.Draw(spriteBatch, dest, this.ButtonDisabledColor);
				spriteBatch.DrawString(this.Font, this.Text, textPos, this.TextDisabledColor, 0f, Vector2.Zero, this.Scale, SpriteEffects.None, 0f);
				return;
			}
			if (base.CaptureInput)
			{
				this.Frame.Draw(spriteBatch, dest, this.ButtonPressedColor);
				spriteBatch.DrawString(this.Font, this.Text, textPos, this.TextPressedColor, 0f, Vector2.Zero, this.Scale, SpriteEffects.None, 0f);
				return;
			}
			if (base.Hovering)
			{
				this.Frame.Draw(spriteBatch, dest, this.ButtonHoverColor);
				spriteBatch.DrawString(this.Font, this.Text, textPos, this.TextHoverColor, 0f, Vector2.Zero, this.Scale, SpriteEffects.None, 0f);
				return;
			}
			this.Frame.Draw(spriteBatch, dest, this.ButtonColor);
			spriteBatch.DrawString(this.Font, this.Text, textPos, this.TextColor, 0f, Vector2.Zero, this.Scale, SpriteEffects.None, 0f);
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
