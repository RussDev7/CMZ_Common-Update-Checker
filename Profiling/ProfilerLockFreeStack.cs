using System;
using System.Threading;

namespace DNA.Profiling
{
	public class ProfilerLockFreeStack<T> where T : class, IProfilerLinkedListNode
	{
		public T Root
		{
			get
			{
				return this._root;
			}
		}

		public void Push(T newNode)
		{
			T t = default(T);
			do
			{
				t = this._root;
				newNode.NextNode = t;
			}
			while (t != Interlocked.CompareExchange<T>(ref this._root, newNode, t));
		}

		public void PushList(T newList)
		{
			IProfilerLinkedListNode profilerLinkedListNode = newList;
			while (profilerLinkedListNode.NextNode != null)
			{
				profilerLinkedListNode = profilerLinkedListNode.NextNode;
			}
			T t = default(T);
			do
			{
				t = this._root;
				profilerLinkedListNode.NextNode = t;
			}
			while (t != Interlocked.CompareExchange<T>(ref this._root, newList, t));
		}

		public T Pop()
		{
			T root;
			do
			{
				root = this._root;
			}
			while (root != null && root != Interlocked.CompareExchange<T>(ref this._root, root.NextNode as T, root));
			return root;
		}

		public T Clear()
		{
			T t = default(T);
			do
			{
				t = this._root;
			}
			while (t != Interlocked.CompareExchange<T>(ref this._root, default(T), t));
			return t;
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
