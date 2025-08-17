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
			Rectangle screenBounds = base.ScreenBounds;
			Vector2 vector = new Vector2((float)screenBounds.Center.X, (float)screenBounds.Center.Y);
			Vector2 vector2 = this.Font.MeasureString(this.Text);
			vector2.Y = (float)this.Font.LineSpacing;
			Vector2 vector3 = Vector2.Zero;
			switch (this.TextAlignment)
			{
			case MenuBarControl.Alignment.Left:
				vector3.X = (float)(screenBounds.Left + 5);
				vector3.Y = vector.Y - (float)(this.Font.LineSpacing / 2);
				break;
			case MenuBarControl.Alignment.Right:
				vector3.X = (float)screenBounds.Right - vector2.X - 5f;
				vector3.Y = vector.Y - (float)(this.Font.LineSpacing / 2);
				break;
			case MenuBarControl.Alignment.Center:
				vector3 = vector - vector2 / 2f;
				break;
			}
			this.Frame.Draw(spriteBatch, screenBounds, this.ButtonColor);
			if (this.Font != null && this.Text != null)
			{
				spriteBatch.DrawString(this.Font, this.Text, vector3, this.TextColor);
			}
		}

		protected override void OnInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			base.CaptureInput = false;
			bool flag = this.HitTest(inputManager.Mouse.Position);
			if (flag && inputManager.Mouse.LeftButtonPressed)
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
