using System;

namespace DNA.Net.Lidgren
{
	internal static class NetFragmentationHelper
	{
		internal static int WriteHeader(byte[] destination, int ptr, int group, int totalBits, int chunkByteSize, int chunkNumber)
		{
			uint num;
			for (num = (uint)group; num >= 128U; num >>= 7)
			{
				destination[ptr++] = (byte)(num | 128U);
			}
			destination[ptr++] = (byte)num;
			uint num2;
			for (num2 = (uint)totalBits; num2 >= 128U; num2 >>= 7)
			{
				destination[ptr++] = (byte)(num2 | 128U);
			}
			destination[ptr++] = (byte)num2;
			uint num3;
			for (num3 = (uint)chunkByteSize; num3 >= 128U; num3 >>= 7)
			{
				destination[ptr++] = (byte)(num3 | 128U);
			}
			destination[ptr++] = (byte)num3;
			uint num4;
			for (num4 = (uint)chunkNumber; num4 >= 128U; num4 >>= 7)
			{
				destination[ptr++] = (byte)(num4 | 128U);
			}
			destination[ptr++] = (byte)num4;
			return ptr;
		}

		internal static int ReadHeader(byte[] buffer, int ptr, out int group, out int totalBits, out int chunkByteSize, out int chunkNumber)
		{
			int num = 0;
			int num2 = 0;
			byte num3;
			do
			{
				num3 = buffer[ptr++];
				num |= (int)(num3 & 127) << num2;
				num2 += 7;
			}
			while ((num3 & 128) != 0);
			group = num;
			num = 0;
			num2 = 0;
			byte num4;
			do
			{
				num4 = buffer[ptr++];
				num |= (int)(num4 & 127) << num2;
				num2 += 7;
			}
			while ((num4 & 128) != 0);
			totalBits = num;
			num = 0;
			num2 = 0;
			byte num5;
			do
			{
				num5 = buffer[ptr++];
				num |= (int)(num5 & 127) << num2;
				num2 += 7;
			}
			while ((num5 & 128) != 0);
			chunkByteSize = num;
			num = 0;
			num2 = 0;
			byte num6;
			do
			{
				num6 = buffer[ptr++];
				num |= (int)(num6 & 127) << num2;
				num2 += 7;
			}
			while ((num6 & 128) != 0);
			chunkNumber = num;
			return ptr;
		}

		internal static int GetFragmentationHeaderSize(int groupId, int totalBytes, int chunkByteSize, int numChunks)
		{
			int len = 4;
			for (uint num = (uint)groupId; num >= 128U; num >>= 7)
			{
				len++;
			}
			for (uint num2 = (uint)(totalBytes * 8); num2 >= 128U; num2 >>= 7)
			{
				len++;
			}
			for (uint num3 = (uint)chunkByteSize; num3 >= 128U; num3 >>= 7)
			{
				len++;
			}
			for (uint num4 = (uint)numChunks; num4 >= 128U; num4 >>= 7)
			{
				len++;
			}
			return len;
		}

		internal static int GetBestChunkSize(int group, int totalBytes, int mtu)
		{
			int tryChunkSize = mtu - 5 - 4;
			int est = NetFragmentationHelper.GetFragmentationHeaderSize(group, totalBytes, tryChunkSize, totalBytes / tryChunkSize);
			tryChunkSize = mtu - 5 - est;
			int headerSize;
			do
			{
				tryChunkSize--;
				int numChunks = totalBytes / tryChunkSize;
				if (numChunks * tryChunkSize < totalBytes)
				{
					numChunks++;
				}
				headerSize = NetFragmentationHelper.GetFragmentationHeaderSize(group, totalBytes, tryChunkSize, numChunks);
			}
			while (tryChunkSize + headerSize + 5 + 1 >= mtu);
			return tryChunkSize;
		}
	}
}
