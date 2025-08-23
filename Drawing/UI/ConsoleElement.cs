using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI
{
	public class ConsoleElement : UIElement, IConsole
	{
		private int LinesSupported
		{
			get
			{
				return 100;
			}
		}

		public ConsoleElement(SpriteFont font)
		{
			this._font = font;
			this._textWriter = new ConsoleElement.ConsoleTextWriter(this);
		}

		public void GrabConsole()
		{
			GameConsole.SetControl(this);
			Console.SetOut(this._textWriter);
		}

		public void Write(char value)
		{
			if (value == '\n')
			{
				this.WriteLine();
				return;
			}
			this._currentMessage.Append(value.ToString());
		}

		public void Write(string value)
		{
			string[] strs = value.Split(new char[] { '\n' });
			for (int i = 0; i < strs.Length; i++)
			{
				this._currentMessage.Append(strs[i]);
				if (i < strs.Length - 1)
				{
					this.WriteLine();
				}
			}
		}

		public void WriteLine(string value)
		{
			this.Write(value);
			this.WriteLine();
		}

		public void WriteLine()
		{
			lock (this._messages)
			{
				this._messages.Enqueue(this._currentMessage);
				this._currentMessage = new ConsoleElement.Message("");
				while (this._messages.Count > this.LinesSupported)
				{
					this._messages.Dequeue();
				}
			}
		}

		public void Clear()
		{
			lock (this._messages)
			{
				this._messages.Clear();
				this._currentMessage = new ConsoleElement.Message("");
			}
		}

		public override Vector2 Size
		{
			get
			{
				return this._size;
			}
			set
			{
				this._size = value;
			}
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime, bool selected)
		{
			lock (this._messages)
			{
				if (this.messages.Length != this._messages.Count)
				{
					this.messages = new ConsoleElement.Message[this._messages.Count];
				}
				this._messages.CopyTo(this.messages, 0);
			}
			int height = this._font.LineSpacing;
			Vector2 pos = new Vector2(base.Location.X, base.Location.Y + this.Size.Y - (float)height);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, RasterizerState.CullNone);
			for (int i = this.messages.Length - 1; i >= 0; i--)
			{
				ConsoleElement.Message j = this.messages[i];
				j.Update(gameTime);
				if (this.ShowAll)
				{
					spriteBatch.DrawOutlinedText(this._font, j.Text, pos, base.Color, Color.Black, 1);
				}
				else if (j.Visibility > 0f)
				{
					spriteBatch.DrawOutlinedText(this._font, j.Text, pos, Color.Lerp(Color.Transparent, base.Color, j.Visibility), Color.Lerp(Color.Transparent, Color.Black, j.Visibility), 1);
				}
				pos.Y -= (float)height;
				if (pos.Y < base.Location.Y)
				{
					break;
				}
			}
			spriteBatch.End();
		}

		private ConsoleElement.ConsoleTextWriter _textWriter;

		private SpriteFont _font;

		private ConsoleElement.Message _currentMessage = new ConsoleElement.Message("");

		private Queue<ConsoleElement.Message> _messages = new Queue<ConsoleElement.Message>();

		public bool ShowAll;

		private Vector2 _size;

		private ConsoleElement.Message[] messages = new ConsoleElement.Message[0];

		private class ConsoleTextWriter : TextWriter
		{
			private ConsoleElement Owner
			{
				get
				{
					return this._owner;
				}
			}

			public override Encoding Encoding
			{
				get
				{
					return Encoding.UTF8;
				}
			}

			public override void WriteLine(string value)
			{
				this._owner.WriteLine(value);
			}

			public override void Write(char value)
			{
				this._owner.Write(value);
			}

			public ConsoleTextWriter(ConsoleElement control)
			{
				this.NewLine = "\n";
				this._owner = control;
			}

			private ConsoleElement _owner;
		}

		private class Message
		{
			public float Visibility
			{
				get
				{
					if (!this.lifeTimer.Expired)
					{
						return 1f;
					}
					return 1f - this.fadeTimer.PercentComplete;
				}
			}

			public Message()
			{
			}

			public Message(string text)
			{
				this.Text = text;
			}

			public Message(string text, Color color)
			{
				this.Text = text;
				this.Color = color;
			}

			public override string ToString()
			{
				return this.Text;
			}

			public void Append(string str)
			{
				this.Text += str;
			}

			public void Update(GameTime gameTime)
			{
				this.lifeTimer.Update(gameTime.ElapsedGameTime);
				if (this.lifeTimer.Expired)
				{
					this.fadeTimer.Update(gameTime.ElapsedGameTime);
				}
			}

			public string Text;

			public Color Color;

			private OneShotTimer lifeTimer = new OneShotTimer(TimeSpan.FromSeconds(10.0));

			private OneShotTimer fadeTimer = new OneShotTimer(TimeSpan.FromSeconds(3.0));
		}
	}
}
