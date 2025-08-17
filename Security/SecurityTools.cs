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
			Random random = new Random();
			return SecurityTools.GeneratePassword(length, charset, random);
		}

		public static string GeneratePassword(int length, char[] charset, Random rand)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < length; i++)
			{
				int num = rand.Next(charset.Length);
				stringBuilder.Append(charset[num]);
			}
			return stringBuilder.ToString();
		}

		public static byte[] EncryptData(byte[] key, byte[] data)
		{
			MemoryStream memoryStream = new MemoryStream();
			AesFastEngine aesFastEngine = new AesFastEngine();
			KeyParameter keyParameter = new KeyParameter(key);
			aesFastEngine.Init(true, keyParameter);
			BufferedBlockCipher bufferedBlockCipher = new BufferedBlockCipher(aesFastEngine);
			CipherStream cipherStream = new CipherStream(memoryStream, null, bufferedBlockCipher);
			BinaryWriter binaryWriter = new BinaryWriter(cipherStream);
			binaryWriter.Write(data.Length);
			binaryWriter.Write(data);
			binaryWriter.Flush();
			int blockSize = bufferedBlockCipher.GetBlockSize();
			int bufOff = bufferedBlockCipher.bufOff;
			int num = blockSize - bufOff % blockSize;
			for (int i = 0; i < num; i++)
			{
				binaryWriter.Write(0);
			}
			cipherStream.Close();
			return memoryStream.ToArray();
		}

		public static byte[] DecryptData(byte[] key, byte[] code)
		{
			MemoryStream memoryStream = new MemoryStream(code);
			AesFastEngine aesFastEngine = new AesFastEngine();
			KeyParameter keyParameter = new KeyParameter(key);
			aesFastEngine.Init(false, keyParameter);
			BufferedBlockCipher bufferedBlockCipher = new BufferedBlockCipher(aesFastEngine);
			CipherStream cipherStream = new CipherStream(memoryStream, bufferedBlockCipher, null);
			BinaryReader binaryReader = new BinaryReader(cipherStream);
			int num = binaryReader.ReadInt32();
			return binaryReader.ReadBytes(num);
		}

		public static string EncryptStringText(string password, string text)
		{
			MD5HashProvider md5HashProvider = new MD5HashProvider();
			Hash hash = md5HashProvider.Compute(Encoding.UTF8.GetBytes(password));
			byte[] array = SecurityTools.EncryptString(hash.Data, text);
			return TextConverter.ToBase32String(array);
		}

		public static string DecryptStringText(string password, string text)
		{
			byte[] array = TextConverter.FromBase32String(text);
			MD5HashProvider md5HashProvider = new MD5HashProvider();
			Hash hash = md5HashProvider.Compute(Encoding.UTF8.GetBytes(password));
			return SecurityTools.DecryptString(hash.Data, array);
		}

		public static byte[] EncryptString(byte[] key, string text)
		{
			MemoryStream memoryStream = new MemoryStream();
			AesFastEngine aesFastEngine = new AesFastEngine();
			KeyParameter keyParameter = new KeyParameter(key);
			aesFastEngine.Init(true, keyParameter);
			BufferedBlockCipher bufferedBlockCipher = new BufferedBlockCipher(aesFastEngine);
			CipherStream cipherStream = new CipherStream(memoryStream, null, bufferedBlockCipher);
			BinaryWriter binaryWriter = new BinaryWriter(cipherStream);
			binaryWriter.Write(text);
			binaryWriter.Flush();
			int blockSize = bufferedBlockCipher.GetBlockSize();
			int bufOff = bufferedBlockCipher.bufOff;
			int num = blockSize - bufOff % blockSize;
			for (int i = 0; i < num; i++)
			{
				binaryWriter.Write(0);
			}
			cipherStream.Close();
			return memoryStream.ToArray();
		}

		public static string DecryptString(byte[] key, byte[] code)
		{
			MemoryStream memoryStream = new MemoryStream(code);
			AesFastEngine aesFastEngine = new AesFastEngine();
			KeyParameter keyParameter = new KeyParameter(key);
			aesFastEngine.Init(false, keyParameter);
			BufferedBlockCipher bufferedBlockCipher = new BufferedBlockCipher(aesFastEngine);
			CipherStream cipherStream = new CipherStream(memoryStream, bufferedBlockCipher, null);
			BinaryReader binaryReader = new BinaryReader(cipherStream);
			return binaryReader.ReadString();
		}

		public static byte[] GenerateKey(string password)
		{
			throw new NotImplementedException();
		}

		public static void WriteSignedData(BinaryWriter writer, RSAKey privateKey, byte[] data)
		{
			RSASignatureProvider rsasignatureProvider = new RSASignatureProvider(new SHA256HashProvider(), privateKey);
			Signature signature = rsasignatureProvider.Sign(data);
			writer.Write(1936089973);
			writer.Write(1);
			writer.Write(data.Length);
			writer.Write(data);
			writer.Write(signature.Data.Length);
			writer.Write(signature.Data);
		}

		public static byte[] ReadSignedData(BinaryReader reader, RSAKey publicKey)
		{
			RSASignatureProvider rsasignatureProvider = new RSASignatureProvider(new SHA256HashProvider(), publicKey);
			if (reader.ReadInt32() != 1936089973 || reader.ReadInt32() != 1)
			{
				throw new Exception("Bad Data Format");
			}
			int num = reader.ReadInt32();
			byte[] array = reader.ReadBytes(num);
			int num2 = reader.ReadInt32();
			byte[] array2 = reader.ReadBytes(num2);
			Signature signature = rsasignatureProvider.FromByteArray(array2);
			if (!signature.Verify(rsasignatureProvider, array))
			{
				throw new Exception("Data Corrupt");
			}
			return array;
		}

		private const int SignedDataID = 1936089973;

		private const int SignedDataVersion = 1;

		public static char[] DefaultCharSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`~!@#$%^&*()-=_+[]{}\\|:';<>,.?".ToCharArray();

		public static char[] SimpleAlphanumericCharSet = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
	}
}
