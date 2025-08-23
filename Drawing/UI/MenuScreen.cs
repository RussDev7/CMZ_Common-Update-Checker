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
	public class MenuScreen : Screen
	{
		public List<MenuItemElement> MenuItems
		{
			get
			{
				return this._menuItems;
			}
		}

		public TimeSpan FlashTime
		{
			get
			{
				return this._flashTimer.MaxTime;
			}
			set
			{
				this._flashTimer.MaxTime = value;
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

		public MenuItemElement SelectedMenuItem
		{
			get
			{
				return this.MenuItems[this.SelectedIndex];
			}
		}

		public MenuItemElement AddMenuItem(string text, object tag)
		{
			MenuItemElement element = new MenuItemElement(text, tag);
			this.MenuItems.Add(element);
			return element;
		}

		public MenuItemElement AddMenuItem(string text, string description, object tag)
		{
			MenuItemElement element = new MenuItemElement(text, description, tag);
			this.MenuItems.Add(element);
			return element;
		}

		public MenuScreen(SpriteFont font, bool drawBehind)
			: base(true, drawBehind)
		{
			this.Font = font;
		}

		public MenuScreen(SpriteFont font, Color textColor, Color selectedColor, bool drawBehind)
			: base(true, drawBehind)
		{
			this.TextColor = textColor;
			this.SelectedColor = selectedColor;
			this.Font = font;
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
				}
				else
				{
					hoverItem = -1;
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
			if (controller.PressedButtons.A || controller.PressedButtons.Start || input.Keyboard.WasKeyPressed(Keys.Enter) || (input.Mouse.LeftButtonPressed && hoverItem >= 0))
			{
				if (this.ClickSound != null)
				{
					SoundManager.Instance.PlayInstance(this.ClickSound);
				}
				this.SelectMenuItem();
			}
			return base.OnPlayerInput(input, controller, chatpad, gameTime);
		}

		public event EventHandler<SelectedMenuItemArgs> MenuItemSelected;

		protected virtual void OnMenuItemSelected(MenuItemElement selectedControl)
		{
		}

		protected virtual void OnMenuItemFocus(MenuItemElement selectedControl)
		{
		}

		private void SelectMenuItem()
		{
			if (this._selectedIndex >= 0)
			{
				MenuItemElement selectedControl = this._menuItems[this._selectedIndex];
				this.OnMenuItemSelected(selectedControl);
				if (this.MenuItemSelected != null)
				{
					this.MenuItemSelected(this, new SelectedMenuItemArgs(selectedControl));
				}
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

		private Vector2 MeasureItem(MenuItemElement item)
		{
			SpriteFont font = ((item.Font == null) ? this.Font : item.Font);
			return font.MeasureString(item.Text) * Screen.Adjuster.ScaleFactor.Y;
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (this._lastSelectedItem != this.SelectedMenuItem)
			{
				this._lastSelectedItem = this.SelectedMenuItem;
				this.OnMenuItemFocus(this._lastSelectedItem);
			}
			base.OnUpdate(game, gameTime);
		}

		public int HitTest(Point p)
		{
			for (int i = 0; i < this._itemLocations.Length; i++)
			{
				if (this._itemLocations[i].Contains(p) && this._menuItems[i].Visible)
				{
					return i;
				}
			}
			return -1;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (this._itemLocations.Length != this._menuItems.Count)
			{
				this._itemLocations = new Rectangle[this._menuItems.Count];
			}
			this._flashTimer.Update(gameTime.ElapsedGameTime);
			if (this._flashTimer.Expired)
			{
				this._flashTimer.Reset();
				this._flashDir = !this._flashDir;
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
				spacing = (float)this.LineSpacing.Value * Screen.Adjuster.ScaleFactor.Y;
			}
			for (int i = 0; i < this._menuItems.Count; i++)
			{
				if (this._menuItems[i].Visible)
				{
					totalHeight += this.MeasureItem(this._menuItems[i]).Y + spacing;
				}
			}
			totalHeight -= spacing;
			Rectangle drawArea = Screen.Adjuster.ScreenRect;
			if (this.DrawArea != null)
			{
				drawArea = this.DrawArea.Value;
			}
			float screenHeight = (float)drawArea.Height;
			float screenWidth = (float)drawArea.Width;
			float yloc = (screenHeight - totalHeight) / 2f + (float)drawArea.Y;
			if (this.VerticalAlignment == MenuScreen.VerticalAlignmentTypes.Top)
			{
				yloc = (float)drawArea.Top;
			}
			else if (this.VerticalAlignment == MenuScreen.VerticalAlignmentTypes.Bottom)
			{
				yloc = (float)drawArea.Bottom - totalHeight;
			}
			for (int j = 0; j < this._menuItems.Count; j++)
			{
				MenuItemElement item = this._menuItems[j];
				if (item.Visible)
				{
					SpriteFont font = ((item.Font == null) ? this.Font : item.Font);
					Color textColor = ((item.TextColor != null) ? item.TextColor.Value : this.TextColor);
					Color outlneColor = ((item.OutlineColor != null) ? item.OutlineColor.Value : this.OutlineColor);
					int outlineWidth = ((item.OnlineWidth != null) ? item.OnlineWidth.Value : this.OutlineWidth);
					Vector2 size = this.MeasureItem(item);
					Vector2 loc = new Vector2((float)drawArea.Left, yloc);
					if (this.HorizontalAlignment == MenuScreen.HorizontalAlignmentTypes.Center)
					{
						loc.X = (float)drawArea.X + (screenWidth - size.X) / 2f;
					}
					else if (this.HorizontalAlignment == MenuScreen.HorizontalAlignmentTypes.Right)
					{
						loc.X = (float)drawArea.Right - size.X;
					}
					yloc += size.Y + spacing;
					Color color = textColor;
					if (j == this._selectedIndex)
					{
						Color seletedColor = ((item.SelectedColor != null) ? item.SelectedColor.Value : this.SelectedColor);
						float blender = (this._flashDir ? this._flashTimer.PercentComplete : (1f - this._flashTimer.PercentComplete));
						color = Color.Lerp(textColor, seletedColor, blender);
					}
					this._itemLocations[j] = new Rectangle((int)loc.X, (int)loc.Y, (int)size.X, (int)size.Y);
					spriteBatch.DrawOutlinedText(font, item.Text, loc, color, outlneColor, (int)Math.Ceiling((double)((float)outlineWidth * Screen.Adjuster.ScaleFactor.Y)), Screen.Adjuster.ScaleFactor.Y, 0f, Vector2.Zero);
				}
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		private bool _flashDir;

		private OneShotTimer _flashTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		public string SelectSound;

		public string ClickSound;

		private List<MenuItemElement> _menuItems = new List<MenuItemElement>();

		private Rectangle[] _itemLocations = new Rectangle[0];

		private int _selectedIndex;

		public MenuScreen.HorizontalAlignmentTypes HorizontalAlignment = MenuScreen.HorizontalAlignmentTypes.Center;

		public MenuScreen.VerticalAlignmentTypes VerticalAlignment = MenuScreen.VerticalAlignmentTypes.Center;

		public Rectangle? DrawArea = null;

		public SpriteFont Font;

		public Color TextColor = Color.White;

		public Color SelectedColor = Color.Red;

		public Color OutlineColor = Color.Black;

		public int OutlineWidth = 2;

		public int? LineSpacing;

		private MenuItemElement _lastSelectedItem;

		public enum HorizontalAlignmentTypes
		{
			Left,
			Right,
			Center
		}

		public enum VerticalAlignmentTypes
		{
			Top,
			Center,
			Bottom
		}
	}
}
