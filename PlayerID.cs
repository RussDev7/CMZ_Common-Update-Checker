using System;
using System.IO;

namespace DNA
{
	public class PlayerID : IComparable<PlayerID>, IEquatable<PlayerID>
	{
		public PlayerID(byte[] hash)
		{
			this._playerHash = hash;
		}

		public void Read(BinaryReader reader)
		{
			int length = reader.ReadInt32();
			this._playerHash = new byte[length];
			for (int i = 0; i < length; i++)
			{
				this._playerHash[i] = reader.ReadByte();
			}
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(this._playerHash.Length);
			for (int i = 0; i < this._playerHash.Length; i++)
			{
				writer.Write(this._playerHash[i]);
			}
		}

		public byte[] Data
		{
			get
			{
				return this._playerHash;
			}
		}

		public override int GetHashCode()
		{
			int res = 0;
			for (int i = 0; i < this._playerHash.Length; i++)
			{
				res ^= (int)this._playerHash[i];
			}
			return res;
		}

		public static bool operator ==(PlayerID a, PlayerID b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(PlayerID a, PlayerID b)
		{
			return !a.Equals(b);
		}

		public override bool Equals(object obj)
		{
			return this.CompareTo((PlayerID)obj) == 0;
		}

		public override string ToString()
		{
			return this._playerHash.ToString();
		}

		public int CompareTo(PlayerID other)
		{
			if (base.GetType() != other.GetType())
			{
				return -1;
			}
			if (this._playerHash.Length != other._playerHash.Length)
			{
				return -1;
			}
			for (int i = 0; i < this._playerHash.Length; i++)
			{
				int ret = (int)(this._playerHash[i] - other._playerHash[i]);
				if (ret != 0)
				{
					return ret;
				}
			}
			return 0;
		}

		public bool Equals(PlayerID other)
		{
			return this.CompareTo(other) == 0;
		}

		private byte[] _playerHash;

		public static readonly PlayerID Null = new PlayerID(null);
	}
}
