using System;
using System.Diagnostics;
using System.Text;

namespace DNA.Net.Lidgren
{
	public sealed class NetConnectionStatistics
	{
		internal NetConnectionStatistics(NetConnection conn)
		{
			this.m_connection = conn;
			this.Reset();
		}

		internal void Reset()
		{
			this.m_sentPackets = 0;
			this.m_receivedPackets = 0;
			this.m_sentBytes = 0;
			this.m_receivedBytes = 0;
		}

		public int SentPackets
		{
			get
			{
				return this.m_sentPackets;
			}
		}

		public int ReceivedPackets
		{
			get
			{
				return this.m_receivedPackets;
			}
		}

		public int SentBytes
		{
			get
			{
				return this.m_sentBytes;
			}
		}

		public int ReceivedBytes
		{
			get
			{
				return this.m_receivedBytes;
			}
		}

		public int ResentMessages
		{
			get
			{
				return this.m_resentMessagesDueToHole + this.m_resentMessagesDueToDelay;
			}
		}

		[Conditional("DEBUG")]
		internal void PacketSent(int numBytes, int numMessages)
		{
			this.m_sentPackets++;
			this.m_sentBytes += numBytes;
			this.m_sentMessages += numMessages;
		}

		[Conditional("DEBUG")]
		internal void PacketReceived(int numBytes, int numMessages)
		{
			this.m_receivedPackets++;
			this.m_receivedBytes += numBytes;
			this.m_receivedMessages += numMessages;
		}

		[Conditional("DEBUG")]
		internal void MessageResent(MessageResendReason reason)
		{
			if (reason == MessageResendReason.Delay)
			{
				this.m_resentMessagesDueToDelay++;
				return;
			}
			this.m_resentMessagesDueToHole++;
		}

		public override string ToString()
		{
			StringBuilder bdr = new StringBuilder();
			bdr.AppendLine(string.Concat(new object[] { "Sent ", this.m_sentBytes, " bytes in ", this.m_sentMessages, " messages in ", this.m_sentPackets, " packets" }));
			bdr.AppendLine(string.Concat(new object[] { "Received ", this.m_receivedBytes, " bytes in ", this.m_receivedMessages, " messages in ", this.m_receivedPackets, " packets" }));
			if (this.m_resentMessagesDueToDelay > 0)
			{
				bdr.AppendLine("Resent messages (delay): " + this.m_resentMessagesDueToDelay);
			}
			if (this.m_resentMessagesDueToDelay > 0)
			{
				bdr.AppendLine("Resent messages (holes): " + this.m_resentMessagesDueToHole);
			}
			int numUnsent = 0;
			int numStored = 0;
			foreach (NetSenderChannelBase sendChan in this.m_connection.m_sendChannels)
			{
				if (sendChan != null)
				{
					numUnsent += sendChan.m_queuedSends.Count;
					NetReliableSenderChannel relSendChan = sendChan as NetReliableSenderChannel;
					if (relSendChan != null)
					{
						for (int i = 0; i < relSendChan.m_storedMessages.Length; i++)
						{
							if (relSendChan.m_storedMessages[i].Message != null)
							{
								numStored++;
							}
						}
					}
				}
			}
			int numWithheld = 0;
			foreach (NetReceiverChannelBase recChan in this.m_connection.m_receiveChannels)
			{
				NetReliableOrderedReceiver relRecChan = recChan as NetReliableOrderedReceiver;
				if (relRecChan != null)
				{
					for (int j = 0; j < relRecChan.m_withheldMessages.Length; j++)
					{
						if (relRecChan.m_withheldMessages[j] != null)
						{
							numWithheld++;
						}
					}
				}
			}
			bdr.AppendLine("Unsent messages: " + numUnsent);
			bdr.AppendLine("Stored messages: " + numStored);
			bdr.AppendLine("Withheld messages: " + numWithheld);
			return bdr.ToString();
		}

		private readonly NetConnection m_connection;

		internal int m_sentPackets;

		internal int m_receivedPackets;

		internal int m_sentMessages;

		internal int m_receivedMessages;

		internal int m_sentBytes;

		internal int m_receivedBytes;

		internal int m_resentMessagesDueToDelay;

		internal int m_resentMessagesDueToHole;
	}
}
