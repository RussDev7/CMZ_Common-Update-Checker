using System;
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
			int hash = ((this.ins_h << 5) ^ (int)this.window[this.strstart + 2]) & 32767;
			short match = (this.prev[this.strstart & 32767] = this.head[hash]);
			this.head[hash] = (short)this.strstart;
			this.ins_h = hash;
			return (int)match & 65535;
		}

		private void SlideWindow()
		{
			Array.Copy(this.window, 32768, this.window, 0, 32768);
			this.matchStart -= 32768;
			this.strstart -= 32768;
			this.blockStart -= 32768;
			for (int i = 0; i < 32768; i++)
			{
				int j = (int)this.head[i] & 65535;
				this.head[i] = (short)((j >= 32768) ? (j - 32768) : 0);
			}
			for (int k = 0; k < 32768; k++)
			{
				int l = (int)this.prev[k] & 65535;
				this.prev[k] = (short)((l >= 32768) ? (l - 32768) : 0);
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
				int more = 65536 - this.lookahead - this.strstart;
				if (more > this.inputEnd - this.inputOff)
				{
					more = this.inputEnd - this.inputOff;
				}
				Array.Copy(this.inputBuf, this.inputOff, this.window, this.strstart + this.lookahead, more);
				this.adler.Update(this.inputBuf, this.inputOff, more);
				this.inputOff += more;
				this.totalIn += more;
				this.lookahead += more;
			}
			if (this.lookahead >= 3)
			{
				this.UpdateHash();
			}
		}

		private bool FindLongestMatch(int curMatch)
		{
			int chainLength = this.max_chain;
			int niceLength = this.niceLength;
			short[] prev = this.prev;
			int scan = this.strstart;
			int best_end = this.strstart + this.matchLen;
			int best_len = Math.Max(this.matchLen, 2);
			int limit = Math.Max(this.strstart - 32506, 0);
			int strend = this.strstart + 258 - 1;
			byte scan_end = this.window[best_end - 1];
			byte scan_end2 = this.window[best_end];
			if (best_len >= this.goodLength)
			{
				chainLength >>= 2;
			}
			if (niceLength > this.lookahead)
			{
				niceLength = this.lookahead;
			}
			do
			{
				if (this.window[curMatch + best_len] == scan_end2 && this.window[curMatch + best_len - 1] == scan_end && this.window[curMatch] == this.window[scan] && this.window[curMatch + 1] == this.window[scan + 1])
				{
					int match = curMatch + 2;
					scan += 2;
					while (this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] && this.window[++scan] == this.window[++match] && scan < strend)
					{
					}
					if (scan > best_end)
					{
						this.matchStart = curMatch;
						best_end = scan;
						best_len = scan - this.strstart;
						if (best_len >= niceLength)
						{
							break;
						}
						scan_end = this.window[best_end - 1];
						scan_end2 = this.window[best_end];
					}
					scan = this.strstart;
				}
			}
			while ((curMatch = (int)prev[curMatch & 32767] & 65535) > limit && --chainLength != 0);
			this.matchLen = Math.Min(best_len, this.lookahead);
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
			int storedLen = this.strstart - this.blockStart;
			if (storedLen >= DeflaterConstants.MAX_BLOCK_SIZE || (this.blockStart < 32768 && storedLen >= 32506) || flush)
			{
				bool lastBlock = finish;
				if (storedLen > DeflaterConstants.MAX_BLOCK_SIZE)
				{
					storedLen = DeflaterConstants.MAX_BLOCK_SIZE;
					lastBlock = false;
				}
				this.huffman.FlushStoredBlock(this.window, this.blockStart, storedLen, lastBlock);
				this.blockStart += storedLen;
				return !lastBlock;
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
				int hashHead;
				if (this.lookahead >= 3 && (hashHead = this.InsertString()) != 0 && this.strategy != DeflateStrategy.HuffmanOnly && this.strstart - hashHead <= 32506 && this.FindLongestMatch(hashHead))
				{
					if (this.huffman.TallyDist(this.strstart - this.matchStart, this.matchLen))
					{
						bool lastBlock = finish && this.lookahead == 0;
						this.huffman.FlushBlock(this.window, this.blockStart, this.strstart - this.blockStart, lastBlock);
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
						bool lastBlock2 = finish && this.lookahead == 0;
						this.huffman.FlushBlock(this.window, this.blockStart, this.strstart - this.blockStart, lastBlock2);
						this.blockStart = this.strstart;
						return !lastBlock2;
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
				int prevMatch = this.matchStart;
				int prevLen = this.matchLen;
				if (this.lookahead >= 3)
				{
					int hashHead = this.InsertString();
					if (this.strategy != DeflateStrategy.HuffmanOnly && hashHead != 0 && this.strstart - hashHead <= 32506 && this.FindLongestMatch(hashHead) && this.matchLen <= 5 && (this.strategy == DeflateStrategy.Filtered || (this.matchLen == 3 && this.strstart - this.matchStart > DeflaterEngine.TOO_FAR)))
					{
						this.matchLen = 2;
					}
				}
				if (prevLen >= 3 && this.matchLen <= prevLen)
				{
					this.huffman.TallyDist(this.strstart - 1 - prevMatch, prevLen);
					prevLen -= 2;
					do
					{
						this.strstart++;
						this.lookahead--;
						if (this.lookahead >= 3)
						{
							this.InsertString();
						}
					}
					while (--prevLen > 0);
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
					int len = this.strstart - this.blockStart;
					if (this.prevAvailable)
					{
						len--;
					}
					bool lastBlock = finish && this.lookahead == 0 && !this.prevAvailable;
					this.huffman.FlushBlock(this.window, this.blockStart, len, lastBlock);
					this.blockStart += len;
					return !lastBlock;
				}
			}
			return true;
		}

		public bool Deflate(bool flush, bool finish)
		{
			for (;;)
			{
				this.FillWindow();
				bool canFlush = flush && this.inputOff == this.inputEnd;
				bool progress;
				switch (this.comprFunc)
				{
				case 0:
					progress = this.DeflateStored(canFlush, finish);
					goto IL_0062;
				case 1:
					progress = this.DeflateFast(canFlush, finish);
					goto IL_0062;
				case 2:
					progress = this.DeflateSlow(canFlush, finish);
					goto IL_0062;
				}
				break;
				IL_0062:
				if (!this.pending.IsFlushed || !progress)
				{
					return progress;
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
			int end = off + len;
			if (0 > off || off > end || end > buf.Length)
			{
				throw new ArgumentOutOfRangeException();
			}
			this.inputBuf = buf;
			this.inputOff = off;
			this.inputEnd = end;
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
