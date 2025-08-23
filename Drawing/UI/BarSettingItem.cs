using System;
using DNA.Input;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI
{
	public class BarSettingItem : SettingItemElement
	{
		public BarSettingItem(string text, float defaultValue)
			: base(text)
		{
			this.changeValueTimer = new OneShotTimer(TimeSpan.FromSeconds(0.1));
			this.Value = defaultValue;
		}

		public bool SetBarValue(MouseInput mouse)
		{
			if (this._location.Contains(mouse.Position) && mouse.LeftButtonDown)
			{
				float loc = (float)(mouse.Position.X - this._location.X);
				this.Value = loc / (float)this._location.Width;
				return true;
			}
			return false;
		}

		public override void OnClicked()
		{
			if ((double)this.Value < 0.25)
			{
				this.Value = 0.25f;
			}
			else if ((double)this.Value < 0.5)
			{
				this.Value = 0.5f;
			}
			else if ((double)this.Value < 0.75)
			{
				this.Value = 0.75f;
			}
			else if (this.Value < 1f)
			{
				this.Value = 1f;
			}
			else
			{
				this.Value = 0f;
			}
			base.OnClicked();
		}

		public override void Increased()
		{
			this.Value += 0.05f;
			if (this.Value > 1f)
			{
				this.Value = 1f;
			}
		}

		public override void Decreased()
		{
			this.Value -= 0.05f;
			if (this.Value < 0f)
			{
				this.Value = 0f;
			}
		}

		public override void OnDraw(DNAGame _game, GraphicsDevice device, SpriteBatch spriteBatch, SpriteFont font, Color textColor, Color outlineColor, int outlineWidth, Vector2 loc)
		{
			Rectangle titleSafe = Screen.Adjuster.ScreenRect;
			Vector2 size = font.MeasureString(">");
			float barWidth = (float)(titleSafe.Right - titleSafe.Center.X - 50) - size.X * 2f;
			float barHeight = size.Y / 2f;
			float barYloc = loc.Y + (size.Y - barHeight) / 2f - 3f;
			spriteBatch.DrawOutlinedText(font, "<", new Vector2((float)(titleSafe.Center.X + 15), loc.Y), textColor, outlineColor, outlineWidth);
			spriteBatch.DrawOutlinedText(font, ">", new Vector2((float)(titleSafe.Right - 15) - size.X, loc.Y), textColor, outlineColor, outlineWidth);
			this._location = new Rectangle((int)((float)(titleSafe.Center.X + 25) + size.X), (int)barYloc, (int)barWidth, (int)barHeight);
			spriteBatch.Draw(_game.DummyTexture, new Rectangle((int)((float)(titleSafe.Center.X + 25) + size.X), (int)barYloc, (int)barWidth, (int)barHeight), Color.Black);
			spriteBatch.Draw(_game.DummyTexture, new Rectangle((int)((float)(titleSafe.Center.X + 26) + size.X), (int)barYloc + 1, (int)((barWidth - 2f) * this.Value), (int)(barHeight - 2f)), Color.White);
			base.OnDraw(_game, device, spriteBatch, font, textColor, outlineColor, outlineWidth, loc);
		}

		public float Value;

		private Rectangle _location = Rectangle.Empty;
	}
}
