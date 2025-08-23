using System;
using System.Text;

namespace DNA.Security.Cryptography.Utilities
{
	public sealed class Arrays
	{
		private Arrays()
		{
		}

		public static bool AreEqual(bool[] a, bool[] b)
		{
			if (a == b)
			{
				return true;
			}
			if (a == null || b == null)
			{
				return false;
			}
			if (a.Length != b.Length)
			{
				return false;
			}
			for (int i = 0; i < a.Length; i++)
			{
				if (a[i] != b[i])
				{
					return false;
				}
			}
			return true;
		}

		public static bool AreEqual(byte[] a, byte[] b)
		{
			return a == b || (a != null && b != null && Arrays.HaveSameContents(a, b));
		}

		[Obsolete("Use 'AreEqual' method instead")]
		public static bool AreSame(byte[] a, byte[] b)
		{
			return Arrays.AreEqual(a, b);
		}

		public static bool AreEqual(int[] a, int[] b)
		{
			return a == b || (a != null && b != null && Arrays.HaveSameContents(a, b));
		}

		private static bool HaveSameContents(byte[] a, byte[] b)
		{
			if (a.Length != b.Length)
			{
				return false;
			}
			for (int i = 0; i < a.Length; i++)
			{
				if (a[i] != b[i])
				{
					return false;
				}
			}
			return true;
		}

		private static bool HaveSameContents(int[] a, int[] b)
		{
			if (a.Length != b.Length)
			{
				return false;
			}
			for (int i = 0; i < a.Length; i++)
			{
				if (a[i] != b[i])
				{
					return false;
				}
			}
			return true;
		}

		public static string ToString(object[] a)
		{
			StringBuilder sb = new StringBuilder(91);
			if (a.Length > 0)
			{
				sb.Append(a[0]);
				for (int index = 1; index < a.Length; index++)
				{
					sb.Append(", ").Append(a[index]);
				}
			}
			sb.Append(']');
			return sb.ToString();
		}

		public static int GetHashCode(byte[] data)
		{
			if (data == null)
			{
				return 0;
			}
			int i = data.Length;
			int hc = i + 1;
			while (--i >= 0)
			{
				hc *= 257;
				hc ^= (int)data[i];
			}
			return hc;
		}

		public static byte[] Clone(byte[] data)
		{
			if (data != null)
			{
				return (byte[])data.Clone();
			}
			return null;
		}

		public static int[] Clone(int[] data)
		{
			if (data != null)
			{
				return (int[])data.Clone();
			}
			return null;
		}
	}
}
