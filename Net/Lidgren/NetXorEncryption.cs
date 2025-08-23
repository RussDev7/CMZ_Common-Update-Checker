using System;
using System.Text;

namespace DNA.Net.Lidgren
{
	public class NetXorEncryption : INetEncryption
	{
		public NetXorEncryption(byte[] key)
		{
			this.m_key = key;
		}

		public NetXorEncryption(string key)
		{
			this.m_key = Encoding.UTF8.GetBytes(key);
		}

		public bool Encrypt(NetOutgoingMessage msg)
		{
			int numBytes = msg.LengthBytes;
			for (int i = 0; i < numBytes; i++)
			{
				int offset = i % this.m_key.Length;
				msg.m_data[i] = msg.m_data[i] ^ this.m_key[offset];
			}
			return true;
		}

		public bool Decrypt(NetIncomingMessage msg)
		{
			int numBytes = msg.LengthBytes;
			for (int i = 0; i < numBytes; i++)
			{
				int offset = i % this.m_key.Length;
				msg.m_data[i] = msg.m_data[i] ^ this.m_key[offset];
			}
			return true;
		}

		private byte[] m_key;
	}
}
