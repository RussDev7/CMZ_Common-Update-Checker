using System;
using System.IO;
using System.Text;
using DNA.Security.Cryptography;
using DNA.Security.Cryptography.Crypto;
using DNA.Security.Cryptography.Crypto.Engines;
using DNA.Security.Cryptography.Crypto.IO;
using DNA.Security.Cryptography.Crypto.Parameters;
using DNA.Text;

namespace DNA.Security
{
	public static class SecurityTools
	{
		public static string GeneratePassword(int length)
		{
			new Random();
			return SecurityTools.GeneratePassword(length, SecurityTools.DefaultCharSet);
		}

		public static string GeneratePassword(int length, char[] charset)
		{
			Random rand = new Random();
			return SecurityTools.GeneratePassword(length, charset, rand);
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

		public static byte[] EncryptData(byte[] key, byte[] data)
		{
			MemoryStream stream = new MemoryStream();
			AesFastEngine encrypter = new AesFastEngine();
			KeyParameter keyParam = new KeyParameter(key);
			encrypter.Init(true, keyParam);
			BufferedBlockCipher cipher = new BufferedBlockCipher(encrypter);
			CipherStream encryptStream = new CipherStream(stream, null, cipher);
			BinaryWriter writer = new BinaryWriter(encryptStream);
			writer.Write(data.Length);
			writer.Write(data);
			writer.Flush();
			int blockSize = cipher.GetBlockSize();
			int dataSize = cipher.bufOff;
			int padBytes = blockSize - dataSize % blockSize;
			for (int i = 0; i < padBytes; i++)
			{
				writer.Write(0);
			}
			encryptStream.Close();
			return stream.ToArray();
		}

		public static byte[] DecryptData(byte[] key, byte[] code)
		{
			MemoryStream stream = new MemoryStream(code);
			AesFastEngine cipher = new AesFastEngine();
			KeyParameter keyParam = new KeyParameter(key);
			cipher.Init(false, keyParam);
			BufferedBlockCipher bufCypher = new BufferedBlockCipher(cipher);
			CipherStream encryptStream = new CipherStream(stream, bufCypher, null);
			BinaryReader reader = new BinaryReader(encryptStream);
			int dlen = reader.ReadInt32();
			return reader.ReadBytes(dlen);
		}

		public static string EncryptStringText(string password, string text)
		{
			MD5HashProvider hasher = new MD5HashProvider();
			Hash hash = hasher.Compute(Encoding.UTF8.GetBytes(password));
			byte[] data = SecurityTools.EncryptString(hash.Data, text);
			return TextConverter.ToBase32String(data);
		}

		public static string DecryptStringText(string password, string text)
		{
			byte[] data = TextConverter.FromBase32String(text);
			MD5HashProvider hasher = new MD5HashProvider();
			Hash hash = hasher.Compute(Encoding.UTF8.GetBytes(password));
			return SecurityTools.DecryptString(hash.Data, data);
		}

		public static byte[] EncryptString(byte[] key, string text)
		{
			MemoryStream stream = new MemoryStream();
			AesFastEngine encrypter = new AesFastEngine();
			KeyParameter keyParam = new KeyParameter(key);
			encrypter.Init(true, keyParam);
			BufferedBlockCipher cipher = new BufferedBlockCipher(encrypter);
			CipherStream encryptStream = new CipherStream(stream, null, cipher);
			BinaryWriter writer = new BinaryWriter(encryptStream);
			writer.Write(text);
			writer.Flush();
			int blockSize = cipher.GetBlockSize();
			int dataSize = cipher.bufOff;
			int padBytes = blockSize - dataSize % blockSize;
			for (int i = 0; i < padBytes; i++)
			{
				writer.Write(0);
			}
			encryptStream.Close();
			return stream.ToArray();
		}

		public static string DecryptString(byte[] key, byte[] code)
		{
			MemoryStream stream = new MemoryStream(code);
			AesFastEngine cipher = new AesFastEngine();
			KeyParameter keyParam = new KeyParameter(key);
			cipher.Init(false, keyParam);
			BufferedBlockCipher bufCypher = new BufferedBlockCipher(cipher);
			CipherStream encryptStream = new CipherStream(stream, bufCypher, null);
			BinaryReader reader = new BinaryReader(encryptStream);
			return reader.ReadString();
		}

		public static byte[] GenerateKey(string password)
		{
			throw new NotImplementedException();
		}

		public static void WriteSignedData(BinaryWriter writer, RSAKey privateKey, byte[] data)
		{
			RSASignatureProvider signer = new RSASignatureProvider(new SHA256HashProvider(), privateKey);
			Signature signiture = signer.Sign(data);
			writer.Write(1936089973);
			writer.Write(1);
			writer.Write(data.Length);
			writer.Write(data);
			writer.Write(signiture.Data.Length);
			writer.Write(signiture.Data);
		}

		public static byte[] ReadSignedData(BinaryReader reader, RSAKey publicKey)
		{
			RSASignatureProvider signer = new RSASignatureProvider(new SHA256HashProvider(), publicKey);
			if (reader.ReadInt32() != 1936089973 || reader.ReadInt32() != 1)
			{
				throw new Exception("Bad Data Format");
			}
			int dataLen = reader.ReadInt32();
			byte[] data = reader.ReadBytes(dataLen);
			int sigLen = reader.ReadInt32();
			byte[] sigData = reader.ReadBytes(sigLen);
			Signature sig = signer.FromByteArray(sigData);
			if (!sig.Verify(signer, data))
			{
				throw new Exception("Data Corrupt");
			}
			return data;
		}

		private const int SignedDataID = 1936089973;

		private const int SignedDataVersion = 1;

		public static char[] DefaultCharSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`~!@#$%^&*()-=_+[]{}\\|:';<>,.?".ToCharArray();

		public static char[] SimpleAlphanumericCharSet = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
	}
}
