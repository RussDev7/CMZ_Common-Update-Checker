using System;
using System.Collections.Generic;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI.Controls
{
	public class UIControlGroup : UIControl
	{
		public override UIControl FocusControl
		{
			get
			{
				if (this._owner == null)
				{
					return this._focusControl;
				}
				return this._owner.FocusControl;
			}
			set
			{
				if (this._owner == null)
				{
					this._focusControl = value;
					return;
				}
				this._focusControl = null;
				this._owner.FocusControl = value;
			}
		}

		public override UIControl CaptureControl
		{
			get
			{
				if (this._owner == null)
				{
					return this._captureControl;
				}
				return this._owner.CaptureControl;
			}
			set
			{
				if (this._owner == null)
				{
					this._captureControl = value;
					return;
				}
				this._captureControl = null;
				this._owner.CaptureControl = value;
			}
		}

		public override Rectangle LocalBounds
		{
			get
			{
				if (this._children.Count == 0)
				{
					return Rectangle.Empty;
				}
				Rectangle bounds = this._children[0].LocalBounds;
				for (int i = 1; i < this._children.Count; i++)
				{
					bounds = Rectangle.Union(bounds, this._children[i].LocalBounds);
				}
				return bounds;
			}
		}

		public UIControlGroup()
		{
			this._children = new UIControl.ControlList<UIControl>(this);
		}

		public IList<UIControl> Children
		{
			get
			{
				return this._children;
			}
		}

		public override Size Size
		{
			get
			{
				Rectangle bounds = this.LocalBounds;
				return new Size(bounds.Width, bounds.Height);
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public override bool HitTest(Point screenPoint)
		{
			return false;
		}

		public override void CollectControls(List<UIControl> controlList)
		{
			base.CollectControls(controlList);
			for (int i = 0; i < this._children.Count; i++)
			{
				this._children[i].CollectControls(controlList);
			}
		}

		protected override void OnChar(char c)
		{
			if (c == '\t')
			{
				List<UIControl> controls = new List<UIControl>();
				this.CollectControls(controls);
				UIControl focusControl = this.FocusControl;
				foreach (UIControl control in controls)
				{
					if (control == focusControl)
					{
						focusControl = null;
					}
					else if (control.IsTabStop && focusControl == null)
					{
						focusControl = control;
						break;
					}
				}
				if (focusControl == null)
				{
					foreach (UIControl control2 in controls)
					{
						if (control2.IsTabStop)
						{
							focusControl = control2;
							break;
						}
					}
				}
				this.FocusControl = focusControl;
			}
			else if (this.FocusControl != null)
			{
				this.FocusControl.ProcessChar(c);
			}
			base.OnChar(c);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			for (int i = 0; i < this._children.Count; i++)
			{
				this._children[i].Draw(device, spriteBatch, gameTime);
			}
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			base.OnUpdate(game, gameTime);
			for (int i = 0; i < this._children.Count; i++)
			{
				this._children[i].Update(game, gameTime);
			}
		}

		protected override void PreProcessInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			for (int i = 0; i < this._children.Count; i++)
			{
				this._children[i].ProcessInput(inputManager, controller, chatPad, gameTime);
			}
			base.PreProcessInput(inputManager, controller, chatPad, gameTime);
		}

		protected override void OnInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			base.OnInput(inputManager, controller, chatPad, gameTime);
			for (int i = 0; i < this._children.Count; i++)
			{
				if (inputManager.Mouse.LeftButtonPressed && this._children[i].HitTest(inputManager.Mouse.Position))
				{
					this._children[i].HasFocus = true;
				}
				this._children[i].ProcessInput(inputManager, controller, chatPad, gameTime);
			}
		}

		private UIControl _focusControl;

		private UIControl _captureControl;

		private UIControl.ControlList<UIControl> _children;
	}
}
