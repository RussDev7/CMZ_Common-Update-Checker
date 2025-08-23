using System;
using System.Collections;
using System.Collections.Generic;

namespace DNA.Collections
{
	public class Tree<T> where T : Tree<T>
	{
		private T _parent
		{
			get
			{
				return this._parentReference.Target;
			}
			set
			{
				this._parentReference.Target = value;
			}
		}

		public T Root
		{
			get
			{
				T t = (T)((object)this);
				while (t.Parent != null)
				{
					t = t.Parent;
				}
				return t;
			}
		}

		public bool IsDecendantOf(T node)
		{
			for (Tree<T> currentNode = this; currentNode != null; currentNode = currentNode.Parent)
			{
				if (currentNode == node)
				{
					return true;
				}
			}
			return false;
		}

		protected virtual void OnParentChanged(T oldParent, T newParent)
		{
		}

		protected virtual void OnChildrenChanged()
		{
		}

		public Tree()
		{
			this._children = new Tree<T>.NodeCollection(this);
		}

		public Tree(int size)
		{
			this._children = new Tree<T>.NodeCollection(this, size);
		}

		public T Parent
		{
			get
			{
				return this._parent;
			}
		}

		public Tree<T>.NodeCollection Children
		{
			get
			{
				return this._children;
			}
		}

		private WeakReference<T> _parentReference = new WeakReference<T>(null);

		private Tree<T>.NodeCollection _children;

		public class NodeCollection : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
		{
			public NodeCollection(Tree<T> owner)
			{
				this._owner = owner;
				this.List = new List<T>();
			}

			public NodeCollection(Tree<T> owner, int size)
			{
				this._owner = owner;
				this.List = new List<T>(size);
			}

			public T this[int index]
			{
				get
				{
					return this.List[index];
				}
				set
				{
					T old = this.List[index];
					this.List[index] = value;
					if (old != value)
					{
						this.OnSetComplete(index, old, value);
					}
				}
			}

			public void Add(T t)
			{
				this.List.Add(t);
				this.OnInsertComplete(this.List.Count - 1, t);
			}

			public void AddRange(IEnumerable<T> tlist)
			{
				foreach (T t in tlist)
				{
					this.List.Add(t);
					this.OnInsertComplete(this.List.Count - 1, t);
				}
			}

			public int IndexOf(T t)
			{
				return this.List.IndexOf(t);
			}

			public bool Contains(T t)
			{
				return this.List.Contains(t);
			}

			public void Insert(int index, T t)
			{
				this.List.Insert(index, t);
				this.OnInsertComplete(index, t);
			}

			private void OnClear()
			{
				for (int i = 0; i < this.List.Count; i++)
				{
					T node = this.List[i];
					node._parent = default(T);
					node.OnParentChanged((T)((object)this._owner), default(T));
				}
			}

			private void OnInsertComplete(int index, T value)
			{
				T node = value;
				node._parent = (T)((object)this._owner);
				node.OnParentChanged(default(T), (T)((object)this._owner));
				this._owner.OnChildrenChanged();
			}

			private void OnRemoveComplete(int index, T value)
			{
				T node = value;
				node._parent = default(T);
				node.OnParentChanged((T)((object)this._owner), default(T));
				this._owner.OnChildrenChanged();
			}

			private void OnSetComplete(int index, T oldValue, T newValue)
			{
				T oldNode = oldValue;
				T newNode = newValue;
				oldNode._parent = default(T);
				newNode._parent = (T)((object)this._owner);
				oldNode.OnParentChanged((T)((object)this._owner), default(T));
				newNode.OnParentChanged(default(T), (T)((object)this._owner));
				this._owner.OnChildrenChanged();
			}

			public T[] ToArray()
			{
				T[] array = new T[this.List.Count];
				this.List.CopyTo(array, 0);
				return array;
			}

			public void Sort(Comparison<T> comparison)
			{
				this.List.Sort(comparison);
			}

			public void RemoveAt(int index)
			{
				T item = this.List[index];
				this.List.RemoveAt(index);
				this.OnRemoveComplete(index, item);
			}

			public void Clear()
			{
				this.OnClear();
				this.List.Clear();
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
				this.List.CopyTo(array, arrayIndex);
			}

			public int Count
			{
				get
				{
					return this.List.Count;
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
				int index = this.List.IndexOf(item);
				if (index < 0)
				{
					return false;
				}
				this.RemoveAt(index);
				return true;
			}

			public IEnumerator<T> GetEnumerator()
			{
				return this.List.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable)this.List).GetEnumerator();
			}

			private Tree<T> _owner;

			private List<T> List;
		}
	}
}
