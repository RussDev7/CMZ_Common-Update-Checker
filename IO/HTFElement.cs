using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DNA.IO
{
	[Serializable]
	public class HTFElement
	{
		public override string ToString()
		{
			if (this._id != "")
			{
				return this._id + "=" + this._value;
			}
			return this._value;
		}

		public string ID
		{
			get
			{
				return this._id;
			}
			set
			{
				this._id = value;
			}
		}

		public string Value
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

		public virtual List<HTFElement> Children
		{
			get
			{
				return this._children;
			}
		}

		public bool IsID(string id)
		{
			return string.Compare(id, this._id, StringComparison.CurrentCultureIgnoreCase) == 0;
		}

		public HTFElement()
		{
		}

		public HTFElement(Stream stream)
		{
			char c;
			for (;;)
			{
				int cv = stream.ReadByte();
				if (cv == -1)
				{
					break;
				}
				c = (char)cv;
				if (!char.IsWhiteSpace(c))
				{
					goto Block_2;
				}
			}
			throw new EndOfStreamException("No Elements Found");
			Block_2:
			if (c != '<')
			{
				throw new IOException("This is not a valid File");
			}
			this.ParseChildren(stream);
		}

		public HTFElement(string value)
		{
			this._value = value;
		}

		public HTFElement(string id, string value)
		{
			this._id = id;
			this._value = value;
		}

		private static char TranslateEscapeChar(char c)
		{
			if (c <= 'b')
			{
				if (c == '0')
				{
					return '\0';
				}
				switch (c)
				{
				case 'a':
					return '\a';
				case 'b':
					return '\b';
				}
			}
			else
			{
				if (c == 'f')
				{
					return '\f';
				}
				if (c == 'n')
				{
					return '\n';
				}
				switch (c)
				{
				case 'r':
					return '\r';
				case 't':
					return '\t';
				case 'v':
					return '\v';
				}
			}
			return c;
		}

		protected void ParseChildren(Stream stream)
		{
			int index = 0;
			int length = 0;
			bool moreChildren = true;
			while (moreChildren)
			{
				HTFElement newChild = new HTFElement();
				moreChildren = newChild.Parse(stream, out length);
				this._children.Add(newChild);
				index += length;
			}
		}

		private bool AddChildNode(Stream stream)
		{
			try
			{
				this.ParseChildren(stream);
			}
			catch (Exception e)
			{
				string message = e.Message;
				return false;
			}
			return true;
		}

		private bool Parse(Stream stream, out int length)
		{
			StringBuilder valueBuilder = new StringBuilder();
			bool done = false;
			bool moreValues = false;
			bool foundID = false;
			bool isLiteral = false;
			bool inQuotes = false;
			bool foundFirstChar = false;
			while (!done)
			{
				int nextval = stream.ReadByte();
				if (nextval == -1)
				{
					throw new EndOfStreamException("Unexpected End of File");
				}
				char c = (char)nextval;
				if (isLiteral)
				{
					c = HTFElement.TranslateEscapeChar(c);
					valueBuilder.Append(c);
					isLiteral = false;
				}
				else if (foundFirstChar || !char.IsWhiteSpace(c))
				{
					if (inQuotes)
					{
						if (c == '"')
						{
							inQuotes = false;
						}
						else if (c == '~')
						{
							isLiteral = true;
						}
						else
						{
							valueBuilder.Append(c);
						}
					}
					else if (c == ',')
					{
						done = true;
						moreValues = true;
					}
					else if (c == '>')
					{
						done = true;
						moreValues = false;
					}
					else if (c == '<')
					{
						this.ParseChildren(stream);
					}
					else if (c == '~')
					{
						isLiteral = true;
					}
					else if (c == '=' && !foundID)
					{
						this._id = valueBuilder.ToString().Trim();
						valueBuilder = new StringBuilder();
					}
					else if (c == '"')
					{
						inQuotes = true;
					}
					else
					{
						valueBuilder.Append(c);
					}
					foundFirstChar = true;
				}
			}
			this._value = valueBuilder.ToString().Trim();
			length = this._value.Length;
			return moreValues;
		}

		private static char GetEscapeChar(char c)
		{
			if (c != '\0')
			{
				switch (c)
				{
				case '\a':
					return 'a';
				case '\b':
					return 'b';
				case '\n':
					return 'n';
				case '\f':
					return 'f';
				case '\r':
					return 'r';
				}
				return c;
			}
			return '0';
		}

		private static bool NeedsEscape(char c)
		{
			return c == '~' || c == '\a' || c == '\b' || c == '\f' || c == '\n' || c == '\r' || c == '\0' || c == '<' || c == '>' || c == ',' || c == '=' || c == '"';
		}

		private static string BuildEscapedString(string s)
		{
			bool quoteText = true;
			bool needsEscape = false;
			foreach (char c in s)
			{
				if (HTFElement.NeedsEscape(c))
				{
					needsEscape = true;
				}
				if (c == '"' || c == '\a' || c == '\b' || c == '\f' || c == '\n' || c == '\r' || c == '\0')
				{
					quoteText = false;
					break;
				}
			}
			if (quoteText && needsEscape)
			{
				return "\"" + s + "\"";
			}
			StringBuilder br = new StringBuilder();
			foreach (char c2 in s)
			{
				if (HTFElement.NeedsEscape(c2))
				{
					br.Append('~');
					br.Append(HTFElement.GetEscapeChar(c2));
				}
				else
				{
					br.Append(c2);
				}
			}
			return br.ToString();
		}

		public void Save(StreamWriter writer)
		{
			this.Save(writer, 0);
		}

		private void Save(StreamWriter writer, int level)
		{
			if (this._id != "" && this._id != null)
			{
				writer.Write(HTFElement.BuildEscapedString(this._id));
				writer.Write('=');
			}
			writer.Write(HTFElement.BuildEscapedString(this._value));
			if (this.Children.Count > 0)
			{
				writer.Write('<');
				writer.WriteLine();
				for (int i = 0; i < this.Children.Count; i++)
				{
					for (int j = 0; j < level; j++)
					{
						writer.Write('\t');
					}
					this.Children[i].Save(writer, level + 1);
					if (i < this.Children.Count - 1)
					{
						writer.Write(',');
					}
					writer.WriteLine();
				}
				for (int k = 0; k < level; k++)
				{
					writer.Write('\t');
				}
				writer.Write('>');
			}
		}

		private const char EscapeChar = '~';

		private string _id = "";

		private string _value = "";

		private List<HTFElement> _children = new List<HTFElement>();
	}
}
