using System;
using DNA.Security.Cryptography.Crypto.Parameters;

namespace DNA.Security.Cryptography.Crypto
{
	public class BufferedBlockCipher : BufferedCipherBase
	{
		protected BufferedBlockCipher()
		{
		}

		public BufferedBlockCipher(IBlockCipher cipher)
		{
			if (cipher == null)
			{
				throw new ArgumentNullException("cipher");
			}
			this.cipher = cipher;
			this.buf = new byte[cipher.GetBlockSize()];
			this.bufOff = 0;
		}

		public override string AlgorithmName
		{
			get
			{
				return this.cipher.AlgorithmName;
			}
		}

		public override void Init(bool forEncryption, ICipherParameters parameters)
		{
			this.forEncryption = forEncryption;
			if (parameters is ParametersWithRandom)
			{
				parameters = ((ParametersWithRandom)parameters).Parameters;
			}
			this.Reset();
			this.cipher.Init(forEncryption, parameters);
		}

		public override int GetBlockSize()
		{
			return this.cipher.GetBlockSize();
		}

		public override int GetUpdateOutputSize(int length)
		{
			int total = length + this.bufOff;
			int leftOver = total % this.buf.Length;
			return total - leftOver;
		}

		public override int GetOutputSize(int length)
		{
			return length + this.bufOff;
		}

		public override int ProcessByte(byte input, byte[] output, int outOff)
		{
			this.buf[this.bufOff++] = input;
			if (this.bufOff != this.buf.Length)
			{
				return 0;
			}
			if (outOff + this.buf.Length > output.Length)
			{
				throw new DataLengthException("output buffer too short");
			}
			this.bufOff = 0;
			return this.cipher.ProcessBlock(this.buf, 0, output, outOff);
		}

		public override byte[] ProcessByte(byte input)
		{
			int outLength = this.GetUpdateOutputSize(1);
			byte[] outBytes = ((outLength > 0) ? new byte[outLength] : null);
			int pos = this.ProcessByte(input, outBytes, 0);
			if (outLength > 0 && pos < outLength)
			{
				byte[] tmp = new byte[pos];
				Array.Copy(outBytes, 0, tmp, 0, pos);
				outBytes = tmp;
			}
			return outBytes;
		}

		public override byte[] ProcessBytes(byte[] input, int inOff, int length)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			if (length < 1)
			{
				return null;
			}
			int outLength = this.GetUpdateOutputSize(length);
			byte[] outBytes = ((outLength > 0) ? new byte[outLength] : null);
			int pos = this.ProcessBytes(input, inOff, length, outBytes, 0);
			if (outLength > 0 && pos < outLength)
			{
				byte[] tmp = new byte[pos];
				Array.Copy(outBytes, 0, tmp, 0, pos);
				outBytes = tmp;
			}
			return outBytes;
		}

		public override int ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff)
		{
			if (length < 1)
			{
				if (length < 0)
				{
					throw new ArgumentException("Can't have a negative input length!");
				}
				return 0;
			}
			else
			{
				int blockSize = this.GetBlockSize();
				int outLength = this.GetUpdateOutputSize(length);
				if (outLength > 0 && outOff + outLength > output.Length)
				{
					throw new DataLengthException("output buffer too short");
				}
				int resultLen = 0;
				int gapLen = this.buf.Length - this.bufOff;
				if (length > gapLen)
				{
					Array.Copy(input, inOff, this.buf, this.bufOff, gapLen);
					resultLen += this.cipher.ProcessBlock(this.buf, 0, output, outOff);
					this.bufOff = 0;
					length -= gapLen;
					inOff += gapLen;
					while (length > this.buf.Length)
					{
						resultLen += this.cipher.ProcessBlock(input, inOff, output, outOff + resultLen);
						length -= blockSize;
						inOff += blockSize;
					}
				}
				Array.Copy(input, inOff, this.buf, this.bufOff, length);
				this.bufOff += length;
				if (this.bufOff == this.buf.Length)
				{
					resultLen += this.cipher.ProcessBlock(this.buf, 0, output, outOff + resultLen);
					this.bufOff = 0;
				}
				return resultLen;
			}
		}

		public override byte[] DoFinal()
		{
			byte[] outBytes = BufferedCipherBase.EmptyBuffer;
			int length = this.GetOutputSize(0);
			if (length > 0)
			{
				outBytes = new byte[length];
				int pos = this.DoFinal(outBytes, 0);
				if (pos < outBytes.Length)
				{
					byte[] tmp = new byte[pos];
					Array.Copy(outBytes, 0, tmp, 0, pos);
					outBytes = tmp;
				}
			}
			else
			{
				this.Reset();
			}
			return outBytes;
		}

		public override byte[] DoFinal(byte[] input, int inOff, int inLen)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			int length = this.GetOutputSize(inLen);
			byte[] outBytes = BufferedCipherBase.EmptyBuffer;
			if (length > 0)
			{
				outBytes = new byte[length];
				int pos = ((inLen > 0) ? this.ProcessBytes(input, inOff, inLen, outBytes, 0) : 0);
				pos += this.DoFinal(outBytes, pos);
				if (pos < outBytes.Length)
				{
					byte[] tmp = new byte[pos];
					Array.Copy(outBytes, 0, tmp, 0, pos);
					outBytes = tmp;
				}
			}
			else
			{
				this.Reset();
			}
			return outBytes;
		}

		public override int DoFinal(byte[] output, int outOff)
		{
			if (this.bufOff != 0)
			{
				if (!this.cipher.IsPartialBlockOkay)
				{
					throw new DataLengthException("data not block size aligned");
				}
				if (outOff + this.bufOff > output.Length)
				{
					throw new DataLengthException("output buffer too short for DoFinal()");
				}
				this.cipher.ProcessBlock(this.buf, 0, this.buf, 0);
				Array.Copy(this.buf, 0, output, outOff, this.bufOff);
			}
			int resultLen = this.bufOff;
			this.Reset();
			return resultLen;
		}

		public override void Reset()
		{
			Array.Clear(this.buf, 0, this.buf.Length);
			this.bufOff = 0;
			this.cipher.Reset();
		}

		internal byte[] buf;

		internal int bufOff;

		internal bool forEncryption;

		internal IBlockCipher cipher;
	}
}
