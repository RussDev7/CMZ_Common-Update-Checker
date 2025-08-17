﻿using System;
using DNA.IO.Checksums;

namespace DNA.IO.Compression.Zip.Compression
{
	public class DeflaterEngine : DeflaterConstants
	{
		public DeflaterEngine(DeflaterPending pending)
		{
			this.pending = pending;
			this.huffman = new DeflaterHuffman(pending);
			this.adler = new Adler32();
			this.window = new byte[65536];
			this.head = new short[32768];
			this.prev = new short[32768];
			this.blockStart = (this.strstart = 1);
		}

		public void Reset()
		{
			this.huffman.Reset();
			this.adler.Reset();
			this.blockStart = (this.strstart = 1);
			this.lookahead = 0;
			this.totalIn = 0;
			this.prevAvailable = false;
			this.matchLen = 2;
			for (int i = 0; i < 32768; i++)
			{
				this.head[i] = 0;
			}
			for (int j = 0; j < 32768; j++)
			{
				this.prev[j] = 0;
			}
		}

		public void ResetAdler()
		{
			this.adler.Reset();
		}

		public int Adler
		{
			get
			{
				return (int)this.adler.Value;
			}
		}

		public int TotalIn
		{
			get
			{
				return this.totalIn;
			}
		}

		public DeflateStrategy Strategy
		{
			get
			{
				return this.strategy;
			}
			set
			{
				this.strategy = value;
			}
		}

		public void SetLevel(int lvl)
		{
			this.goodLength = DeflaterConstants.GOOD_LENGTH[lvl];
			this.max_lazy = DeflaterConstants.MAX_LAZY[lvl];
			this.niceLength = DeflaterConstants.NICE_LENGTH[lvl];
			this.max_chain = DeflaterConstants.MAX_CHAIN[lvl];
			if (DeflaterConstants.COMPR_FUNC[lvl] != this.comprFunc)
			{
				switch (this.comprFunc)
				{
				case 0:
					if (this.strstart > this.blockStart)
					{
						this.huffman.FlushStoredBlock(this.window, this.blockStart, this.strstart - this.blockStart, false);
						this.blockStart = this.strstart;
					}
					this.UpdateHash();
					break;
				case 1:
					if (this.strstart > this.blockStart)
					{
						this.huffman.FlushBlock(this.window, this.blockStart, this.strstart - this.blockStart, false);
						this.blockStart = this.strstart;
					}
					break;
				case 2:
					if (this.prevAvailable)
					{
						this.huffman.TallyLit((int)(this.window[this.strstart - 1] & byte.MaxValue));
					}
					if (this.strstart > this.blockStart)
					{
						this.huffman.FlushBlock(this.window, this.blockStart, this.strstart - this.blockStart, false);
						this.blockStart = this.strstart;
					}
					this.prevAvailable = false;
					this.matchLen = 2;
					break;
				}
				this.comprFunc = DeflaterConstants.COMPR_FUNC[lvl];
			}
		}

		private void UpdateHash()
		{
			this.ins_h = ((int)this.window[this.strstart] << 5) ^ (int)this.window[this.strstart + 1];
		}

		private int InsertString()
		{
			int num = ((this.ins_h << 5) ^ (int)this.window[this.strstart + 2]) & 32767;
			short num2 = (this.prev[this.strstart & 32767] = this.head[num]);
			this.head[num] = (short)this.strstart;
			this.ins_h = num;
			return (int)num2 & 65535;
		}

		private void SlideWindow()
		{
			Array.Copy(this.window, 32768, this.window, 0, 32768);
			this.matchStart -= 32768;
			this.strstart -= 32768;
			this.blockStart -= 32768;
			for (int i = 0; i < 32768; i++)
			{
				int num = (int)this.head[i] & 65535;
				this.head[i] = (short)((num >= 32768) ? (num - 32768) : 0);
			}
			for (int j = 0; j < 32768; j++)
			{
				int num2 = (int)this.prev[j] & 65535;
				this.prev[j] = (short)((num2 >= 32768) ? (num2 - 32768) : 0);
			}
		}

		public void FillWindow()
		{
			if (this.strstart >= 65274)
			{
				this.SlideWindow();
			}
			while (this.lookahead < 262 && this.inputOff < this.inputEnd)
			{
				int num = 65536 - this.lookahead - this.strstart;
				if (num > this.inputEnd - this.inputOff)
				{
					num = this.inputEnd - this.inputOff;
				}
				Array.Copy(this.inputBuf, this.inputOff, this.window, this.strstart + this.lookahead, num);
				this.adler.Update(this.inputBuf, this.inputOff, num);
				this.inputOff += num;
				this.totalIn += num;
				this.lookahead += num;
			}
			if (this.lookahead >= 3)
			{
				this.UpdateHash();
			}
		}

