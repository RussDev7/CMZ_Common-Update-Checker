using System;
using System.Collections.Generic;

namespace DNA.Collections
{
	public static class ArrayTools
	{
		public static T[][] AllocSquareJaggedArray<T>(int x, int y)
		{
			T[][] array = new T[x][];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new T[y];
			}
			return array;
		}

		public static void Randomize<T>(IList<T> array, Random rand)
		{
			if (array is T[])
			{
				T[] array2 = (T[])array;
				for (int i = 0; i < array2.Length; i++)
				{
					int num = i + rand.Next(array2.Length - i);
					T t = array2[num];
					array2[num] = array2[i];
					array2[i] = t;
				}
				return;
			}
			for (int j = 0; j < array.Count; j++)
			{
				int num2 = j + rand.Next(array.Count - j);
				T t2 = array[num2];
				array[num2] = array[j];
				array[j] = t2;
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
			int num = h;
			int num2 = d;
			T t = list[(d + h) / 2];
			do
			{
				if (direction == Order.Ascending)
				{
					while (comparison(list[num2], t) < 0)
					{
						num2++;
					}
					while (comparison(list[num], t) > 0)
					{
						num--;
					}
				}
				else
				{
					while (comparison(list[num2], t) > 0)
					{
						num2++;
					}
					while (comparison(list[num], t) < 0)
					{
						num--;
					}
				}
				if (num >= num2)
				{
					if (num != num2)
					{
						T t2 = list[num];
						list[num] = list[num2];
						list[num2] = t2;
					}
					num--;
					num2++;
				}
			}
			while (num2 <= num);
			if (d < num)
			{
				ArrayTools.QSort_r<T>(list, comparison, d, num, direction);
			}
			if (num2 < h)
			{
				ArrayTools.QSort_r<T>(list, comparison, num2, h, direction);
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
			int num = h;
			int num2 = d;
			T t = list[(d + h) / 2];
			do
			{
				if (direction == Order.Ascending)
				{
					for (;;)
					{
						T t2 = list[num2];
						if (t2.CompareTo(t) >= 0)
						{
							break;
						}
						num2++;
					}
					for (;;)
					{
						T t3 = list[num];
						if (t3.CompareTo(t) <= 0)
						{
							break;
						}
						num--;
					}
				}
				else
				{
					for (;;)
					{
						T t4 = list[num2];
						if (t4.CompareTo(t) <= 0)
						{
							break;
						}
						num2++;
					}
					for (;;)
					{
						T t5 = list[num];
						if (t5.CompareTo(t) >= 0)
						{
							break;
						}
						num--;
					}
				}
				if (num >= num2)
				{
					if (num != num2)
					{
						T t6 = list[num];
						list[num] = list[num2];
						list[num2] = t6;
					}
					num--;
					num2++;
				}
			}
			while (num2 <= num);
			if (d < num)
			{
				ArrayTools.QSort_r<T>(list, d, num, direction);
			}
			if (num2 < h)
			{
				ArrayTools.QSort_r<T>(list, num2, h, direction);
			}
		}
	}
}
