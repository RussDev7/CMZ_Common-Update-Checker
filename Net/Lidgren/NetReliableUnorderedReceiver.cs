using System;

namespace DNA.Net.Lidgren
{
	internal sealed class NetReliableUnorderedReceiver : NetReceiverChannelBase
	{
		public NetReliableUnorderedReceiver(NetConnection connection, int windowSize)
			: base(connection)
		{
			this.m_windowSize = windowSize;
			this.m_earlyReceived = new NetBitVector(windowSize);
		}

		private void AdvanceWindow()
		{
			this.m_earlyReceived.Set(this.m_windowStart % this.m_windowSize, false);
			this.m_windowStart = (this.m_windowStart + 1) % 1024;
		}

		internal override void ReceiveMessage(NetIncomingMessage message)
		{
			int relate = NetUtility.RelativeSequenceNumber(message.m_sequenceNumber, this.m_windowStart);
			this.m_connection.QueueAck(message.m_receivedMessageType, message.m_sequenceNumber);
			if (relate == 0)
			{
				this.AdvanceWindow();
				this.m_peer.ReleaseMessage(message);
				int nextSeqNr = (message.m_sequenceNumber + 1) % 1024;
				while (this.m_earlyReceived[nextSeqNr % this.m_windowSize])
				{
					this.AdvanceWindow();
					nextSeqNr++;
				}
				return;
			}
			if (relate < 0)
			{
				return;
			}
			if (relate > this.m_windowSize)
			{
				return;
			}
			this.m_earlyReceived.Set(message.m_sequenceNumber % this.m_windowSize, true);
			this.m_peer.ReleaseMessage(message);
		}

		private int m_windowStart;

		private int m_windowSize;

		private NetBitVector m_earlyReceived;
	}
}
