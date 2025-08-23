using System;
using System.Collections;
using System.Collections.Generic;

namespace DNA.Collections
{
	public class Set<T> : ICollection<T>, IEnumerable<T>, IEnumerable
	{
		public int Count
		{
			get
			{
				return this.count;
			}
		}

		public Set()
		{
			this.Init(10, null);
		}

		public Set(IEqualityComparer<T> comparer)
		{
			this.Init(10, comparer);
		}

		public Set(IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		public Set(IEnumerable<T> collection, IEqualityComparer<T> comparer)
		{
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			int capacity = 0;
			ICollection<T> col = collection as ICollection<T>;
			if (col != null)
			{
				capacity = col.Count;
			}
			this.Init(capacity, comparer);
			foreach (T item in collection)
			{
				this.Add(item);
			}
		}

		private void Init(int capacity, IEqualityComparer<T> comparer)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException("capacity");
			}
			this.comparer = comparer ?? EqualityComparer<T>.Default;
			if (capacity == 0)
			{
				capacity = 10;
			}
			capacity = (int)((float)capacity / 0.9f) + 1;
			this.InitArrays(capacity);
			this.generation = 0;
		}

		private void InitArrays(int size)
		{
			this.table = new int[size];
			this.links = new Set<T>.Link[size];
			this.empty_slot = -1;
			this.slots = new T[size];
			this.touched = 0;
			this.threshold = (int)((float)this.table.Length * 0.9f);
			if (this.threshold == 0 && this.table.Length > 0)
			{
				this.threshold = 1;
			}
		}

		private bool SlotsContainsAt(int index, int hash, T item)
		{
			Set<T>.Link link;
			for (int current = this.table[index] - 1; current != -1; current = link.Next)
			{
				link = this.links[current];
				if (link.HashCode == hash && ((hash == -2147483648 && (item == null || this.slots[current] == null)) ? (item == null && null == this.slots[current]) : this.comparer.Equals(item, this.slots[current])))
				{
					return true;
				}
			}
			return false;
		}

		public void CopyTo(T[] array)
		{
			this.CopyTo(array, 0, this.count);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			this.CopyTo(array, arrayIndex, this.count);
		}

