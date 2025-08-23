using System;

namespace DNA.Net.Lidgren
{
	internal sealed class NetReliableSequencedReceiver : NetReceiverChannelBase
	{
		public NetReliableSequencedReceiver(NetConnection connection, int windowSize)
			: base(connection)
		{
			this.m_windowSize = windowSize;
		}

		private void AdvanceWindow()
		{
			this.m_windowStart = (this.m_windowStart + 1) % 1024;
		}

		internal override void ReceiveMessage(NetIncomingMessage message)
		{
			int nr = message.m_sequenceNumber;
			int relate = NetUtility.RelativeSequenceNumber(nr, this.m_windowStart);
			this.m_connection.QueueAck(message.m_receivedMessageType, nr);
			if (relate == 0)
			{
				this.AdvanceWindow();
				this.m_peer.ReleaseMessage(message);
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
			this.m_windowStart = (this.m_windowStart + relate) % 1024;
			this.m_peer.ReleaseMessage(message);
		}

		private int m_windowStart;

		private int m_windowSize;
	}
}
