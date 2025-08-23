using System;
using DNA.IO.Compression.Zip.Compression.Streams;

namespace DNA.IO.Compression.Zip.Compression
{
	public class InflaterHuffmanTree
	{
		static InflaterHuffmanTree()
		{
			try
			{
				byte[] codeLengths = new byte[288];
				int i = 0;
				while (i < 144)
				{
					codeLengths[i++] = 8;
				}
				while (i < 256)
				{
					codeLengths[i++] = 9;
				}
				while (i < 280)
				{
					codeLengths[i++] = 7;
				}
				while (i < 288)
				{
					codeLengths[i++] = 8;
				}
				InflaterHuffmanTree.defLitLenTree = new InflaterHuffmanTree(codeLengths);
				codeLengths = new byte[32];
				i = 0;
				while (i < 32)
				{
					codeLengths[i++] = 5;
				}
				InflaterHuffmanTree.defDistTree = new InflaterHuffmanTree(codeLengths);
			}
			catch (Exception)
			{
				throw new CompressionException("InflaterHuffmanTree: static tree length illegal");
			}
		}

		public InflaterHuffmanTree(byte[] codeLengths)
		{
			this.BuildTree(codeLengths);
		}

		private void BuildTree(byte[] codeLengths)
		{
			int[] blCount = new int[InflaterHuffmanTree.MAX_BITLEN + 1];
			int[] nextCode = new int[InflaterHuffmanTree.MAX_BITLEN + 1];
			foreach (int bits in codeLengths)
			{
				if (bits > 0)
				{
					blCount[bits]++;
				}
			}
			int code = 0;
			int treeSize = 512;
			for (int bits2 = 1; bits2 <= InflaterHuffmanTree.MAX_BITLEN; bits2++)
			{
				nextCode[bits2] = code;
				code += blCount[bits2] << 16 - bits2;
				if (bits2 >= 10)
				{
					int start = nextCode[bits2] & 130944;
					int end = code & 130944;
					treeSize += end - start >> 16 - bits2;
				}
			}
			this.tree = new short[treeSize];
			int treePtr = 512;
			for (int bits3 = InflaterHuffmanTree.MAX_BITLEN; bits3 >= 10; bits3--)
			{
				int end2 = code & 130944;
				code -= blCount[bits3] << 16 - bits3;
				int start2 = code & 130944;
				for (int j = start2; j < end2; j += 128)
				{
					this.tree[(int)DeflaterHuffman.BitReverse(j)] = (short)((-treePtr << 4) | bits3);
					treePtr += 1 << bits3 - 9;
				}
			}
			for (int k = 0; k < codeLengths.Length; k++)
			{
				int bits4 = (int)codeLengths[k];
				if (bits4 != 0)
				{
					code = nextCode[bits4];
					int revcode = (int)DeflaterHuffman.BitReverse(code);
					if (bits4 <= 9)
					{
						do
						{
							this.tree[revcode] = (short)((k << 4) | bits4);
							revcode += 1 << bits4;
						}
						while (revcode < 512);
					}
					else
					{
						int subTree = (int)this.tree[revcode & 511];
						int treeLen = 1 << (subTree & 15);
						subTree = -(subTree >> 4);
						do
						{
							this.tree[subTree | (revcode >> 9)] = (short)((k << 4) | bits4);
							revcode += 1 << bits4;
						}
						while (revcode < treeLen);
					}
					nextCode[bits4] = code + (1 << 16 - bits4);
				}
			}
		}

		public int GetSymbol(StreamManipulator input)
		{
			int lookahead;
			if ((lookahead = input.PeekBits(9)) >= 0)
			{
				int symbol;
				if ((symbol = (int)this.tree[lookahead]) >= 0)
				{
					input.DropBits(symbol & 15);
					return symbol >> 4;
				}
				int subtree = -(symbol >> 4);
				int bitlen = symbol & 15;
				if ((lookahead = input.PeekBits(bitlen)) >= 0)
				{
					symbol = (int)this.tree[subtree | (lookahead >> 9)];
					input.DropBits(symbol & 15);
					return symbol >> 4;
				}
				int bits = input.AvailableBits;
				lookahead = input.PeekBits(bits);
				symbol = (int)this.tree[subtree | (lookahead >> 9)];
				if ((symbol & 15) <= bits)
				{
					input.DropBits(symbol & 15);
					return symbol >> 4;
				}
				return -1;
			}
			else
			{
				int bits2 = input.AvailableBits;
				lookahead = input.PeekBits(bits2);
				int symbol = (int)this.tree[lookahead];
				if (symbol >= 0 && (symbol & 15) <= bits2)
				{
					input.DropBits(symbol & 15);
					return symbol >> 4;
				}
				return -1;
			}
		}

		private static int MAX_BITLEN = 15;

		private short[] tree;

		public static InflaterHuffmanTree defLitLenTree;

		public static InflaterHuffmanTree defDistTree;
	}
}
