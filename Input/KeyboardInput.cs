using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DNA.Input
{
	public class KeyboardInput
	{
		public KeyboardInput()
		{
		}

		public KeyboardInput(PlayerIndex index)
		{
			this._playerIndex = new PlayerIndex?(index);
		}

		public PlayerIndex? PlayerIndex
		{
			get
			{
				return this._playerIndex;
			}
		}

		public KeyboardState CurrentState
		{
			get
			{
				return this._currentState;
			}
		}

		public KeyboardState LastState
		{
			get
			{
				return this._lastState;
			}
		}

		public bool IsKeyDown(Keys key)
		{
			return this._currentState.IsKeyDown(key);
		}

		public bool WasKeyPressed(Keys key)
		{
			return this._currentState.IsKeyDown(key) && this._lastState.IsKeyUp(key);
		}

		public bool WasKeyReleased(Keys key)
		{
			return this._currentState.IsKeyUp(key) && this._lastState.IsKeyDown(key);
		}

		public void Update()
		{
			this._lastState = this._currentState;
			if (this._playerIndex != null)
			{
				this._currentState = Keyboard.GetState(this._playerIndex.Value);
				return;
			}
			this._currentState = Keyboard.GetState();
		}

		private PlayerIndex? _playerIndex;

		private KeyboardState _lastState;

		private KeyboardState _currentState;
	}
}
