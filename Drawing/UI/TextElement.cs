using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI
{
	public class TextElement : UIElement
	{
		public TimeSpan PulseTime
		{
			get
			{
				return this._pulseTime;
			}
			set
			{
				this._pulseTime = value;
			}
		}

		public float PulseSize
		{
			get
			{
				return this._pulseSize;
			}
			set
			{
				this._pulseSize = value;
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
				if (this._text != value)
				{
					this._text = value;
					this._dirtyText = true;
				}
			}
		}

		public Color OutlineColor
		{
			get
			{
				return this._outLineColor;
			}
			set
			{
				this._outLineColor = value;
			}
		}

		public int OutlineWidth
		{
			get
			{
				return this._outLineWidth;
			}
			set
			{
				this._outLineWidth = value;
			}
		}

		public TextElement(SpriteFont font, string text, Vector2 position, Color color, Color outlineColor, int outlineWidth)
		{
			this.Text = text;
			this.Font = font;
			base.Location = position;
			base.Color = color;
			this.OutlineColor = Color.Black;
			this.OutlineWidth = outlineWidth;
		}

		public TextElement(SpriteFont font, string text, Vector2 position, Color color)
		{
			this.Text = text;
			this.Font = font;
			base.Location = position;
			base.Color = color;
		}

		public TextElement(string text, SpriteFont font)
		{
			this.Text = text;
			this.Font = font;
		}

		public TextElement(SpriteFont font)
		{
			this.Font = font;
		}

		public override Vector2 Size
		{
			get
			{
				return this.Font.MeasureString(this.Text);
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		protected virtual Color GetForColor(bool selected)
		{
			return base.Color;
		}

		protected virtual void ProcessText(string text, StringBuilder builder)
		{
			builder.Append(text);
		}

		protected void DirtyText()
		{
			this._dirtyText = true;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime, bool selected)
		{
			if (this._dirtyText)
			{
				this._textToDraw.Length = 0;
				this.ProcessText(this._text, this._textToDraw);
				this._dirtyText = false;
			}
			if (this._pulseTime > TimeSpan.Zero)
			{
				if (this._pulseDir)
				{
					this._currenPulseTime += gameTime.ElapsedGameTime;
					if (this._currenPulseTime > this._pulseTime)
					{
						this._currenPulseTime = this._pulseTime;
						this._pulseDir = !this._pulseDir;
					}
				}
				else
				{
					this._currenPulseTime -= gameTime.ElapsedGameTime;
					if (this._currenPulseTime < TimeSpan.Zero)
					{
						this._currenPulseTime = TimeSpan.Zero;
						this._pulseDir = !this._pulseDir;
					}
				}
				float factor = (float)(1.0 + (double)this.PulseSize * this._currenPulseTime.TotalSeconds / this._pulseTime.TotalSeconds) * (this.ScaleOnScreenResize ? Screen.Adjuster.ScaleFactor.Y : 1f);
				Vector2 org = new Vector2(this.Size.X / 2f, this.Size.Y / 2f);
				if (this.OutlineWidth > 0)
				{
					spriteBatch.DrawOutlinedText(this.Font, this._textToDraw, base.Location + org, this.GetForColor(selected), this.OutlineColor, this.OutlineWidth, factor, 0f, org);
					return;
				}
				spriteBatch.DrawString(this.Font, this._textToDraw, base.Location + org, this.GetForColor(selected), 0f, org, factor, SpriteEffects.None, 1f);
				return;
			}
			else
			{
				float factor2 = (this.ScaleOnScreenResize ? Screen.Adjuster.ScaleFactor.Y : 1f);
				if (this.OutlineWidth > 0)
				{
					spriteBatch.DrawOutlinedText(this.Font, this._textToDraw, base.Location, this.GetForColor(selected), this.OutlineColor, (int)Math.Ceiling((double)((float)this.OutlineWidth * factor2)), factor2, 0f, new Vector2(0f, 0f));
					return;
				}
				spriteBatch.DrawString(this.Font, this._textToDraw, base.Location, this.GetForColor(selected), 0f, new Vector2(0f, 0f), factor2, SpriteEffects.None, 0f);
				return;
			}
		}

		private bool _dirtyText = true;

		private StringBuilder _textToDraw = new StringBuilder();

		private bool _pulseDir;

		private TimeSpan _currenPulseTime = TimeSpan.FromSeconds(0.0);

		private TimeSpan _pulseTime = TimeSpan.FromSeconds(0.0);

		private float _pulseSize = 0.1f;

		private string _text = "<Text>";

		public SpriteFont Font;

		private Color _outLineColor = Color.Black;

		private int _outLineWidth = 2;

		public bool ScaleOnScreenResize = true;
	}
}
