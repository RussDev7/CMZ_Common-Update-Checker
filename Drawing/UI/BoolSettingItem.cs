using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI
{
	public class BoolSettingItem : SettingItemElement
	{
		public BoolSettingItem(string text, bool isOn)
			: base(text)
		{
			this.TextValues[0] = CommonResources.On;
			this.TextValues[1] = CommonResources.Off;
			this.On = isOn;
		}

		public BoolSettingItem(string text, bool isOn, string onText, string offText)
			: base(text)
		{
			this.TextValues[0] = onText;
			this.TextValues[1] = offText;
			this.On = isOn;
		}

		public override void OnClicked()
		{
			this.On = !this.On;
			base.OnClicked();
		}

		public override void Decreased()
		{
			this.OnClicked();
		}

		public override void Increased()
		{
			this.OnClicked();
		}

		public override bool ChangeValue(TimeSpan elapsedGameTime)
		{
			return false;
		}

		public override void OnDraw(DNAGame _game, GraphicsDevice device, SpriteBatch spriteBatch, SpriteFont font, Color textColor, Color outlineColor, int outlineWidth, Vector2 loc)
		{
			Rectangle titleSafe = device.Viewport.TitleSafeArea;
			Vector2 size = font.MeasureString(">");
			float TextWidth = (float)(titleSafe.Right - titleSafe.Center.X - 120) - size.X * 2f;
			float bracketLocation = (float)(titleSafe.Center.X + 50);
			string text = (this.On ? this.TextValues[0] : this.TextValues[1]);
			float textLoc = bracketLocation + size.X + 10f + TextWidth / 2f - font.MeasureString(text).X / 2f;
			spriteBatch.DrawOutlinedText(font, text, new Vector2(textLoc, loc.Y), textColor, outlineColor, outlineWidth);
			base.OnDraw(_game, device, spriteBatch, font, textColor, outlineColor, outlineWidth, loc);
		}

		private string[] TextValues = new string[2];

		public bool On;
	}
}
