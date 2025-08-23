using System;
using System.Collections.Generic;
using DNA.Drawing.UI.Controls;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.Drawing.UI
{
	public class CreditsScreen : UIControlScreen
	{
		public CreditsScreen(SpriteFont titleFont, SpriteFont headerFont, SpriteFont normalFont, bool drawBehind)
			: base(drawBehind)
		{
			this._headerFont = headerFont;
			this._normalFont = normalFont;
			this._titleFont = titleFont;
		}

		public CreditsScreenItem AddCreditsItem(string name)
		{
			CreditsScreenItem item = new CreditsScreenItem(name);
			this.Items.Add(item);
			this.totalLength += this._normalFont.MeasureString(name).Y;
			return item;
		}

		public CreditsScreenItem AddCreditsItem(string name, ItemTypes itemType)
		{
			CreditsScreenItem item = new CreditsScreenItem(name, itemType);
			this.Items.Add(item);
			if (itemType == ItemTypes.Title)
			{
				this.totalLength += this._titleFont.MeasureString(name).Y;
			}
			else if (itemType == ItemTypes.Header)
			{
				this.totalLength += this._headerFont.MeasureString(name).Y;
			}
			else
			{
				this.totalLength += this._normalFont.MeasureString(name).Y;
			}
			return item;
		}

		protected override bool OnPlayerInput(InputManager input, GameController controller, KeyboardInput chatpad, GameTime gameTime)
		{
			if (controller.PressedButtons.B || controller.PressedButtons.Back || input.Keyboard.WasKeyPressed(Keys.Escape))
			{
				base.PopMe();
			}
			float scrollrateValue = ((input.Mouse.DeltaWheel != 0) ? ((float)(-(float)input.Mouse.DeltaWheel * 100)) : (controller.CurrentState.ThumbSticks.Left.Y * 200f));
			this.scrollRate = 40f + scrollrateValue;
			return base.OnPlayerInput(input, controller, chatpad, gameTime);
		}

		public override void OnPushed()
		{
			this._topItemDrawLocation = 720f;
			this.scrollRate = 40f;
			base.OnPushed();
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			Rectangle titleSafeArea = game.GraphicsDevice.Viewport.TitleSafeArea;
			if (this._topItemDrawLocation + this.totalLength < 0f)
			{
				this._topItemDrawLocation = -this.totalLength;
			}
			if (this._topItemDrawLocation > 720f)
			{
				this._topItemDrawLocation = 720f;
			}
			this._topItemDrawLocation -= (float)gameTime.ElapsedGameTime.TotalSeconds * this.scrollRate;
			base.Update(game, gameTime);
		}

		public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle titleSafe = device.Viewport.TitleSafeArea;
			Vector2 drawLoc = new Vector2(0f, this._topItemDrawLocation);
			spriteBatch.Begin();
			for (int i = 0; i < this.Items.Count; i++)
			{
				SpriteFont currentFont;
				if (this.Items[i].ItemType == ItemTypes.Title)
				{
					currentFont = this._titleFont;
				}
				else if (this.Items[i].ItemType == ItemTypes.Header)
				{
					currentFont = this._headerFont;
				}
				else
				{
					currentFont = this._normalFont;
				}
				Vector2 strSize = currentFont.MeasureString(this.Items[i].Name);
				if (drawLoc.Y > -50f && drawLoc.Y < 720f)
				{
					Color itemColor = ((this.Items[i].TextColor != null) ? this.Items[i].TextColor.Value : this.TextColor);
					if (this.LeftAligned)
					{
						drawLoc.X = (float)titleSafe.Left;
					}
					else
					{
						drawLoc.X = (float)titleSafe.Center.X - strSize.X / 2f;
					}
					spriteBatch.DrawOutlinedText(currentFont, this.Items[i].Name, drawLoc, itemColor, Color.Black, 1);
				}
				drawLoc.Y += strSize.Y;
			}
			spriteBatch.End();
			base.Draw(device, spriteBatch, gameTime);
		}

		private SpriteFont _headerFont;

		private SpriteFont _normalFont;

		private SpriteFont _titleFont;

		private List<CreditsScreenItem> Items = new List<CreditsScreenItem>();

		private float _topItemDrawLocation = 720f;

		public Color TextColor = Color.White;

		public bool LeftAligned = true;

		private float scrollRate = 40f;

		private float totalLength;
	}
}