		private bool FindLongestMatch(int curMatch)
		{
			int num = this.max_chain;
			int num2 = this.niceLength;
			short[] array = this.prev;
			int num3 = this.strstart;
			int num4 = this.strstart + this.matchLen;
			int num5 = Math.Max(this.matchLen, 2);
			int num6 = Math.Max(this.strstart - 32506, 0);
			int num7 = this.strstart + 258 - 1;
			byte b = this.window[num4 - 1];
			byte b2 = this.window[num4];
			if (num5 >= this.goodLength)
			{
				num >>= 2;
			}
			if (num2 > this.lookahead)
			{
				num2 = this.lookahead;
			}
			do
			{
				if (this.window[curMatch + num5] == b2 && this.window[curMatch + num5 - 1] == b && this.window[curMatch] == this.window[num3] && this.window[curMatch + 1] == this.window[num3 + 1])
				{
					int num8 = curMatch + 2;
					num3 += 2;
					while (this.window[++num3] == this.window[++num8] && this.window[++num3] == this.window[++num8] && this.window[++num3] == this.window[++num8] && this.window[++num3] == this.window[++num8] && this.window[++num3] == this.window[++num8] && this.window[++num3] == this.window[++num8] && this.window[++num3] == this.window[++num8] && this.window[++num3] == this.window[++num8] && num3 < num7)
					{
					}
					if (num3 > num4)
					{
						this.matchStart = curMatch;
						num4 = num3;
						num5 = num3 - this.strstart;
						if (num5 >= num2)
						{
							break;
						}
						b = this.window[num4 - 1];
						b2 = this.window[num4];
					}
					num3 = this.strstart;
				}
			}
			while ((curMatch = (int)array[curMatch & 32767] & 65535) > num6 && --num != 0);
			this.matchLen = Math.Min(num5, this.lookahead);
			return this.matchLen >= 3;
		}

		public void SetDictionary(byte[] buffer, int offset, int length)
		{
			this.adler.Update(buffer, offset, length);
			if (length < 3)
			{
				return;
			}
			if (length > 32506)
			{
				offset += length - 32506;
				length = 32506;
			}
			Array.Copy(buffer, offset, this.window, this.strstart, length);
			this.UpdateHash();
			length--;
			while (--length > 0)
			{
				this.InsertString();
				this.strstart++;
			}
			this.strstart += 2;
			this.blockStart = this.strstart;
		}

		private bool DeflateStored(bool flush, bool finish)
		{
			if (!flush && this.lookahead == 0)
			{
				return false;
			}
			this.strstart += this.lookahead;
			this.lookahead = 0;
			int num = this.strstart - this.blockStart;
			if (num >= DeflaterConstants.MAX_BLOCK_SIZE || (this.blockStart < 32768 && num >= 32506) || flush)
			{
				bool flag = finish;
				if (num > DeflaterConstants.MAX_BLOCK_SIZE)
				{
					num = DeflaterConstants.MAX_BLOCK_SIZE;
					flag = false;
				}
				this.huffman.FlushStoredBlock(this.window, this.blockStart, num, flag);
				this.blockStart += num;
				return !flag;
			}
			return true;
		}

		private bool DeflateFast(bool flush, bool finish)
		{
			if (this.lookahead < 262 && !flush)
			{
				return false;
			}
			while (this.lookahead >= 262 || flush)
			{
				if (this.lookahead == 0)
				{
					this.huffman.FlushBlock(this.window, this.blockStart, this.strstart - this.blockStart, finish);
					this.blockStart = this.strstart;
					return false;
				}
				if (this.strstart > 65274)
				{
					this.SlideWindow();
				}
				int num;
				if (this.lookahead >= 3 && (num = this.InsertString()) != 0 && this.strategy != DeflateStrategy.HuffmanOnly && this.strstart - num <= 32506 && this.FindLongestMatch(num))
				{
					if (this.huffman.TallyDist(this.strstart - this.matchStart, this.matchLen))
					{
						bool flag = finish && this.lookahead == 0;
						this.huffman.FlushBlock(this.window, this.blockStart, this.strstart - this.blockStart, flag);
						this.blockStart = this.strstart;
					}
					this.lookahead -= this.matchLen;
					if (this.matchLen <= this.max_lazy && this.lookahead >= 3)
					{
						while (--this.matchLen > 0)
						{
							this.strstart++;
							this.InsertString();
						}
						this.strstart++;
					}
					else
					{
						this.strstart += this.matchLen;
						if (this.lookahead >= 2)
						{
							this.UpdateHash();
						}
					}
					this.matchLen = 2;
				}
				else
				{
					this.huffman.TallyLit((int)(this.window[this.strstart] & byte.MaxValue));
					this.strstart++;
					this.lookahead--;
					if (this.huffman.IsFull())
					{
						bool flag2 = finish && this.lookahead == 0;
						this.huffman.FlushBlock(this.window, this.blockStart, this.strstart - this.blockStart, flag2);
						this.blockStart = this.strstart;
						return !flag2;
					}
				}
			}
			return true;
		}

