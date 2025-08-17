using System;
using System.Text;

namespace DNA.Text
{
	public static class TextConverter
	{
		private static char Base32IndexToChar(int index)
		{
			char c;
			if (index < 10)
			{
				c = (char)(48 + index);
			}
			else
			{
				c = (char)(65 + (index - 10));
			}
			char c2 = c;
			switch (c2)
			{
			case '0':
				c = 'W';
				break;
			case '1':
				c = 'X';
				break;
			default:
				if (c2 != 'I')
				{
					if (c2 == 'O')
					{
						c = 'Y';
					}
				}
				else
				{
					c = 'Z';
				}
				break;
			}
			return c;
		}

		private static int Base32CharToIndex(char rchar)
		{
			switch (rchar)
			{
			case 'W':
				rchar = '0';
				break;
			case 'X':
				rchar = '1';
				break;
			case 'Y':
				rchar = 'O';
				break;
			case 'Z':
				rchar = 'I';
				break;
			}
			int num;
			if (rchar >= '0' && rchar <= '9')
			{
				num = (int)(rchar - '0');
			}
			else
			{
				if (rchar < 'A' || rchar > 'Z')
				{
					throw new FormatException("charactor is out of Base32 Range");
				}
				num = (int)(rchar - 'A' + '\n');
			}
			return num;
		}

		public static string ToBase32String(byte[] bytes)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int i = 0;
			int j = 0;
			int num = 0;
			while (j < bytes.Length)
			{
				if (i <= 8 && j < bytes.Length)
				{
					byte b = bytes[j];
					j++;
					num |= (int)b << i;
					i += 8;
				}
				char c = TextConverter.Base32IndexToChar(num & 31);
				stringBuilder.Append(c);
				num >>= 5;
				i -= 5;
			}
			while (i > 0)
			{
				char c2 = TextConverter.Base32IndexToChar(num & 31);
				stringBuilder.Append(c2);
				num >>= 5;
				i -= 5;
			}
			return stringBuilder.ToString();
		}

		public static byte[] FromBase32String(string str)
		{
			str = str.ToUpper();
			int num = str.Length * 5 / 8;
			byte[] array = new byte[num];
			int i = 0;
			int j = 0;
			int num2 = 0;
			int num3 = 0;
			while (j < str.Length)
			{
				while (i < 8)
				{
					int num4 = TextConverter.Base32CharToIndex(str[j++]);
					num2 |= num4 << i;
					i += 5;
				}
				byte b = (byte)(num2 & 255);
				num2 >>= 8;
				i -= 8;
				array[num3++] = b;
			}
			return array;
		}
	}
}
