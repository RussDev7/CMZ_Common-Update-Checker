using System;

namespace DNA.IO.Compression.Zip.Compression
{
	public class DeflaterHuffman
	{
		public static short BitReverse(int toReverse)
		{
			return (short)(((int)DeflaterHuffman.bit4Reverse[toReverse & 15] << 12) | ((int)DeflaterHuffman.bit4Reverse[(toReverse >> 4) & 15] << 8) | ((int)DeflaterHuffman.bit4Reverse[(toReverse >> 8) & 15] << 4) | (int)DeflaterHuffman.bit4Reverse[toReverse >> 12]);
		}

		static DeflaterHuffman()
		{
			int i = 0;
			while (i < 144)
			{
				DeflaterHuffman.staticLCodes[i] = DeflaterHuffman.BitReverse(48 + i << 8);
				DeflaterHuffman.staticLLength[i++] = 8;
			}
			while (i < 256)
			{
				DeflaterHuffman.staticLCodes[i] = DeflaterHuffman.BitReverse(256 + i << 7);
				DeflaterHuffman.staticLLength[i++] = 9;
			}
			while (i < 280)
			{
				DeflaterHuffman.staticLCodes[i] = DeflaterHuffman.BitReverse(-256 + i << 9);
				DeflaterHuffman.staticLLength[i++] = 7;
			}
			while (i < DeflaterHuffman.LITERAL_NUM)
			{
				DeflaterHuffman.staticLCodes[i] = DeflaterHuffman.BitReverse(-88 + i << 8);
				DeflaterHuffman.staticLLength[i++] = 8;
			}
			DeflaterHuffman.staticDCodes = new short[DeflaterHuffman.DIST_NUM];
			DeflaterHuffman.staticDLength = new byte[DeflaterHuffman.DIST_NUM];
			for (i = 0; i < DeflaterHuffman.DIST_NUM; i++)
			{
				DeflaterHuffman.staticDCodes[i] = DeflaterHuffman.BitReverse(i << 11);
				DeflaterHuffman.staticDLength[i] = 5;
			}
		}

		public DeflaterHuffman(DeflaterPending pending)
		{
			this.pending = pending;
			this.literalTree = new DeflaterHuffman.Tree(this, DeflaterHuffman.LITERAL_NUM, 257, 15);
			this.distTree = new DeflaterHuffman.Tree(this, DeflaterHuffman.DIST_NUM, 1, 15);
			this.blTree = new DeflaterHuffman.Tree(this, DeflaterHuffman.BITLEN_NUM, 4, 7);
			this.d_buf = new short[DeflaterHuffman.BUFSIZE];
			this.l_buf = new byte[DeflaterHuffman.BUFSIZE];
		}

		public void Reset()
		{
			this.last_lit = 0;
			this.extra_bits = 0;
			this.literalTree.Reset();
			this.distTree.Reset();
			this.blTree.Reset();
		}

		private int Lcode(int len)
		{
			if (len == 255)
			{
				return 285;
			}
			int code = 257;
			while (len >= 8)
			{
				code += 4;
				len >>= 1;
			}
			return code + len;
		}

		private int Dcode(int distance)
		{
			int code = 0;
			while (distance >= 4)
			{
				code += 2;
				distance >>= 1;
			}
			return code + distance;
		}

		public void SendAllTrees(int blTreeCodes)
		{
			this.blTree.BuildCodes();
			this.literalTree.BuildCodes();
			this.distTree.BuildCodes();
			this.pending.WriteBits(this.literalTree.numCodes - 257, 5);
			this.pending.WriteBits(this.distTree.numCodes - 1, 5);
			this.pending.WriteBits(blTreeCodes - 4, 4);
			for (int rank = 0; rank < blTreeCodes; rank++)
			{
				this.pending.WriteBits((int)this.blTree.length[DeflaterHuffman.BL_ORDER[rank]], 3);
			}
			this.literalTree.WriteTree(this.blTree);
			this.distTree.WriteTree(this.blTree);
		}

