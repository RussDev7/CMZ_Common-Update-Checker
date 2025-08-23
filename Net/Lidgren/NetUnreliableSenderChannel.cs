﻿using System;
using System.Threading;

namespace DNA.Net.Lidgren
{
	internal sealed class NetUnreliableSenderChannel : NetSenderChannelBase
	{
		internal override int WindowSize
		{
			get
			{
				return this.m_windowSize;
			}
		}

		internal NetUnreliableSenderChannel(NetConnection connection, int windowSize)
		{
			this.m_connection = connection;
			this.m_windowSize = windowSize;
			this.m_windowStart = 0;
			this.m_sendStart = 0;
			this.m_receivedAcks = new NetBitVector(1024);
			this.m_queuedSends = new NetQueue<NetOutgoingMessage>(8);
		}

		internal override int GetAllowedSends()
		{
			return this.m_windowSize - (this.m_sendStart + 1024 - this.m_windowStart) % this.m_windowSize;
		}

		internal override void Reset()
		{
			this.m_receivedAcks.Clear();
			this.m_queuedSends.Clear();
			this.m_windowStart = 0;
			this.m_sendStart = 0;
		}

		internal override NetSendResult Enqueue(NetOutgoingMessage message)
		{
			int queueLen = this.m_queuedSends.Count + 1;
			int left = this.m_windowSize - (this.m_sendStart + 1024 - this.m_windowStart) % 1024;
			if (queueLen > left)
			{
				return NetSendResult.Dropped;
			}
			this.m_queuedSends.Enqueue(message);
			return NetSendResult.Sent;
		}

		internal override void SendQueuedMessages(float now)
		{
			int num = this.GetAllowedSends();
			if (num < 1)
			{
				return;
			}
			while (this.m_queuedSends.Count > 0 && num > 0)
			{
				NetOutgoingMessage om;
				if (this.m_queuedSends.TryDequeue(out om))
				{
					this.ExecuteSend(now, om);
				}
				num--;
			}
		}

		private void ExecuteSend(float now, NetOutgoingMessage message)
		{
			int seqNr = this.m_sendStart;
			this.m_sendStart = (this.m_sendStart + 1) % 1024;
			this.m_connection.QueueSendMessage(message, seqNr);
			Interlocked.Decrement(ref message.m_recyclingCount);
			if (message.m_recyclingCount <= 0)
			{
				this.m_connection.m_peer.Recycle(message);
			}
		}

		internal override void ReceiveAcknowledge(float now, int seqNr)
		{
			int relate = NetUtility.RelativeSequenceNumber(seqNr, this.m_windowStart);
			if (relate < 0)
			{
				return;
			}
			if (relate == 0)
			{
				this.m_receivedAcks[this.m_windowStart] = false;
				this.m_windowStart = (this.m_windowStart + 1) % 1024;
				return;
			}
			this.m_receivedAcks[seqNr] = true;
			while (this.m_windowStart != seqNr)
			{
				this.m_receivedAcks[this.m_windowStart] = false;
				this.m_windowStart = (this.m_windowStart + 1) % 1024;
			}
		}

		private NetConnection m_connection;

		private int m_windowStart;

		private int m_windowSize;

		private int m_sendStart;

		private NetBitVector m_receivedAcks;
	}
}
