using System;
using System.Diagnostics;

namespace DNA.Net.Lidgren
{
	[DebuggerDisplay("LengthBits={LengthBits}")]
	public sealed class NetOutgoingMessage : NetBuffer
	{
		internal NetOutgoingMessage()
		{
		}

		internal void Reset()
		{
			this.m_messageType = NetMessageType.LibraryError;
			this.m_bitLength = 0;
			this.m_isSent = false;
			this.m_recyclingCount = 0;
			this.m_fragmentGroup = 0;
		}

		internal int Encode(byte[] intoBuffer, int ptr, int sequenceNumber)
		{
			intoBuffer[ptr++] = (byte)this.m_messageType;
			byte low = (byte)((sequenceNumber << 1) | ((this.m_fragmentGroup == 0) ? 0 : 1));
			intoBuffer[ptr++] = low;
			intoBuffer[ptr++] = (byte)(sequenceNumber >> 7);
			if (this.m_fragmentGroup == 0)
			{
				intoBuffer[ptr++] = (byte)this.m_bitLength;
				intoBuffer[ptr++] = (byte)(this.m_bitLength >> 8);
				int byteLen = NetUtility.BytesToHoldBits(this.m_bitLength);
				if (byteLen > 0)
				{
					Buffer.BlockCopy(this.m_data, 0, intoBuffer, ptr, byteLen);
					ptr += byteLen;
				}
			}
			else
			{
				int wasPtr = ptr;
				intoBuffer[ptr++] = (byte)this.m_bitLength;
				intoBuffer[ptr++] = (byte)(this.m_bitLength >> 8);
				ptr = NetFragmentationHelper.WriteHeader(intoBuffer, ptr, this.m_fragmentGroup, this.m_fragmentGroupTotalBits, this.m_fragmentChunkByteSize, this.m_fragmentChunkNumber);
				int hdrLen = ptr - wasPtr - 2;
				int realBitLength = this.m_bitLength + hdrLen * 8;
				intoBuffer[wasPtr] = (byte)realBitLength;
				intoBuffer[wasPtr + 1] = (byte)(realBitLength >> 8);
				int byteLen2 = NetUtility.BytesToHoldBits(this.m_bitLength);
				if (byteLen2 > 0)
				{
					Buffer.BlockCopy(this.m_data, this.m_fragmentChunkNumber * this.m_fragmentChunkByteSize, intoBuffer, ptr, byteLen2);
					ptr += byteLen2;
				}
			}
			return ptr;
		}

		internal int GetEncodedSize()
		{
			int retval = 5;
			if (this.m_fragmentGroup != 0)
			{
				retval += NetFragmentationHelper.GetFragmentationHeaderSize(this.m_fragmentGroup, this.m_fragmentGroupTotalBits / 8, this.m_fragmentChunkByteSize, this.m_fragmentChunkNumber);
			}
			return retval + base.LengthBytes;
		}

		public bool Encrypt(INetEncryption encryption)
		{
			return encryption.Encrypt(this);
		}

		public override string ToString()
		{
			return string.Concat(new object[] { "[NetOutgoingMessage ", this.m_messageType, " ", base.LengthBytes, " bytes]" });
		}

		internal NetMessageType m_messageType;

		internal bool m_isSent;

		internal int m_recyclingCount;

		internal int m_fragmentGroup;

		internal int m_fragmentGroupTotalBits;

		internal int m_fragmentChunkByteSize;

		internal int m_fragmentChunkNumber;
	}
}
