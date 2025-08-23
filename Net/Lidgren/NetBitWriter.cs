using System;

namespace DNA.Net.Lidgren
{
	public static class NetBitWriter
	{
		public static byte ReadByte(byte[] fromBuffer, int numberOfBits, int readBitOffset)
		{
			int bytePtr = readBitOffset >> 3;
			int startReadAtIndex = readBitOffset - bytePtr * 8;
			if (startReadAtIndex == 0 && numberOfBits == 8)
			{
				return fromBuffer[bytePtr];
			}
			byte returnValue = (byte)(fromBuffer[bytePtr] >> startReadAtIndex);
			int numberOfBitsInSecondByte = numberOfBits - (8 - startReadAtIndex);
			if (numberOfBitsInSecondByte < 1)
			{
				return (byte)((int)returnValue & (255 >> 8 - numberOfBits));
			}
			byte second = fromBuffer[bytePtr + 1];
			second &= (byte)(255 >> 8 - numberOfBitsInSecondByte);
			return returnValue | (byte)(second << numberOfBits - numberOfBitsInSecondByte);
		}

		public static void ReadBytes(byte[] fromBuffer, int numberOfBytes, int readBitOffset, byte[] destination, int destinationByteOffset)
		{
			int readPtr = readBitOffset >> 3;
			int startReadAtIndex = readBitOffset - readPtr * 8;
			if (startReadAtIndex == 0)
			{
				Buffer.BlockCopy(fromBuffer, readPtr, destination, destinationByteOffset, numberOfBytes);
				return;
			}
			int secondPartLen = 8 - startReadAtIndex;
			int secondMask = 255 >> secondPartLen;
			for (int i = 0; i < numberOfBytes; i++)
			{
				int b = fromBuffer[readPtr] >> startReadAtIndex;
				readPtr++;
				int second = (int)fromBuffer[readPtr] & secondMask;
				destination[destinationByteOffset++] = (byte)(b | (second << secondPartLen));
			}
		}

		public static void WriteByte(byte source, int numberOfBits, byte[] destination, int destBitOffset)
		{
			if (numberOfBits == 0)
			{
				return;
			}
			source = (byte)((int)source & (255 >> 8 - numberOfBits));
			int p = destBitOffset >> 3;
			int bitsUsed = destBitOffset & 7;
			int bitsFree = 8 - bitsUsed;
			int bitsLeft = bitsFree - numberOfBits;
			if (bitsLeft >= 0)
			{
				int mask = (255 >> bitsFree) | (255 << 8 - bitsLeft);
				destination[p] = (byte)(((int)destination[p] & mask) | ((int)source << bitsUsed));
				return;
			}
			destination[p] = (byte)(((int)destination[p] & (255 >> bitsFree)) | ((int)source << bitsUsed));
			p++;
			destination[p] = (byte)(((int)destination[p] & (255 << numberOfBits - bitsFree)) | (source >> bitsFree));
		}

		public static void WriteBytes(byte[] source, int sourceByteOffset, int numberOfBytes, byte[] destination, int destBitOffset)
		{
			int dstBytePtr = destBitOffset >> 3;
			int firstPartLen = destBitOffset % 8;
			if (firstPartLen == 0)
			{
				Buffer.BlockCopy(source, sourceByteOffset, destination, dstBytePtr, numberOfBytes);
				return;
			}
			int lastPartLen = 8 - firstPartLen;
			for (int i = 0; i < numberOfBytes; i++)
			{
				byte src = source[sourceByteOffset + i];
				int num = dstBytePtr;
				destination[num] &= (byte)(255 >> lastPartLen);
				int num2 = dstBytePtr;
				destination[num2] |= (byte)(src << firstPartLen);
				dstBytePtr++;
				int num3 = dstBytePtr;
				destination[num3] &= (byte)(255 << firstPartLen);
				int num4 = dstBytePtr;
				destination[num4] |= (byte)(src >> lastPartLen);
			}
		}

