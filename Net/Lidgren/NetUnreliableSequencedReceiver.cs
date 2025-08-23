using System;

namespace DNA.Net.Lidgren
{
	internal sealed class NetUnreliableSequencedReceiver : NetReceiverChannelBase
	{
		public NetUnreliableSequencedReceiver(NetConnection connection)
			: base(connection)
		{
		}

		internal override void ReceiveMessage(NetIncomingMessage msg)
		{
			int nr = msg.m_sequenceNumber;
			this.m_connection.QueueAck(msg.m_receivedMessageType, nr);
			int relate = NetUtility.RelativeSequenceNumber(nr, this.m_lastReceivedSequenceNumber);
			if (relate < 0)
			{
				return;
			}
			this.m_lastReceivedSequenceNumber = nr;
			this.m_peer.ReleaseMessage(msg);
		}

		private int m_lastReceivedSequenceNumber;
	}
}
