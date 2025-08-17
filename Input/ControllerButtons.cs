using System;
using Microsoft.Xna.Framework.Input;

namespace DNA.Input
{
	public struct ControllerButtons
	{
		public ControllerButtons(Buttons buttons)
		{
			this._buttons = buttons;
		}

		public static bool operator !=(ControllerButtons left, ControllerButtons right)
		{
			return left._buttons != right._buttons;
		}

		public static bool operator ==(ControllerButtons left, ControllerButtons right)
		{
			return left._buttons != right._buttons;
		}

		public bool this[Buttons button]
		{
			get
			{
				return (this._buttons & button) != (Buttons)0;
			}
		}

		public bool A
		{
			get
			{
				return (this._buttons & Buttons.A) != (Buttons)0;
			}
		}

		public bool B
		{
			get
			{
				return (this._buttons & Buttons.B) != (Buttons)0;
			}
		}

		public bool Back
		{
			get
			{
				return (this._buttons & Buttons.Back) != (Buttons)0;
			}
		}

		public bool BigButton
		{
			get
			{
				return (this._buttons & Buttons.BigButton) != (Buttons)0;
			}
		}

		public bool LeftShoulder
		{
			get
			{
				return (this._buttons & Buttons.LeftShoulder) != (Buttons)0;
			}
		}

		public bool LeftStick
		{
			get
			{
				return (this._buttons & Buttons.LeftStick) != (Buttons)0;
			}
		}

		public bool RightShoulder
		{
			get
			{
				return (this._buttons & Buttons.RightShoulder) != (Buttons)0;
			}
		}

		public bool RightStick
		{
			get
			{
				return (this._buttons & Buttons.RightStick) != (Buttons)0;
			}
		}

		public bool Start
		{
			get
			{
				return (this._buttons & Buttons.Start) != (Buttons)0;
			}
		}

		public bool X
		{
			get
			{
				return (this._buttons & Buttons.X) != (Buttons)0;
			}
		}

		public bool Y
		{
			get
			{
				return (this._buttons & Buttons.Y) != (Buttons)0;
			}
		}

		public bool RightTrigger
		{
			get
			{
				return (this._buttons & Buttons.RightTrigger) != (Buttons)0;
			}
		}

		public bool LeftTrigger
		{
			get
			{
				return (this._buttons & Buttons.LeftTrigger) != (Buttons)0;
			}
		}

		public override bool Equals(object obj)
		{
			return obj is ControllerButtons && ((ControllerButtons)obj)._buttons == this._buttons;
		}

		public override int GetHashCode()
		{
			return this._buttons.GetHashCode();
		}

		public override string ToString()
		{
			return this._buttons.ToString();
		}

		private Buttons _buttons;
	}
}
