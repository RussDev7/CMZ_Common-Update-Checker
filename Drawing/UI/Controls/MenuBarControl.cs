using System;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI.Controls
{
	public class MenuBarControl : UIControl
	{
		public ScalableFrame Frame { get; set; }

		public SpriteFont Font { get; set; }

		public string Text { get; set; }

		public override Size Size
		{
			get
			{
				return this._size;
			}
			set
			{
				this._size = value;
			}
		}

		public Color ButtonColor { get; set; }

		public Color TextColor { get; set; }

		public MenuBarControl()
		{
			this.ButtonColor = Color.White;
			this.TextColor = Color.Black;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle dest = base.ScreenBounds;
			Vector2 center = new Vector2((float)dest.Center.X, (float)dest.Center.Y);
			Vector2 textSize = this.Font.MeasureString(this.Text);
			textSize.Y = (float)this.Font.LineSpacing;
			Vector2 textPos = Vector2.Zero;
			switch (this.TextAlignment)
			{
			case MenuBarControl.Alignment.Left:
				textPos.X = (float)(dest.Left + 5);
				textPos.Y = center.Y - (float)(this.Font.LineSpacing / 2);
				break;
			case MenuBarControl.Alignment.Right:
				textPos.X = (float)dest.Right - textSize.X - 5f;
				textPos.Y = center.Y - (float)(this.Font.LineSpacing / 2);
				break;
			case MenuBarControl.Alignment.Center:
				textPos = center - textSize / 2f;
				break;
			}
			this.Frame.Draw(spriteBatch, dest, this.ButtonColor);
			if (this.Font != null && this.Text != null)
			{
				spriteBatch.DrawString(this.Font, this.Text, textPos, this.TextColor);
			}
		}

		protected override void OnInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			base.CaptureInput = false;
			bool hitTest = this.HitTest(inputManager.Mouse.Position);
			if (hitTest && inputManager.Mouse.LeftButtonPressed)
			{
				this.DragMenu = true;
			}
			if (inputManager.Mouse.LeftButtonReleased)
			{
				this.DragMenu = false;
			}
			base.OnInput(inputManager, controller, chatPad, gameTime);
		}

		public bool DragMenu;

		public MenuBarControl.Alignment TextAlignment;

		private Size _size = new Size(100, 100);

		public enum Alignment
		{
			Left,
			Right,
			Center
		}
	}
}
