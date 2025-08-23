using System;
using System.Collections.Generic;
using DNA.Audio;
using DNA.Drawing.UI.Controls;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.Drawing.UI
{
	public class SettingScreen : UIControlScreen
	{
		public List<SettingItemElement> MenuItems
		{
			get
			{
				return this._menuItems;
			}
		}

		public int SelectedIndex
		{
			get
			{
				return this._selectedIndex;
			}
			set
			{
				this._selectedIndex = value;
			}
		}

		public SettingScreen(DNAGame game, SpriteFont font, bool drawBehind)
			: base(drawBehind)
		{
			this.Font = font;
			this._game = game;
			this.TextColor = Color.White;
			this.SelectedColor = Color.Red;
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

		public SettingScreen(DNAGame game, SpriteFont font, Color textColor, Color selectedColor, bool drawBehind)
			: base(drawBehind)
		{
			this.TextColor = textColor;
			this.SelectedColor = selectedColor;
			this.Font = font;
			this._game = game;
		}

		protected override bool OnPlayerInput(InputManager input, GameController controller, KeyboardInput chatpad, GameTime gameTime)
		{
			int hoverItem = this.HitTest(input.Mouse.Position);
			if (!this._mouseActive)
			{
				hoverItem = -1;
			}
			if (input.Mouse.DeltaPosition != Vector2.Zero)
			{
				this._mouseActive = true;
			}
			bool clickItem = true;
			if (hoverItem >= 0)
			{
				if (this._menuItems[hoverItem].Visible)
				{
					if (this._selectedIndex != hoverItem)
					{
						if (this.SelectSound != null)
						{
							SoundManager.Instance.PlayInstance(this.SelectSound);
						}
						this._selectedIndex = hoverItem;
					}
					BarSettingItem barItem = this._menuItems[hoverItem] as BarSettingItem;
					if (barItem != null)
					{
						clickItem = !barItem.SetBarValue(input.Mouse);
					}
				}
			}
			float deadZone = 0.25f;
			if (controller.PressedDPad.Down || controller.PressedButtons.RightShoulder || (controller.CurrentState.ThumbSticks.Left.Y < -deadZone && controller.LastState.ThumbSticks.Left.Y > -deadZone) || (controller.CurrentState.ThumbSticks.Right.Y < -deadZone && controller.LastState.ThumbSticks.Right.Y > -deadZone) || input.Keyboard.WasKeyPressed(Keys.Down))
			{
				if (this.SelectSound != null)
				{
					SoundManager.Instance.PlayInstance(this.SelectSound);
				}
				this.SelectNext();
			}
			if (controller.PressedDPad.Up || controller.PressedButtons.LeftShoulder || (controller.CurrentState.ThumbSticks.Left.Y > deadZone && controller.LastState.ThumbSticks.Left.Y < deadZone) || (controller.CurrentState.ThumbSticks.Right.Y > deadZone && controller.LastState.ThumbSticks.Right.Y < deadZone) || input.Keyboard.WasKeyPressed(Keys.Up))
			{
				if (this.SelectSound != null)
				{
					SoundManager.Instance.PlayInstance(this.SelectSound);
				}
				this.SelectPrevious();
			}
			if (controller.PressedButtons.B || controller.PressedButtons.Back || input.Keyboard.WasKeyPressed(Keys.Escape))
			{
				if (this.ClickSound != null)
				{
					SoundManager.Instance.PlayInstance(this.ClickSound);
				}
				base.PopMe();
			}
			if (controller.PressedButtons.A || input.Keyboard.WasKeyPressed(Keys.Enter) || (input.Mouse.LeftButtonPressed && this.HitTest(input.Mouse.Position) >= 0))
			{
				if (this.ClickSound != null)
				{
					SoundManager.Instance.PlayInstance(this.ClickSound);
				}
				if (clickItem)
				{
					this.ClickedMenuItem();
				}
			}
			if (controller.PressedDPad.Left || (controller.CurrentState.ThumbSticks.Left.X < -deadZone && controller.LastState.ThumbSticks.Left.X > -deadZone) || (controller.CurrentState.ThumbSticks.Right.X < -deadZone && controller.LastState.ThumbSticks.Right.X > -deadZone) || input.Keyboard.WasKeyPressed(Keys.Left) || (input.Mouse.DeltaWheel < 0 && this.HitTest(input.Mouse.Position) >= 0))
			{
				this.DecreasedMenuItem();
				this._menuItems[this._selectedIndex].ResetTimer();
			}
			else if ((controller.CurrentState.DPad.Left == ButtonState.Pressed || controller.CurrentState.ThumbSticks.Left.X < -deadZone || controller.CurrentState.ThumbSticks.Right.X < -deadZone || input.Keyboard.IsKeyDown(Keys.Left)) && this._menuItems[this._selectedIndex].ChangeValue(gameTime.ElapsedGameTime))
			{
				this.DecreasedMenuItem();
			}
			if (controller.PressedDPad.Right || (controller.CurrentState.ThumbSticks.Left.X > deadZone && controller.LastState.ThumbSticks.Left.X < deadZone) || (controller.CurrentState.ThumbSticks.Right.X > deadZone && controller.LastState.ThumbSticks.Right.X < deadZone) || input.Keyboard.WasKeyPressed(Keys.Right) || (input.Mouse.DeltaWheel > 0 && this.HitTest(input.Mouse.Position) >= 0))
			{
				this.IncreasedMenuItem();
				this._menuItems[this._selectedIndex].ResetTimer();
			}
			else if ((controller.CurrentState.DPad.Right == ButtonState.Pressed || controller.CurrentState.ThumbSticks.Left.X > deadZone || controller.CurrentState.ThumbSticks.Right.X > deadZone || input.Keyboard.IsKeyDown(Keys.Right)) && this._menuItems[this._selectedIndex].ChangeValue(gameTime.ElapsedGameTime))
			{
				this.IncreasedMenuItem();
			}
			return base.OnPlayerInput(input, controller, chatpad, gameTime);
		}

		private void ClickedMenuItem()
		{
			if (this._selectedIndex >= 0)
			{
				SettingItemElement selectedControl = this._menuItems[this._selectedIndex];
				selectedControl.OnClicked();
			}
		}

		private void IncreasedMenuItem()
		{
			if (this._selectedIndex >= 0)
			{
				SettingItemElement selectedControl = this._menuItems[this._selectedIndex];
				selectedControl.Increased();
			}
		}

		private void DecreasedMenuItem()
		{
			if (this._selectedIndex >= 0)
			{
				SettingItemElement selectedControl = this._menuItems[this._selectedIndex];
				selectedControl.Decreased();
			}
		}

		private void SelectFirst()
		{
			if (this._menuItems.Count == 0)
			{
				this._selectedIndex = -1;
				return;
			}
			this._selectedIndex = 0;
		}

		private void SelectNext()
		{
			if (this._menuItems.Count == 0)
			{
				this._selectedIndex = -1;
				return;
			}
			int startIndex = this._selectedIndex;
			for (;;)
			{
				this._selectedIndex++;
				if (this._selectedIndex >= this._menuItems.Count)
				{
					this._selectedIndex = 0;
				}
				if (this._selectedIndex == startIndex && !this._menuItems[this._selectedIndex].Visible)
				{
					break;
				}
				if (this._menuItems[this._selectedIndex].Visible)
				{
					return;
				}
			}
			this._selectedIndex = -1;
		}

		private void SelectPrevious()
		{
			if (this._menuItems.Count == 0)
			{
				this._selectedIndex = -1;
				return;
			}
			int startIndex = this._selectedIndex;
			for (;;)
			{
				this._selectedIndex--;
				if (this._selectedIndex < 0)
				{
					this._selectedIndex = this._menuItems.Count - 1;
				}
				if (this._selectedIndex == startIndex && !this._menuItems[this._selectedIndex].Visible)
				{
					break;
				}
				if (this._menuItems[this._selectedIndex].Visible)
				{
					return;
				}
			}
			this._selectedIndex = -1;
		}

		private Vector2 MeasureItem(SettingItemElement item)
		{
			SpriteFont font = ((item.Font == null) ? this.Font : item.Font);
			return font.MeasureString(item.Text);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (this._itemLocations.Length != this._menuItems.Count)
			{
				this._itemLocations = new Rectangle[this._menuItems.Count];
			}
			while (this._selectedIndex >= this._menuItems.Count || !this.MenuItems[this._selectedIndex].Visible)
			{
				this._selectedIndex++;
				this._selectedIndex %= this._menuItems.Count;
			}
			spriteBatch.Begin();
			float totalHeight = 0f;
			float spacing = 0f;
			if (this.LineSpacing != null)
			{
				spacing = (float)this.LineSpacing.Value;
			}
			for (int i = 0; i < this._menuItems.Count; i++)
			{
				if (this._menuItems[i].Visible)
				{
					totalHeight += this.MeasureItem(this._menuItems[i]).Y + spacing;
				}
			}
			if (this.DrawArea == null)
			{
				this.DrawArea = new Rectangle?(device.Viewport.TitleSafeArea);
			}
			totalHeight -= spacing;
			float screenHeight = (float)this.DrawArea.Value.Height;
			int width = this.DrawArea.Value.Width;
			float yloc = (screenHeight - totalHeight) / 2f + (float)this.DrawArea.Value.Y;
			if (this.MenuStart != null)
			{
				yloc = this.MenuStart.Value;
			}
			if (yloc < (float)this.DrawArea.Value.Top)
			{
				yloc = (float)this.DrawArea.Value.Top;
			}
			for (int j = 0; j < this._menuItems.Count; j++)
			{
				SettingItemElement item = this._menuItems[j];
				if (item.Visible)
				{
					SpriteFont font = ((item.Font == null) ? this.Font : item.Font);
					Color textColor = ((item.TextColor != null) ? item.TextColor.Value : this.TextColor);
					Color outlineColor = ((item.OutlineColor != null) ? item.OutlineColor.Value : this.OutlineColor);
					int outlineWidth = ((item.OnlineWidth != null) ? item.OnlineWidth.Value : this.OnlineWidth);
					Vector2 size = this.MeasureItem(item);
					if (j == this._selectedIndex)
					{
						textColor = ((item.SelectedColor != null) ? item.SelectedColor.Value : this.SelectedColor);
					}
					this._itemLocations[j] = new Rectangle(this.DrawArea.Value.Left, (int)yloc, this.DrawArea.Value.Width, (int)size.Y);
					item.OnDraw(this._game, device, spriteBatch, font, textColor, outlineColor, outlineWidth, new Vector2((float)this.DrawArea.Value.Left, yloc));
					yloc += size.Y + spacing;
				}
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		public string SelectSound;

		public string ClickSound;

		private List<SettingItemElement> _menuItems = new List<SettingItemElement>();

		private int _selectedIndex;

		public float? MenuStart = null;

		private Rectangle[] _itemLocations = new Rectangle[0];

		public int? LineSpacing;

		public SpriteFont Font;

		public Color TextColor;

		public Color SelectedColor;

		public Color OutlineColor = Color.Black;

		public int OnlineWidth = 2;

		private DNAGame _game;

		public Rectangle? DrawArea = null;
	}
}
