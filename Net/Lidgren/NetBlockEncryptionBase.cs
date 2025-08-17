using System;

namespace DNA.Net.Lidgren
{
	public abstract class NetBlockEncryptionBase : INetEncryption
	{
		public abstract int BlockSize { get; }

		public NetBlockEncryptionBase()
		{
			this.m_tmp = new byte[this.BlockSize];
		}

		public bool Encrypt(NetOutgoingMessage msg)
		{
			int lengthBits = msg.LengthBits;
			int lengthBytes = msg.LengthBytes;
			int blockSize = this.BlockSize;
			int num = (int)Math.Ceiling((double)lengthBytes / (double)blockSize);
			int num2 = num * blockSize;
			msg.EnsureBufferSize(num2 * 8 + 32);
			msg.LengthBits = num2 * 8;
			for (int i = 0; i < num; i++)
			{
				this.EncryptBlock(msg.m_data, i * blockSize, this.m_tmp);
				Buffer.BlockCopy(this.m_tmp, 0, msg.m_data, i * blockSize, this.m_tmp.Length);
			}
			msg.Write((uint)lengthBits);
			return true;
		}

		public bool Decrypt(NetIncomingMessage msg)
		{
			int num = msg.LengthBytes - 4;
			int blockSize = this.BlockSize;
			int num2 = num / blockSize;
			if (num2 * blockSize != num)
			{
				return false;
			}
			for (int i = 0; i < num2; i++)
			{
				this.DecryptBlock(msg.m_data, i * blockSize, this.m_tmp);
				Buffer.BlockCopy(this.m_tmp, 0, msg.m_data, i * blockSize, this.m_tmp.Length);
			}
			uint num3 = NetBitWriter.ReadUInt32(msg.m_data, 32, num * 8);
			msg.m_bitLength = (int)num3;
			return true;
		}

		protected abstract void EncryptBlock(byte[] source, int sourceOffset, byte[] destination);

		protected abstract void DecryptBlock(byte[] source, int sourceOffset, byte[] destination);

		private byte[] m_tmp;
	}
}
