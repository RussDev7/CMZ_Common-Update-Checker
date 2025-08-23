﻿using System;
using Microsoft.Xna.Framework.Input;

namespace DNA.Input
{
	public struct ControllerDPad
	{
		public ControllerDPad(bool upValue, bool downValue, bool leftValue, bool rightValue)
		{
			this._up = upValue;
			this._down = downValue;
			this._left = leftValue;
			this._right = rightValue;
		}

		public static bool operator !=(ControllerDPad left, ControllerDPad right)
		{
			return left.Up != right.Up || left.Down != right.Down || left.Right != right.Right || left.Left != right.Left;
		}

		public static bool operator ==(ControllerDPad left, ControllerDPad right)
		{
			return left.Up == right.Up && left.Down == right.Down && left.Right == right.Right && left.Left == right.Left;
		}

		public bool Down
		{
			get
			{
				return this._down;
			}
		}

		public bool Left
		{
			get
			{
				return this._left;
			}
		}

		public bool Right
		{
			get
			{
				return this._right;
			}
		}

		public bool Up
		{
			get
			{
				return this._up;
			}
		}

		public bool this[Buttons btn]
		{
			get
			{
				bool result = false;
				switch (btn)
				{
				case Buttons.DPadUp:
					result = this._up;
					break;
				case Buttons.DPadDown:
					result = this._down;
					break;
				case Buttons.DPadUp | Buttons.DPadDown:
					break;
				case Buttons.DPadLeft:
					result = this._left;
					break;
				default:
					if (btn == Buttons.DPadRight)
					{
						result = this._right;
					}
					break;
				}
				return result;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is ControllerDPad)
			{
				ControllerDPad b = (ControllerDPad)obj;
				return this == b;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return this._up.GetHashCode() ^ this._down.GetHashCode() ^ this._left.GetHashCode() ^ this._right.GetHashCode();
		}

		public override string ToString()
		{
			string ret = "";
			if (this._up)
			{
				return ret + "Up";
			}
			if (this._down)
			{
				if (ret.Length > 0)
				{
					ret += "|";
				}
				return ret + "Down";
			}
			if (this._left)
			{
				if (ret.Length > 0)
				{
					ret += "|";
				}
				return ret + "Left";
			}
			if (this._right)
			{
				if (ret.Length > 0)
				{
					ret += "|";
				}
				return ret + "Right";
			}
			return ret;
		}

		private bool _up;

		private bool _down;

		private bool _left;

		private bool _right;
	}
}
