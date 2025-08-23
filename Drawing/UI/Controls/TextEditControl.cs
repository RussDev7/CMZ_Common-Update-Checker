using System;
using System.Text;
using DNA.Input;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI.Controls
{
	public class TextEditControl : UIControl
	{
		public ScalableFrame Frame { get; set; }

		public SpriteFont Font { get; set; }

		public int MaxChars { get; set; }

		public bool HideInput
		{
			get
			{
				return this._hideInput;
			}
			set
			{
				this._hideInput = value;
				this.Text = this._text;
			}
		}

		public string Text
		{
			get
			{
				return this._text;
			}
			set
			{
				this._text = value;
				this._curPos = this._text.Length;
				if (this.HideInput)
				{
					StringBuilder builder = new StringBuilder();
					builder.Append('*', this._text.Length);
					this._visibleText = builder.ToString();
					return;
				}
				this._visibleText = this._text;
			}
		}

		public int CursorPos
		{
			get
			{
				return this._curPos;
			}
			set
			{
				this._curPos = value;
				if (this._curPos < 0)
				{
					this._curPos = 0;
				}
				if (this._curPos > this.Text.Length)
				{
					this._curPos = this.Text.Length;
				}
			}
		}

		public Color FrameColor { get; set; }

		public Color TextColor { get; set; }

		public TextEditControl()
		{
			this.Text = "";
			this.HideInput = false;
			this.MaxChars = -1;
			this.FrameColor = Color.White;
			this.TextColor = Color.Black;
			base.IsTabStop = true;
		}

		public override Size Size
		{
			get
			{
				return new Size((int)(this.Scale * (float)this._size.Width), (int)(this.Scale * (float)this.Font.LineSpacing));
			}
			set
			{
				this._size = value;
			}
		}

		protected override void OnInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			bool hitTest = this.HitTest(inputManager.Mouse.Position);
			if (hitTest && inputManager.Mouse.LeftButtonPressed)
			{
				base.CaptureInput = true;
			}
			if (base.CaptureInput)
			{
				this._curPos = this.LetterHitTest(inputManager.Mouse.Position.X);
			}
			if (inputManager.Mouse.LeftButtonReleased)
			{
				base.CaptureInput = false;
			}
			base.OnInput(inputManager, controller, chatPad, gameTime);
		}

		public int LetterHitTest(int xloc)
		{
			Rectangle textBounds = this.Frame.CenterRegion(base.ScreenBounds);
			if (xloc <= textBounds.Left)
			{
				return 0;
			}
			lock (this._builder)
			{
				this._builder.Clear();
				for (int i = 0; i < this._visibleText.Length; i++)
				{
					this._builder.Append(this._visibleText, i, 1);
					Vector2 size = this.Font.MeasureString(this._builder) * this.Scale;
					if ((float)xloc <= (float)textBounds.Left + size.X)
					{
						return i;
					}
				}
			}
			return (int)((float)this.Text.Length * this.Scale);
		}

		public event EventHandler EnterPressed;

		protected override void OnChar(char c)
		{
			StringBuilder textBuilder = new StringBuilder(this.Text);
			switch (c)
			{
			case '\b':
				if (textBuilder.Length > 0 && this._curPos > 0)
				{
					textBuilder.Remove(this._curPos - 1, 1);
					this._curPos--;
				}
				break;
			case '\t':
				break;
			default:
				if (c != '\r')
				{
					if (c != '\u0016' && (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsWhiteSpace(c)) && (this.MaxChars < 0 || this.Text.Length < this.MaxChars))
					{
						StringBuilder tb2 = new StringBuilder(textBuilder.ToString());
						tb2.Insert(this._curPos, c);
						Vector2 newSize = this.Font.MeasureString(tb2) * this.Scale;
						Rectangle textBounds = this.Frame.CenterRegion(base.ScreenBounds);
						if (newSize.X < (float)textBounds.Width)
						{
							textBuilder.Insert(this._curPos, c);
							this._curPos++;
						}
					}
				}
				else
				{
					if (this.EnterPressed != null)
					{
						this.EnterPressed(this, new EventArgs());
					}
					textBuilder = new StringBuilder(this.Text);
				}
				break;
			}
			this.Text = textBuilder.ToString();
			base.OnChar(c);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			this._cursorTimer.Update(gameTime.ElapsedGameTime);
			base.OnUpdate(game, gameTime);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle screen = base.ScreenBounds;
			this.Frame.Draw(spriteBatch, screen, this.FrameColor);
			Rectangle textBounds = this.Frame.CenterRegion(base.ScreenBounds);
			spriteBatch.DrawString(this.Font, this._visibleText, new Vector2((float)textBounds.Left, (float)screen.Top), this.TextColor, 0f, Vector2.Zero, this.Scale, SpriteEffects.None, 0f);
			if (base.HasFocus && this._cursorTimer.PercentComplete < 0.5f)
			{
				Vector2 strMeasure = this.Font.MeasureString(this._visibleText.Substring(0, this._curPos)) * this.Scale;
				spriteBatch.DrawString(this.Font, "|", new Vector2((float)(textBounds.Left + (int)strMeasure.X), (float)(screen.Top - 2)), this.TextColor, 0f, Vector2.Zero, this.Scale, SpriteEffects.None, 0f);
			}
		}

		private bool _hideInput;

		public float Scale = 1f;

		private string _visibleText;

		private string _text;

		private int _curPos;

		private Size _size = new Size(200, 40);

		private StringBuilder _builder = new StringBuilder();

		private OneShotTimer _cursorTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5), true);

		private StringBuilder _drawBuilder = new StringBuilder();
	}
}
