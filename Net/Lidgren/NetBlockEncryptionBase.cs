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
			int payloadBitLength = msg.LengthBits;
			int numBytes = msg.LengthBytes;
			int blockSize = this.BlockSize;
			int numBlocks = (int)Math.Ceiling((double)numBytes / (double)blockSize);
			int dstSize = numBlocks * blockSize;
			msg.EnsureBufferSize(dstSize * 8 + 32);
			msg.LengthBits = dstSize * 8;
			for (int i = 0; i < numBlocks; i++)
			{
				this.EncryptBlock(msg.m_data, i * blockSize, this.m_tmp);
				Buffer.BlockCopy(this.m_tmp, 0, msg.m_data, i * blockSize, this.m_tmp.Length);
			}
			msg.Write((uint)payloadBitLength);
			return true;
		}

		public bool Decrypt(NetIncomingMessage msg)
		{
			int numEncryptedBytes = msg.LengthBytes - 4;
			int blockSize = this.BlockSize;
			int numBlocks = numEncryptedBytes / blockSize;
			if (numBlocks * blockSize != numEncryptedBytes)
			{
				return false;
			}
			for (int i = 0; i < numBlocks; i++)
			{
				this.DecryptBlock(msg.m_data, i * blockSize, this.m_tmp);
				Buffer.BlockCopy(this.m_tmp, 0, msg.m_data, i * blockSize, this.m_tmp.Length);
			}
			uint realSize = NetBitWriter.ReadUInt32(msg.m_data, 32, numEncryptedBytes * 8);
			msg.m_bitLength = (int)realSize;
			return true;
		}

		protected abstract void EncryptBlock(byte[] source, int sourceOffset, byte[] destination);

		protected abstract void DecryptBlock(byte[] source, int sourceOffset, byte[] destination);

		private byte[] m_tmp;
	}
}
