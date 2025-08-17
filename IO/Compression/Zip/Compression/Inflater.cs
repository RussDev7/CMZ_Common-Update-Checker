﻿using System;
using DNA.IO.Checksums;
using DNA.IO.Compression.Zip.Compression.Streams;

namespace DNA.IO.Compression.Zip.Compression
{
	public class Inflater
	{
		public Inflater()
			: this(false)
		{
		}

		public Inflater(bool noHeader)
		{
			this.noHeader = noHeader;
			this.adler = new Adler32();
			this.input = new StreamManipulator();
			this.outputWindow = new OutputWindow();
			this.mode = (noHeader ? 2 : 0);
		}

		public void Reset()
		{
			this.mode = (this.noHeader ? 2 : 0);
			this.totalIn = (this.totalOut = 0);
			this.input.Reset();
			this.outputWindow.Reset();
			this.dynHeader = null;
			this.litlenTree = null;
			this.distTree = null;
			this.isLastBlock = false;
			this.adler.Reset();
		}

		private bool DecodeHeader()
		{
			int num = this.input.PeekBits(16);
			if (num < 0)
			{
				return false;
			}
			this.input.DropBits(16);
			num = ((num << 8) | (num >> 8)) & 65535;
			if (num % 31 != 0)
			{
				throw new CompressionException("Header checksum illegal");
			}
			if ((num & 3840) != Deflater.Deflated << 8)
			{
				throw new CompressionException("Compression Method unknown");
			}
			if ((num & 32) == 0)
			{
				this.mode = 2;
			}
			else
			{
				this.mode = 1;
				this.neededBits = 32;
			}
			return true;
		}

		private bool DecodeDict()
		{
			while (this.neededBits > 0)
			{
				int num = this.input.PeekBits(8);
				if (num < 0)
				{
					return false;
				}
				this.input.DropBits(8);
				this.readAdler = (this.readAdler << 8) | num;
				this.neededBits -= 8;
			}
			return false;
		}

		private bool DecodeHuffman()
		{
			int i = this.outputWindow.GetFreeSpace();
			while (i >= 258)
			{
				int num;
				switch (this.mode)
				{
				case 7:
					while (((num = this.litlenTree.GetSymbol(this.input)) & -256) == 0)
					{
						this.outputWindow.Write(num);
						if (--i < 258)
						{
							return true;
						}
					}
					if (num >= 257)
					{
						try
						{
							this.repLength = Inflater.CPLENS[num - 257];
							this.neededBits = Inflater.CPLEXT[num - 257];
						}
						catch (Exception)
						{
							throw new CompressionException("Illegal rep length code");
						}
						goto IL_00C5;
					}
					if (num < 0)
					{
						return false;
					}
					this.distTree = null;
					this.litlenTree = null;
					this.mode = 2;
					return true;
				case 8:
					goto IL_00C5;
				case 9:
					goto IL_0114;
				case 10:
					break;
				default:
					throw new CompressionException("Inflater unknown mode");
				}
				IL_0154:
				if (this.neededBits > 0)
				{
					this.mode = 10;
					int num2 = this.input.PeekBits(this.neededBits);
					if (num2 < 0)
					{
						return false;
					}
					this.input.DropBits(this.neededBits);
					this.repDist += num2;
				}
				this.outputWindow.Repeat(this.repLength, this.repDist);
				i -= this.repLength;
				this.mode = 7;
				continue;
				IL_0114:
				num = this.distTree.GetSymbol(this.input);
				if (num < 0)
				{
					return false;
				}
				try
				{
					this.repDist = Inflater.CPDIST[num];
					this.neededBits = Inflater.CPDEXT[num];
				}
				catch (Exception)
				{
					throw new CompressionException("Illegal rep dist code");
				}
				goto IL_0154;
				IL_00C5:
				if (this.neededBits > 0)
				{
					this.mode = 8;
					int num3 = this.input.PeekBits(this.neededBits);
					if (num3 < 0)
					{
						return false;
					}
					this.input.DropBits(this.neededBits);
					this.repLength += num3;
				}
				this.mode = 9;
				goto IL_0114;
			}
			return true;
		}

