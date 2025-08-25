using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using DNA.Text;

namespace DNA.Security
{
	public static class SecurityTools
	{
		public static string GeneratePassword(int length)
		{
			return SecurityTools.GeneratePassword(length, SecurityTools.DefaultCharSet);
		}

		public static string GeneratePassword(int length, char[] charset)
		{
			return SecurityTools.GeneratePassword(length, charset, new Random());
		}

		public static string GeneratePassword(int length, char[] charset, Random rand)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < length; i++)
			{
				int idx = rand.Next(charset.Length);
				sb.Append(charset[idx]);
			}
			return sb.ToString();
		}

		private static SymmetricAlgorithm CreateAesEcbNoPadding(byte[] key)
		{
			SymmetricAlgorithm aes;
			try
			{
				aes = new AesCryptoServiceProvider();
			}
			catch
			{
				aes = new AesManaged();
			}
			aes.Mode = CipherMode.ECB;
			aes.Padding = PaddingMode.None;
			aes.Key = key;
			aes.IV = new byte[aes.BlockSize / 8];
			return aes;
		}

		private static byte[] EncryptEcbNoPadding(byte[] key, byte[] plainPadded)
		{
			byte[] array;
			using (SymmetricAlgorithm aes = SecurityTools.CreateAesEcbNoPadding(key))
			{
				using (ICryptoTransform enc = aes.CreateEncryptor())
				{
					array = enc.TransformFinalBlock(plainPadded, 0, plainPadded.Length);
				}
			}
			return array;
		}

		private static byte[] DecryptEcbNoPadding(byte[] key, byte[] cipher)
		{
			byte[] array;
			using (SymmetricAlgorithm aes = SecurityTools.CreateAesEcbNoPadding(key))
			{
				using (ICryptoTransform dec = aes.CreateDecryptor())
				{
					array = dec.TransformFinalBlock(cipher, 0, cipher.Length);
				}
			}
			return array;
		}

		private static byte[] PadToBlockMultiple(byte[] data)
		{
			int rem = data.Length % 16;
			int pad = ((rem == 0) ? 16 : (16 - rem));
			byte[] output = new byte[data.Length + pad];
			Buffer.BlockCopy(data, 0, output, 0, data.Length);
			return output;
		}

		public static byte[] EncryptData(byte[] key, byte[] data)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			byte[] plain;
			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter bw = new BinaryWriter(ms))
				{
					bw.Write(data.Length);
					bw.Write(data);
					bw.Flush();
					plain = ms.ToArray();
				}
			}
			byte[] padded = SecurityTools.PadToBlockMultiple(plain);
			return SecurityTools.EncryptEcbNoPadding(key, padded);
		}

		public static byte[] DecryptData(byte[] key, byte[] code)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (code == null)
			{
				throw new ArgumentNullException("code");
			}
			if (code.Length % 16 != 0)
			{
				throw new ArgumentException("Ciphertext length must be a multiple of 16.", "code");
			}
			byte[] plain = SecurityTools.DecryptEcbNoPadding(key, code);
			byte[] array;
			using (MemoryStream ms = new MemoryStream(plain))
			{
				using (BinaryReader br = new BinaryReader(ms))
				{
					int dlen = br.ReadInt32();
					if (dlen < 0 || dlen > plain.Length - 4)
					{
						throw new InvalidDataException("Invalid length prefix in decrypted data.");
					}
					array = br.ReadBytes(dlen);
				}
			}
			return array;
		}

		public static byte[] EncryptString(byte[] key, string text)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (text == null)
			{
				text = string.Empty;
			}
			byte[] plain;
			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter bw = new BinaryWriter(ms))
				{
					bw.Write(text);
					bw.Flush();
					plain = ms.ToArray();
				}
			}
			byte[] padded = SecurityTools.PadToBlockMultiple(plain);
			return SecurityTools.EncryptEcbNoPadding(key, padded);
		}

		public static string DecryptString(byte[] key, byte[] code)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (code == null)
			{
				throw new ArgumentNullException("code");
			}
			if (code.Length % 16 != 0)
			{
				throw new ArgumentException("Ciphertext length must be a multiple of 16.", "code");
			}
			byte[] plain = SecurityTools.DecryptEcbNoPadding(key, code);
			string text;
			using (MemoryStream ms = new MemoryStream(plain))
			{
				using (BinaryReader br = new BinaryReader(ms))
				{
					text = br.ReadString();
				}
			}
			return text;
		}

		public static string EncryptStringText(string password, string text)
		{
			if (password == null)
			{
				password = string.Empty;
			}
			byte[] key = SecurityTools.GenerateKey(password);
			byte[] data = SecurityTools.EncryptString(key, text);
			return TextConverter.ToBase32String(data);
		}

		public static string DecryptStringText(string password, string text)
		{
			if (password == null)
			{
				password = string.Empty;
			}
			byte[] data = TextConverter.FromBase32String(text);
			byte[] key = SecurityTools.GenerateKey(password);
			return SecurityTools.DecryptString(key, data);
		}

		public static byte[] GenerateKey(string password)
		{
			if (password == null)
			{
				password = string.Empty;
			}
			byte[] array;
			using (MD5 md5 = MD5.Create())
			{
				array = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
			}
			return array;
		}

		private const int AesBlockSizeBytes = 16;

		public static char[] DefaultCharSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`~!@#$%^&*()-=_+[]{}\\|:';<>,.?".ToCharArray();

		public static char[] SimpleAlphanumericCharSet = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
	}
}
