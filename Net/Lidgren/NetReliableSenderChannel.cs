using System;
using System.Threading;

namespace DNA.Net.Lidgren
{
	internal sealed class NetReliableSenderChannel : NetSenderChannelBase
	{
		internal override int WindowSize
		{
			get
			{
				return this.m_windowSize;
			}
		}

		internal NetReliableSenderChannel(NetConnection connection, int windowSize)
		{
			this.m_connection = connection;
			this.m_windowSize = windowSize;
			this.m_windowStart = 0;
			this.m_sendStart = 0;
			this.m_receivedAcks = new NetBitVector(1024);
			this.m_storedMessages = new NetStoredReliableMessage[this.m_windowSize];
			this.m_queuedSends = new NetQueue<NetOutgoingMessage>(8);
			this.m_resendDelay = this.m_connection.GetResendDelay();
		}

		internal override int GetAllowedSends()
		{
			return this.m_windowSize - (this.m_sendStart + 1024 - this.m_windowStart) % 1024;
		}

		internal override void Reset()
		{
			this.m_receivedAcks.Clear();
			for (int i = 0; i < this.m_storedMessages.Length; i++)
			{
				this.m_storedMessages[i].Reset();
			}
			this.m_queuedSends.Clear();
			this.m_windowStart = 0;
			this.m_sendStart = 0;
		}

		internal override NetSendResult Enqueue(NetOutgoingMessage message)
		{
			this.m_queuedSends.Enqueue(message);
			int queueLen = this.m_queuedSends.Count;
			int left = this.m_windowSize - (this.m_sendStart + 1024 - this.m_windowStart) % 1024;
			if (queueLen <= left)
			{
				return NetSendResult.Sent;
			}
			return NetSendResult.Queued;
		}

		internal override void SendQueuedMessages(float now)
		{
			for (int i = 0; i < this.m_storedMessages.Length; i++)
			{
				NetOutgoingMessage om = this.m_storedMessages[i].Message;
				if (om != null)
				{
					float t = this.m_storedMessages[i].LastSent;
					if (t > 0f && now - t > this.m_resendDelay)
					{
						int startSlot = this.m_windowStart % this.m_windowSize;
						int seqNr = this.m_windowStart;
						while (startSlot != i)
						{
							startSlot--;
							if (startSlot < 0)
							{
								startSlot = this.m_windowSize - 1;
							}
							seqNr--;
						}
						this.m_connection.QueueSendMessage(om, seqNr);
						this.m_storedMessages[i].LastSent = now;
						NetStoredReliableMessage[] storedMessages = this.m_storedMessages;
						int num2 = i;
						storedMessages[num2].NumSent = storedMessages[num2].NumSent + 1;
					}
				}
			}
			int num = this.GetAllowedSends();
			if (num < 1)
			{
				return;
			}
			while (this.m_queuedSends.Count > 0 && num > 0)
			{
				NetOutgoingMessage om2;
				if (this.m_queuedSends.TryDequeue(out om2))
				{
					this.ExecuteSend(now, om2);
				}
				num--;
			}
		}

		private void ExecuteSend(float now, NetOutgoingMessage message)
		{
			int seqNr = this.m_sendStart;
			this.m_sendStart = (this.m_sendStart + 1) % 1024;
			this.m_connection.QueueSendMessage(message, seqNr);
			int storeIndex = seqNr % this.m_windowSize;
			NetStoredReliableMessage[] storedMessages = this.m_storedMessages;
			int num = storeIndex;
			storedMessages[num].NumSent = storedMessages[num].NumSent + 1;
			this.m_storedMessages[storeIndex].Message = message;
			this.m_storedMessages[storeIndex].LastSent = now;
		}

		private void DestoreMessage(int storeIndex)
		{
			NetOutgoingMessage storedMessage = this.m_storedMessages[storeIndex].Message;
			if (storedMessage != null)
			{
				Interlocked.Decrement(ref storedMessage.m_recyclingCount);
				if (storedMessage.m_recyclingCount <= 0)
				{
					this.m_connection.m_peer.Recycle(storedMessage);
				}
			}
			this.m_storedMessages[storeIndex] = default(NetStoredReliableMessage);
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
				this.DestoreMessage(this.m_windowStart % this.m_windowSize);
				this.m_windowStart = (this.m_windowStart + 1) % 1024;
				while (this.m_receivedAcks.Get(this.m_windowStart))
				{
					this.m_receivedAcks[this.m_windowStart] = false;
					this.DestoreMessage(this.m_windowStart % this.m_windowSize);
					this.m_windowStart = (this.m_windowStart + 1) % 1024;
				}
				return;
			}
			int sendRelate = NetUtility.RelativeSequenceNumber(seqNr, this.m_sendStart);
			if (sendRelate <= 0)
			{
				if (!this.m_receivedAcks[seqNr])
				{
					this.m_receivedAcks[seqNr] = true;
				}
			}
			else if (sendRelate > 0)
			{
				return;
			}
			int rnr = seqNr;
			do
			{
				rnr--;
				if (rnr < 0)
				{
					rnr = 1023;
				}
				if (!this.m_receivedAcks[rnr])
				{
					int slot = rnr % this.m_windowSize;
					if (this.m_storedMessages[slot].NumSent == 1)
					{
						NetOutgoingMessage rmsg = this.m_storedMessages[slot].Message;
						if (now - this.m_storedMessages[slot].LastSent >= this.m_resendDelay * 0.35f)
						{
							this.m_storedMessages[slot].LastSent = now;
							NetStoredReliableMessage[] storedMessages = this.m_storedMessages;
							int num = slot;
							storedMessages[num].NumSent = storedMessages[num].NumSent + 1;
							this.m_connection.QueueSendMessage(rmsg, rnr);
						}
					}
				}
			}
			while (rnr != this.m_windowStart);
		}

		private NetConnection m_connection;

		private int m_windowStart;

		private int m_windowSize;

		private int m_sendStart;

		private NetBitVector m_receivedAcks;

		internal NetStoredReliableMessage[] m_storedMessages;

		internal float m_resendDelay;
	}
}