		[CLSCompliant(false)]
		public static ushort ReadUInt16(byte[] fromBuffer, int numberOfBits, int readBitOffset)
		{
			if (numberOfBits <= 8)
			{
				return (ushort)NetBitWriter.ReadByte(fromBuffer, numberOfBits, readBitOffset);
			}
			ushort returnValue = (ushort)NetBitWriter.ReadByte(fromBuffer, 8, readBitOffset);
			numberOfBits -= 8;
			readBitOffset += 8;
			if (numberOfBits <= 8)
			{
				returnValue |= (ushort)(NetBitWriter.ReadByte(fromBuffer, numberOfBits, readBitOffset) << 8);
			}
			return returnValue;
		}

		[CLSCompliant(false)]
		public static uint ReadUInt32(byte[] fromBuffer, int numberOfBits, int readBitOffset)
		{
			if (numberOfBits <= 8)
			{
				return (uint)NetBitWriter.ReadByte(fromBuffer, numberOfBits, readBitOffset);
			}
			uint returnValue = (uint)NetBitWriter.ReadByte(fromBuffer, 8, readBitOffset);
			numberOfBits -= 8;
			readBitOffset += 8;
			if (numberOfBits <= 8)
			{
				return returnValue | (uint)((uint)NetBitWriter.ReadByte(fromBuffer, numberOfBits, readBitOffset) << 8);
			}
			returnValue |= (uint)((uint)NetBitWriter.ReadByte(fromBuffer, 8, readBitOffset) << 8);
			numberOfBits -= 8;
			readBitOffset += 8;
			if (numberOfBits <= 8)
			{
				uint r = (uint)NetBitWriter.ReadByte(fromBuffer, numberOfBits, readBitOffset);
				r <<= 16;
				return returnValue | r;
			}
			returnValue |= (uint)((uint)NetBitWriter.ReadByte(fromBuffer, 8, readBitOffset) << 16);
			numberOfBits -= 8;
			readBitOffset += 8;
			return returnValue | (uint)((uint)NetBitWriter.ReadByte(fromBuffer, numberOfBits, readBitOffset) << 24);
		}

		[CLSCompliant(false)]
		public static void WriteUInt16(ushort source, int numberOfBits, byte[] destination, int destinationBitOffset)
		{
			if (numberOfBits == 0)
			{
				return;
			}
			if (numberOfBits <= 8)
			{
				NetBitWriter.WriteByte((byte)source, numberOfBits, destination, destinationBitOffset);
				return;
			}
			NetBitWriter.WriteByte((byte)source, 8, destination, destinationBitOffset);
			numberOfBits -= 8;
			if (numberOfBits > 0)
			{
				NetBitWriter.WriteByte((byte)(source >> 8), numberOfBits, destination, destinationBitOffset + 8);
			}
		}

		[CLSCompliant(false)]
		public static int WriteUInt32(uint source, int numberOfBits, byte[] destination, int destinationBitOffset)
		{
			int returnValue = destinationBitOffset + numberOfBits;
			if (numberOfBits <= 8)
			{
				NetBitWriter.WriteByte((byte)source, numberOfBits, destination, destinationBitOffset);
				return returnValue;
			}
			NetBitWriter.WriteByte((byte)source, 8, destination, destinationBitOffset);
			destinationBitOffset += 8;
			numberOfBits -= 8;
			if (numberOfBits <= 8)
			{
				NetBitWriter.WriteByte((byte)(source >> 8), numberOfBits, destination, destinationBitOffset);
				return returnValue;
			}
			NetBitWriter.WriteByte((byte)(source >> 8), 8, destination, destinationBitOffset);
			destinationBitOffset += 8;
			numberOfBits -= 8;
			if (numberOfBits <= 8)
			{
				NetBitWriter.WriteByte((byte)(source >> 16), numberOfBits, destination, destinationBitOffset);
				return returnValue;
			}
			NetBitWriter.WriteByte((byte)(source >> 16), 8, destination, destinationBitOffset);
			destinationBitOffset += 8;
			numberOfBits -= 8;
			NetBitWriter.WriteByte((byte)(source >> 24), numberOfBits, destination, destinationBitOffset);
			return returnValue;
		}

