using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace DNA.Net.Lidgren
{
	public sealed class NetBitVector
	{
		public int Capacity
		{
			get
			{
				return this.m_capacity;
			}
		}

		public NetBitVector(int bitsCapacity)
		{
			this.m_capacity = bitsCapacity;
			this.m_data = new int[(bitsCapacity + 31) / 32];
		}

		public bool IsEmpty()
		{
			return this.m_numBitsSet == 0;
		}

		public int Count()
		{
			return this.m_numBitsSet;
		}

		public void RotateDown()
		{
			int lenMinusOne = this.m_data.Length - 1;
			int firstBit = this.m_data[0] & 1;
			for (int i = 0; i < lenMinusOne; i++)
			{
				this.m_data[i] = ((this.m_data[i] >> 1) & int.MaxValue) | (this.m_data[i + 1] << 31);
			}
			int lastIndex = this.m_capacity - 1 - 32 * lenMinusOne;
			int cur = this.m_data[lenMinusOne];
			cur >>= 1;
			cur |= firstBit << lastIndex;
			this.m_data[lenMinusOne] = cur;
		}

		public int GetFirstSetIndex()
		{
			int idx = 0;
			int data;
			for (data = this.m_data[0]; data == 0; data = this.m_data[idx])
			{
				idx++;
			}
			int a = 0;
			while (((data >> a) & 1) == 0)
			{
				a++;
			}
			return idx * 32 + a;
		}

		public bool Get(int bitIndex)
		{
			return (this.m_data[bitIndex / 32] & (1 << bitIndex % 32)) != 0;
		}

		public void Set(int bitIndex, bool value)
		{
			int idx = bitIndex / 32;
			if (value)
			{
				if ((this.m_data[idx] & (1 << bitIndex % 32)) == 0)
				{
					this.m_numBitsSet++;
				}
				this.m_data[idx] |= 1 << bitIndex % 32;
				return;
			}
			if ((this.m_data[idx] & (1 << bitIndex % 32)) != 0)
			{
				this.m_numBitsSet--;
			}
			this.m_data[idx] &= ~(1 << bitIndex % 32);
		}

		[IndexerName("Bit")]
		public bool this[int index]
		{
			get
			{
				return this.Get(index);
			}
			set
			{
				this.Set(index, value);
			}
		}

		public void Clear()
		{
			Array.Clear(this.m_data, 0, this.m_data.Length);
			this.m_numBitsSet = 0;
		}

		public override string ToString()
		{
			StringBuilder bdr = new StringBuilder(this.m_capacity + 2);
			bdr.Append('[');
			for (int i = 0; i < this.m_capacity; i++)
			{
				bdr.Append(this.Get(this.m_capacity - i - 1) ? '1' : '0');
			}
			bdr.Append(']');
			return bdr.ToString();
		}

		private readonly int m_capacity;

		private readonly int[] m_data;

		private int m_numBitsSet;
	}
}
