using System;
using System.Collections.Generic;

namespace DNA.Collections
{
	public static class ArrayTools
	{
		public static T[][] AllocSquareJaggedArray<T>(int x, int y)
		{
			T[][] ret = new T[x][];
			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = new T[y];
			}
			return ret;
		}

		public static void Randomize<T>(IList<T> array, Random rand)
		{
			if (array is T[])
			{
				T[] ary = (T[])array;
				for (int i = 0; i < ary.Length; i++)
				{
					int swapIndex = i + rand.Next(ary.Length - i);
					T temp = ary[swapIndex];
					ary[swapIndex] = ary[i];
					ary[i] = temp;
				}
				return;
			}
			for (int j = 0; j < array.Count; j++)
			{
				int swapIndex2 = j + rand.Next(array.Count - j);
				T temp2 = array[swapIndex2];
				array[swapIndex2] = array[j];
				array[j] = temp2;
			}
		}

		public static void Randomize<T>(IList<T> array)
		{
			ArrayTools.Randomize<T>(array, new Random());
		}

		public static void QSort<T>(IList<T> list, Comparison<T> comparison)
		{
			ArrayTools.QSort_r<T>(list, comparison, 0, list.Count - 1, Order.Ascending);
		}

		private static void QSort_r<T>(IList<T> list, Comparison<T> comparison, int d, int h, Order direction)
		{
			if (list.Count == 0)
			{
				return;
			}
			int i = h;
			int j = d;
			T obj = list[(d + h) / 2];
			do
			{
				if (direction == Order.Ascending)
				{
					while (comparison(list[j], obj) < 0)
					{
						j++;
					}
					while (comparison(list[i], obj) > 0)
					{
						i--;
					}
				}
				else
				{
					while (comparison(list[j], obj) > 0)
					{
						j++;
					}
					while (comparison(list[i], obj) < 0)
					{
						i--;
					}
				}
				if (i >= j)
				{
					if (i != j)
					{
						T zal = list[i];
						list[i] = list[j];
						list[j] = zal;
					}
					i--;
					j++;
				}
			}
			while (j <= i);
			if (d < i)
			{
				ArrayTools.QSort_r<T>(list, comparison, d, i, direction);
			}
			if (j < h)
			{
				ArrayTools.QSort_r<T>(list, comparison, j, h, direction);
			}
		}

		public static void QSort<T>(IList<T> list) where T : IComparable<T>
		{
			ArrayTools.QSort_r<T>(list, 0, list.Count - 1, Order.Ascending);
		}

		private static void QSort_r<T>(IList<T> list, int d, int h, Order direction) where T : IComparable<T>
		{
			if (list.Count == 0)
			{
				return;
			}
			int i = h;
			int j = d;
			T obj = list[(d + h) / 2];
			do
			{
				if (direction == Order.Ascending)
				{
					for (;;)
					{
						T t = list[j];
						if (t.CompareTo(obj) >= 0)
						{
							break;
						}
						j++;
					}
					for (;;)
					{
						T t2 = list[i];
						if (t2.CompareTo(obj) <= 0)
						{
							break;
						}
						i--;
					}
				}
				else
				{
					for (;;)
					{
						T t3 = list[j];
						if (t3.CompareTo(obj) <= 0)
						{
							break;
						}
						j++;
					}
					for (;;)
					{
						T t4 = list[i];
						if (t4.CompareTo(obj) >= 0)
						{
							break;
						}
						i--;
					}
				}
				if (i >= j)
				{
					if (i != j)
					{
						T zal = list[i];
						list[i] = list[j];
						list[j] = zal;
					}
					i--;
					j++;
				}
			}
			while (j <= i);
			if (d < i)
			{
				ArrayTools.QSort_r<T>(list, d, i, direction);
			}
			if (j < h)
			{
				ArrayTools.QSort_r<T>(list, j, h, direction);
			}
		}
	}
}
