using System;
using DNA.Input;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI
{
	public class Screen
	{
		public virtual bool CaptureMouse
		{
			get
			{
				return this._captureMouse;
			}
			set
			{
				this._captureMouse = value;
			}
		}

		public virtual bool ShowMouseCursor
		{
			get
			{
				return this._showMouseCursor;
			}
			set
			{
				this._showMouseCursor = value;
			}
		}

		static Screen()
		{
			SignedInGamer.SignedIn += Screen.SignedInGamer_SignedIn;
			SignedInGamer.SignedOut += Screen.SignedInGamer_SignedOut;
		}

		public static PlayerIndex? SelectedPlayerIndex
		{
			get
			{
				return Screen._selectedPlayerIndex;
			}
			set
			{
				Screen._selectedPlayerIndex = value;
			}
		}

		public static SignedInGamer CurrentGamer
		{
			get
			{
				if (Screen._selectedPlayerIndex == null)
				{
					return null;
				}
				return Gamer.SignedInGamers[Screen._selectedPlayerIndex.Value];
			}
		}

		public virtual bool Exiting { get; set; }

		public bool DrawBehind
		{
			get
			{
				return this.BackgroundColor == null && this._drawBehind;
			}
		}

		public virtual bool AcceptInput
		{
			get
			{
				return this._acceptInput;
			}
			set
			{
				this._acceptInput = value;
			}
		}

		public virtual bool DoUpdate
		{
			get
			{
				return this._doUpdate;
			}
			set
			{
				this._doUpdate = value;
			}
		}

		public Screen(bool acceptInput, bool drawBehind)
		{
			this._acceptInput = acceptInput;
			this._drawBehind = drawBehind;
		}

		public event EventHandler<EventArgs> LostFocus;

		public virtual void OnLostFocus()
		{
			this._mouseActive = false;
			if (this.LostFocus != null)
			{
				this.LostFocus(this, new EventArgs());
			}
		}

		public event EventHandler<UpdateEventArgs> Updating;

		protected virtual void OnUpdate(DNAGame game, GameTime gameTime)
		{
		}

		public virtual void Update(DNAGame game, GameTime gameTime)
		{
			if (this.DoUpdate)
			{
				if (this.Updating != null)
				{
					this._updateEventArgs.GameTime = gameTime;
					this.Updating(this, this._updateEventArgs);
				}
				this.OnUpdate(game, gameTime);
			}
		}

		public event EventHandler<CharEventArgs> ProcessingChar;

		public event EventHandler<InputEventArgs> ProcessingInput;

		protected virtual bool OnInput(InputManager inputManager, GameTime gameTime)
		{
			return !this.AcceptInput;
		}

		protected virtual bool OnChar(GameTime gameTime, char c)
		{
			return !this.AcceptInput;
		}

		public virtual bool ProcessChar(GameTime gameTime, char c)
		{
			if (!this.AcceptInput)
			{
				return true;
			}
			bool flag = this.OnChar(gameTime, c);
			if (this.ProcessingChar != null)
			{
				CharEventArgs charEventArgs = new CharEventArgs(c, gameTime, flag);
				this.ProcessingChar(this, charEventArgs);
				flag = charEventArgs.ContiuneProcessing;
			}
			return flag;
		}

		public virtual bool ProcessInput(InputManager inputManager, GameTime gameTime)
		{
			if (!this.AcceptInput)
			{
				return true;
			}
			bool flag = this.OnInput(inputManager, gameTime);
			if (this.ProcessingInput != null)
			{
				InputEventArgs inputEventArgs = new InputEventArgs(inputManager, gameTime, flag);
				this.ProcessingInput(this, inputEventArgs);
				flag = inputEventArgs.ContiuneProcessing;
			}
			if (Screen._selectedPlayerIndex != null)
			{
				flag = flag || this.ProcessInput(inputManager, inputManager.Controllers[(int)Screen._selectedPlayerIndex.Value], inputManager.ChatPads[(int)Screen._selectedPlayerIndex.Value], gameTime);
			}
			return flag;
		}

		public event EventHandler<ControllerInputEventArgs> ProcessingPlayerInput;

		protected virtual bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			if (controller.Activity)
			{
				this._mouseActive = false;
			}
			return !this.AcceptInput;
		}

		protected virtual bool ProcessInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			bool flag = this.OnPlayerInput(inputManager, controller, chatPad, gameTime);
			if (this.ProcessingPlayerInput != null)
			{
				this._controllerEventArgs.Mouse = inputManager.Mouse;
				this._controllerEventArgs.Keyboard = inputManager.Keyboard;
				this._controllerEventArgs.Chatpad = chatPad;
				this._controllerEventArgs.Controller = controller;
				this._controllerEventArgs.GameTime = gameTime;
				this._controllerEventArgs.ContinueProcessing = flag;
				this.ProcessingPlayerInput(this, this._controllerEventArgs);
				flag = flag || this._controllerEventArgs.ContinueProcessing;
			}
			return flag;
		}

		public event EventHandler<DrawEventArgs> BeforeDraw;

		public event EventHandler<DrawEventArgs> AfterDraw;

		protected virtual void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
		}

		public virtual void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			this.args.Device = device;
			this.args.GameTime = gameTime;
			if (this.BeforeDraw != null)
			{
				this.BeforeDraw(this, this.args);
			}
			if (this.BackgroundImage != null)
			{
				device.Clear(ClearOptions.DepthBuffer, Color.Red, 1f, 0);
				int width = device.Viewport.Width;
				int height = device.Viewport.Height;
				int num = width * this.BackgroundImage.Height / height;
				int num2 = num - width;
				int num3 = num2 / 2;
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone);
				spriteBatch.Draw(this.BackgroundImage, new Rectangle(-num3, 0, num, height), new Rectangle?(new Rectangle(0, 0, this.BackgroundImage.Width, this.BackgroundImage.Height)), Color.White);
				spriteBatch.End();
			}
			else if (this.BackgroundColor != null)
			{
				device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, this.BackgroundColor.Value, 1f, 0);
			}
			else
			{
				device.Clear(ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);
			}
			this.OnDraw(device, spriteBatch, gameTime);
			if (this.AfterDraw != null)
			{
				this.AfterDraw(this, this.args);
			}
		}

		public event EventHandler Pushed;

		public event EventHandler Poped;

		public virtual void OnPushed()
		{
			this.Exiting = false;
			if (this.Pushed != null)
			{
				this.Pushed(this, new EventArgs());
			}
		}

		public virtual void OnPoped()
		{
			if (this.Poped != null)
			{
				this.Poped(this, new EventArgs());
			}
		}

		public void PopMe()
		{
			this.Exiting = true;
		}

		public static event EventHandler<SignedOutEventArgs> PlayerSignedOut;

		public static event EventHandler<SignedInEventArgs> PlayerSignedIn;

		private static void SignedInGamer_SignedOut(object sender, SignedOutEventArgs e)
		{
			if (Screen._selectedPlayerIndex != null && e.Gamer.PlayerIndex == Screen._selectedPlayerIndex.Value && Screen.PlayerSignedOut != null)
			{
				Screen.PlayerSignedOut(sender, e);
			}
		}

		private static void SignedInGamer_SignedIn(object sender, SignedInEventArgs e)
		{
			if (Screen._selectedPlayerIndex != null && e.Gamer.PlayerIndex == Screen._selectedPlayerIndex.Value && Screen.PlayerSignedIn != null)
			{
				Screen.PlayerSignedIn(sender, e);
			}
		}

		public static ScreenAdjuster Adjuster = new ScreenAdjuster();

		private bool _captureMouse;

		private bool _showMouseCursor = true;

		public Color? BackgroundColor = null;

		public Texture2D BackgroundImage;

		private static PlayerIndex? _selectedPlayerIndex = null;

		private bool _drawBehind;

		private bool _acceptInput = true;

		private bool _doUpdate = true;

		protected bool _mouseActive;

		private UpdateEventArgs _updateEventArgs = new UpdateEventArgs();

		private ControllerInputEventArgs _controllerEventArgs = new ControllerInputEventArgs();

		private DrawEventArgs args = new DrawEventArgs();
	}
}
