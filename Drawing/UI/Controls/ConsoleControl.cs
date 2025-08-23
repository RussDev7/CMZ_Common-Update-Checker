using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DNA.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI.Controls
{
	public class ConsoleControl : UIControl, IConsole
	{
		public override Size Size
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

		private int LinesSupported
		{
			get
			{
				return 100;
			}
		}

		public ConsoleControl()
		{
			this._textWriter = new ConsoleControl.ConsoleTextWriter(this);
		}

		public ConsoleControl(SpriteFont font)
		{
			this._font = font;
			this._textWriter = new ConsoleControl.ConsoleTextWriter(this);
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
				this._currentMessage = new ConsoleControl.Message("");
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
				this._currentMessage = new ConsoleControl.Message("");
			}
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			lock (this._messages)
			{
				if (this.messages.Length != this._messages.Count)
				{
					this.messages = new ConsoleControl.Message[this._messages.Count];
				}
				this._messages.CopyTo(this.messages, 0);
			}
			int lineSpacing = this._font.LineSpacing;
			Vector2 pos = new Vector2((float)base.ScreenPosition.X, (float)base.ScreenBounds.Bottom);
			StringBuilder outputMessage = new StringBuilder();
			for (int i = this.messages.Length - 1; i >= 0; i--)
			{
				ConsoleControl.Message j = this.messages[i];
				j.Update(gameTime);
				DrawingTools.SplitText(j.Text, outputMessage, this._font, this.Size.Width);
				Vector2 textSize = this._font.MeasureString(outputMessage);
				if (pos.Y - textSize.Y < (float)base.ScreenBounds.Top)
				{
					if (textSize.Y <= (float)this._font.LineSpacing)
					{
						return;
					}
					while (pos.Y - textSize.Y < (float)base.ScreenBounds.Top)
					{
						int index = outputMessage.IndexOf('\n');
						if (index == -1)
						{
							throw new Exception("Don't know how this can happen");
						}
						outputMessage.Remove(0, index + 1);
						textSize = this._font.MeasureString(outputMessage);
					}
				}
				pos.Y -= textSize.Y;
				spriteBatch.DrawString(this._font, outputMessage, pos, Color.Black);
				if (pos.Y < (float)base.ScreenPosition.Y)
				{
					return;
				}
			}
		}

		private ConsoleControl.ConsoleTextWriter _textWriter;

		private SpriteFont _font;

		private ConsoleControl.Message _currentMessage = new ConsoleControl.Message("");

		private Queue<ConsoleControl.Message> _messages = new Queue<ConsoleControl.Message>();

		private Size _size;

		private ConsoleControl.Message[] messages = new ConsoleControl.Message[0];

		private class ConsoleTextWriter : TextWriter
		{
			private ConsoleControl Owner
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

			public ConsoleTextWriter(ConsoleControl control)
			{
				this.NewLine = "\n";
				this._owner = control;
			}

			private ConsoleControl _owner;
		}

		private class Message
		{
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
			}

			public string Text;

			public Color Color;
		}
	}
}
