using System;
using System.Collections;
using System.Collections.Generic;

namespace DNA.Collections
{
	public class NotifyList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
	{
		public event EventHandler Cleared;

		public event EventHandler Inserted;

		public event EventHandler Removed;

		public event EventHandler Set;

		public event EventHandler Modified;

		protected virtual void OnModified()
		{
		}

		protected virtual void OnClear()
		{
		}

		protected virtual void OnClearComplete()
		{
		}

		protected virtual void OnInsert(int index, T value)
		{
		}

		protected virtual void OnInsertComplete(int index, T value)
		{
		}

		protected virtual void OnRemove(int index, T value)
		{
		}

		protected virtual void OnRemoveComplete(int index, T value)
		{
		}

		protected virtual void OnSet(int index, T oldValue, T newValue)
		{
		}

		protected virtual void OnSetComplete(int index, T oldValue, T newValue)
		{
		}

		protected virtual void OnValidate(T value)
		{
		}

		public int IndexOf(T item)
		{
			return this._list.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			this.OnValidate(item);
			this.OnInsert(index, item);
			this._list.Insert(index, item);
			this.OnInsertComplete(index, item);
			if (this.Inserted != null)
			{
				this.Inserted(this, null);
			}
			if (this.Modified != null)
			{
				this.Modified(this, null);
			}
		}

		public void RemoveAt(int index)
		{
			T item = this._list[index];
			this.OnRemove(index, item);
			this._list.RemoveAt(index);
			this.OnRemoveComplete(index, item);
			if (this.Removed != null)
			{
				this.Removed(this, null);
			}
			if (this.Modified != null)
			{
				this.Modified(this, null);
			}
		}

		public T this[int index]
		{
			get
			{
				return this._list[index];
			}
			set
			{
				this.OnValidate(value);
				T old = this._list[index];
				this.OnSet(index, this._list[index], value);
				this._list[index] = value;
				this.OnSetComplete(index, old, value);
				if (this.Set != null)
				{
					this.Set(this, null);
				}
				if (this.Modified != null)
				{
					this.Modified(this, null);
				}
			}
		}

		public void Add(T item)
		{
			this.OnValidate(item);
			this.OnInsert(this._list.Count, item);
			this._list.Add(item);
			this.OnInsertComplete(this._list.Count - 1, item);
			if (this.Inserted != null)
			{
				this.Inserted(this, null);
			}
			if (this.Modified != null)
			{
				this.Modified(this, null);
			}
		}

		public void Clear()
		{
			this.OnClear();
			this._list.Clear();
			this.OnClearComplete();
			if (this.Cleared != null)
			{
				this.Cleared(this, null);
			}
			if (this.Modified != null)
			{
				this.Modified(this, null);
			}
		}

		public bool Contains(T item)
		{
			return this._list.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			this._list.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get
			{
				return this._list.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public bool Remove(T item)
		{
			int index = this._list.IndexOf(item);
			if (index < 0)
			{
				return false;
			}
			this.OnRemove(index, item);
			this._list.RemoveAt(index);
			this.OnRemoveComplete(index, item);
			if (this.Removed != null)
			{
				this.Removed(this, null);
			}
			if (this.Modified != null)
			{
				this.Modified(this, null);
			}
			return true;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this._list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._list.GetEnumerator();
		}

		private List<T> _list = new List<T>();
	}
}