		private bool DeflateSlow(bool flush, bool finish)
		{
			if (this.lookahead < 262 && !flush)
			{
				return false;
			}
			while (this.lookahead >= 262 || flush)
			{
				if (this.lookahead == 0)
				{
					if (this.prevAvailable)
					{
						this.huffman.TallyLit((int)(this.window[this.strstart - 1] & byte.MaxValue));
					}
					this.prevAvailable = false;
					this.huffman.FlushBlock(this.window, this.blockStart, this.strstart - this.blockStart, finish);
					this.blockStart = this.strstart;
					return false;
				}
				if (this.strstart >= 65274)
				{
					this.SlideWindow();
				}
				int num = this.matchStart;
				int num2 = this.matchLen;
				if (this.lookahead >= 3)
				{
					int num3 = this.InsertString();
					if (this.strategy != DeflateStrategy.HuffmanOnly && num3 != 0 && this.strstart - num3 <= 32506 && this.FindLongestMatch(num3) && this.matchLen <= 5 && (this.strategy == DeflateStrategy.Filtered || (this.matchLen == 3 && this.strstart - this.matchStart > DeflaterEngine.TOO_FAR)))
					{
						this.matchLen = 2;
					}
				}
				if (num2 >= 3 && this.matchLen <= num2)
				{
					this.huffman.TallyDist(this.strstart - 1 - num, num2);
					num2 -= 2;
					do
					{
						this.strstart++;
						this.lookahead--;
						if (this.lookahead >= 3)
						{
							this.InsertString();
						}
					}
					while (--num2 > 0);
					this.strstart++;
					this.lookahead--;
					this.prevAvailable = false;
					this.matchLen = 2;
				}
				else
				{
					if (this.prevAvailable)
					{
						this.huffman.TallyLit((int)(this.window[this.strstart - 1] & byte.MaxValue));
					}
					this.prevAvailable = true;
					this.strstart++;
					this.lookahead--;
				}
				if (this.huffman.IsFull())
				{
					int num4 = this.strstart - this.blockStart;
					if (this.prevAvailable)
					{
						num4--;
					}
					bool flag = finish && this.lookahead == 0 && !this.prevAvailable;
					this.huffman.FlushBlock(this.window, this.blockStart, num4, flag);
					this.blockStart += num4;
					return !flag;
				}
			}
			return true;
		}

		public bool Deflate(bool flush, bool finish)
		{
			for (;;)
			{
				this.FillWindow();
				bool flag = flush && this.inputOff == this.inputEnd;
				bool flag2;
				switch (this.comprFunc)
				{
				case 0:
					flag2 = this.DeflateStored(flag, finish);
					goto IL_0062;
				case 1:
					flag2 = this.DeflateFast(flag, finish);
					goto IL_0062;
				case 2:
					flag2 = this.DeflateSlow(flag, finish);
					goto IL_0062;
				}
				break;
				IL_0062:
				if (!this.pending.IsFlushed || !flag2)
				{
					return flag2;
				}
			}
			throw new InvalidOperationException("unknown comprFunc");
		}

		public void SetInput(byte[] buf, int off, int len)
		{
			if (this.inputOff < this.inputEnd)
			{
				throw new InvalidOperationException("Old input was not completely processed");
			}
			int num = off + len;
			if (0 > off || off > num || num > buf.Length)
			{
				throw new ArgumentOutOfRangeException();
			}
			this.inputBuf = buf;
			this.inputOff = off;
			this.inputEnd = num;
		}

		public bool NeedsInput()
		{
			return this.inputEnd == this.inputOff;
		}

		private static int TOO_FAR = 4096;

		private int ins_h;

		private short[] head;

		private short[] prev;

		private int matchStart;

		private int matchLen;

		private bool prevAvailable;

		private int blockStart;

		private int strstart;

		private int lookahead;

		private byte[] window;

		private DeflateStrategy strategy;

		private int max_chain;

		private int max_lazy;

		private int niceLength;

		private int goodLength;

		private int comprFunc;

		private byte[] inputBuf;

		private int totalIn;

		private int inputOff;

		private int inputEnd;

		private DeflaterPending pending;

		private DeflaterHuffman huffman;

		private Adler32 adler;
	}
}