		[CLSCompliant(false)]
		public static int WriteUInt64(ulong source, int numberOfBits, byte[] destination, int destinationBitOffset)
		{
			int returnValue = destinationBitOffset + numberOfBits;
			if (numberOfBits <= 8)
			{
				NetBitWriter.WriteByte((byte)source, numberOfBits, destination, destinationBitOffset);
				return returnValue;
			}
			NetBitWriter.WriteByte((byte)source, 8, destination, destinationBitOffset);
			destinationBitOffset += 8;
			numberOfBits -= 8;
			if (numberOfBits <= 8)
			{
				NetBitWriter.WriteByte((byte)(source >> 8), numberOfBits, destination, destinationBitOffset);
				return returnValue;
			}
			NetBitWriter.WriteByte((byte)(source >> 8), 8, destination, destinationBitOffset);
			destinationBitOffset += 8;
			numberOfBits -= 8;
			if (numberOfBits <= 8)
			{
				NetBitWriter.WriteByte((byte)(source >> 16), numberOfBits, destination, destinationBitOffset);
				return returnValue;
			}
			NetBitWriter.WriteByte((byte)(source >> 16), 8, destination, destinationBitOffset);
			destinationBitOffset += 8;
			numberOfBits -= 8;
			if (numberOfBits <= 8)
			{
				NetBitWriter.WriteByte((byte)(source >> 24), numberOfBits, destination, destinationBitOffset);
				return returnValue;
			}
			NetBitWriter.WriteByte((byte)(source >> 24), 8, destination, destinationBitOffset);
			destinationBitOffset += 8;
			numberOfBits -= 8;
			if (numberOfBits <= 8)
			{
				NetBitWriter.WriteByte((byte)(source >> 32), numberOfBits, destination, destinationBitOffset);
				return returnValue;
			}
			NetBitWriter.WriteByte((byte)(source >> 32), 8, destination, destinationBitOffset);
			destinationBitOffset += 8;
			numberOfBits -= 8;
			if (numberOfBits <= 8)
			{
				NetBitWriter.WriteByte((byte)(source >> 40), numberOfBits, destination, destinationBitOffset);
				return returnValue;
			}
			NetBitWriter.WriteByte((byte)(source >> 40), 8, destination, destinationBitOffset);
			destinationBitOffset += 8;
			numberOfBits -= 8;
			if (numberOfBits <= 8)
			{
				NetBitWriter.WriteByte((byte)(source >> 48), numberOfBits, destination, destinationBitOffset);
				return returnValue;
			}
			NetBitWriter.WriteByte((byte)(source >> 48), 8, destination, destinationBitOffset);
			destinationBitOffset += 8;
			numberOfBits -= 8;
			if (numberOfBits <= 8)
			{
				NetBitWriter.WriteByte((byte)(source >> 56), numberOfBits, destination, destinationBitOffset);
				return returnValue;
			}
			NetBitWriter.WriteByte((byte)(source >> 56), 8, destination, destinationBitOffset);
			destinationBitOffset += 8;
			numberOfBits -= 8;
			return returnValue;
		}

		[CLSCompliant(false)]
		public static int WriteVariableUInt32(byte[] intoBuffer, int offset, uint value)
		{
			int retval = 0;
			uint num = value;
			while (num >= 128U)
			{
				intoBuffer[offset + retval] = (byte)(num | 128U);
				num >>= 7;
				retval++;
			}
			intoBuffer[offset + retval] = (byte)num;
			return retval + 1;
		}

		[CLSCompliant(false)]
		public static uint ReadVariableUInt32(byte[] buffer, ref int offset)
		{
			int num = 0;
			int num2 = 0;
			while (num2 != 35)
			{
				byte num3 = buffer[offset++];
				num |= (int)(num3 & 127) << num2;
				num2 += 7;
				if ((num3 & 128) == 0)
				{
					return (uint)num;
				}
			}
			throw new FormatException("Bad 7-bit encoded integer");
		}
	}
}