		public void CompressBlock()
		{
			for (int i = 0; i < this.last_lit; i++)
			{
				int litlen = (int)(this.l_buf[i] & byte.MaxValue);
				int dist = (int)this.d_buf[i];
				if (dist-- != 0)
				{
					int lc = this.Lcode(litlen);
					this.literalTree.WriteSymbol(lc);
					int bits = (lc - 261) / 4;
					if (bits > 0 && bits <= 5)
					{
						this.pending.WriteBits(litlen & ((1 << bits) - 1), bits);
					}
					int dc = this.Dcode(dist);
					this.distTree.WriteSymbol(dc);
					bits = dc / 2 - 1;
					if (bits > 0)
					{
						this.pending.WriteBits(dist & ((1 << bits) - 1), bits);
					}
				}
				else
				{
					this.literalTree.WriteSymbol(litlen);
				}
			}
			this.literalTree.WriteSymbol(DeflaterHuffman.EOF_SYMBOL);
		}

		public void FlushStoredBlock(byte[] stored, int storedOffset, int storedLength, bool lastBlock)
		{
			this.pending.WriteBits(lastBlock ? 1 : 0, 3);
			this.pending.AlignToByte();
			this.pending.WriteShort(storedLength);
			this.pending.WriteShort(~storedLength);
			this.pending.WriteBlock(stored, storedOffset, storedLength);
			this.Reset();
		}

		public void FlushBlock(byte[] stored, int storedOffset, int storedLength, bool lastBlock)
		{
			short[] freqs = this.literalTree.freqs;
			int eof_SYMBOL = DeflaterHuffman.EOF_SYMBOL;
			freqs[eof_SYMBOL] += 1;
			this.literalTree.BuildTree();
			this.distTree.BuildTree();
			this.literalTree.CalcBLFreq(this.blTree);
			this.distTree.CalcBLFreq(this.blTree);
			this.blTree.BuildTree();
			int blTreeCodes = 4;
			for (int i = 18; i > blTreeCodes; i--)
			{
				if (this.blTree.length[DeflaterHuffman.BL_ORDER[i]] > 0)
				{
					blTreeCodes = i + 1;
				}
			}
			int opt_len = 14 + blTreeCodes * 3 + this.blTree.GetEncodedLength() + this.literalTree.GetEncodedLength() + this.distTree.GetEncodedLength() + this.extra_bits;
			int static_len = this.extra_bits;
			for (int j = 0; j < DeflaterHuffman.LITERAL_NUM; j++)
			{
				static_len += (int)(this.literalTree.freqs[j] * (short)DeflaterHuffman.staticLLength[j]);
			}
			for (int k = 0; k < DeflaterHuffman.DIST_NUM; k++)
			{
				static_len += (int)(this.distTree.freqs[k] * (short)DeflaterHuffman.staticDLength[k]);
			}
			if (opt_len >= static_len)
			{
				opt_len = static_len;
			}
			if (storedOffset >= 0 && storedLength + 4 < opt_len >> 3)
			{
				this.FlushStoredBlock(stored, storedOffset, storedLength, lastBlock);
				return;
			}
			if (opt_len == static_len)
			{
				this.pending.WriteBits(2 + (lastBlock ? 1 : 0), 3);
				this.literalTree.SetStaticCodes(DeflaterHuffman.staticLCodes, DeflaterHuffman.staticLLength);
				this.distTree.SetStaticCodes(DeflaterHuffman.staticDCodes, DeflaterHuffman.staticDLength);
				this.CompressBlock();
				this.Reset();
				return;
			}
			this.pending.WriteBits(4 + (lastBlock ? 1 : 0), 3);
			this.SendAllTrees(blTreeCodes);
			this.CompressBlock();
			this.Reset();
		}

		public bool IsFull()
		{
			return this.last_lit >= DeflaterHuffman.BUFSIZE;
		}

		public bool TallyLit(int lit)
		{
			this.d_buf[this.last_lit] = 0;
			this.l_buf[this.last_lit++] = (byte)lit;
			short[] freqs = this.literalTree.freqs;
			freqs[lit] += 1;
			return this.IsFull();
		}

