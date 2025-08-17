﻿using System;

namespace DNA.Net.Lidgren
{
	internal sealed class NetReliableOrderedReceiver : NetReceiverChannelBase
	{
		public NetReliableOrderedReceiver(NetConnection connection, int windowSize)
			: base(connection)
		{
			this.m_windowSize = windowSize;
			this.m_withheldMessages = new NetIncomingMessage[windowSize];
			this.m_earlyReceived = new NetBitVector(windowSize);
		}

		private void AdvanceWindow()
		{
			this.m_earlyReceived.Set(this.m_windowStart % this.m_windowSize, false);
			this.m_windowStart = (this.m_windowStart + 1) % 1024;
		}

		internal override void ReceiveMessage(NetIncomingMessage message)
		{
			int num = NetUtility.RelativeSequenceNumber(message.m_sequenceNumber, this.m_windowStart);
			this.m_connection.QueueAck(message.m_receivedMessageType, message.m_sequenceNumber);
			if (num == 0)
			{
				this.AdvanceWindow();
				this.m_peer.ReleaseMessage(message);
				int num2 = (message.m_sequenceNumber + 1) % 1024;
				while (this.m_earlyReceived[num2 % this.m_windowSize])
				{
					message = this.m_withheldMessages[num2 % this.m_windowSize];
					this.m_withheldMessages[num2 % this.m_windowSize] = null;
					this.m_peer.ReleaseMessage(message);
					this.AdvanceWindow();
					num2++;
				}
				return;
			}
			if (num < 0)
			{
				return;
			}
			if (num > this.m_windowSize)
			{
				return;
			}
			this.m_earlyReceived.Set(message.m_sequenceNumber % this.m_windowSize, true);
			this.m_withheldMessages[message.m_sequenceNumber % this.m_windowSize] = message;
		}

		private int m_windowStart;

		private int m_windowSize;

		private NetBitVector m_earlyReceived;

		internal NetIncomingMessage[] m_withheldMessages;
	}
}
