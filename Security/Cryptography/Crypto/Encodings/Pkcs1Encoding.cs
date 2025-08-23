using System;
using DNA.Security.Cryptography.Crypto.Parameters;
using DNA.Security.Cryptography.Security;
using DNA.Security.Cryptography.Utilities;

namespace DNA.Security.Cryptography.Crypto.Encodings
{
	public class Pkcs1Encoding : IAsymmetricBlockCipher
	{
		public static bool StrictLengthEnabled
		{
			get
			{
				return Pkcs1Encoding.strictLengthEnabled[0];
			}
			set
			{
				Pkcs1Encoding.strictLengthEnabled[0] = value;
			}
		}

		static Pkcs1Encoding()
		{
			string strictProperty = Platform.GetEnvironmentVariable("DNA.Security.Cryptography.Pkcs1.Strict");
			Pkcs1Encoding.strictLengthEnabled = new bool[] { strictProperty == null || strictProperty.Equals("true") };
		}

		public Pkcs1Encoding(IAsymmetricBlockCipher cipher)
		{
			this.engine = cipher;
			this.useStrictLength = Pkcs1Encoding.StrictLengthEnabled;
		}

		public IAsymmetricBlockCipher GetUnderlyingCipher()
		{
			return this.engine;
		}

		public string AlgorithmName
		{
			get
			{
				return this.engine.AlgorithmName + "/PKCS1Padding";
			}
		}

		public void Init(bool forEncryption, ICipherParameters parameters)
		{
			AsymmetricKeyParameter kParam;
			if (parameters is ParametersWithRandom)
			{
				ParametersWithRandom rParam = (ParametersWithRandom)parameters;
				this.random = rParam.Random;
				kParam = (AsymmetricKeyParameter)rParam.Parameters;
			}
			else
			{
				this.random = new SecureRandom();
				kParam = (AsymmetricKeyParameter)parameters;
			}
			this.engine.Init(forEncryption, parameters);
			this.forPrivateKey = kParam.IsPrivate;
			this.forEncryption = forEncryption;
		}

		public int GetInputBlockSize()
		{
			int baseBlockSize = this.engine.GetInputBlockSize();
			if (!this.forEncryption)
			{
				return baseBlockSize;
			}
			return baseBlockSize - 10;
		}

		public int GetOutputBlockSize()
		{
			int baseBlockSize = this.engine.GetOutputBlockSize();
			if (!this.forEncryption)
			{
				return baseBlockSize - 10;
			}
			return baseBlockSize;
		}

		public byte[] ProcessBlock(byte[] input, int inOff, int length)
		{
			if (!this.forEncryption)
			{
				return this.DecodeBlock(input, inOff, length);
			}
			return this.EncodeBlock(input, inOff, length);
		}

		private byte[] EncodeBlock(byte[] input, int inOff, int inLen)
		{
			if (inLen > this.GetInputBlockSize())
			{
				throw new ArgumentException("input data too large", "inLen");
			}
			byte[] block = new byte[this.engine.GetInputBlockSize()];
			if (this.forPrivateKey)
			{
				block[0] = 1;
				for (int i = 1; i != block.Length - inLen - 1; i++)
				{
					block[i] = byte.MaxValue;
				}
			}
			else
			{
				this.random.NextBytes(block);
				block[0] = 2;
				for (int j = 1; j != block.Length - inLen - 1; j++)
				{
					while (block[j] == 0)
					{
						block[j] = (byte)this.random.NextInt();
					}
				}
			}
			block[block.Length - inLen - 1] = 0;
			Array.Copy(input, inOff, block, block.Length - inLen, inLen);
			return this.engine.ProcessBlock(block, 0, block.Length);
		}

		private byte[] DecodeBlock(byte[] input, int inOff, int inLen)
		{
			byte[] block = this.engine.ProcessBlock(input, inOff, inLen);
			if (block.Length < this.GetOutputBlockSize())
			{
				throw new InvalidCipherTextException("block truncated");
			}
			byte type = block[0];
			if (type != 1 && type != 2)
			{
				throw new InvalidCipherTextException("unknown block type");
			}
			if (this.useStrictLength && block.Length != this.engine.GetOutputBlockSize())
			{
				throw new InvalidCipherTextException("block incorrect size");
			}
			int start;
			for (start = 1; start != block.Length; start++)
			{
				byte pad = block[start];
				if (pad == 0)
				{
					break;
				}
				if (type == 1 && pad != 255)
				{
					throw new InvalidCipherTextException("block padding incorrect");
				}
			}
			start++;
			if (start >= block.Length || start < 10)
			{
				throw new InvalidCipherTextException("no data in block");
			}
			byte[] result = new byte[block.Length - start];
			Array.Copy(block, start, result, 0, result.Length);
			return result;
		}

		public const string StrictLengthEnabledProperty = "DNA.Security.Cryptography.Pkcs1.Strict";

		private const int HeaderLength = 10;

		private static readonly bool[] strictLengthEnabled;

		private SecureRandom random;

		private IAsymmetricBlockCipher engine;

		private bool forEncryption;

		private bool forPrivateKey;

		private bool useStrictLength;
	}
}