		public bool TallyDist(int dist, int len)
		{
			this.d_buf[this.last_lit] = (short)dist;
			this.l_buf[this.last_lit++] = (byte)(len - 3);
			int lc = this.Lcode(len - 3);
			short[] freqs = this.literalTree.freqs;
			int num = lc;
			freqs[num] += 1;
			if (lc >= 265 && lc < 285)
			{
				this.extra_bits += (lc - 261) / 4;
			}
			int dc = this.Dcode(dist - 1);
			short[] freqs2 = this.distTree.freqs;
			int num2 = dc;
			freqs2[num2] += 1;
			if (dc >= 4)
			{
				this.extra_bits += dc / 2 - 1;
			}
			return this.IsFull();
		}

		private static int BUFSIZE = 16384;

		private static int LITERAL_NUM = 286;

		private static int DIST_NUM = 30;

		private static int BITLEN_NUM = 19;

		private static int REP_3_6 = 16;

		private static int REP_3_10 = 17;

		private static int REP_11_138 = 18;

		private static int EOF_SYMBOL = 256;

		private static int[] BL_ORDER = new int[]
		{
			16, 17, 18, 0, 8, 7, 9, 6, 10, 5,
			11, 4, 12, 3, 13, 2, 14, 1, 15
		};

		private static byte[] bit4Reverse = new byte[]
		{
			0, 8, 4, 12, 2, 10, 6, 14, 1, 9,
			5, 13, 3, 11, 7, 15
		};

		public DeflaterPending pending;

		private DeflaterHuffman.Tree literalTree;

		private DeflaterHuffman.Tree distTree;

		private DeflaterHuffman.Tree blTree;

		private short[] d_buf;

		private byte[] l_buf;

		private int last_lit;

		private int extra_bits;

		private static short[] staticLCodes = new short[DeflaterHuffman.LITERAL_NUM];

		private static byte[] staticLLength = new byte[DeflaterHuffman.LITERAL_NUM];

		private static short[] staticDCodes;

		private static byte[] staticDLength;

		public class Tree
		{
			public Tree(DeflaterHuffman dh, int elems, int minCodes, int maxLength)
			{
				this.dh = dh;
				this.minNumCodes = minCodes;
				this.maxLength = maxLength;
				this.freqs = new short[elems];
				this.bl_counts = new int[maxLength];
			}

			public void Reset()
			{
				for (int i = 0; i < this.freqs.Length; i++)
				{
					this.freqs[i] = 0;
				}
				this.codes = null;
				this.length = null;
			}

			public void WriteSymbol(int code)
			{
				this.dh.pending.WriteBits((int)this.codes[code] & 65535, (int)this.length[code]);
			}

			public void CheckEmpty()
			{
				bool empty = true;
				for (int i = 0; i < this.freqs.Length; i++)
				{
					if (this.freqs[i] != 0)
					{
						empty = false;
					}
				}
				if (!empty)
				{
					throw new CompressionException("!Empty");
				}
			}

			public void SetStaticCodes(short[] stCodes, byte[] stLength)
			{
				this.codes = stCodes;
				this.length = stLength;
			}

			public void BuildCodes()
			{
				int[] nextCode = new int[this.maxLength];
				int code = 0;
				this.codes = new short[this.freqs.Length];
				for (int bits = 0; bits < this.maxLength; bits++)
				{
					nextCode[bits] = code;
					code += this.bl_counts[bits] << 15 - bits;
				}
				for (int i = 0; i < this.numCodes; i++)
				{
					int bits2 = (int)this.length[i];
					if (bits2 > 0)
					{
						this.codes[i] = DeflaterHuffman.BitReverse(nextCode[bits2 - 1]);
						nextCode[bits2 - 1] += 1 << 16 - bits2;
					}
				}
			}

