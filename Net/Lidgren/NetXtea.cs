using System;
using System.Security.Cryptography;
using System.Text;

namespace DNA.Net.Lidgren
{
	public sealed class NetXtea : NetBlockEncryptionBase
	{
		public override int BlockSize
		{
			get
			{
				return 8;
			}
		}

		public NetXtea(byte[] key, int rounds)
		{
			if (key.Length < 16)
			{
				throw new NetException("Key too short!");
			}
			this.m_numRounds = rounds;
			this.m_sum0 = new uint[this.m_numRounds];
			this.m_sum1 = new uint[this.m_numRounds];
			uint[] tmp = new uint[8];
			int index;
			int num2 = (index = 0);
			while (index < 4)
			{
				tmp[index] = BitConverter.ToUInt32(key, num2);
				index++;
				num2 += 4;
			}
			num2 = (index = 0);
			while (index < 32)
			{
				this.m_sum0[index] = (uint)(num2 + (int)tmp[num2 & 3]);
				num2 += -1640531527;
				this.m_sum1[index] = (uint)(num2 + (int)tmp[(num2 >> 11) & 3]);
				index++;
			}
		}

		public NetXtea(byte[] key)
			: this(key, 32)
		{
		}

		public NetXtea(string key)
			: this(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(key)), 32)
		{
		}

		protected override void EncryptBlock(byte[] source, int sourceOffset, byte[] destination)
		{
			uint v0 = NetXtea.BytesToUInt(source, sourceOffset);
			uint v = NetXtea.BytesToUInt(source, sourceOffset + 4);
			for (int i = 0; i != this.m_numRounds; i++)
			{
				v0 += (((v << 4) ^ (v >> 5)) + v) ^ this.m_sum0[i];
				v += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ this.m_sum1[i];
			}
			NetXtea.UIntToBytes(v0, destination, 0);
			NetXtea.UIntToBytes(v, destination, 4);
		}

		protected override void DecryptBlock(byte[] source, int sourceOffset, byte[] destination)
		{
			uint v0 = NetXtea.BytesToUInt(source, sourceOffset);
			uint v = NetXtea.BytesToUInt(source, sourceOffset + 4);
			for (int i = this.m_numRounds - 1; i >= 0; i--)
			{
				v -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ this.m_sum1[i];
				v0 -= (((v << 4) ^ (v >> 5)) + v) ^ this.m_sum0[i];
			}
			NetXtea.UIntToBytes(v0, destination, 0);
			NetXtea.UIntToBytes(v, destination, 4);
		}

		private static uint BytesToUInt(byte[] bytes, int offset)
		{
			uint retval = (uint)((uint)bytes[offset] << 24);
			retval |= (uint)((uint)bytes[++offset] << 16);
			retval |= (uint)((uint)bytes[++offset] << 8);
			return retval | (uint)bytes[++offset];
		}

		private static void UIntToBytes(uint value, byte[] destination, int destinationOffset)
		{
			destination[destinationOffset++] = (byte)(value >> 24);
			destination[destinationOffset++] = (byte)(value >> 16);
			destination[destinationOffset++] = (byte)(value >> 8);
			destination[destinationOffset++] = (byte)value;
		}

		private const int c_blockSize = 8;

		private const int c_keySize = 16;

		private const int c_delta = -1640531527;

		private readonly int m_numRounds;

		private readonly uint[] m_sum0;

		private readonly uint[] m_sum1;
	}
}
