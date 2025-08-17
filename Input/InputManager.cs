﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.Input
{
	public class InputManager
	{
		public MouseInput Mouse
		{
			get
			{
				return this._mouseState;
			}
		}

		public KeyboardInput Keyboard
		{
			get
			{
				return this._keyboardState;
			}
		}

		public KeyboardInput[] ChatPads
		{
			get
			{
				return this._chatPads;
			}
		}

		public GameController[] Controllers
		{
			get
			{
				return this._controllers;
			}
		}

		public ControllerButtons ButtonsPressed
		{
			get
			{
				return this._buttonsPressed;
			}
		}

		public InputManager(DNAGame game)
		{
			this._game = game;
			this._mouseState = new MouseInput(game);
			this._controllers[0] = new GameController(PlayerIndex.One);
			this._controllers[1] = new GameController(PlayerIndex.Two);
			this._controllers[2] = new GameController(PlayerIndex.Three);
			this._controllers[3] = new GameController(PlayerIndex.Four);
			this._keyboardState = new KeyboardInput();
			this._chatPads[0] = new KeyboardInput(PlayerIndex.One);
			this._chatPads[1] = new KeyboardInput(PlayerIndex.Two);
			this._chatPads[2] = new KeyboardInput(PlayerIndex.Three);
			this._chatPads[3] = new KeyboardInput(PlayerIndex.Four);
		}

		protected void HandleChangeInMouseCapture()
		{
			bool captureMouse = this._game.ScreenManager.CaptureMouse;
			bool isActive = this._game.IsActive;
			if (isActive)
			{
				if (isActive != this._previousFrameWasActive && captureMouse)
				{
					this.ResetMouse(true);
				}
				else if (captureMouse != this._previousFrameCaptureMouse)
				{
					this.ResetMouse(true);
				}
			}
			this._previousFrameWasActive = isActive;
			this._previousFrameCaptureMouse = captureMouse;
		}

		protected void ResetMouse(bool zeroDeltas)
		{
			Viewport viewport = this._game.GraphicsDevice.Viewport;
			this.Mouse.SetPosition(viewport.Width / 2, viewport.Height / 2, zeroDeltas);
		}

		public void Update()
		{
			this.HandleChangeInMouseCapture();
			if (this._game.IsActive)
			{
				this._mouseState.Update();
				this._keyboardState.Update();
				for (int i = 0; i < this._controllers.Length; i++)
				{
					this._chatPads[i].Update();
				}
			}
			Buttons buttons = (Buttons)0;
			for (int j = 0; j < this._controllers.Length; j++)
			{
				GameController gameController = this._controllers[j];
				gameController.Update();
				if (gameController.PressedButtons.A)
				{
					buttons |= Buttons.A;
				}
				if (gameController.PressedButtons.B)
				{
					buttons |= Buttons.B;
				}
				if (gameController.PressedButtons.Back)
				{
					buttons |= Buttons.Back;
				}
				if (gameController.PressedButtons.BigButton)
				{
					buttons |= Buttons.BigButton;
				}
				if (gameController.PressedButtons.LeftShoulder)
				{
					buttons |= Buttons.LeftShoulder;
				}
				if (gameController.PressedButtons.LeftStick)
				{
					buttons |= Buttons.LeftStick;
				}
				if (gameController.PressedButtons.RightShoulder)
				{
					buttons |= Buttons.RightShoulder;
				}
				if (gameController.PressedButtons.RightStick)
				{
					buttons |= Buttons.RightStick;
				}
				if (gameController.PressedButtons.Start)
				{
					buttons |= Buttons.Start;
				}
				if (gameController.PressedButtons.X)
				{
					buttons |= Buttons.X;
				}
				if (gameController.PressedButtons.Y)
				{
					buttons |= Buttons.Y;
				}
			}
			this._buttonsPressed = new ControllerButtons(buttons);
			if (this._game.IsActive && this._game.ScreenManager.CaptureMouse)
			{
				this.ResetMouse(false);
			}
		}

		private DNAGame _game;

		private MouseInput _mouseState;

		private KeyboardInput _keyboardState;

		private KeyboardInput[] _chatPads = new KeyboardInput[4];

		private GameController[] _controllers = new GameController[4];

		private ControllerButtons _buttonsPressed;

		private bool _previousFrameCaptureMouse;

		private bool _previousFrameWasActive;
	}
}
