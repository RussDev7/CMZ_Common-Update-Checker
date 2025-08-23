using System;
using System.IO;

namespace DNA.Drawing.Imaging.Photoshop
{
	public class RleWriter
	{
		public RleWriter(Stream stream)
		{
			this.rleLock = new object();
			this.stream = stream;
		}

		public unsafe int Write(byte[] data, int offset, int count)
		{
			if (!Util.CheckBufferBounds(data, offset, count))
			{
				throw new ArgumentOutOfRangeException();
			}
			if (count == 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			int num;
			lock (this.rleLock)
			{
				long startPosition = this.stream.Position;
				this.data = data;
				this.offset = offset;
				fixed (byte* ptrData = &data[0])
				{
					byte* ptr = ptrData + offset;
					byte* ptrEnd = ptr + count;
					this.EncodeToStream(ptr, ptrEnd);
				}
				num = (int)(this.stream.Position - startPosition);
			}
			return num;
		}

		private void ClearPacket()
		{
			this.rlePacket = false;
			this.packetLength = 0;
		}

		private void WriteRlePacket()
		{
			byte header = (byte)(1 - this.packetLength);
			this.stream.WriteByte(header);
			this.stream.WriteByte(this.lastValue);
		}

		private void WriteRawPacket()
		{
			byte header = (byte)(this.packetLength - 1);
			this.stream.WriteByte(header);
			this.stream.Write(this.data, this.idxDataRawPacket, this.packetLength);
		}

		private void WritePacket()
		{
			if (this.rlePacket)
			{
				this.WriteRlePacket();
				return;
			}
			this.WriteRawPacket();
		}

		private unsafe int EncodeToStream(byte* ptr, byte* ptrEnd)
		{
			this.idxDataRawPacket = this.offset;
			this.rlePacket = false;
			this.lastValue = *ptr;
			this.packetLength = 1;
			ptr++;
			int totalLength = 1;
			while (ptr < ptrEnd)
			{
				byte color = *ptr;
				if (this.packetLength == 1)
				{
					this.rlePacket = color == this.lastValue;
					this.lastValue = color;
					this.packetLength = 2;
				}
				else if (this.packetLength == this.maxPacketLength)
				{
					this.WritePacket();
					this.rlePacket = false;
					this.lastValue = color;
					this.idxDataRawPacket = this.offset + totalLength;
					this.packetLength = 1;
				}
				else if (this.rlePacket)
				{
					if (color == this.lastValue)
					{
						this.packetLength++;
					}
					else
					{
						this.WriteRlePacket();
						this.rlePacket = false;
						this.lastValue = color;
						this.idxDataRawPacket = this.offset + totalLength;
						this.packetLength = 1;
					}
				}
				else if (color == this.lastValue)
				{
					this.packetLength--;
					this.WriteRawPacket();
					this.rlePacket = true;
					this.packetLength = 2;
				}
				else
				{
					this.lastValue = color;
					this.packetLength++;
				}
				ptr++;
				totalLength++;
			}
			this.WritePacket();
			this.ClearPacket();
			return totalLength;
		}

		private int maxPacketLength = 128;

		private object rleLock;

		private Stream stream;

		private byte[] data;

		private int offset;

		private bool rlePacket;

		private int packetLength;

		private int idxDataRawPacket;

		private byte lastValue;
	}
}
