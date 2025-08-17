using System;
using System.Diagnostics;
using System.Net;

namespace DNA.Net.Lidgren
{
	[DebuggerDisplay("Type={MessageType} LengthBits={LengthBits}")]
	public sealed class NetIncomingMessage : NetBuffer
	{
		public NetIncomingMessageType MessageType
		{
			get
			{
				return this.m_incomingMessageType;
			}
		}

		public NetDeliveryMethod DeliveryMethod
		{
			get
			{
				return NetUtility.GetDeliveryMethod(this.m_receivedMessageType);
			}
		}

		public int SequenceChannel
		{
			get
			{
				return (int)(this.m_receivedMessageType - (NetMessageType)NetUtility.GetDeliveryMethod(this.m_receivedMessageType));
			}
		}

		public IPEndPoint SenderEndPoint
		{
			get
			{
				return this.m_senderEndPoint;
			}
		}

		public NetConnection SenderConnection
		{
			get
			{
				return this.m_senderConnection;
			}
		}

		public double ReceiveTime
		{
			get
			{
				return this.m_receiveTime;
			}
		}

		internal NetIncomingMessage()
		{
		}

		internal NetIncomingMessage(NetIncomingMessageType tp)
		{
			this.m_incomingMessageType = tp;
		}

		internal void Reset()
		{
			this.m_incomingMessageType = NetIncomingMessageType.Error;
			this.m_readPosition = 0;
			this.m_receivedMessageType = NetMessageType.LibraryError;
			this.m_senderConnection = null;
			this.m_bitLength = 0;
			this.m_isFragment = false;
		}

		public bool Decrypt(INetEncryption encryption)
		{
			return encryption.Decrypt(this);
		}

		public double ReadTime(bool highPrecision)
		{
			return base.ReadTime(this.m_senderConnection, highPrecision);
		}

		public override string ToString()
		{
			return string.Concat(new object[] { "[NetIncomingMessage #", this.m_sequenceNumber, " ", base.LengthBytes, " bytes]" });
		}

		internal NetIncomingMessageType m_incomingMessageType;

		internal IPEndPoint m_senderEndPoint;

		internal NetConnection m_senderConnection;

		internal int m_sequenceNumber;

		internal NetMessageType m_receivedMessageType;

		internal bool m_isFragment;

		internal double m_receiveTime;
	}
}
