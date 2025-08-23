using System;
using System.Collections.Generic;
using DNA.Audio;
using DNA.Input;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.Drawing.UI
{
	public class LongListScreen : Screen
	{
		public List<LongListScreen.MenuItem> MenuItems
		{
			get
			{
				return this._menuItems;
			}
		}

		public LongListScreen.MenuItem SelectedItem
		{
			get
			{
				if (this._selectedIndex < 0 || this._selectedIndex >= this._menuItems.Count)
				{
					return null;
				}
				return this._menuItems[this._selectedIndex];
			}
		}

		public LongListScreen(bool drawBehind)
			: base(true, drawBehind)
		{
		}

		public event EventHandler<SelectedEventArgs> Clicked;

		protected virtual void OnClicked(LongListScreen.MenuItem selectedItem)
		{
		}

		public void Click()
		{
			this.OnClicked(this.SelectedItem);
			if (this.Clicked != null)
			{
				this.Clicked(this, new SelectedEventArgs(this.SelectedItem.Tag));
			}
			if (this.ClickSound != null)
			{
				SoundManager.Instance.PlayInstance(this.ClickSound);
			}
		}

		public event EventHandler<SelectedEventArgs> BackClicked;

		protected virtual void OnBack()
		{
		}

		public void Back()
		{
			this.OnBack();
			if (this.BackClicked != null)
			{
				this.BackClicked(this, null);
			}
			if (this.ClickSound != null)
			{
				SoundManager.Instance.PlayInstance(this.ClickSound);
			}
		}

		public int HitTest(Point p)
		{
			for (int i = 0; i < this._itemLocations.Length; i++)
			{
				if (this._itemLocations[i].Contains(p))
				{
					return i;
				}
			}
			return -1;
		}

		protected override bool OnPlayerInput(InputManager input, GameController controller, KeyboardInput chatpad, GameTime gameTime)
		{
			bool isXbox = false;
			bool selectup = false;
			bool selectdown = false;
			float sectionHeight = (float)(this.viewPort.Bounds.Height / 3);
			float mouseDownPos = (float)this.viewPort.Bounds.Bottom - sectionHeight;
			float mouseUpPos = sectionHeight;
			float mouseAccelDownPos = (float)this.viewPort.Bounds.Bottom - sectionHeight * 0.2f;
			float mouseAccelUpPos = sectionHeight * 0.2f;
			if (controller.CurrentState.ThumbSticks.Left.Y < -0.25f || controller.CurrentState.DPad.Down == ButtonState.Pressed || controller.CurrentState.Triggers.Right > 0.25f || input.Keyboard.IsKeyDown(Keys.Down) || (!isXbox && (float)input.Mouse.Position.Y > mouseDownPos && !this.hitTestTrue && input.Mouse.Position.X < this.viewPort.Bounds.Center.X) || (!isXbox && input.Mouse.Position.Y > this.destRect.Bottom && input.Mouse.Position.X < this.viewPort.Bounds.Center.X))
			{
				selectdown = true;
				if (!this.lastselectdown)
				{
					this.accelerateDelayTimer.Reset();
					this.accelerateTimer.Reset();
				}
			}
			if (controller.CurrentState.ThumbSticks.Left.Y > 0.25f || controller.CurrentState.DPad.Up == ButtonState.Pressed || controller.CurrentState.Triggers.Left > 0.25f || input.Keyboard.IsKeyDown(Keys.Up) || (!isXbox && (float)input.Mouse.Position.Y < mouseUpPos && !this.hitTestTrue))
			{
				selectup = true;
				if (!this.lastselectup)
				{
					this.accelerateDelayTimer.Reset();
					this.accelerateTimer.Reset();
				}
			}
			if (Math.Abs(controller.CurrentState.ThumbSticks.Left.Y) > 0.8f || (!isXbox && (float)input.Mouse.Position.Y < mouseAccelUpPos) || (!isXbox && (float)input.Mouse.Position.Y > mouseAccelDownPos))
			{
				this.accelerateDelayTimer.Update(gameTime.ElapsedGameTime);
			}
			if (this.accelerateDelayTimer.Expired)
			{
				this.accelerateTimer.Update(gameTime.ElapsedGameTime);
			}
			float scrolltimerTime = this.accelerateTimer.PercentComplete * (float)this.scrollRate.TotalSeconds / 2f;
			float absofStick = Math.Abs(controller.CurrentState.ThumbSticks.Left.Y);
			float mouseValue;
			if (!isXbox && (float)input.Mouse.Position.Y < mouseUpPos && !this.hitTestTrue)
			{
				mouseValue = (mouseUpPos - (float)input.Mouse.Position.Y) / sectionHeight;
			}
			else if (!isXbox && (float)input.Mouse.Position.Y > mouseDownPos && !this.hitTestTrue)
			{
				mouseValue = ((float)input.Mouse.Position.Y - mouseDownPos) / sectionHeight;
			}
			else
			{
				mouseValue = 0f;
			}
			float maxOfStickandMouse = Math.Max(absofStick, mouseValue);
			if (maxOfStickandMouse > 0f)
			{
				this.scrollTimer.MaxTime = TimeSpan.FromSeconds(this.scrollRate.TotalSeconds / (double)maxOfStickandMouse - (double)scrolltimerTime);
			}
			else
			{
				this.scrollTimer.MaxTime = TimeSpan.FromSeconds(0.1);
			}
			if ((controller.CurrentState.ThumbSticks.Left.Y < -0.25f && controller.LastState.ThumbSticks.Left.Y > -0.25f) || (controller.PressedDPad.Down || (controller.CurrentState.Triggers.Right > 0.25f && controller.LastState.Triggers.Right < 0.25f)) || (input.Keyboard.WasKeyPressed(Keys.Down) || (!isXbox && (float)input.Mouse.Position.Y > mouseDownPos && (float)input.Mouse.LastPosition.Y <= mouseDownPos && !this.hitTestTrue)) || input.Mouse.DeltaWheel < 0)
			{
				if (this._middleIndex < this._menuItems.Count - 1 && this.SelectSound != null && input.Mouse.DeltaWheel < 0)
				{
					SoundManager.Instance.PlayInstance(this.SelectSound);
				}
				this._middleIndex++;
			}
			if ((controller.CurrentState.ThumbSticks.Left.Y > 0.25f && controller.LastState.ThumbSticks.Left.Y < 0.25f) || (controller.PressedDPad.Up || (controller.CurrentState.Triggers.Left > 0.25f && controller.LastState.Triggers.Left < 0.25f)) || (input.Keyboard.WasKeyPressed(Keys.Up) || (!isXbox && (float)input.Mouse.Position.Y < mouseUpPos && (float)input.Mouse.LastPosition.Y >= mouseUpPos && !this.hitTestTrue)) || input.Mouse.DeltaWheel > 0)
			{
				if (this._middleIndex > 0 && this.SelectSound != null && input.Mouse.DeltaWheel > 0)
				{
					SoundManager.Instance.PlayInstance(this.SelectSound);
				}
				this._middleIndex--;
			}
			if (selectdown)
			{
				this.holdTimer.Update(gameTime.ElapsedGameTime);
				if (this.holdTimer.Expired)
				{
					this.scrollTimer.Update(gameTime.ElapsedGameTime);
					if (this.scrollTimer.Expired)
					{
						this.scrollTimer.Reset();
						if (this.accelerateTimer.Expired)
						{
							this._middleIndex += 10;
						}
						else
						{
							this._middleIndex++;
						}
						if (this._middleIndex < this._menuItems.Count && this.SelectSound != null)
						{
							SoundManager.Instance.PlayInstance(this.SelectSound);
						}
					}
				}
			}
			else if (selectup)
			{
				this.holdTimer.Update(gameTime.ElapsedGameTime);
				if (this.holdTimer.Expired)
				{
					this.scrollTimer.Update(gameTime.ElapsedGameTime);
					if (this.scrollTimer.Expired)
					{
						this.scrollTimer.Reset();
						if (this.accelerateTimer.Expired)
						{
							this._middleIndex -= 10;
						}
						else
						{
							this._middleIndex--;
						}
						if (this._middleIndex >= 0 && this.SelectSound != null)
						{
							SoundManager.Instance.PlayInstance(this.SelectSound);
						}
					}
				}
			}
			if (this._middleIndex < 0)
			{
				this._middleIndex = 0;
			}
			if (this._middleIndex >= this._menuItems.Count)
			{
				this._middleIndex = this._menuItems.Count - 1;
			}
			if (controller.PressedButtons.A || controller.PressedButtons.Start || input.Keyboard.WasKeyPressed(Keys.Enter))
			{
				if (this.MenuItems.Count > 0)
				{
					this.Click();
				}
				else
				{
					this.Back();
				}
			}
			else if (input.Mouse.LeftButtonPressed)
			{
				if (this.MenuItems.Count > 0)
				{
					if (this.HitTest(input.Mouse.Position) >= 0)
					{
						this.Click();
					}
				}
				else
				{
					this.Back();
				}
			}
			if (controller.PressedButtons.B || controller.PressedButtons.Back || input.Keyboard.WasKeyPressed(Keys.Escape))
			{
				this.Back();
			}
			base.OnPlayerInput(input, controller, chatpad, gameTime);
			if ((!selectdown && this.lastselectdown) || (!selectup && this.lastselectup))
			{
				this.holdTimer.Reset();
			}
			this.lastselectdown = selectdown;
			this.lastselectup = selectup;
			int hoverItem = this.HitTest(input.Mouse.Position);
			if (hoverItem >= 0)
			{
				this.hitTestTrue = true;
				if (this._selectedIndex != hoverItem)
				{
					if (this.SelectSound != null && !selectdown && !selectup)
					{
						SoundManager.Instance.PlayInstance(this.SelectSound);
					}
					this._selectedIndex = hoverItem;
				}
			}
			else
			{
				this.hitTestTrue = false;
				if (this._selectedIndex != this._middleIndex)
				{
					if (this.SelectSound != null)
					{
						SoundManager.Instance.PlayInstance(this.SelectSound);
					}
					this._selectedIndex = this._middleIndex;
				}
			}
			return base.OnPlayerInput(input, controller, chatpad, gameTime);
		}

		private void _drawPreviousItems(int middleIndex, float startYPos, GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			int itemIndex = middleIndex - 1;
			if (itemIndex < 0 || itemIndex >= this._menuItems.Count)
			{
				return;
			}
			float ypos = startYPos;
			while (itemIndex >= 0)
			{
				LongListScreen.MenuItem item = this._menuItems[itemIndex];
				if (itemIndex == this._selectedIndex)
				{
					item.Selected = true;
				}
				else
				{
					item.Selected = false;
				}
				Vector2 itemSize = item.Measure();
				ypos -= itemSize.Y;
				if (ypos + itemSize.Y < (float)this.destRect.Top)
				{
					IL_00CF:
					while (itemIndex >= 0)
					{
						this._setItemLocation(itemIndex, Rectangle.Empty);
						itemIndex--;
					}
					return;
				}
				this._setItemLocation(itemIndex, new Rectangle(this.destRect.Left, (int)ypos, (int)itemSize.X, (int)itemSize.Y));
				item.Draw(device, spriteBatch, gameTime, new Vector2((float)this.destRect.Left, ypos));
				itemIndex--;
			}
			goto IL_00CF;
		}

		private void _drawPostItems(int middleIndex, float startYPos, GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			int itemIndex = middleIndex + 1;
			if (itemIndex < 0 || itemIndex >= this._menuItems.Count)
			{
				return;
			}
			float ypos = startYPos;
			while (itemIndex < this._menuItems.Count)
			{
				LongListScreen.MenuItem item = this._menuItems[itemIndex];
				if (itemIndex == this._selectedIndex)
				{
					item.Selected = true;
				}
				else
				{
					item.Selected = false;
				}
				Vector2 itemSize = item.Measure();
				if (ypos > (float)this.viewPort.Bounds.Bottom)
				{
					IL_00DA:
					while (itemIndex < this._menuItems.Count)
					{
						this._setItemLocation(itemIndex, Rectangle.Empty);
						itemIndex++;
					}
					return;
				}
				this._setItemLocation(itemIndex, new Rectangle(this.destRect.Left, (int)ypos, (int)itemSize.X, (int)itemSize.Y));
				item.Draw(device, spriteBatch, gameTime, new Vector2((float)this.destRect.Left, ypos));
				itemIndex++;
				ypos += itemSize.Y;
			}
			goto IL_00DA;
		}

		private void _drawSelectedItem(int middleIndex, float startYPos, GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (middleIndex < 0 || middleIndex >= this._menuItems.Count)
			{
				return;
			}
			LongListScreen.MenuItem item = this._menuItems[middleIndex];
			Vector2 selectedItemSize = item.Measure();
			item.Draw(device, spriteBatch, gameTime, new Vector2((float)this.destRect.Left, startYPos));
			this._setItemLocation(middleIndex, new Rectangle(this.destRect.Left, (int)startYPos, (int)selectedItemSize.X, (int)selectedItemSize.Y));
		}

		private void _setItemLocation(int index, Rectangle location)
		{
			try
			{
				this._itemLocations[index] = location;
			}
			catch
			{
			}
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (this._itemLocations.Length != this._menuItems.Count)
			{
				this._itemLocations = new Rectangle[this._menuItems.Count];
			}
			this.viewPort = device.Viewport;
			int middleIndex = this._middleIndex;
			if (middleIndex < 0 || middleIndex >= this._menuItems.Count)
			{
				return;
			}
			LongListScreen.MenuItem item = this._menuItems[middleIndex];
			if (middleIndex == this._selectedIndex)
			{
				item.Selected = true;
			}
			else
			{
				item.Selected = false;
			}
			Vector2 selectedItemSize = item.Measure();
			float startYPos = (float)this.destRect.Center.Y - selectedItemSize.Y / 2f;
			spriteBatch.Begin();
			this._drawSelectedItem(middleIndex, startYPos, device, spriteBatch, gameTime);
			this._drawPreviousItems(middleIndex, startYPos, device, spriteBatch, gameTime);
			this._drawPostItems(middleIndex, startYPos + selectedItemSize.Y, device, spriteBatch, gameTime);
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		public const float deadZone = 0.25f;

		private List<LongListScreen.MenuItem> _menuItems = new List<LongListScreen.MenuItem>();

		protected int _middleIndex;

		protected int _selectedIndex;

		private OneShotTimer holdTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private OneShotTimer scrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.10000000149011612));

		private OneShotTimer accelerateDelayTimer = new OneShotTimer(TimeSpan.FromSeconds(3.0));

		private OneShotTimer accelerateTimer = new OneShotTimer(TimeSpan.FromSeconds(5.0));

		private TimeSpan scrollRate = TimeSpan.FromSeconds(0.05000000074505806);

		private bool lastselectup;

		private bool lastselectdown;

		public string SelectSound;

		public string ClickSound;

		private Rectangle[] _itemLocations = new Rectangle[0];

		public Rectangle destRect;

		private Viewport viewPort = default(Viewport);

		private bool hitTestTrue;

		public abstract class MenuItem
		{
			public MenuItem(object tag)
			{
				this.Tag = tag;
			}

			public abstract void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime, Vector2 pos);

			public abstract Vector2 Measure();

			public object Tag;

			public bool Selected;
		}

		public class TextItem : LongListScreen.MenuItem
		{
			public TextItem(string text, SpriteFont font, object tag)
				: base(tag)
			{
				this.Text = text;
				this.Font = font;
			}

			public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime, Vector2 pos)
			{
				Color color = this.Color;
				if (this.Selected)
				{
					this.flashTimer.Update(gameTime.ElapsedGameTime);
					if (this.flashTimer.Expired)
					{
						this.flashTimer.Reset();
						this.selectedDirection = !this.selectedDirection;
					}
					if (this.selectedDirection)
					{
						color = Color.Lerp(this.Color, this.SelectedColor, this.flashTimer.PercentComplete);
					}
					else
					{
						color = Color.Lerp(this.SelectedColor, this.Color, this.flashTimer.PercentComplete);
					}
				}
				spriteBatch.DrawOutlinedText(this.Font, this.Text, pos, color, Color.Black, 1);
			}

			public override Vector2 Measure()
			{
				return this.Font.MeasureString(this.Text);
			}

			public string Text;

			public SpriteFont Font;

			public Color Color = Color.White;

			public Color SelectedColor = Color.Red;

			private OneShotTimer flashTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

			private bool selectedDirection;
		}
	}
}
