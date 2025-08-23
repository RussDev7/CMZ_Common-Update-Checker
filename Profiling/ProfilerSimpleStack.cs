using System;

namespace DNA.Profiling
{
	public class ProfilerSimpleStack<T> where T : class, IProfilerLinkedListNode
	{
		public T Root
		{
			get
			{
				return this._root;
			}
		}

		public void PushList(T newList)
		{
			IProfilerLinkedListNode i = newList;
			while (i.NextNode != null)
			{
				i = i.NextNode;
			}
			i.NextNode = this._root;
			this._root = newList;
		}

		public void Push(T newNode)
		{
			newNode.NextNode = this._root;
			this._root = newNode;
		}

		public T Pop()
		{
			T result = this._root;
			if (this._root != null)
			{
				this._root = this._root.NextNode as T;
			}
			return result;
		}

		public void Clear()
		{
			this._root = default(T);
		}

		public bool Empty
		{
			get
			{
				return this._root == null;
			}
		}

		private T _root = default(T);
	}
}
