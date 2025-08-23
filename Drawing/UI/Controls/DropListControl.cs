using System;
using System.Collections.Generic;
using DNA.Collections;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI.Controls
{
	public class DropListControl<T> : UIControl
	{
		public ScalableFrame Frame { get; set; }

		public SpriteFont Font { get; set; }

		public Sprite DropArrow { get; set; }

		public event EventHandler SelectedIndexChanged;

		public DropListControl()
		{
			this._items = new NotifyList<T>();
			this._items.Modified += this._items_Modified;
		}

		private void _items_Modified(object sender, EventArgs e)
		{
			this.SelectedIndex = 0;
		}

		public IList<T> Items
		{
			get
			{
				return this._items;
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
				if (this._selectedIndex != value)
				{
					this._selectedIndex = value;
					if (this.SelectedIndexChanged != null)
					{
						this.SelectedIndexChanged(this, null);
					}
				}
			}
		}

		public T SelectedItem
		{
			get
			{
				if (this._items.Count == 0)
				{
					return default(T);
				}
				return this._items[this._selectedIndex];
			}
			set
			{
				int result = this._items.IndexOf(value);
				if (result < 0)
				{
					throw new Exception("Item Not in List");
				}
				this.SelectedIndex = result;
			}
		}

		public override Size Size
		{
			get
			{
				if (base.CaptureInput)
				{
					return new Size((int)((float)this._size.Width * this.Scale), (int)((float)(this.Font.LineSpacing * (this._items.Count + 1)) * this.Scale));
				}
				return new Size((int)((float)this._size.Width * this.Scale), (int)((float)this.Font.LineSpacing * this.Scale));
			}
			set
			{
				this._size = value;
			}
		}

		private Rectangle FrameRect
		{
			get
			{
				return new Rectangle(base.ScreenPosition.X, base.ScreenPosition.Y, (int)((float)this._size.Width * this.Scale), (int)((float)this.Font.LineSpacing * this.Scale));
			}
		}

		private Rectangle ExpandRect
		{
			get
			{
				Rectangle frameRect = this.FrameRect;
				return new Rectangle(frameRect.Left, frameRect.Bottom, (int)((float)frameRect.Width * this.Scale), (int)((float)(this.Font.LineSpacing * this._items.Count) * this.Scale));
			}
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle frameRect = this.FrameRect;
			this.Frame.Draw(spriteBatch, frameRect, Color.White);
			Rectangle textRect = this.Frame.CenterRegion(frameRect);
			string text = "";
			if (this.SelectedItem != null)
			{
				T selectedItem = this.SelectedItem;
				text = selectedItem.ToString();
			}
			Vector2 textSize = this.Font.MeasureString(text) * this.Scale;
			Vector2 textLoc = new Vector2((float)textRect.Left, (float)textRect.Center.Y - textSize.Y / 2f);
			float lineSpacing = (float)this.Font.LineSpacing * this.Scale;
			spriteBatch.DrawString(this.Font, text, textLoc, Color.Black, 0f, Vector2.Zero, this.Scale, SpriteEffects.None, 0f);
			Rectangle dropArrowLoc = new Rectangle(textRect.Right - (int)((float)this.DropArrow.Width * this.Scale), textRect.Center.Y - (int)((float)this.DropArrow.Height * this.Scale / 2f), (int)((float)this.DropArrow.Width * this.Scale), (int)((float)this.DropArrow.Height * this.Scale));
			spriteBatch.Draw(this.DropArrow, dropArrowLoc, Color.White);
			if (base.CaptureInput)
			{
				Rectangle expandRect = new Rectangle(frameRect.Left, frameRect.Bottom, frameRect.Width, (int)(lineSpacing * (float)this._items.Count));
				spriteBatch.Draw(UIControl.DummyTexture, expandRect, Color.Black);
				expandRect.Inflate(-1, -1);
				spriteBatch.Draw(UIControl.DummyTexture, expandRect, Color.White);
				for (int i = 0; i < this._items.Count; i++)
				{
					if (i == this._expandSel)
					{
						Rectangle selRect = new Rectangle(expandRect.Left, expandRect.Top + (int)(lineSpacing * (float)i), expandRect.Width, (int)lineSpacing);
						spriteBatch.Draw(UIControl.DummyTexture, selRect, Color.Black);
						SpriteFont font = this.Font;
						T t = this._items[i];
						spriteBatch.DrawString(font, t.ToString(), new Vector2((float)expandRect.Left, lineSpacing * (float)i + (float)expandRect.Top), Color.White, 0f, Vector2.Zero, this.Scale, SpriteEffects.None, 0f);
					}
					else
					{
						SpriteFont font2 = this.Font;
						T t2 = this._items[i];
						spriteBatch.DrawString(font2, t2.ToString(), new Vector2((float)expandRect.Left, lineSpacing * (float)i + (float)expandRect.Top), Color.Black, 0f, Vector2.Zero, this.Scale, SpriteEffects.None, 0f);
					}
				}
			}
		}

		protected override void OnInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			bool hitTest = this.HitTest(inputManager.Mouse.Position);
			if (inputManager.Mouse.LeftButtonPressed)
			{
				if (hitTest)
				{
					if (!base.CaptureInput)
					{
						this._expandSel = this._selectedIndex;
						this._skipRelease = true;
						base.CaptureInput = true;
					}
				}
				else
				{
					base.CaptureInput = false;
				}
			}
			if (hitTest && base.CaptureInput)
			{
				Rectangle expandRect = this.ExpandRect;
				if (expandRect.Contains(inputManager.Mouse.Position))
				{
					this._expandSel = (inputManager.Mouse.Position.Y - expandRect.Top) * this.Items.Count / expandRect.Height;
				}
				if (inputManager.Mouse.LeftButtonReleased)
				{
					if (this._skipRelease)
					{
						this._skipRelease = false;
					}
					else
					{
						this.SelectedIndex = this._expandSel;
						base.CaptureInput = false;
					}
				}
			}
			base.OnInput(inputManager, controller, chatPad, gameTime);
		}

		public float Scale = 1f;

		private int _selectedIndex;

		private int _expandSel;

		private NotifyList<T> _items;

		private Size _size = new Size(200, 40);

		private bool _skipRelease;
	}
}
