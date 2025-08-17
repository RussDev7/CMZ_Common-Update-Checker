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
			IProfilerLinkedListNode profilerLinkedListNode = newList;
			while (profilerLinkedListNode.NextNode != null)
			{
				profilerLinkedListNode = profilerLinkedListNode.NextNode;
			}
			profilerLinkedListNode.NextNode = this._root;
			this._root = newList;
		}

		public void Push(T newNode)
		{
			newNode.NextNode = this._root;
			this._root = newNode;
		}

		public T Pop()
		{
			T root = this._root;
			if (this._root != null)
			{
				this._root = this._root.NextNode as T;
			}
			return root;
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
