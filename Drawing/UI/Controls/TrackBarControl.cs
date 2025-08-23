using System;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI.Controls
{
	public class TrackBarControl : UIControl
	{
		public int MinValue
		{
			get
			{
				return this._minValue;
			}
			set
			{
				this._minValue = value;
			}
		}

		public int MaxValue
		{
			get
			{
				return this._maxValue;
			}
			set
			{
				this._maxValue = value;
			}
		}

		public override Size Size
		{
			get
			{
				return this._size;
			}
			set
			{
				this._size = new Size(value.Width, 20);
			}
		}

		public int Value
		{
			get
			{
				return this._value;
			}
			set
			{
				this._value = value;
			}
		}

		protected bool Hovering
		{
			get
			{
				return this._hovering;
			}
			set
			{
				this._hovering = value;
			}
		}

		protected override void OnInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			bool hitTest = this.GetSliderLocation(base.ScreenBounds).Contains(inputManager.Mouse.Position);
			if (hitTest)
			{
				this.Hovering = true;
				if (inputManager.Mouse.LeftButtonPressed)
				{
					base.CaptureInput = true;
				}
			}
			else
			{
				this.Hovering = false;
			}
			if (inputManager.Mouse.LeftButtonReleased)
			{
				base.CaptureInput = false;
			}
			if (base.CaptureInput)
			{
				Rectangle trackBounds = this.GetTrackLocation(base.ScreenBounds);
				int screen = inputManager.Mouse.Position.X;
				int value = (screen - trackBounds.Left) * (this.MaxValue - this.MinValue) / (trackBounds.Width - 1);
				value += this.MinValue;
				if (value > this.MaxValue)
				{
					this.Value = this.MaxValue;
				}
				else if (value < this.MinValue)
				{
					this.Value = this.MinValue;
				}
				else
				{
					this.Value = value;
				}
			}
			base.OnInput(inputManager, controller, chatPad, gameTime);
		}

		private Rectangle GetSliderLocation(Rectangle screenBounds)
		{
			Rectangle trackBounds = this.GetTrackLocation(screenBounds);
			int sliderX = this._value * (trackBounds.Width - 1) / (this.MaxValue - this.MinValue) + trackBounds.Left;
			return new Rectangle(sliderX - 6, screenBounds.Y, 12, screenBounds.Height);
		}

		private Rectangle GetTrackLocation(Rectangle screenBounds)
		{
			return new Rectangle(screenBounds.Left, screenBounds.Center.Y - 4, screenBounds.Width - 6, 8);
		}

		public override bool HitTest(Point screenPoint)
		{
			return this.GetSliderLocation(base.ScreenBounds).Contains(screenPoint);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle dest = base.ScreenBounds;
			Rectangle sliderRect = this.GetSliderLocation(dest);
			Rectangle trackRect = this.GetTrackLocation(dest);
			Rectangle filledSliderRect = new Rectangle(trackRect.X, trackRect.Y, sliderRect.Center.X - trackRect.X, trackRect.Height);
			Color col = Color.White;
			if (base.CaptureInput)
			{
				col = Color.Black;
			}
			else if (this.Hovering)
			{
				col = Color.Gray;
			}
			spriteBatch.Draw(UIControl.DummyTexture, trackRect, Color.Black);
			trackRect.Inflate(-1, -1);
			spriteBatch.Draw(UIControl.DummyTexture, trackRect, Color.White);
			filledSliderRect.Inflate(-1, -1);
			spriteBatch.Draw(UIControl.DummyTexture, filledSliderRect, this.FillColor);
			spriteBatch.Draw(UIControl.DummyTexture, sliderRect, Color.Black);
			sliderRect.Inflate(-1, -1);
			spriteBatch.Draw(UIControl.DummyTexture, sliderRect, col);
		}

		private const int ControlHeight = 20;

		private const int SliderWidth = 12;

		private const int TrackWidth = 8;

		public Color FillColor = Color.Gray;

		private int _minValue;

		private int _maxValue = 100;

		private Size _size = new Size(100, 20);

		private int _value = 50;

		private bool _hovering;
	}
}
