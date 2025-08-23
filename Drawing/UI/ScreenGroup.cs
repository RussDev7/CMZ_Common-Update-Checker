using System;
using System.Collections.Generic;
using System.Threading;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI
{
	public class ScreenGroup : Screen
	{
		public override bool CaptureMouse
		{
			get
			{
				lock (this)
				{
					for (int i = 0; i < this.screensList.Length; i++)
					{
						Screen screen = this.screensList[i];
						if (screen.AcceptInput)
						{
							return screen.CaptureMouse;
						}
						if (!screen.DrawBehind)
						{
							break;
						}
					}
				}
				return false;
			}
			set
			{
				throw new Exception("Cannot explictly set Caput Mouse Input on a Screen Group");
			}
		}

		public override bool ShowMouseCursor
		{
			get
			{
				lock (this)
				{
					for (int i = 0; i < this.screensList.Length; i++)
					{
						Screen screen = this.screensList[i];
						if (screen.AcceptInput)
						{
							return screen.ShowMouseCursor;
						}
						if (!screen.DrawBehind)
						{
							break;
						}
					}
				}
				return false;
			}
			set
			{
				throw new Exception("Cannot explictly set Caput Mouse Input on a Screen Group");
			}
		}

		public ScreenGroup(bool drawBehind)
			: base(false, drawBehind)
		{
		}

		public void ShowDialogScreen(DialogScreen screen, ThreadStart callback)
		{
			screen.Callback = callback;
			this.PushScreen(screen);
		}

		public void ShowPCDialogScreen(PCDialogScreen screen, ThreadStart callback)
		{
			screen.Callback = callback;
			this.PushScreen(screen);
		}

		public override bool AcceptInput
		{
			get
			{
				lock (this)
				{
					for (int i = 0; i < this.screensList.Length; i++)
					{
						Screen screen = this.screensList[i];
						if (screen.AcceptInput)
						{
							return true;
						}
						if (!screen.DrawBehind)
						{
							break;
						}
					}
				}
				return false;
			}
			set
			{
				throw new Exception("Cannot explictly set Accept Input on a Screen Group");
			}
		}

		public override bool Exiting
		{
			get
			{
				return this._screens.Count == 0 || base.Exiting;
			}
			set
			{
				base.Exiting = value;
			}
		}

		public void Clear()
		{
			while (this.PopScreen() != null)
			{
			}
		}

		public void PushScreen(Screen screen)
		{
			lock (this)
			{
				if (this._screens.Count > 0)
				{
					Screen oldScreen = this._screens.Peek();
					oldScreen.OnLostFocus();
				}
				this._screens.Push(screen);
				screen.OnPushed();
				this.screensList = this._screens.ToArray();
			}
		}

		public Screen PopScreen()
		{
			Screen screen2;
			lock (this)
			{
				if (this._screens.Count == 0)
				{
					screen2 = null;
				}
				else
				{
					Screen screen = this._screens.Pop();
					screen.OnPoped();
					screen.OnLostFocus();
					this.screensList = this._screens.ToArray();
					screen2 = screen;
				}
			}
			return screen2;
		}

		public bool Contains(Screen screen)
		{
			return this._screens.Contains(screen);
		}

		public Screen CurrentScreen
		{
			get
			{
				if (this._screens.Count == 0)
				{
					return null;
				}
				return this._screens.Peek();
			}
		}

		public override bool ProcessChar(GameTime gameTime, char c)
		{
			bool flag2;
			lock (this)
			{
				bool continueProcessing = true;
				int i = 0;
				while (i < this.screensList.Length && continueProcessing)
				{
					Screen screen = this.screensList[i];
					continueProcessing = continueProcessing && screen.ProcessChar(gameTime, c);
					i++;
				}
				flag2 = continueProcessing && base.ProcessChar(gameTime, c);
			}
			return flag2;
		}

		public override bool ProcessInput(InputManager inputManager, GameTime gameTime)
		{
			bool flag2;
			lock (this)
			{
				bool continueProcessing = true;
				int i = 0;
				while (i < this.screensList.Length && continueProcessing)
				{
					Screen screen = this.screensList[i];
					continueProcessing = continueProcessing && screen.ProcessInput(inputManager, gameTime);
					i++;
				}
				flag2 = continueProcessing && base.ProcessInput(inputManager, gameTime);
			}
			return flag2;
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			base.Update(game, gameTime);
			lock (this)
			{
				while (this._screens.Count != 0 && this._screens.Peek().Exiting)
				{
					Screen poped = this.PopScreen();
					poped.Exiting = false;
				}
				for (int i = 0; i < this.screensList.Length; i++)
				{
					Screen screen = this.screensList[i];
					screen.Update(game, gameTime);
					if (!screen.DrawBehind)
					{
						break;
					}
				}
			}
		}

		public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			base.Draw(device, spriteBatch, gameTime);
			if (this._screens.Count == 0)
			{
				return;
			}
			int endScreen = this.screensList.Length - 1;
			for (int i = 0; i < this.screensList.Length; i++)
			{
				Screen screen = this.screensList[i];
				if (!screen.DrawBehind)
				{
					endScreen = i;
					break;
				}
			}
			for (int j = endScreen; j >= 0; j--)
			{
				Screen screen2 = this.screensList[j];
				screen2.Draw(device, spriteBatch, gameTime);
			}
		}

		private Stack<Screen> _screens = new Stack<Screen>();

		private Screen[] screensList = new Screen[0];
	}
}
