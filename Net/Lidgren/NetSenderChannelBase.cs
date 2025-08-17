﻿using System;

namespace DNA.Net.Lidgren
{
	internal abstract class NetSenderChannelBase
	{
		internal abstract int WindowSize { get; }

		internal abstract int GetAllowedSends();

		internal abstract NetSendResult Enqueue(NetOutgoingMessage message);

		internal abstract void SendQueuedMessages(float now);

		internal abstract void Reset();

		internal abstract void ReceiveAcknowledge(float now, int sequenceNumber);

		internal NetQueue<NetOutgoingMessage> m_queuedSends;
	}
}
