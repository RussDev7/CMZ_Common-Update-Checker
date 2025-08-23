using System;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI.Controls
{
	public class TextRegionControl : TextControl
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

		public TextRegionControl(SpriteFont font)
			: base(font)
		{
		}

		public TextRegionControl(string text, SpriteFont font)
			: base(text, font)
		{
		}

		protected override void ProcessText(string text, StringBuilder builder)
		{
			StringBuilder sb = new StringBuilder();
			StringBuilder sb2 = new StringBuilder();
			float sizeOfLine = (float)this._size.Width;
			foreach (char c in text)
			{
				sb.Append(c);
				if (char.IsSeparator(c))
				{
					if (this.Font.MeasureString(sb).X > sizeOfLine)
					{
						builder.Append(sb2);
						builder.Append('\n');
						sb.Remove(0, sb2.Length);
					}
					sb2.Length = 0;
					sb2.Append(sb);
				}
			}
			if (this.Font.MeasureString(sb).X > sizeOfLine)
			{
				builder.Append(sb2);
				builder.Append('\n');
				sb.Remove(0, sb2.Length);
			}
			builder.Append(sb);
		}

		private Size _size;
	}
}
