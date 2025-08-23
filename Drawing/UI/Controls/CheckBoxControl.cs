using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI.Controls
{
	public class CheckBoxControl : ButtonControl
	{
		public SpriteFont Font { get; set; }

		public string Text { get; set; }

		public bool TextOnRight { get; set; }

		public bool Checked { get; set; }

		public Sprite UncheckedImage { get; set; }

		public Sprite CheckedImage { get; set; }

		public Color TextColor { get; set; }

		public Color CheckColor { get; set; }

		public Color CheckPressedColor { get; set; }

		public CheckBoxControl()
		{
			this.TextColor = Color.Black;
			this.CheckColor = Color.White;
			this.CheckPressedColor = Color.Gray;
			this.TextOnRight = true;
			this.Checked = false;
		}

		public CheckBoxControl(Sprite box, Sprite check)
		{
			this.TextColor = Color.Black;
			this.CheckColor = Color.White;
			this.CheckPressedColor = Color.Gray;
			this.UncheckedImage = box;
			this.CheckedImage = check;
		}

		public override Size Size
		{
			get
			{
				Vector2 textSize = Vector2.Zero;
				textSize = this.Font.MeasureString(this.Text);
				return new Size((int)((textSize.X + (float)this.UncheckedImage.Width + 5f) * this.Scale), (int)(((textSize.Y > (float)this.UncheckedImage.Height) ? textSize.Y : ((float)this.UncheckedImage.Height)) * this.Scale));
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle dest = base.ScreenBounds;
			Vector2 textSize = this.Font.MeasureString(this.Text) * this.Scale;
			Color buttonColor = this.CheckColor;
			if (base.Hovering || base.CaptureInput)
			{
				buttonColor = this.CheckPressedColor;
			}
			float height = (float)this.UncheckedImage.Height * this.Scale;
			float lineSpacing = (float)this.Font.LineSpacing * this.Scale;
			float offset = 2f * this.Scale;
			float checkSpace = 5f * this.Scale;
			if (this.TextOnRight)
			{
				spriteBatch.DrawString(this.Font, this.Text, new Vector2((float)(dest.X + this.UncheckedImage.Width) + checkSpace, (float)dest.Y), this.TextColor, 0f, Vector2.Zero, this.Scale, SpriteEffects.None, 0f);
				spriteBatch.Draw(this.UncheckedImage, new Vector2((float)dest.X, (float)dest.Y + offset + lineSpacing / 2f - height / 2f), this.Scale, buttonColor);
				if (this.Checked)
				{
					spriteBatch.Draw(this.CheckedImage, new Vector2((float)dest.X, (float)dest.Y + offset + lineSpacing / 2f - height / 2f), this.Scale, buttonColor);
					return;
				}
			}
			else
			{
				spriteBatch.DrawString(this.Font, this.Text, new Vector2((float)dest.Left, (float)dest.Y), this.TextColor, 0f, Vector2.Zero, this.Scale, SpriteEffects.None, 0f);
				spriteBatch.Draw(this.UncheckedImage, new Vector2((float)dest.X + textSize.X + checkSpace, (float)dest.Y + offset + lineSpacing / 2f - height / 2f), this.Scale, buttonColor);
				if (this.Checked)
				{
					spriteBatch.Draw(this.CheckedImage, new Vector2((float)dest.X + textSize.X + checkSpace, (float)dest.Y + offset + lineSpacing / 2f - height / 2f), this.Scale, buttonColor);
				}
			}
		}

		public override void OnPressed()
		{
			this.Checked = !this.Checked;
			base.OnPressed();
		}

		private const int CheckSpace = 5;

		public new float Scale;
	}
}
