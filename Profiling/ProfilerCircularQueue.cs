using System;

namespace DNA.Profiling
{
	public class ProfilerCircularQueue<T>
	{
		public ProfilerCircularQueue(int size)
		{
			this._buffer = new T[size];
			this._head = 0;
		}

		public void Reset()
		{
			this._head = 0;
		}

		public void Add(T value)
		{
			this._buffer[this._head] = value;
			if (++this._head == this._buffer.Length)
			{
				this._head = 0;
			}
		}

		public T[] Buffer
		{
			get
			{
				return this._buffer;
			}
		}

		public int Head
		{
			get
			{
				return this._head;
			}
		}

		private T[] _buffer;

		private int _head;
	}
}
