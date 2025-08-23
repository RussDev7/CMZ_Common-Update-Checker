using System;
using System.Windows.Forms;
using DNA.Input;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.Drawing.UI
{
	public class PCKeyboardInputScreen : PCDialogScreen
	{
		public string TextInput
		{
			get
			{
				return this._textInput;
			}
		}

		public string TextInput2
		{
			get
			{
				return this._textInput2;
			}
		}

		public string TextInput3
		{
			get
			{
				return this._textInput3;
			}
		}

		public string DefaultText
		{
			set
			{
				this._defaultText = value;
			}
		}

		public string DefaultText2
		{
			set
			{
				this._defaultText2 = value;
			}
		}

		public string DefaultText3
		{
			set
			{
				this._defaultText3 = value;
			}
		}

		public string ErrorMessage
		{
			get
			{
				return this._errorMessage;
			}
			set
			{
				this._errorMessage = value;
			}
		}

		public PCKeyboardInputScreen(DNAGame game, string title, string description, Texture2D bgImage, SpriteFont font, bool drawBehind, ScalableFrame frame)
			: base(title, description, null, true, bgImage, font, drawBehind, frame)
		{
			base.UseDefaultValues();
			this._game = game;
		}

		public PCKeyboardInputScreen(DNAGame game, string title, string description1, string description2, Texture2D bgImage, SpriteFont font, bool drawBehind, ScalableFrame frame)
			: base(title, description1, null, true, bgImage, font, drawBehind, frame)
		{
			base.UseDefaultValues();
			this._description2 = description2;
			this._game = game;
		}

		public PCKeyboardInputScreen(DNAGame game, string title, string description1, string description2, string description3, Texture2D bgImage, SpriteFont font, bool drawBehind, ScalableFrame frame)
			: base(title, description1, null, true, bgImage, font, drawBehind, frame)
		{
			base.UseDefaultValues();
			this._description2 = description2;
			this._description3 = description3;
			this._game = game;
		}

		public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			base.Draw(device, spriteBatch, gameTime);
			Rectangle titleSafe = device.Viewport.TitleSafeArea;
			this._drawCursorTimer.Update(gameTime.ElapsedGameTime);
			if (this._drawCursorTimer.Expired)
			{
				this._drawCursorTimer.Reset();
				this._drawCursor = !this._drawCursor;
			}
			spriteBatch.Begin();
			Vector2 location = new Vector2((float)(titleSafe.Center.X - this._bgImage.Width / 2) + this.DescriptionPadding.X, this._endOfDescriptionLoc);
			this._input1Rectangle = new Rectangle((int)location.X, (int)location.Y, (int)((float)this._bgImage.Width - this.DescriptionPadding.X * 2f), 27);
			spriteBatch.Draw(this._game.DummyTexture, this._input1Rectangle, Color.White);
			spriteBatch.DrawString(this._font, this._textInput, location, Color.Black);
			if (this._drawCursor && this._cursorLine == 1)
			{
				spriteBatch.DrawString(this._font, "_", location + new Vector2(this._font.MeasureString(this._textInput).X, 0f), Color.Black);
			}
			if (this._description2 != null)
			{
				location.Y += 35f;
				spriteBatch.DrawOutlinedText(this._font, this._description2, location, Color.White, Color.Black, 1);
				location.Y += this._font.MeasureString(this._description2).Y;
				this._input2Rectangle = new Rectangle((int)location.X, (int)location.Y, (int)((float)this._bgImage.Width - this.DescriptionPadding.X * 2f), 27);
				spriteBatch.Draw(this._game.DummyTexture, this._input2Rectangle, Color.White);
				spriteBatch.DrawString(this._font, this._textInput2, location, Color.Black);
				if (this._drawCursor && this._cursorLine == 2)
				{
					spriteBatch.DrawString(this._font, "_", location + new Vector2(this._font.MeasureString(this._textInput2).X, 0f), Color.Black);
				}
			}
			if (this._description3 != null)
			{
				location.Y += 35f;
				spriteBatch.DrawOutlinedText(this._font, this._description3, location, Color.White, Color.Black, 1);
				location.Y += this._font.MeasureString(this._description3).Y;
				this._input3Rectangle = new Rectangle((int)location.X, (int)location.Y, (int)((float)this._bgImage.Width - this.DescriptionPadding.X * 2f), 27);
				spriteBatch.Draw(this._game.DummyTexture, this._input3Rectangle, Color.White);
				spriteBatch.DrawString(this._font, this._textInput3, location, Color.Black);
				if (this._drawCursor && this._cursorLine == 3)
				{
					spriteBatch.DrawString(this._font, "_", location + new Vector2(this._font.MeasureString(this._textInput3).X, 0f), Color.Black);
				}
			}
			if (this._errorMessage != null)
			{
				location.Y += 35f;
				spriteBatch.DrawOutlinedText(this._font, this._errorMessage, location, Color.Red, Color.Black, 1);
			}
			spriteBatch.End();
		}

		public override void OnPoped()
		{
			this._defaultText = "";
			this._defaultText2 = "";
			this._defaultText3 = "";
			base.OnPoped();
		}

		public override void OnPushed()
		{
			this._textInput = this._defaultText;
			this._textInput2 = this._defaultText2;
			this._textInput3 = this._defaultText3;
			base.OnPushed();
		}

		public void SetCursor(int line)
		{
			if (line >= 1 && line <= 3)
			{
				this._cursorLine = line;
			}
		}

		public override bool ProcessChar(GameTime gameTime, char c)
		{
			string input = "";
			switch (this._cursorLine)
			{
			case 1:
				input = this._textInput;
				break;
			case 2:
				input = this._textInput2;
				break;
			case 3:
				input = this._textInput3;
				break;
			}
			bool updateInput = true;
			if (c == '\b' || this._font.MeasureString(input + "M_").X <= (float)this._input1Rectangle.Width)
			{
				switch (c)
				{
				case '\b':
					if (input.Length > 0)
					{
						input = input.Substring(0, input.Length - 1);
					}
					break;
				case '\t':
					updateInput = false;
					switch (this._cursorLine)
					{
					case 1:
						if (this._textInput2 != null)
						{
							this._cursorLine = 2;
						}
						break;
					case 2:
						if (this._textInput3 != null)
						{
							this._cursorLine = 3;
						}
						else
						{
							this._cursorLine = 1;
						}
						break;
					case 3:
						this._cursorLine = 1;
						break;
					}
					break;
				default:
					if (c != '\u0016')
					{
						if (!char.IsControl(c))
						{
							input += c;
						}
					}
					else if (Clipboard.ContainsText())
					{
						string clipboard = Clipboard.GetText();
						int length = clipboard.Length;
						while (this._font.MeasureString(input + clipboard.Substring(0, length) + "M_").X > (float)this._input1Rectangle.Width)
						{
							length--;
						}
						input += clipboard.Substring(0, length);
					}
					break;
				}
			}
			if (updateInput)
			{
				switch (this._cursorLine)
				{
				case 1:
					this._textInput = input;
					break;
				case 2:
					this._textInput2 = input;
					break;
				case 3:
					this._textInput3 = input;
					break;
				}
			}
			return base.ProcessChar(gameTime, c);
		}

		protected override bool OnPlayerInput(InputManager input, GameController controller, KeyboardInput chatpad, GameTime gameTime)
		{
			input.Keyboard.CurrentState.GetPressedKeys();
			if (input.Mouse.LeftButtonPressed)
			{
				if (this._input1Rectangle.Contains(input.Mouse.Position))
				{
					this._cursorLine = 1;
				}
				else if (this._input2Rectangle.Contains(input.Mouse.Position))
				{
					this._cursorLine = 2;
				}
				else if (this._input3Rectangle.Contains(input.Mouse.Position))
				{
					this._cursorLine = 3;
				}
			}
			else if ((double)controller.CurrentState.Triggers.Left < -0.1 || (double)controller.CurrentState.Triggers.Right < -0.1 || controller.PressedDPad.Down || input.Keyboard.WasKeyPressed(Microsoft.Xna.Framework.Input.Keys.Down))
			{
				switch (this._cursorLine)
				{
				case 1:
					if (this._textInput2 != null)
					{
						this._cursorLine = 2;
					}
					break;
				case 2:
					if (this._textInput3 != null)
					{
						this._cursorLine = 3;
					}
					break;
				}
			}
			else if (((double)controller.CurrentState.Triggers.Left > 0.1 || (double)controller.CurrentState.Triggers.Right > 0.1 || controller.PressedDPad.Up || input.Keyboard.WasKeyPressed(Microsoft.Xna.Framework.Input.Keys.Up)) && this._cursorLine > 1)
			{
				this._cursorLine--;
			}
			return base.OnPlayerInput(input, controller, chatpad, gameTime);
		}

		private DNAGame _game;

		private string _textInput = "";

		private string _textInput2 = "";

		private string _textInput3 = "";

		private string _defaultText = "";

		private string _defaultText2 = "";

		private string _defaultText3 = "";

		private OneShotTimer _drawCursorTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		private bool _drawCursor = true;

		private string _description2;

		private string _description3;

		protected int _cursorLine = 1;

		private string _errorMessage;

		private Rectangle _input1Rectangle = Rectangle.Empty;

		private Rectangle _input2Rectangle = Rectangle.Empty;

		private Rectangle _input3Rectangle = Rectangle.Empty;
	}
}
