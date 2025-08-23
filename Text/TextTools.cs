using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DNA.Text
{
	public static class TextTools
	{
		public static int IndexOf(this StringBuilder text, char c)
		{
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == c)
				{
					return i;
				}
			}
			return -1;
		}

		public static int CountSame(this string a, int starta, string b, int startb)
		{
			int same = 0;
			while (same + starta < a.Length && same + startb < b.Length && a[starta + same] == b[startb + same])
			{
				same++;
			}
			return same;
		}

		public static string RemovePatternWhiteSpace(this string source)
		{
			throw new NotImplementedException();
		}

		public static string ReplaceAny(this string source, char[] chars, string newValue)
		{
			foreach (char c in chars)
			{
				source = source.Replace(c.ToString(), newValue);
			}
			return source;
		}

		public static string ReplaceAny(this string source, string[] strings, string newValue)
		{
			foreach (string c in strings)
			{
				source = source.Replace(c, newValue);
			}
			return source;
		}

		public static string Capitalize(this string word)
		{
			StringBuilder sb = new StringBuilder(word.ToLower());
			sb[0] = char.ToUpper(sb[0]);
			return sb.ToString();
		}

		public static string[] SplitWords(this string text)
		{
			MatchCollection matches = TextTools.SplitWordsRE.Matches(text);
			string[] output = new string[matches.Count];
			int cnt = 0;
			foreach (object obj in matches)
			{
				Match i = (Match)obj;
				output[cnt++] = i.Value;
			}
			return output;
		}

		public static string IntsToString(int[] ints)
		{
			byte[] bytes = TextTools.IntsToBytes(ints);
			string ret = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
			return ret.Replace("\0", "");
		}

		public static byte[] IntsToBytes(int[] ints)
		{
			byte[] ret = new byte[ints.Length * 4];
			int byteIndex = 0;
			for (int i = 0; i < ints.Length; i++)
			{
				ret[byteIndex++] = (byte)ints[i];
				ret[byteIndex++] = (byte)(ints[i] >> 8);
				ret[byteIndex++] = (byte)(ints[i] >> 16);
				ret[byteIndex++] = (byte)(ints[i] >> 24);
			}
			return ret;
		}

		public static int[] BytesToInts(byte[] strBytes)
		{
			List<int> intsOut = new List<int>();
			int shift = 0;
			int byteIndex = 0;
			int acc = 0;
			while (byteIndex < strBytes.Length)
			{
				if (shift >= 32)
				{
					intsOut.Add(acc);
					acc = 0;
					shift = 0;
				}
				acc |= (int)strBytes[byteIndex] << shift;
				byteIndex++;
				shift += 8;
			}
			if (shift > 0)
			{
				intsOut.Add(acc);
			}
			return intsOut.ToArray();
		}

		public static int[] StringToInts(string str, int maxInts)
		{
			Encoding encoding = Encoding.UTF8;
			char[] chars = str.ToCharArray();
			new List<int>();
			int maxBytes = maxInts * 4;
			int charCount = 0;
			while (encoding.GetByteCount(chars, 0, charCount) < maxBytes && charCount < chars.Length)
			{
				charCount++;
			}
			if (encoding.GetByteCount(chars, 0, charCount) > maxBytes)
			{
				charCount--;
			}
			byte[] strBytes = Encoding.UTF8.GetBytes(chars, 0, charCount);
			return TextTools.BytesToInts(strBytes);
		}

		public static int[] StringToInts(string str)
		{
			new List<int>();
			byte[] strBytes = Encoding.UTF8.GetBytes(str);
			return TextTools.BytesToInts(strBytes);
		}

		private static Regex SplitWordsRE = new Regex("[a-zA-Z]+", RegexOptions.Compiled);
	}
}
