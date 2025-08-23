using System;
using System.Text;

namespace DNA.Security.Cryptography
{
	public abstract class Hash : IComparable<Hash>, IEquatable<Hash>
	{
		public byte[] Data
		{
			get
			{
				return this._data;
			}
		}

		protected Hash(byte[] data)
		{
			this._data = data;
		}

		public override int GetHashCode()
		{
			int res = 0;
			for (int i = 0; i < this._data.Length; i++)
			{
				res ^= (int)this._data[i];
			}
			return res;
		}

		public static bool operator ==(Hash a, Hash b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Hash a, Hash b)
		{
			return !a.Equals(b);
		}

		public override bool Equals(object obj)
		{
			return this.CompareTo((Hash)obj) == 0;
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < this._data.Length; i++)
			{
				string str = this._data[i].ToString("X2");
				builder.Append(str);
			}
			return builder.ToString();
		}

		public int CompareTo(Hash other)
		{
			if (base.GetType() != other.GetType())
			{
				return -1;
			}
			if (this._data.Length != other._data.Length)
			{
				return -1;
			}
			for (int i = 0; i < this._data.Length; i++)
			{
				int ret = (int)(this._data[i] - other._data[i]);
				if (ret != 0)
				{
					return ret;
				}
			}
			return 0;
		}

		public bool Equals(Hash other)
		{
			return this.CompareTo(other) == 0;
		}

		private byte[] _data;
	}
}