		private bool DecodeChksum()
		{
			while (this.neededBits > 0)
			{
				int num = this.input.PeekBits(8);
				if (num < 0)
				{
					return false;
				}
				this.input.DropBits(8);
				this.readAdler = (this.readAdler << 8) | num;
				this.neededBits -= 8;
			}
			if (this.adler.Value != (uint)this.readAdler)
			{
				throw new CompressionException(string.Concat(new object[]
				{
					"Adler chksum doesn't match: ",
					(int)this.adler.Value,
					" vs. ",
					this.readAdler
				}));
			}
			this.mode = 12;
			return false;
		}

		private bool Decode()
		{
			switch (this.mode)
			{
			case 0:
				return this.DecodeHeader();
			case 1:
				return this.DecodeDict();
			case 2:
				if (this.isLastBlock)
				{
					if (this.noHeader)
					{
						this.mode = 12;
						return false;
					}
					this.input.SkipToByteBoundary();
					this.neededBits = 32;
					this.mode = 11;
					return true;
				}
				else
				{
					int num = this.input.PeekBits(3);
					if (num < 0)
					{
						return false;
					}
					this.input.DropBits(3);
					if ((num & 1) != 0)
					{
						this.isLastBlock = true;
					}
					switch (num >> 1)
					{
					case 0:
						this.input.SkipToByteBoundary();
						this.mode = 3;
						break;
					case 1:
						this.litlenTree = InflaterHuffmanTree.defLitLenTree;
						this.distTree = InflaterHuffmanTree.defDistTree;
						this.mode = 7;
						break;
					case 2:
						this.dynHeader = new InflaterDynHeader();
						this.mode = 6;
						break;
					default:
						throw new CompressionException("Unknown block type " + num);
					}
					return true;
				}
				break;
			case 3:
				if ((this.uncomprLen = this.input.PeekBits(16)) < 0)
				{
					return false;
				}
				this.input.DropBits(16);
				this.mode = 4;
				break;
			case 4:
				break;
			case 5:
				goto IL_01A9;
			case 6:
				if (!this.dynHeader.Decode(this.input))
				{
					return false;
				}
				this.litlenTree = this.dynHeader.BuildLitLenTree();
				this.distTree = this.dynHeader.BuildDistTree();
				this.mode = 7;
				goto IL_022D;
			case 7:
			case 8:
			case 9:
			case 10:
				goto IL_022D;
			case 11:
				return this.DecodeChksum();
			case 12:
				return false;
			default:
				throw new CompressionException("Inflater.Decode unknown mode");
			}
			int num2 = this.input.PeekBits(16);
			if (num2 < 0)
			{
				return false;
			}
			this.input.DropBits(16);
			if (num2 != (this.uncomprLen ^ 65535))
			{
				throw new CompressionException("broken uncompressed block");
			}
			this.mode = 5;
			IL_01A9:
			int num3 = this.outputWindow.CopyStored(this.input, this.uncomprLen);
			this.uncomprLen -= num3;
			if (this.uncomprLen == 0)
			{
				this.mode = 2;
				return true;
			}
			return !this.input.IsNeedingInput;
			IL_022D:
			return this.DecodeHuffman();
		}

		public void SetDictionary(byte[] buffer)
		{
			this.SetDictionary(buffer, 0, buffer.Length);
		}

		public void SetDictionary(byte[] buffer, int offset, int len)
		{
			if (!this.IsNeedingDictionary)
			{
				throw new InvalidOperationException();
			}
			this.adler.Update(buffer, offset, len);
			if (this.adler.Value != (uint)this.readAdler)
			{
				throw new CompressionException("Wrong adler checksum");
			}
			this.adler.Reset();
			this.outputWindow.CopyDict(buffer, offset, len);
			this.mode = 2;
		}

		public void SetInput(byte[] buf)
		{
			this.SetInput(buf, 0, buf.Length);
		}