			private void BuildLength(int[] childs)
			{
				this.length = new byte[this.freqs.Length];
				int numNodes = childs.Length / 2;
				int numLeafs = (numNodes + 1) / 2;
				int overflow = 0;
				for (int i = 0; i < this.maxLength; i++)
				{
					this.bl_counts[i] = 0;
				}
				int[] lengths = new int[numNodes];
				lengths[numNodes - 1] = 0;
				for (int j = numNodes - 1; j >= 0; j--)
				{
					if (childs[2 * j + 1] != -1)
					{
						int bitLength = lengths[j] + 1;
						if (bitLength > this.maxLength)
						{
							bitLength = this.maxLength;
							overflow++;
						}
						lengths[childs[2 * j]] = (lengths[childs[2 * j + 1]] = bitLength);
					}
					else
					{
						int bitLength2 = lengths[j];
						this.bl_counts[bitLength2 - 1]++;
						this.length[childs[2 * j]] = (byte)lengths[j];
					}
				}
				if (overflow == 0)
				{
					return;
				}
				int incrBitLen = this.maxLength - 1;
				for (;;)
				{
					if (this.bl_counts[--incrBitLen] != 0)
					{
						do
						{
							this.bl_counts[incrBitLen]--;
							this.bl_counts[++incrBitLen]++;
							overflow -= 1 << this.maxLength - 1 - incrBitLen;
						}
						while (overflow > 0 && incrBitLen < this.maxLength - 1);
						if (overflow <= 0)
						{
							break;
						}
					}
				}
				this.bl_counts[this.maxLength - 1] += overflow;
				this.bl_counts[this.maxLength - 2] -= overflow;
				int nodePtr = 2 * numLeafs;
				for (int bits = this.maxLength; bits != 0; bits--)
				{
					int k = this.bl_counts[bits - 1];
					while (k > 0)
					{
						int childPtr = 2 * childs[nodePtr++];
						if (childs[childPtr + 1] == -1)
						{
							this.length[childs[childPtr]] = (byte)bits;
							k--;
						}
					}
				}
			}

			public void BuildTree()
			{
				int numSymbols = this.freqs.Length;
				int[] heap = new int[numSymbols];
				int heapLen = 0;
				int maxCode = 0;
				for (int i = 0; i < numSymbols; i++)
				{
					int freq = (int)this.freqs[i];
					if (freq != 0)
					{
						int pos = heapLen++;
						int ppos;
						while (pos > 0 && (int)this.freqs[heap[ppos = (pos - 1) / 2]] > freq)
						{
							heap[pos] = heap[ppos];
							pos = ppos;
						}
						heap[pos] = i;
						maxCode = i;
					}
				}
				while (heapLen < 2)
				{
					int node = ((maxCode < 2) ? (++maxCode) : 0);
					heap[heapLen++] = node;
				}
				this.numCodes = Math.Max(maxCode + 1, this.minNumCodes);
				int numLeafs = heapLen;
				int[] childs = new int[4 * heapLen - 2];
				int[] values = new int[2 * heapLen - 1];
				int numNodes = numLeafs;
				for (int j = 0; j < heapLen; j++)
				{
					int node2 = heap[j];
					childs[2 * j] = node2;
					childs[2 * j + 1] = -1;
					values[j] = (int)this.freqs[node2] << 8;
					heap[j] = j;
				}
				do
				{
					int first = heap[0];
					int last = heap[--heapLen];
					int ppos2 = 0;
					int path;
					for (path = 1; path < heapLen; path = path * 2 + 1)
					{
						if (path + 1 < heapLen && values[heap[path]] > values[heap[path + 1]])
						{
							path++;
						}
						heap[ppos2] = heap[path];
						ppos2 = path;
					}
					int lastVal = values[last];
					while ((path = ppos2) > 0 && values[heap[ppos2 = (path - 1) / 2]] > lastVal)
					{
						heap[path] = heap[ppos2];
					}
					heap[path] = last;
					int second = heap[0];
					last = numNodes++;
					childs[2 * last] = first;
					childs[2 * last + 1] = second;
					int mindepth = Math.Min(values[first] & 255, values[second] & 255);
					lastVal = (values[last] = values[first] + values[second] - mindepth + 1);
					ppos2 = 0;
					for (path = 1; path < heapLen; path = ppos2 * 2 + 1)
					{
						if (path + 1 < heapLen && values[heap[path]] > values[heap[path + 1]])
						{
							path++;
						}
						heap[ppos2] = heap[path];
						ppos2 = path;
					}
					while ((path = ppos2) > 0 && values[heap[ppos2 = (path - 1) / 2]] > lastVal)
					{
						heap[path] = heap[ppos2];
					}
					heap[path] = last;
				}
				while (heapLen > 1);
				if (heap[0] != childs.Length / 2 - 1)
				{
					throw new CompressionException("Heap invariant violated");
				}
				this.BuildLength(childs);
			}

