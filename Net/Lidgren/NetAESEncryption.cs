﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DNA.Net.Lidgren
{
	public class NetAESEncryption : INetEncryption
	{
		static NetAESEncryption()
		{
			AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
			List<int> temp = new List<int>();
			foreach (KeySizes keysize in aes.LegalKeySizes)
			{
				for (int i = keysize.MinSize; i <= keysize.MaxSize; i += keysize.SkipSize)
				{
					if (!temp.Contains(i))
					{
						temp.Add(i);
					}
					if (i == keysize.MaxSize)
					{
						break;
					}
				}
			}
			NetAESEncryption.m_keysizes = temp;
			temp = new List<int>();
			foreach (KeySizes keysize2 in aes.LegalBlockSizes)
			{
				for (int j = keysize2.MinSize; j <= keysize2.MaxSize; j += keysize2.SkipSize)
				{
					if (!temp.Contains(j))
					{
						temp.Add(j);
					}
					if (j == keysize2.MaxSize)
					{
						break;
					}
				}
			}
			NetAESEncryption.m_blocksizes = temp;
		}

		public NetAESEncryption(byte[] key, byte[] iv)
		{
			if (!NetAESEncryption.m_keysizes.Contains(key.Length * 8))
			{
				throw new NetException(string.Format("Not a valid key size. (Valid values are: {0})", NetUtility.MakeCommaDelimitedList<int>(NetAESEncryption.m_keysizes)));
			}
			if (!NetAESEncryption.m_blocksizes.Contains(iv.Length * 8))
			{
				throw new NetException(string.Format("Not a valid iv size. (Valid values are: {0})", NetUtility.MakeCommaDelimitedList<int>(NetAESEncryption.m_blocksizes)));
			}
			this.m_key = key;
			this.m_iv = iv;
			this.m_bitSize = this.m_key.Length * 8;
		}

		public NetAESEncryption(string key, int bitsize)
		{
			if (!NetAESEncryption.m_keysizes.Contains(bitsize))
			{
				throw new NetException(string.Format("Not a valid key size. (Valid values are: {0})", NetUtility.MakeCommaDelimitedList<int>(NetAESEncryption.m_keysizes)));
			}
			byte[] entropy = Encoding.UTF32.GetBytes(key);
			HMACSHA512 hmacsha512 = new HMACSHA512(Convert.FromBase64String("i88NEiez3c50bHqr3YGasDc4p8jRrxJAaiRiqixpvp4XNAStP5YNoC2fXnWkURtkha6M8yY901Gj07IRVIRyGL=="));
			hmacsha512.Initialize();
			for (int i = 0; i < 1000; i++)
			{
				entropy = hmacsha512.ComputeHash(entropy);
			}
			int keylen = bitsize / 8;
			this.m_key = new byte[keylen];
			Buffer.BlockCopy(entropy, 0, this.m_key, 0, keylen);
			this.m_iv = new byte[NetAESEncryption.m_blocksizes[0] / 8];
			Buffer.BlockCopy(entropy, entropy.Length - this.m_iv.Length - 1, this.m_iv, 0, this.m_iv.Length);
			this.m_bitSize = bitsize;
		}

		public NetAESEncryption(string key)
			: this(key, NetAESEncryption.m_keysizes[0])
		{
		}

		public bool Encrypt(NetOutgoingMessage msg)
		{
			try
			{
				using (AesCryptoServiceProvider aesCryptoServiceProvider = new AesCryptoServiceProvider
				{
					KeySize = this.m_bitSize,
					Mode = CipherMode.CBC
				})
				{
					using (ICryptoTransform cryptoTransform = aesCryptoServiceProvider.CreateEncryptor(this.m_key, this.m_iv))
					{
						using (MemoryStream memoryStream = new MemoryStream())
						{
							using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
							{
								cryptoStream.Write(msg.m_data, 0, msg.m_data.Length);
							}
							msg.m_data = memoryStream.ToArray();
						}
					}
				}
			}
			catch
			{
				return false;
			}
			return true;
		}

		public bool Decrypt(NetIncomingMessage msg)
		{
			try
			{
				using (AesCryptoServiceProvider aesCryptoServiceProvider = new AesCryptoServiceProvider
				{
					KeySize = this.m_bitSize,
					Mode = CipherMode.CBC
				})
				{
					using (ICryptoTransform cryptoTransform = aesCryptoServiceProvider.CreateDecryptor(this.m_key, this.m_iv))
					{
						using (MemoryStream memoryStream = new MemoryStream())
						{
							using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
							{
								cryptoStream.Write(msg.m_data, 0, msg.m_data.Length);
							}
							msg.m_data = memoryStream.ToArray();
						}
					}
				}
			}
			catch
			{
				return false;
			}
			return true;
		}

		private readonly byte[] m_key;

		private readonly byte[] m_iv;

		private readonly int m_bitSize;

		private static readonly List<int> m_keysizes;

		private static readonly List<int> m_blocksizes;
	}
}
