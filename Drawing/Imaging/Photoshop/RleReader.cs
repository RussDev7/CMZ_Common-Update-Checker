using System;
using System.IO;

namespace DNA.Drawing.Imaging.Photoshop
{
	public class RleReader
	{
		public RleReader(Stream stream)
		{
			this.stream = stream;
		}

		public unsafe int Read(byte[] buffer, int offset, int count)
		{
			if (!Util.CheckBufferBounds(buffer, offset, count))
			{
				throw new ArgumentOutOfRangeException();
			}
			fixed (byte* ptrBuffer = &buffer[0])
			{
				int bytesLeft = count;
				int bufferIdx = offset;
				while (bytesLeft > 0)
				{
					sbyte rawPacketLength = (sbyte)this.stream.ReadByte();
					if (rawPacketLength > 0)
					{
						int readLength = (int)(rawPacketLength + 1);
						if (bytesLeft < readLength)
						{
							throw new RleException("Raw packet overruns the decode window.");
						}
						this.stream.Read(buffer, bufferIdx, readLength);
						bufferIdx += readLength;
						bytesLeft -= readLength;
					}
					else if (rawPacketLength > -128)
					{
						int runLength = (int)(1 - rawPacketLength);
						byte byteValue = (byte)this.stream.ReadByte();
						if (runLength > bytesLeft)
						{
							throw new RleException("RLE packet overruns the decode window.");
						}
						byte* ptr = ptrBuffer + bufferIdx;
						byte* ptrEnd = ptr + runLength;
						while (ptr < ptrEnd)
						{
							*ptr = byteValue;
							ptr++;
						}
						bufferIdx += runLength;
						bytesLeft -= runLength;
					}
				}
				return count - bytesLeft;
			}
		}

		private Stream stream;
	}
}
