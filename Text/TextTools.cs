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
			int num = 0;
			while (num + starta < a.Length && num + startb < b.Length && a[starta + num] == b[startb + num])
			{
				num++;
			}
			return num;
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
			foreach (string text in strings)
			{
				source = source.Replace(text, newValue);
			}
			return source;
		}

		public static string Capitalize(this string word)
		{
			StringBuilder stringBuilder = new StringBuilder(word.ToLower());
			stringBuilder[0] = char.ToUpper(stringBuilder[0]);
			return stringBuilder.ToString();
		}

		public static string[] SplitWords(this string text)
		{
			MatchCollection matchCollection = TextTools.SplitWordsRE.Matches(text);
			string[] array = new string[matchCollection.Count];
			int num = 0;
			foreach (object obj in matchCollection)
			{
				Match match = (Match)obj;
				array[num++] = match.Value;
			}
			return array;
		}

		public static string IntsToString(int[] ints)
		{
			byte[] array = TextTools.IntsToBytes(ints);
			string @string = Encoding.UTF8.GetString(array, 0, array.Length);
			return @string.Replace("\0", "");
		}

		public static byte[] IntsToBytes(int[] ints)
		{
			byte[] array = new byte[ints.Length * 4];
			int num = 0;
			for (int i = 0; i < ints.Length; i++)
			{
				array[num++] = (byte)ints[i];
				array[num++] = (byte)(ints[i] >> 8);
				array[num++] = (byte)(ints[i] >> 16);
				array[num++] = (byte)(ints[i] >> 24);
			}
			return array;
		}

		public static int[] BytesToInts(byte[] strBytes)
		{
			List<int> list = new List<int>();
			int num = 0;
			int i = 0;
			int num2 = 0;
			while (i < strBytes.Length)
			{
				if (num >= 32)
				{
					list.Add(num2);
					num2 = 0;
					num = 0;
				}
				num2 |= (int)strBytes[i] << num;
				i++;
				num += 8;
			}
			if (num > 0)
			{
				list.Add(num2);
			}
			return list.ToArray();
		}

		public static int[] StringToInts(string str, int maxInts)
		{
			Encoding utf = Encoding.UTF8;
			char[] array = str.ToCharArray();
			new List<int>();
			int num = maxInts * 4;
			int num2 = 0;
			while (utf.GetByteCount(array, 0, num2) < num && num2 < array.Length)
			{
				num2++;
			}
			if (utf.GetByteCount(array, 0, num2) > num)
			{
				num2--;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(array, 0, num2);
			return TextTools.BytesToInts(bytes);
		}

		public static int[] StringToInts(string str)
		{
			new List<int>();
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			return TextTools.BytesToInts(bytes);
		}

		private static Regex SplitWordsRE = new Regex("[a-zA-Z]+", RegexOptions.Compiled);
	}
}