		public void CopyTo(T[] array, int arrayIndex, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}
			if (arrayIndex > array.Length)
			{
				throw new ArgumentException("index larger than largest valid index of array");
			}
			if (array.Length - arrayIndex < count)
			{
				throw new ArgumentException("Destination array cannot hold the requested elements!");
			}
			int i = 0;
			int items = 0;
			while (i < this.touched && items < count)
			{
				if (this.GetLinkHashCode(i) != 0)
				{
					array[arrayIndex++] = this.slots[i];
				}
				i++;
			}
		}

		private void Resize()
		{
			int newSize = Set<T>.PrimeHelper.ToPrime((this.table.Length << 1) | 1);
			int[] newTable = new int[newSize];
			Set<T>.Link[] newLinks = new Set<T>.Link[newSize];
			for (int i = 0; i < this.table.Length; i++)
			{
				for (int current = this.table[i] - 1; current != -1; current = this.links[current].Next)
				{
					int hashCode = (newLinks[current].HashCode = this.GetItemHashCode(this.slots[current]));
					int index = (hashCode & int.MaxValue) % newSize;
					newLinks[current].Next = newTable[index] - 1;
					newTable[index] = current + 1;
				}
			}
			this.table = newTable;
			this.links = newLinks;
			T[] newSlots = new T[newSize];
			Array.Copy(this.slots, 0, newSlots, 0, this.touched);
			this.slots = newSlots;
			this.threshold = (int)((float)newSize * 0.9f);
		}

		private int GetLinkHashCode(int index)
		{
			return this.links[index].HashCode & int.MinValue;
		}

		private int GetItemHashCode(T item)
		{
			if (item == null)
			{
				return int.MinValue;
			}
			return this.comparer.GetHashCode(item) | int.MinValue;
		}

		public bool Add(T item)
		{
			int hashCode = this.GetItemHashCode(item);
			int index = (hashCode & int.MaxValue) % this.table.Length;
			if (this.SlotsContainsAt(index, hashCode, item))
			{
				return false;
			}
			if (++this.count > this.threshold)
			{
				this.Resize();
				index = (hashCode & int.MaxValue) % this.table.Length;
			}
			int current = this.empty_slot;
			if (current == -1)
			{
				current = this.touched++;
			}
			else
			{
				this.empty_slot = this.links[current].Next;
			}
			this.links[current].HashCode = hashCode;
			this.links[current].Next = this.table[index] - 1;
			this.table[index] = current + 1;
			this.slots[current] = item;
			this.generation++;
			return true;
		}

		public IEqualityComparer<T> Comparer
		{
			get
			{
				return this.comparer;
			}
		}

		public void Clear()
		{
			this.count = 0;
			Array.Clear(this.table, 0, this.table.Length);
			Array.Clear(this.slots, 0, this.slots.Length);
			Array.Clear(this.links, 0, this.links.Length);
			this.empty_slot = -1;
			this.touched = 0;
			this.generation++;
		}

		public bool Contains(T item)
		{
			int hashCode = this.GetItemHashCode(item);
			int index = (hashCode & int.MaxValue) % this.table.Length;
			return this.SlotsContainsAt(index, hashCode, item);
		}

		public bool Remove(T item)
		{
			int hashCode = this.GetItemHashCode(item);
			int index = (hashCode & int.MaxValue) % this.table.Length;
			int current = this.table[index] - 1;
			if (current == -1)
			{
				return false;
			}
			int prev = -1;
			do
			{
				Set<T>.Link link = this.links[current];
				if (link.HashCode == hashCode && ((hashCode == -2147483648 && (item == null || this.slots[current] == null)) ? (item == null && null == this.slots[current]) : this.comparer.Equals(this.slots[current], item)))
				{
					break;
				}
				prev = current;
				current = link.Next;
			}
			while (current != -1);
			if (current == -1)
			{
				return false;
			}
			this.count--;
			if (prev == -1)
			{
				this.table[index] = this.links[current].Next + 1;
			}
			else
			{
				this.links[prev].Next = this.links[current].Next;
			}
			this.links[current].Next = this.empty_slot;
			this.empty_slot = current;
			this.links[current].HashCode = 0;
			this.slots[current] = default(T);
			this.generation++;
			return true;
		}

		public int RemoveWhere(Predicate<T> match)
		{
			if (match == null)
			{
				throw new ArgumentNullException("match");
			}
			List<T> candidates = new List<T>();
			foreach (T item in this)
			{
				if (match(item))
				{
					candidates.Add(item);
				}
			}
			foreach (T item2 in candidates)
			{
				this.Remove(item2);
			}
			return candidates.Count;
		}

		public void TrimExcess()
		{
			this.Resize();
		}

		public void IntersectWith(IEnumerable<T> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			Set<T> other_set = this.ToSet(other);
			this.RemoveWhere((T item) => !other_set.Contains(item));
		}

		public void ExceptWith(IEnumerable<T> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			foreach (T item in other)
			{
				this.Remove(item);
			}
		}

		public bool Overlaps(IEnumerable<T> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			foreach (T item in other)
			{
				if (this.Contains(item))
				{
					return true;
				}
			}
			return false;
		}

		public bool SetEquals(IEnumerable<T> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			Set<T> other_set = this.ToSet(other);
			if (this.count != other_set.Count)
			{
				return false;
			}
			foreach (T item in this)
			{
				if (!other_set.Contains(item))
				{
					return false;
				}
			}
			return true;
		}

		public void SymmetricExceptWith(IEnumerable<T> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			foreach (T item in this.ToSet(other))
			{
				if (!this.Add(item))
				{
					this.Remove(item);
				}
			}
		}

		private Set<T> ToSet(IEnumerable<T> enumerable)
		{
			Set<T> set = enumerable as Set<T>;
			if (set == null || !this.Comparer.Equals(set.Comparer))
			{
				set = new Set<T>(enumerable, this.Comparer);
			}
			return set;
		}

		public void UnionWith(IEnumerable<T> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			foreach (T item in other)
			{
				this.Add(item);
			}
		}

		private bool CheckIsSubsetOf(Set<T> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			foreach (T item in this)
			{
				if (!other.Contains(item))
				{
					return false;
				}
			}
			return true;
		}

		public bool IsSubsetOf(IEnumerable<T> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			if (this.count == 0)
			{
				return true;
			}
			Set<T> other_set = this.ToSet(other);
			return this.count <= other_set.Count && this.CheckIsSubsetOf(other_set);
		}

		public bool IsProperSubsetOf(IEnumerable<T> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			if (this.count == 0)
			{
				return true;
			}
			Set<T> other_set = this.ToSet(other);
			return this.count < other_set.Count && this.CheckIsSubsetOf(other_set);
		}

		private bool CheckIsSupersetOf(Set<T> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			foreach (T item in other)
			{
				if (!this.Contains(item))
				{
					return false;
				}
			}
			return true;
		}

		public bool IsSupersetOf(IEnumerable<T> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			Set<T> other_set = this.ToSet(other);
			return this.count >= other_set.Count && this.CheckIsSupersetOf(other_set);
		}

		public bool IsProperSupersetOf(IEnumerable<T> other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			Set<T> other_set = this.ToSet(other);
			return this.count > other_set.Count && this.CheckIsSupersetOf(other_set);
		}

		public static IEqualityComparer<Set<T>> CreateSetComparer()
		{
			return Set<T>.setComparer;
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return new Set<T>.Enumerator(this);
		}

		bool ICollection<T>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		void ICollection<T>.Add(T item)
		{
			this.Add(item);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Set<T>.Enumerator(this);
		}

		public Set<T>.Enumerator GetEnumerator()
		{
			return new Set<T>.Enumerator(this);
		}

		private const int INITIAL_SIZE = 10;

		private const float DEFAULT_LOAD_FACTOR = 0.9f;

		private const int NO_SLOT = -1;

		private const int HASH_FLAG = -2147483648;

		private int[] table;

		private Set<T>.Link[] links;

		private T[] slots;

		private int touched;

		private int empty_slot;

		private int count;

		private int threshold;

		private IEqualityComparer<T> comparer;

		private int generation;

		private static readonly Set<T>.SetEqualityComparer setComparer = new Set<T>.SetEqualityComparer();

		private struct Link
		{
			public int HashCode;

			public int Next;
		}

		private class SetEqualityComparer : IEqualityComparer<Set<T>>
		{
			public bool Equals(Set<T> lhs, Set<T> rhs)
			{
				if (lhs == rhs)
				{
					return true;
				}
				if (lhs == null || rhs == null || lhs.Count != rhs.Count)
				{
					return false;
				}
				foreach (T item in lhs)
				{
					if (!rhs.Contains(item))
					{
						return false;
					}
				}
				return true;
			}

			public int GetHashCode(Set<T> hashset)
			{
				if (hashset == null)
				{
					return 0;
				}
				IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
				int hash = 0;
				foreach (T item in hashset)
				{
					hash ^= comparer.GetHashCode(item);
				}
				return hash;
			}
		}

		[Serializable]
		public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
		{
			internal Enumerator(Set<T> hashset)
			{
				this = default(Set<T>.Enumerator);
				this.hashset = hashset;
				this.stamp = hashset.generation;
			}

			public bool MoveNext()
			{
				this.CheckState();
				if (this.next < 0)
				{
					return false;
				}
				while (this.next < this.hashset.touched)
				{
					int cur = this.next++;
					if (this.hashset.GetLinkHashCode(cur) != 0)
					{
						this.current = this.hashset.slots[cur];
						return true;
					}
				}
				this.next = -1;
				return false;
			}

			public T Current
			{
				get
				{
					return this.current;
				}
			}

			object IEnumerator.Current
			{
				get
				{
					this.CheckState();
					if (this.next <= 0)
					{
						throw new InvalidOperationException("Current is not valid");
					}
					return this.current;
				}
			}

			void IEnumerator.Reset()
			{
				this.CheckState();
				this.next = 0;
			}

			public void Dispose()
			{
				this.hashset = null;
			}

			private void CheckState()
			{
				if (this.hashset == null)
				{
					throw new ObjectDisposedException(null);
				}
				if (this.hashset.generation != this.stamp)
				{
					throw new InvalidOperationException("Set have been modified while it was iterated over");
				}
			}

			private Set<T> hashset;

			private int next;

			private int stamp;

			private T current;
		}

		private static class PrimeHelper
		{
			private static bool TestPrime(int x)
			{
				if ((x & 1) != 0)
				{
					int top = (int)Math.Sqrt((double)x);
					for (int i = 3; i < top; i += 2)
					{
						if (x % i == 0)
						{
							return false;
						}
					}
					return true;
				}
				return x == 2;
			}

			private static int CalcPrime(int x)
			{
				for (int i = (x & -2) - 1; i < 2147483647; i += 2)
				{
					if (Set<T>.PrimeHelper.TestPrime(i))
					{
						return i;
					}
				}
				return x;
			}

			public static int ToPrime(int x)
			{
				for (int i = 0; i < Set<T>.PrimeHelper.primes_table.Length; i++)
				{
					if (x <= Set<T>.PrimeHelper.primes_table[i])
					{
						return Set<T>.PrimeHelper.primes_table[i];
					}
				}
				return Set<T>.PrimeHelper.CalcPrime(x);
			}

			private static readonly int[] primes_table = new int[]
			{
				11, 19, 37, 73, 109, 163, 251, 367, 557, 823,
				1237, 1861, 2777, 4177, 6247, 9371, 14057, 21089, 31627, 47431,
				71143, 106721, 160073, 240101, 360163, 540217, 810343, 1215497, 1823231, 2734867,
				4102283, 6153409, 9230113, 13845163
			};
		}
	}
}