		public void SetInput(byte[] buffer, int offset, int length)
		{
			this.input.SetInput(buffer, offset, length);
			this.totalIn += length;
		}

		public int Inflate(byte[] buf)
		{
			return this.Inflate(buf, 0, buf.Length);
		}

		public int Inflate(byte[] buf, int offset, int len)
		{
			if (len < 0)
			{
				throw new ArgumentOutOfRangeException("len < 0");
			}
			if (len == 0)
			{
				if (!this.IsFinished)
				{
					this.Decode();
				}
				return 0;
			}
			int num = 0;
			for (;;)
			{
				if (this.mode != 11)
				{
					int num2 = this.outputWindow.CopyOutput(buf, offset, len);
					this.adler.Update(buf, offset, num2);
					offset += num2;
					num += num2;
					this.totalOut += num2;
					len -= num2;
					if (len == 0)
					{
						break;
					}
				}
				if (!this.Decode() && (this.outputWindow.GetAvailable() <= 0 || this.mode == 11))
				{
					return num;
				}
			}
			return num;
		}

		public bool IsNeedingInput
		{
			get
			{
				return this.input.IsNeedingInput;
			}
		}

		public bool IsNeedingDictionary
		{
			get
			{
				return this.mode == 1 && this.neededBits == 0;
			}
		}

		public bool IsFinished
		{
			get
			{
				return this.mode == 12 && this.outputWindow.GetAvailable() == 0;
			}
		}

		public int Adler
		{
			get
			{
				if (!this.IsNeedingDictionary)
				{
					return (int)this.adler.Value;
				}
				return this.readAdler;
			}
		}

		public int TotalOut
		{
			get
			{
				return this.totalOut;
			}
		}

		public int TotalIn
		{
			get
			{
				return this.totalIn - this.RemainingInput;
			}
		}

		public int RemainingInput
		{
			get
			{
				return this.input.AvailableBytes;
			}
		}

		private const int DECODE_HEADER = 0;

		private const int DECODE_DICT = 1;

		private const int DECODE_BLOCKS = 2;

		private const int DECODE_STORED_LEN1 = 3;

		private const int DECODE_STORED_LEN2 = 4;

		private const int DECODE_STORED = 5;

		private const int DECODE_DYN_HEADER = 6;

		private const int DECODE_HUFFMAN = 7;

		private const int DECODE_HUFFMAN_LENBITS = 8;

		private const int DECODE_HUFFMAN_DIST = 9;

		private const int DECODE_HUFFMAN_DISTBITS = 10;

		private const int DECODE_CHKSUM = 11;

		private const int FINISHED = 12;

		private static int[] CPLENS = new int[]
		{
			3, 4, 5, 6, 7, 8, 9, 10, 11, 13,
			15, 17, 19, 23, 27, 31, 35, 43, 51, 59,
			67, 83, 99, 115, 131, 163, 195, 227, 258
		};

		private static int[] CPLEXT = new int[]
		{
			0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
			1, 1, 2, 2, 2, 2, 3, 3, 3, 3,
			4, 4, 4, 4, 5, 5, 5, 5, 0
		};

		private static int[] CPDIST = new int[]
		{
			1, 2, 3, 4, 5, 7, 9, 13, 17, 25,
			33, 49, 65, 97, 129, 193, 257, 385, 513, 769,
			1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577
		};

		private static int[] CPDEXT = new int[]
		{
			0, 0, 0, 0, 1, 1, 2, 2, 3, 3,
			4, 4, 5, 5, 6, 6, 7, 7, 8, 8,
			9, 9, 10, 10, 11, 11, 12, 12, 13, 13
		};

		private int mode;

		private int readAdler;

		private int neededBits;

		private int repLength;

		private int repDist;

		private int uncomprLen;

		private bool isLastBlock;

		private int totalOut;

		private int totalIn;

		private bool noHeader;

		private StreamManipulator input;

		private OutputWindow outputWindow;

		private InflaterDynHeader dynHeader;

		private InflaterHuffmanTree litlenTree;

		private InflaterHuffmanTree distTree;

		private Adler32 adler;
	}
}
