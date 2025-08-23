using System;
using System.Diagnostics;
using System.Drawing;

namespace DNA.Drawing.Imaging.Photoshop
{
	public static class Util
	{
		public unsafe static void Fill(byte* ptr, byte value, int size)
		{
			byte* pEnd = ptr + size;
			while (ptr < pEnd)
			{
				*ptr = byte.MaxValue;
				ptr++;
			}
		}

		public unsafe static void SwapBytes2(byte* ptr)
		{
			byte byte0 = *ptr;
			*ptr = ptr[1];
			ptr[1] = byte0;
		}

		public unsafe static void SwapBytes4(byte* ptr)
		{
			byte byte0 = *ptr;
			byte @byte = ptr[1];
			*ptr = ptr[3];
			ptr[1] = ptr[2];
			ptr[2] = @byte;
			ptr[3] = byte0;
		}

		public unsafe static void SwapBytes(byte* ptr, int nLength)
		{
			for (long i = 0L; i < (long)(nLength / 2); i += 1L)
			{
				byte t = ptr[i];
				ptr[i] = *(ptr + nLength - i - 1);
				*(ptr + nLength - i - 1) = t;
			}
		}

		public unsafe static void SwapByteArray2(byte[] byteArray, int startIdx, int count)
		{
			int endIdx = startIdx + count * 2;
			if (byteArray.Length < endIdx)
			{
				throw new IndexOutOfRangeException();
			}
			fixed (byte* arrayPtr = &byteArray[0])
			{
				byte* ptr = arrayPtr + startIdx;
				byte* endPtr = arrayPtr + endIdx;
				while (ptr < endPtr)
				{
					Util.SwapBytes2(ptr);
					ptr += 2;
				}
			}
		}

		public unsafe static void SwapByteArray4(byte[] byteArray, int startIdx, int count)
		{
			int endIdx = startIdx + count * 4;
			if (byteArray.Length < endIdx)
			{
				throw new IndexOutOfRangeException();
			}
			fixed (byte* arrayPtr = &byteArray[0])
			{
				byte* ptr = arrayPtr + startIdx;
				byte* endPtr = arrayPtr + endIdx;
				while (ptr < endPtr)
				{
					Util.SwapBytes4(ptr);
					ptr += 4;
				}
			}
		}

		public static int BytesPerRow(Rectangle rect, int depth)
		{
			if (depth == 1)
			{
				return (rect.Width + 7) / 8;
			}
			return rect.Width * Util.BytesFromBitDepth(depth);
		}

		public static int RoundUp(int value, int stride)
		{
			return (value + stride - 1) / stride * stride;
		}

		public static int BytesFromBitDepth(int depth)
		{
			if (depth <= 8)
			{
				if (depth == 1 || depth == 8)
				{
					return 1;
				}
			}
			else
			{
				if (depth == 16)
				{
					return 2;
				}
				if (depth == 32)
				{
					return 4;
				}
			}
			throw new ArgumentException("Invalid bit depth.");
		}

		public static short ChannelCount(this PsdColorMode colorMode)
		{
			switch (colorMode)
			{
			case PsdColorMode.Bitmap:
			case PsdColorMode.Grayscale:
			case PsdColorMode.Indexed:
			case PsdColorMode.Duotone:
				return 1;
			case PsdColorMode.RGB:
			case PsdColorMode.Multichannel:
			case PsdColorMode.Lab:
				return 3;
			case PsdColorMode.CMYK:
				return 4;
			}
			throw new ArgumentException("Unknown color mode.");
		}

		public static bool CheckBufferBounds(byte[] data, int offset, int count)
		{
			return offset >= 0 && count >= 0 && offset + count <= data.Length;
		}

		public static char[] SIGNATURE_8BIM = "8BIM".ToCharArray();

		[DebuggerDisplay("Top = {Top}, Bottom = {Bottom}, Left = {Left}, Right = {Right}")]
		public struct RectanglePosition
		{
			public int Top { get; set; }

			public int Bottom { get; set; }

			public int Left { get; set; }

			public int Right { get; set; }
		}
	}
}