			public int GetEncodedLength()
			{
				int len = 0;
				for (int i = 0; i < this.freqs.Length; i++)
				{
					len += (int)(this.freqs[i] * (short)this.length[i]);
				}
				return len;
			}

			public void CalcBLFreq(DeflaterHuffman.Tree blTree)
			{
				int curlen = -1;
				int i = 0;
				while (i < this.numCodes)
				{
					int count = 1;
					int nextlen = (int)this.length[i];
					int max_count;
					int min_count;
					if (nextlen == 0)
					{
						max_count = 138;
						min_count = 3;
					}
					else
					{
						max_count = 6;
						min_count = 3;
						if (curlen != nextlen)
						{
							short[] array = blTree.freqs;
							int num = nextlen;
							array[num] += 1;
							count = 0;
						}
					}
					curlen = nextlen;
					i++;
					while (i < this.numCodes && curlen == (int)this.length[i])
					{
						i++;
						if (++count >= max_count)
						{
							break;
						}
					}
					if (count < min_count)
					{
						short[] array2 = blTree.freqs;
						int num2 = curlen;
						array2[num2] += (short)count;
					}
					else if (curlen != 0)
					{
						short[] array3 = blTree.freqs;
						int rep_3_ = DeflaterHuffman.REP_3_6;
						array3[rep_3_] += 1;
					}
					else if (count <= 10)
					{
						short[] array4 = blTree.freqs;
						int rep_3_2 = DeflaterHuffman.REP_3_10;
						array4[rep_3_2] += 1;
					}
					else
					{
						short[] array5 = blTree.freqs;
						int rep_11_ = DeflaterHuffman.REP_11_138;
						array5[rep_11_] += 1;
					}
				}
			}

			public void WriteTree(DeflaterHuffman.Tree blTree)
			{
				int curlen = -1;
				int i = 0;
				while (i < this.numCodes)
				{
					int count = 1;
					int nextlen = (int)this.length[i];
					int max_count;
					int min_count;
					if (nextlen == 0)
					{
						max_count = 138;
						min_count = 3;
					}
					else
					{
						max_count = 6;
						min_count = 3;
						if (curlen != nextlen)
						{
							blTree.WriteSymbol(nextlen);
							count = 0;
						}
					}
					curlen = nextlen;
					i++;
					while (i < this.numCodes && curlen == (int)this.length[i])
					{
						i++;
						if (++count >= max_count)
						{
							break;
						}
					}
					if (count < min_count)
					{
						while (count-- > 0)
						{
							blTree.WriteSymbol(curlen);
						}
					}
					else if (curlen != 0)
					{
						blTree.WriteSymbol(DeflaterHuffman.REP_3_6);
						this.dh.pending.WriteBits(count - 3, 2);
					}
					else if (count <= 10)
					{
						blTree.WriteSymbol(DeflaterHuffman.REP_3_10);
						this.dh.pending.WriteBits(count - 3, 3);
					}
					else
					{
						blTree.WriteSymbol(DeflaterHuffman.REP_11_138);
						this.dh.pending.WriteBits(count - 11, 7);
					}
				}
			}

			public short[] freqs;

			public byte[] length;

			public int minNumCodes;

			public int numCodes;

			private short[] codes;

			private int[] bl_counts;

			private int maxLength;

			private DeflaterHuffman dh;
		}
	}
}
