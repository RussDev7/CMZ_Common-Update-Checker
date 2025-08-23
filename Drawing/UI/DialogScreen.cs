﻿using System;
using System.Collections.Generic;
using System.Threading;
using DNA.Audio;
using DNA.Input;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.Drawing.UI
{
	public class DialogScreen : Screen
	{
		protected Rectangle _buttonAloc
		{
			get
			{
				return this._button0Loc;
			}
		}

		protected Rectangle _buttonBloc
		{
			get
			{
				return this._button1Loc;
			}
		}

		protected Rectangle _buttonYloc
		{
			get
			{
				return this._button2Loc;
			}
		}

		protected Rectangle _buttonXloc
		{
			get
			{
				return this._button3Loc;
			}
		}

		public int OptionSelected
		{
			get
			{
				return this._optionSelected;
			}
		}

		public DialogScreen(string title, string description, string[] options, bool printCancel, Texture2D bgImage, SpriteFont font, bool drawBehind)
			: base(true, drawBehind)
		{
			this.Title = title;
			this._descriptionText = new TextRegionElement(description, font);
			this._descriptionText.OutlineWidth = 1;
			if (options != null)
			{
				this._options = new string[options.Length];
			}
			this._options = options;
			this._bgImage = bgImage;
			this._font = font;
			if (options == null)
			{
				this.optionCurrentlySelected = 0;
			}
			this._buttonOptions = new string[printCancel ? 2 : 1];
			this._buttonOptions[0] = " " + CommonResources.OK;
			if (printCancel)
			{
				this._buttonOptions[1] = " " + CommonResources.Cancel;
			}
		}

		public DialogScreen(string title, string description, string[] buttonOptions, Texture2D bgImage, SpriteFont font, bool drawBehind)
			: base(true, drawBehind)
		{
			this.Title = title;
			this._descriptionText = new TextRegionElement(description, font);
			this._descriptionText.OutlineWidth = 1;
			this._options = null;
			this._bgImage = bgImage;
			this._font = font;
			this.optionCurrentlySelected = 0;
			if (buttonOptions != null)
			{
				this._buttonOptions = new string[buttonOptions.Length];
				this._buttonOptions = buttonOptions;
				return;
			}
			this._buttonOptions = new string[1];
			this._buttonOptions[0] = " " + CommonResources.OK;
		}

		public void SetButtonOptions(string[] buttonOptions)
		{
			this._button0Loc = (this._button1Loc = (this._button2Loc = (this._button3Loc = Rectangle.Empty)));
			if (buttonOptions != null)
			{
				this._buttonOptions = new string[buttonOptions.Length];
				this._buttonOptions = buttonOptions;
				return;
			}
			this._buttonOptions = new string[1];
			this._buttonOptions[0] = " " + CommonResources.OK;
		}

		public override void OnPushed()
		{
			if (this.OpenSound != null)
			{
				SoundManager.Instance.PlayInstance(this.OpenSound);
			}
			float w = (float)this._bgImage.Width;
			this.GetOptionsLines(w);
			base.OnPushed();
		}

		private void GetOptionsLines(float w)
		{
			if (!this._optionsLinesCalculated && this._options != null)
			{
				this._optionsLinesCalculated = true;
				for (int i = 0; i < this._options.Length; i++)
				{
					string currentOption = this._options[i];
					float currentCount = 0f;
					int last = 0;
					int start = 0;
					if (currentOption != null)
					{
						for (int j = 0; j < currentOption.Length; j++)
						{
							if (currentOption[j] == '\n')
							{
								if (this._font.MeasureString(currentOption.Substring(start, j - start + 1)).X > w - this.OptionsPadding.X * 2f)
								{
									this.optionLinesToPrint.Add(currentOption.Substring(start, last - start));
									if (this.optionsStartLine.Count < i + 1)
									{
										this.optionsStartLine.Add(this.optionLinesToPrint.Count - 1);
									}
									this.optionLinesToPrint.Add(currentOption.Substring(last + 1, j - last));
								}
								else
								{
									this.optionLinesToPrint.Add(currentOption.Substring(start, j - start));
									if (this.optionsStartLine.Count < i + 1)
									{
										this.optionsStartLine.Add(this.optionLinesToPrint.Count - 1);
									}
								}
								start = j + 1;
								currentCount = 0f;
								last = j;
							}
							if (currentOption[j] == ' ')
							{
								float subStringLength = this._font.MeasureString(currentOption.Substring(last, j - last)).X;
								currentCount += subStringLength;
								if (currentCount > w - this.OptionsPadding.X * 2f)
								{
									this.optionLinesToPrint.Add(currentOption.Substring(start, last - start + 1));
									if (this.optionsStartLine.Count < i + 1)
									{
										this.optionsStartLine.Add(this.optionLinesToPrint.Count - 1);
									}
									start = last + 1;
									currentCount = subStringLength;
									last = j + 1;
								}
								else
								{
									last = j;
								}
							}
							if (j == currentOption.Length - 1)
							{
								if (this._font.MeasureString(currentOption.Substring(start, j - start + 1)).X > w - this.OptionsPadding.X * 2f)
								{
									this.optionLinesToPrint.Add(currentOption.Substring(start, last - start + 1));
									if (this.optionsStartLine.Count < i + 1)
									{
										this.optionsStartLine.Add(this.optionLinesToPrint.Count - 1);
									}
									this.optionLinesToPrint.Add(currentOption.Substring(last + 1, j - last));
								}
								else
								{
									this.optionLinesToPrint.Add(currentOption.Substring(start, j - start + 1));
									if (this.optionsStartLine.Count < i + 1)
									{
										this.optionsStartLine.Add(this.optionLinesToPrint.Count - 1);
									}
								}
							}
						}
					}
				}
			}
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle titleSafe = Screen.Adjuster.ScreenRect;
			float w = (float)this._bgImage.Width;
			float h = (float)this._bgImage.Height;
			Rectangle DialogDestination = new Rectangle((int)((float)titleSafe.Center.X - w / 2f), (int)((float)titleSafe.Center.Y - h / 2f), (int)w, (int)h);
			if (this.DummyTexture == null)
			{
				this.DummyTexture = new Texture2D(device, 1, 1);
				this.DummyTexture.SetData<Color>(new Color[] { Color.White });
			}
			spriteBatch.Begin();
			Rectangle dest = new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height);
			spriteBatch.Draw(this.DummyTexture, dest, new Color(0f, 0f, 0f, 0.5f));
			spriteBatch.Draw(this._bgImage, DialogDestination, Color.White);
			spriteBatch.DrawOutlinedText(this._font, this.Title, new Vector2((float)DialogDestination.X + this.TitlePadding.X, (float)DialogDestination.Y + this.TitlePadding.Y), this.TitleColor, Color.Black, 1);
			float lineHeight = (float)DialogDestination.Y + this.DescriptionPadding.Y;
			float singleLineHeight = (float)this._font.LineSpacing;
			float xLoc = (float)DialogDestination.X + this.DescriptionPadding.X;
			float yLoc = (float)DialogDestination.Y + this.DescriptionPadding.Y + (float)this._font.LineSpacing;
			this._descriptionText.Location = new Vector2(xLoc, yLoc);
			this._descriptionText.Size = new Vector2(w - this.DescriptionPadding.X * 2f, h - this.DescriptionPadding.Y - (float)this._font.LineSpacing);
			this._descriptionText.Draw(device, spriteBatch, gameTime, false);
			this._endOfDescriptionLoc = yLoc + singleLineHeight;
			lineHeight = (float)(DialogDestination.Y + DialogDestination.Height) - this.OptionsPadding.Y - this._font.MeasureString(this.Title).Y * (float)(this.optionLinesToPrint.Count + 2) - this.ButtonsPadding.Y;
			for (int i = 0; i < this.optionLinesToPrint.Count; i++)
			{
				if (i >= this.optionsStartLine[this.optionCurrentlySelected])
				{
					if (this.optionCurrentlySelected == this._options.Length - 1 || i < this.optionsStartLine[this.optionCurrentlySelected + 1])
					{
						if (i == this.optionsStartLine[this.optionCurrentlySelected])
						{
							this.flashTimer.Update(gameTime.ElapsedGameTime);
							if (this.flashTimer.Expired)
							{
								this.flashTimer.Reset();
								this.selectedDirection = !this.selectedDirection;
							}
						}
						Color color;
						if (this.selectedDirection)
						{
							color = Color.Lerp(this.OptionsColor, this.OptionsSelectedColor, this.flashTimer.PercentComplete);
						}
						else
						{
							color = Color.Lerp(this.OptionsSelectedColor, this.OptionsColor, this.flashTimer.PercentComplete);
						}
						lineHeight += this._font.MeasureString(this.Title).Y;
						spriteBatch.DrawOutlinedText(this._font, this.optionLinesToPrint[i], new Vector2((float)DialogDestination.X + this.OptionsPadding.X, lineHeight), color, Color.Black, 1);
					}
					else
					{
						lineHeight += this._font.MeasureString(this.Title).Y;
						spriteBatch.DrawOutlinedText(this._font, this.optionLinesToPrint[i], new Vector2((float)DialogDestination.X + this.OptionsPadding.X, lineHeight), this.OptionsColor, Color.Black, 1);
					}
				}
				else
				{
					lineHeight += this._font.MeasureString(this.Title).Y;
					spriteBatch.DrawOutlinedText(this._font, this.optionLinesToPrint[i], new Vector2((float)DialogDestination.X + this.OptionsPadding.X, lineHeight), this.OptionsColor, Color.Black, 1);
				}
			}
			Vector2 size = this._font.MeasureString(this._buttonOptions[0]);
			float imagefactor = size.Y / (float)ControllerImages.A.Height;
			int imageWidth = (int)((float)ControllerImages.A.Width * imagefactor);
			lineHeight = (float)(DialogDestination.Y + DialogDestination.Height) - this.ButtonsPadding.Y - this._font.MeasureString(this.Title).Y;
			spriteBatch.Draw(ControllerImages.A, new Rectangle((int)((float)DialogDestination.X + this.ButtonsPadding.X), (int)lineHeight, imageWidth, (int)size.Y), Color.White);
			this._button0Loc = new Rectangle((int)((float)DialogDestination.X + this.ButtonsPadding.X + (float)imageWidth), (int)lineHeight, (int)size.X, (int)size.Y);
			spriteBatch.DrawOutlinedText(this._font, this._buttonOptions[0], new Vector2((float)DialogDestination.X + this.ButtonsPadding.X + (float)imageWidth, lineHeight), this.ButtonsColor, Color.Black, 1);
			if (this._buttonOptions.Length > 1)
			{
				size = this._font.MeasureString(this._buttonOptions[1]);
				imagefactor = size.Y / (float)ControllerImages.B.Height;
				imageWidth = (int)((float)ControllerImages.B.Width * imagefactor);
				spriteBatch.Draw(ControllerImages.B, new Rectangle((int)((float)DialogDestination.X + this.ButtonsPadding.X + (float)imageWidth + this._font.MeasureString(this._buttonOptions[0]).X + 10f), (int)lineHeight, imageWidth, (int)size.Y), Color.White);
				this._button1Loc = new Rectangle((int)((float)DialogDestination.X + this.ButtonsPadding.X + (float)(imageWidth * 2) + this._font.MeasureString(this._buttonOptions[0]).X + 10f), (int)lineHeight, (int)size.X, (int)size.Y);
				spriteBatch.DrawOutlinedText(this._font, this._buttonOptions[1], new Vector2((float)DialogDestination.X + this.ButtonsPadding.X + (float)(imageWidth * 2) + this._font.MeasureString(this._buttonOptions[0]).X + 10f, lineHeight), this.ButtonsColor, Color.Black, 1);
				if (this._buttonOptions.Length > 2)
				{
					size = this._font.MeasureString(this._buttonOptions[2]);
					imagefactor = size.Y / (float)ControllerImages.Y.Height;
					imageWidth = (int)((float)ControllerImages.Y.Width * imagefactor);
					spriteBatch.Draw(ControllerImages.Y, new Rectangle((int)((float)DialogDestination.X + this.ButtonsPadding.X + (float)(imageWidth * 2) + this._font.MeasureString(this._buttonOptions[0] + this._buttonOptions[1]).X + 20f), (int)lineHeight, imageWidth, (int)size.Y), Color.White);
					this._button2Loc = new Rectangle((int)((float)DialogDestination.X + this.ButtonsPadding.X + (float)(imageWidth * 3) + this._font.MeasureString(this._buttonOptions[0] + this._buttonOptions[1]).X + 20f), (int)lineHeight, (int)size.X, (int)size.Y);
					spriteBatch.DrawOutlinedText(this._font, this._buttonOptions[2], new Vector2((float)DialogDestination.X + this.ButtonsPadding.X + (float)(imageWidth * 3) + this._font.MeasureString(this._buttonOptions[0] + this._buttonOptions[1]).X + 20f, lineHeight), this.ButtonsColor, Color.Black, 1);
					if (this._buttonOptions.Length > 3)
					{
						size = this._font.MeasureString(this._buttonOptions[3]);
						imagefactor = size.Y / (float)ControllerImages.X.Height;
						imageWidth = (int)((float)ControllerImages.X.Width * imagefactor);
						spriteBatch.Draw(ControllerImages.X, new Rectangle((int)((float)DialogDestination.X + this.ButtonsPadding.X + (float)(imageWidth * 3) + this._font.MeasureString(this._buttonOptions[0]).X + this._font.MeasureString(this._buttonOptions[1]).X + this._font.MeasureString(this._buttonOptions[2]).X + 30f), (int)lineHeight, imageWidth, (int)size.Y), Color.White);
						this._button3Loc = new Rectangle((int)((float)DialogDestination.X + this.ButtonsPadding.X + (float)(imageWidth * 4) + this._font.MeasureString(this._buttonOptions[0]).X + this._font.MeasureString(this._buttonOptions[1]).X + this._font.MeasureString(this._buttonOptions[2]).X + 30f), (int)lineHeight, (int)size.X, (int)size.Y);
						spriteBatch.DrawOutlinedText(this._font, this._buttonOptions[3], new Vector2((float)DialogDestination.X + this.ButtonsPadding.X + (float)(imageWidth * 4) + this._font.MeasureString(this._buttonOptions[0]).X + this._font.MeasureString(this._buttonOptions[1]).X + this._font.MeasureString(this._buttonOptions[2]).X + 30f, lineHeight), this.ButtonsColor, Color.Black, 1);
					}
				}
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override bool OnPlayerInput(InputManager input, GameController controller, KeyboardInput chatpad, GameTime gameTime)
		{
			if (controller.PressedButtons.A || controller.PressedButtons.Start || input.Keyboard.WasKeyPressed(Keys.Enter) || (input.Mouse.LeftButtonPressed && this._button0Loc.Contains(input.Mouse.Position)))
			{
				this._optionSelected = this.optionCurrentlySelected;
				if (this.ClickSound != null)
				{
					SoundManager.Instance.PlayInstance(this.ClickSound);
				}
				base.PopMe();
				if (this.Callback != null)
				{
					this.Callback();
				}
			}
			if (this._buttonOptions.Length > 2 && (controller.PressedButtons.Y || (input.Mouse.LeftButtonPressed && this._button2Loc.Contains(input.Mouse.Position))))
			{
				this._optionSelected = 2;
				if (this.ClickSound != null)
				{
					SoundManager.Instance.PlayInstance(this.ClickSound);
				}
				base.PopMe();
				if (this.Callback != null)
				{
					this.Callback();
				}
			}
			if (this._buttonOptions.Length > 3 && (controller.PressedButtons.X || (input.Mouse.LeftButtonPressed && this._button3Loc.Contains(input.Mouse.Position))))
			{
				this._optionSelected = 3;
				if (this.ClickSound != null)
				{
					SoundManager.Instance.PlayInstance(this.ClickSound);
				}
				base.PopMe();
				if (this.Callback != null)
				{
					this.Callback();
				}
			}
			if (controller.PressedButtons.Back || controller.PressedButtons.B || input.Keyboard.WasKeyPressed(Keys.Escape) || (input.Mouse.LeftButtonPressed && this._button1Loc.Contains(input.Mouse.Position)))
			{
				this._optionSelected = -1;
				if (this.ClickSound != null)
				{
					SoundManager.Instance.PlayInstance(this.ClickSound);
				}
				base.PopMe();
				if (this.Callback != null)
				{
					this.Callback();
				}
			}
			if (controller.CurrentState.ThumbSticks.Left.Y > 0f || controller.CurrentState.IsButtonDown(Buttons.DPadUp) || input.Keyboard.IsKeyDown(Keys.Up))
			{
				if (this._options != null && !this.JoystickMoved)
				{
					this.JoystickMoved = true;
					if (this.optionCurrentlySelected > 0)
					{
						this.optionCurrentlySelected--;
						if (this.ClickSound != null)
						{
							SoundManager.Instance.PlayInstance(this.ClickSound);
						}
					}
				}
			}
			else if (controller.CurrentState.ThumbSticks.Left.Y < 0f || controller.CurrentState.IsButtonDown(Buttons.DPadDown) || input.Keyboard.IsKeyDown(Keys.Down))
			{
				if (this._options != null && !this.JoystickMoved)
				{
					this.JoystickMoved = true;
					if (this.optionCurrentlySelected < this._options.Length - 1)
					{
						this.optionCurrentlySelected++;
						if (this.ClickSound != null)
						{
							SoundManager.Instance.PlayInstance(this.ClickSound);
						}
					}
				}
			}
			else if (this.JoystickMoved)
			{
				this.JoystickMoved = false;
			}
			return base.OnPlayerInput(input, controller, chatpad, gameTime);
		}

		public string Title;

		private string[] _options;

		protected Texture2D _bgImage;

		protected SpriteFont _font;

		public Vector2 TitlePadding = new Vector2(20f, 5f);

		public Vector2 DescriptionPadding = new Vector2(10f, 10f);

		public Vector2 OptionsPadding = new Vector2(10f, 10f);

		public Vector2 ButtonsPadding = new Vector2(10f, 10f);

		public Color TitleColor = Color.White;

		public Color DescriptionColor = Color.White;

		public Color OptionsColor = Color.White;

		public Color OptionsSelectedColor = Color.Red;

		public Color ButtonsColor = Color.White;

		public string ClickSound;

		public string OpenSound;

		private TextRegionElement _descriptionText;

		private List<string> optionLinesToPrint = new List<string>();

		private List<int> optionsStartLine = new List<int>();

		private bool _optionsLinesCalculated;

		protected int _optionSelected = -1;

		private int optionCurrentlySelected;

		private OneShotTimer flashTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private bool selectedDirection;

		private bool JoystickMoved;

		public ThreadStart Callback;

		private Texture2D DummyTexture;

		private Rectangle _button0Loc = default(Rectangle);

		private Rectangle _button1Loc = default(Rectangle);

		private Rectangle _button2Loc = Rectangle.Empty;

		private Rectangle _button3Loc = Rectangle.Empty;

		protected string[] _buttonOptions;

		protected float _endOfDescriptionLoc;
	}
}
