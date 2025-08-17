using System;
using DNA.Input;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.UI.Controls
{
	public abstract class ButtonControl : UIControl
	{
		public ButtonControl()
		{
		}

		protected bool Hovering
		{
			get
			{
				return this._hovering;
			}
			set
			{
				this._hovering = value;
			}
		}

		public event EventHandler Pressed;

		public virtual void OnPressed()
		{
			if (this.Pressed != null)
			{
				this.Pressed(this, new EventArgs());
			}
		}

		protected override void OnInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			bool flag = this.HitTest(inputManager.Mouse.Position);
			if (flag)
			{
				this.Hovering = true;
				if (inputManager.Mouse.LeftButtonPressed)
				{
					base.CaptureInput = true;
				}
			}
			else
			{
				this.Hovering = false;
			}
			if (inputManager.Mouse.LeftButtonReleased)
			{
				if (flag && base.CaptureInput)
				{
					this.OnPressed();
				}
				base.CaptureInput = false;
			}
			base.OnInput(inputManager, controller, chatPad, gameTime);
		}

		public float Scale = 1f;

		private bool _hovering;
	}
}
