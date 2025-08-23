using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DNA.Net.Lidgren
{
	[DebuggerDisplay("Count={Count} Capacity={Capacity}")]
	public sealed class NetQueue<T>
	{
		public int Count
		{
			get
			{
				return this.m_size;
			}
		}

		public int Capacity
		{
			get
			{
				return this.m_items.Length;
			}
		}

		public NetQueue(int initialCapacity)
		{
			this.m_lock = new object();
			this.m_items = new T[initialCapacity];
		}

		public void Enqueue(T item)
		{
			lock (this.m_lock)
			{
				if (this.m_size == this.m_items.Length)
				{
					this.SetCapacity(this.m_items.Length + 8);
				}
				int slot = (this.m_head + this.m_size) % this.m_items.Length;
				this.m_items[slot] = item;
				this.m_size++;
			}
		}

		public void Enqueue(IEnumerable<T> items)
		{
			lock (this.m_lock)
			{
				foreach (T item in items)
				{
					if (this.m_size == this.m_items.Length)
					{
						this.SetCapacity(this.m_items.Length + 8);
					}
					int slot = (this.m_head + this.m_size) % this.m_items.Length;
					this.m_items[slot] = item;
					this.m_size++;
				}
			}
		}

		public void EnqueueFirst(T item)
		{
			lock (this.m_lock)
			{
				if (this.m_size >= this.m_items.Length)
				{
					this.SetCapacity(this.m_items.Length + 8);
				}
				this.m_head--;
				if (this.m_head < 0)
				{
					this.m_head = this.m_items.Length - 1;
				}
				this.m_items[this.m_head] = item;
				this.m_size++;
			}
		}

		private void SetCapacity(int newCapacity)
		{
			if (this.m_size == 0 && this.m_size == 0)
			{
				this.m_items = new T[newCapacity];
				this.m_head = 0;
				return;
			}
			T[] newItems = new T[newCapacity];
			if (this.m_head + this.m_size - 1 < this.m_items.Length)
			{
				Array.Copy(this.m_items, this.m_head, newItems, 0, this.m_size);
			}
			else
			{
				Array.Copy(this.m_items, this.m_head, newItems, 0, this.m_items.Length - this.m_head);
				Array.Copy(this.m_items, 0, newItems, this.m_items.Length - this.m_head, this.m_size - (this.m_items.Length - this.m_head));
			}
			this.m_items = newItems;
			this.m_head = 0;
		}

		public bool TryDequeue(out T item)
		{
			if (this.m_size == 0)
			{
				item = default(T);
				return false;
			}
			bool flag2;
			lock (this.m_lock)
			{
				if (this.m_size == 0)
				{
					item = default(T);
					flag2 = false;
				}
				else
				{
					item = this.m_items[this.m_head];
					this.m_items[this.m_head] = default(T);
					this.m_head = (this.m_head + 1) % this.m_items.Length;
					this.m_size--;
					flag2 = true;
				}
			}
			return flag2;
		}

		public int TryDrain(IList<T> addTo)
		{
			if (this.m_size == 0)
			{
				return 0;
			}
			int num;
			lock (this.m_lock)
			{
				int added = this.m_size;
				while (this.m_size > 0)
				{
					T item = this.m_items[this.m_head];
					addTo.Add(item);
					this.m_items[this.m_head] = default(T);
					this.m_head = (this.m_head + 1) % this.m_items.Length;
					this.m_size--;
				}
				num = added;
			}
			return num;
		}

		public T TryPeek(int offset)
		{
			if (this.m_size == 0)
			{
				return default(T);
			}
			T t;
			lock (this.m_lock)
			{
				if (this.m_size == 0)
				{
					t = default(T);
				}
				else
				{
					t = this.m_items[(this.m_head + offset) % this.m_items.Length];
				}
			}
			return t;
		}

		public bool Contains(T item)
		{
			lock (this.m_lock)
			{
				int ptr = this.m_head;
				for (int i = 0; i < this.m_size; i++)
				{
					if (this.m_items[ptr] == null)
					{
						if (item == null)
						{
							return true;
						}
					}
					else if (this.m_items[ptr].Equals(item))
					{
						return true;
					}
					ptr = (ptr + 1) % this.m_items.Length;
				}
			}
			return false;
		}

		public T[] ToArray()
		{
			T[] array;
			lock (this.m_lock)
			{
				T[] retval = new T[this.m_size];
				int ptr = this.m_head;
				for (int i = 0; i < this.m_size; i++)
				{
					retval[i] = this.m_items[ptr++];
					if (ptr >= this.m_items.Length)
					{
						ptr = 0;
					}
				}
				array = retval;
			}
			return array;
		}

		public void Clear()
		{
			lock (this.m_lock)
			{
				for (int i = 0; i < this.m_items.Length; i++)
				{
					this.m_items[i] = default(T);
				}
				this.m_head = 0;
				this.m_size = 0;
			}
		}

		private T[] m_items;

		private readonly object m_lock;

		private int m_size;

		private int m_head;
	}
}
