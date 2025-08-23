using System;
using System.Text;

namespace DNA.Text
{
	public static class TextConverter
	{
		private static char Base32IndexToChar(int index)
		{
			char rchar;
			if (index < 10)
			{
				rchar = (char)(48 + index);
			}
			else
			{
				rchar = (char)(65 + (index - 10));
			}
			char c = rchar;
			switch (c)
			{
			case '0':
				rchar = 'W';
				break;
			case '1':
				rchar = 'X';
				break;
			default:
				if (c != 'I')
				{
					if (c == 'O')
					{
						rchar = 'Y';
					}
				}
				else
				{
					rchar = 'Z';
				}
				break;
			}
			return rchar;
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
			int currentNum;
			if (rchar >= '0' && rchar <= '9')
			{
				currentNum = (int)(rchar - '0');
			}
			else
			{
				if (rchar < 'A' || rchar > 'Z')
				{
					throw new FormatException("charactor is out of Base32 Range");
				}
				currentNum = (int)(rchar - 'A' + '\n');
			}
			return currentNum;
		}

		public static string ToBase32String(byte[] bytes)
		{
			StringBuilder sb = new StringBuilder();
			int bits = 0;
			int currentByteIndex = 0;
			int currentByte = 0;
			while (currentByteIndex < bytes.Length)
			{
				if (bits <= 8 && currentByteIndex < bytes.Length)
				{
					byte newByte = bytes[currentByteIndex];
					currentByteIndex++;
					currentByte |= (int)newByte << bits;
					bits += 8;
				}
				char rchar = TextConverter.Base32IndexToChar(currentByte & 31);
				sb.Append(rchar);
				currentByte >>= 5;
				bits -= 5;
			}
			while (bits > 0)
			{
				char rchar2 = TextConverter.Base32IndexToChar(currentByte & 31);
				sb.Append(rchar2);
				currentByte >>= 5;
				bits -= 5;
			}
			return sb.ToString();
		}

		public static byte[] FromBase32String(string str)
		{
			str = str.ToUpper();
			int numBytes = str.Length * 5 / 8;
			byte[] bytes = new byte[numBytes];
			int bitCount = 0;
			int currentCharIndex = 0;
			int data = 0;
			int outIndex = 0;
			while (currentCharIndex < str.Length)
			{
				while (bitCount < 8)
				{
					int currentNum = TextConverter.Base32CharToIndex(str[currentCharIndex++]);
					data |= currentNum << bitCount;
					bitCount += 5;
				}
				byte databyte = (byte)(data & 255);
				data >>= 8;
				bitCount -= 8;
				bytes[outIndex++] = databyte;
			}
			return bytes;
		}
	}
}
