using System;
using System.Diagnostics;
using System.Net;

namespace DNA.Net.Lidgren
{
	[DebuggerDisplay("RemoteUniqueIdentifier={RemoteUniqueIdentifier} RemoteEndPoint={remoteEndPoint}")]
	public class NetConnection
	{
		internal void InitExpandMTU(double now)
		{
			this.m_lastSentMTUAttemptTime = now + (double)this.m_peerConfiguration.m_expandMTUFrequency + 1.5 + (double)this.m_averageRoundtripTime;
			this.m_largestSuccessfulMTU = 512;
			this.m_smallestFailedMTU = -1;
			this.m_currentMTU = this.m_peerConfiguration.MaximumTransmissionUnit;
		}

		private void MTUExpansionHeartbeat(double now)
		{
			if (this.m_expandMTUStatus == NetConnection.ExpandMTUStatus.Finished)
			{
				return;
			}
			if (this.m_expandMTUStatus != NetConnection.ExpandMTUStatus.None)
			{
				if (now > this.m_lastSentMTUAttemptTime + (double)this.m_peerConfiguration.ExpandMTUFrequency)
				{
					this.m_mtuAttemptFails++;
					if (this.m_mtuAttemptFails == 3)
					{
						this.FinalizeMTU(this.m_currentMTU);
						return;
					}
					this.m_smallestFailedMTU = this.m_lastSentMTUAttemptSize;
					this.ExpandMTU(now, false);
				}
				return;
			}
			if (!this.m_peerConfiguration.m_autoExpandMTU)
			{
				this.FinalizeMTU(this.m_currentMTU);
				return;
			}
			this.ExpandMTU(now, true);
		}

		private void ExpandMTU(double now, bool succeeded)
		{
			int tryMTU;
			if (this.m_smallestFailedMTU == -1)
			{
				tryMTU = (int)((float)this.m_currentMTU * 1.25f);
			}
			else
			{
				tryMTU = (int)(((float)this.m_smallestFailedMTU + (float)this.m_largestSuccessfulMTU) / 2f);
			}
			if (tryMTU > 8190)
			{
				tryMTU = 8190;
			}
			if (tryMTU == this.m_largestSuccessfulMTU)
			{
				this.FinalizeMTU(this.m_largestSuccessfulMTU);
				return;
			}
			this.SendExpandMTU(now, tryMTU);
		}

		private void SendExpandMTU(double now, int size)
		{
			NetOutgoingMessage om = this.m_peer.CreateMessage(size);
			byte[] tmp = new byte[size];
			om.Write(tmp);
			om.m_messageType = NetMessageType.ExpandMTURequest;
			int len = om.Encode(this.m_peer.m_sendBuffer, 0, 0);
			if (!this.m_peer.SendMTUPacket(len, this.m_remoteEndPoint))
			{
				if (this.m_smallestFailedMTU == -1 || size < this.m_smallestFailedMTU)
				{
					this.m_smallestFailedMTU = size;
					this.m_mtuAttemptFails++;
					if (this.m_mtuAttemptFails >= this.m_peerConfiguration.ExpandMTUFailAttempts)
					{
						this.FinalizeMTU(this.m_largestSuccessfulMTU);
						return;
					}
				}
				this.ExpandMTU(now, false);
				return;
			}
			this.m_lastSentMTUAttemptSize = size;
			this.m_lastSentMTUAttemptTime = now;
		}

		private void FinalizeMTU(int size)
		{
			if (this.m_expandMTUStatus == NetConnection.ExpandMTUStatus.Finished)
			{
				return;
			}
			this.m_expandMTUStatus = NetConnection.ExpandMTUStatus.Finished;
			this.m_currentMTU = size;
			int currentMTU = this.m_currentMTU;
			int maximumTransmissionUnit = this.m_peerConfiguration.m_maximumTransmissionUnit;
		}

		private void SendMTUSuccess(int size)
		{
			NetOutgoingMessage om = this.m_peer.CreateMessage(4);
			om.Write(size);
			om.m_messageType = NetMessageType.ExpandMTUSuccess;
			int len = om.Encode(this.m_peer.m_sendBuffer, 0, 0);
			bool connectionReset;
			this.m_peer.SendPacket(len, this.m_remoteEndPoint, 1, out connectionReset);
		}

		private void HandleExpandMTUSuccess(double now, int size)
		{
			if (size > this.m_largestSuccessfulMTU)
			{
				this.m_largestSuccessfulMTU = size;
			}
			if (size < this.m_currentMTU)
			{
				return;
			}
			this.m_currentMTU = size;
			this.ExpandMTU(now, true);
		}

		public float AverageRoundtripTime
		{
			get
			{
				return this.m_averageRoundtripTime;
			}
		}

		public float RemoteTimeOffset
		{
			get
			{
				return (float)this.m_remoteTimeOffset;
			}
		}

		internal void InitializeRemoteTimeOffset(float remoteSendTime)
		{
			this.m_remoteTimeOffset = (double)remoteSendTime + (double)this.m_averageRoundtripTime / 2.0 - NetTime.Now;
		}

		public double GetLocalTime(double remoteTimestamp)
		{
			return remoteTimestamp - this.m_remoteTimeOffset;
		}

		public double GetRemoteTime(double localTimestamp)
		{
			return localTimestamp + this.m_remoteTimeOffset;
		}

		internal void InitializePing()
		{
			float now = (float)NetTime.Now;
			this.m_sentPingTime = now;
			this.m_sentPingTime -= this.m_peerConfiguration.PingInterval * 0.25f;
			this.m_sentPingTime -= NetRandom.Instance.NextSingle() * (this.m_peerConfiguration.PingInterval * 0.75f);
			this.m_timeoutDeadline = now + this.m_peerConfiguration.m_connectionTimeout * 2f;
			this.SendPing();
		}

		internal void SendPing()
		{
			this.m_sentPingNumber++;
			this.m_sentPingTime = (float)NetTime.Now;
			NetOutgoingMessage om = this.m_peer.CreateMessage(1);
			om.Write((byte)this.m_sentPingNumber);
			om.m_messageType = NetMessageType.Ping;
			int len = om.Encode(this.m_peer.m_sendBuffer, 0, 0);
			bool connectionReset;
			this.m_peer.SendPacket(len, this.m_remoteEndPoint, 1, out connectionReset);
		}

		internal void SendPong(int pingNumber)
		{
			NetOutgoingMessage om = this.m_peer.CreateMessage(5);
			om.Write((byte)pingNumber);
			om.Write((float)NetTime.Now);
			om.m_messageType = NetMessageType.Pong;
			int len = om.Encode(this.m_peer.m_sendBuffer, 0, 0);
			bool connectionReset;
			this.m_peer.SendPacket(len, this.m_remoteEndPoint, 1, out connectionReset);
		}

		internal void ReceivedPong(float now, int pongNumber, float remoteSendTime)
		{
			if ((byte)pongNumber != (byte)this.m_sentPingNumber)
			{
				return;
			}
			this.m_timeoutDeadline = now + this.m_peerConfiguration.m_connectionTimeout;
			float rtt = now - this.m_sentPingTime;
			double diff = (double)remoteSendTime + (double)rtt / 2.0 - (double)now;
			if (this.m_averageRoundtripTime < 0f)
			{
				this.m_remoteTimeOffset = diff;
				this.m_averageRoundtripTime = rtt;
			}
			else
			{
				this.m_averageRoundtripTime = this.m_averageRoundtripTime * 0.7f + rtt * 0.3f;
				this.m_remoteTimeOffset = (this.m_remoteTimeOffset * (double)(this.m_sentPingNumber - 1) + diff) / (double)this.m_sentPingNumber;
			}
			float resendDelay = this.GetResendDelay();
			foreach (NetSenderChannelBase chan in this.m_sendChannels)
			{
				NetReliableSenderChannel rchan = chan as NetReliableSenderChannel;
				if (rchan != null)
				{
					rchan.m_resendDelay = resendDelay;
				}
			}
			if (this.m_peer.m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.ConnectionLatencyUpdated))
			{
				NetIncomingMessage update = this.m_peer.CreateIncomingMessage(NetIncomingMessageType.ConnectionLatencyUpdated, 4);
				update.m_senderConnection = this;
				update.m_senderEndPoint = this.m_remoteEndPoint;
				update.Write(rtt);
				this.m_peer.ReleaseMessage(update);
			}
		}

		public object Tag
		{
			get
			{
				return this.m_tag;
			}
			set
			{
				this.m_tag = value;
			}
		}

		public NetPeer Peer
		{
			get
			{
				return this.m_peer;
			}
		}

		public NetConnectionStatus Status
		{
			get
			{
				return this.m_visibleStatus;
			}
		}

		public NetConnectionStatistics Statistics
		{
			get
			{
				return this.m_statistics;
			}
		}

		public IPEndPoint RemoteEndPoint
		{
			get
			{
				return this.m_remoteEndPoint;
			}
		}

		public long RemoteUniqueIdentifier
		{
			get
			{
				return this.m_remoteUniqueIdentifier;
			}
		}

		public NetOutgoingMessage LocalHailMessage
		{
			get
			{
				return this.m_localHailMessage;
			}
		}

		internal float GetResendDelay()
		{
			float avgRtt = this.m_averageRoundtripTime;
			if (avgRtt <= 0f)
			{
				avgRtt = 0.1f;
			}
			return 0.02f + avgRtt * 2f;
		}

		internal NetConnection(NetPeer peer, IPEndPoint remoteEndPoint)
		{
			this.m_peer = peer;
			this.m_peerConfiguration = this.m_peer.Configuration;
			this.m_status = NetConnectionStatus.None;
			this.m_visibleStatus = NetConnectionStatus.None;
			this.m_remoteEndPoint = remoteEndPoint;
			this.m_sendChannels = new NetSenderChannelBase[99];
			this.m_receiveChannels = new NetReceiverChannelBase[99];
			this.m_queuedOutgoingAcks = new NetQueue<NetTuple<NetMessageType, int>>(4);
			this.m_queuedIncomingAcks = new NetQueue<NetTuple<NetMessageType, int>>(4);
			this.m_statistics = new NetConnectionStatistics(this);
			this.m_averageRoundtripTime = -1f;
			this.m_currentMTU = this.m_peerConfiguration.MaximumTransmissionUnit;
		}

		internal void MutateEndPoint(IPEndPoint endPoint)
		{
			this.m_remoteEndPoint = endPoint;
		}

		internal void SetStatus(NetConnectionStatus status, string reason)
		{
			if (status == this.m_status)
			{
				return;
			}
			this.m_status = status;
			if (reason == null)
			{
				reason = string.Empty;
			}
			if (this.m_status == NetConnectionStatus.Connected)
			{
				this.m_timeoutDeadline = (float)NetTime.Now + this.m_peerConfiguration.m_connectionTimeout;
			}
			if (this.m_peerConfiguration.IsMessageTypeEnabled(NetIncomingMessageType.StatusChanged))
			{
				NetIncomingMessage info = this.m_peer.CreateIncomingMessage(NetIncomingMessageType.StatusChanged, 4 + reason.Length + ((reason.Length > 126) ? 2 : 1));
				info.m_senderConnection = this;
				info.m_senderEndPoint = this.m_remoteEndPoint;
				info.Write((byte)this.m_status);
				info.Write(reason);
				this.m_peer.ReleaseMessage(info);
				return;
			}
			this.m_visibleStatus = this.m_status;
		}

		internal void Heartbeat(float now, uint frameCounter)
		{
			if (frameCounter % 8U == 0U)
			{
				if (now > this.m_timeoutDeadline)
				{
					this.ExecuteDisconnect("Connection timed out", true);
				}
				if (this.m_status == NetConnectionStatus.Connected)
				{
					if (now > this.m_sentPingTime + this.m_peer.m_configuration.m_pingInterval)
					{
						this.SendPing();
					}
					this.MTUExpansionHeartbeat((double)now);
				}
				if (this.m_disconnectRequested)
				{
					this.ExecuteDisconnect(this.m_disconnectMessage, true);
					return;
				}
			}
			byte[] sendBuffer = this.m_peer.m_sendBuffer;
			int mtu = this.m_currentMTU;
			if (frameCounter % 3U == 0U)
			{
				while (this.m_queuedOutgoingAcks.Count > 0)
				{
					int acks = (mtu - (this.m_sendBufferWritePtr + 5)) / 3;
					if (acks > this.m_queuedOutgoingAcks.Count)
					{
						acks = this.m_queuedOutgoingAcks.Count;
					}
					this.m_sendBufferNumMessages++;
					sendBuffer[this.m_sendBufferWritePtr++] = 134;
					sendBuffer[this.m_sendBufferWritePtr++] = 0;
					sendBuffer[this.m_sendBufferWritePtr++] = 0;
					int len = acks * 3 * 8;
					sendBuffer[this.m_sendBufferWritePtr++] = (byte)len;
					sendBuffer[this.m_sendBufferWritePtr++] = (byte)(len >> 8);
					for (int i = 0; i < acks; i++)
					{
						NetTuple<NetMessageType, int> tuple;
						this.m_queuedOutgoingAcks.TryDequeue(out tuple);
						sendBuffer[this.m_sendBufferWritePtr++] = (byte)tuple.Item1;
						sendBuffer[this.m_sendBufferWritePtr++] = (byte)tuple.Item2;
						sendBuffer[this.m_sendBufferWritePtr++] = (byte)(tuple.Item2 >> 8);
					}
					if (this.m_queuedOutgoingAcks.Count > 0)
					{
						bool connectionReset;
						this.m_peer.SendPacket(this.m_sendBufferWritePtr, this.m_remoteEndPoint, this.m_sendBufferNumMessages, out connectionReset);
						this.m_sendBufferWritePtr = 0;
						this.m_sendBufferNumMessages = 0;
					}
				}
				NetTuple<NetMessageType, int> incAck;
				while (this.m_queuedIncomingAcks.TryDequeue(out incAck))
				{
					NetSenderChannelBase chan = this.m_sendChannels[(int)(incAck.Item1 - NetMessageType.UserUnreliable)];
					if (chan == null)
					{
						chan = this.CreateSenderChannel(incAck.Item1);
					}
					chan.ReceiveAcknowledge(now, incAck.Item2);
				}
			}
			if (this.m_peer.m_executeFlushSendQueue)
			{
				for (int j = this.m_sendChannels.Length - 1; j >= 0; j--)
				{
					NetSenderChannelBase channel = this.m_sendChannels[j];
					if (channel != null)
					{
						channel.SendQueuedMessages(now);
					}
				}
			}
			if (this.m_sendBufferWritePtr > 0)
			{
				bool connectionReset;
				this.m_peer.SendPacket(this.m_sendBufferWritePtr, this.m_remoteEndPoint, this.m_sendBufferNumMessages, out connectionReset);
				this.m_sendBufferWritePtr = 0;
				this.m_sendBufferNumMessages = 0;
			}
		}

		internal void QueueSendMessage(NetOutgoingMessage om, int seqNr)
		{
			int sz = om.GetEncodedSize();
			if (sz > this.m_currentMTU)
			{
				this.m_peer.LogWarning("Message larger than MTU! Fragmentation must have failed!");
			}
			if (this.m_sendBufferWritePtr + sz > this.m_currentMTU)
			{
				bool connReset;
				this.m_peer.SendPacket(this.m_sendBufferWritePtr, this.m_remoteEndPoint, this.m_sendBufferNumMessages, out connReset);
				this.m_sendBufferWritePtr = 0;
				this.m_sendBufferNumMessages = 0;
			}
			this.m_sendBufferWritePtr = om.Encode(this.m_peer.m_sendBuffer, this.m_sendBufferWritePtr, seqNr);
			this.m_sendBufferNumMessages++;
		}

		public NetSendResult SendMessage(NetOutgoingMessage msg, NetDeliveryMethod method, int sequenceChannel)
		{
			return this.m_peer.SendMessage(msg, this, method, sequenceChannel);
		}

		internal NetSendResult EnqueueMessage(NetOutgoingMessage msg, NetDeliveryMethod method, int sequenceChannel)
		{
			if (this.m_status != NetConnectionStatus.Connected)
			{
				return NetSendResult.FailedNotConnected;
			}
			NetMessageType tp = (NetMessageType)(method + (byte)sequenceChannel);
			msg.m_messageType = tp;
			int channelSlot = (int)(method - NetDeliveryMethod.Unreliable) + sequenceChannel;
			NetSenderChannelBase chan = this.m_sendChannels[channelSlot];
			if (chan == null)
			{
				chan = this.CreateSenderChannel(tp);
			}
			if (msg.GetEncodedSize() > this.m_currentMTU)
			{
				throw new NetException("Message too large! Fragmentation failure?");
			}
			NetSendResult retval = chan.Enqueue(msg);
			if (retval == NetSendResult.Sent && !this.m_peerConfiguration.m_autoFlushSendQueue)
			{
				retval = NetSendResult.Queued;
			}
			return retval;
		}

		private NetSenderChannelBase CreateSenderChannel(NetMessageType tp)
		{
			NetSenderChannelBase chan;
			lock (this.m_sendChannels)
			{
				NetDeliveryMethod method = NetUtility.GetDeliveryMethod(tp);
				int sequenceChannel = (int)(tp - (NetMessageType)method);
				int channelSlot = (int)(method - NetDeliveryMethod.Unreliable) + sequenceChannel;
				if (this.m_sendChannels[channelSlot] != null)
				{
					chan = this.m_sendChannels[channelSlot];
				}
				else
				{
					NetDeliveryMethod netDeliveryMethod = method;
					switch (netDeliveryMethod)
					{
					case NetDeliveryMethod.Unreliable:
					case NetDeliveryMethod.UnreliableSequenced:
						chan = new NetUnreliableSenderChannel(this, NetUtility.GetWindowSize(method));
						break;
					default:
						switch (netDeliveryMethod)
						{
						case NetDeliveryMethod.ReliableUnordered:
						case NetDeliveryMethod.ReliableSequenced:
							break;
						default:
							if (netDeliveryMethod == NetDeliveryMethod.ReliableOrdered)
							{
								chan = new NetReliableSenderChannel(this, NetUtility.GetWindowSize(method));
								goto IL_0092;
							}
							break;
						}
						chan = new NetReliableSenderChannel(this, NetUtility.GetWindowSize(method));
						break;
					}
					IL_0092:
					this.m_sendChannels[channelSlot] = chan;
				}
			}
			return chan;
		}

		internal void ReceivedLibraryMessage(NetMessageType tp, int ptr, int payloadLength)
		{
			float now = (float)NetTime.Now;
			switch (tp)
			{
			case NetMessageType.Ping:
			{
				int pingNr = (int)this.m_peer.m_receiveBuffer[ptr++];
				this.SendPong(pingNr);
				return;
			}
			case NetMessageType.Pong:
			{
				NetIncomingMessage pmsg = this.m_peer.SetupReadHelperMessage(ptr, payloadLength);
				int pongNr = (int)pmsg.ReadByte();
				float remoteSendTime = pmsg.ReadSingle();
				this.ReceivedPong(now, pongNr, remoteSendTime);
				return;
			}
			case NetMessageType.Acknowledge:
			{
				for (int i = 0; i < payloadLength; i += 3)
				{
					NetMessageType acktp = (NetMessageType)this.m_peer.m_receiveBuffer[ptr++];
					int seqNr = (int)this.m_peer.m_receiveBuffer[ptr++];
					seqNr |= (int)this.m_peer.m_receiveBuffer[ptr++] << 8;
					this.m_queuedIncomingAcks.Enqueue(new NetTuple<NetMessageType, int>(acktp, seqNr));
				}
				return;
			}
			case NetMessageType.Disconnect:
			{
				NetIncomingMessage msg = this.m_peer.SetupReadHelperMessage(ptr, payloadLength);
				this.ExecuteDisconnect(msg.ReadString(), false);
				return;
			}
			case NetMessageType.NatIntroduction:
				this.m_peer.HandleNatIntroduction(ptr);
				return;
			case NetMessageType.ExpandMTURequest:
				this.SendMTUSuccess(payloadLength);
				return;
			case NetMessageType.ExpandMTUSuccess:
			{
				NetIncomingMessage emsg = this.m_peer.SetupReadHelperMessage(ptr, payloadLength);
				int size = emsg.ReadInt32();
				this.HandleExpandMTUSuccess((double)now, size);
				return;
			}
			}
			this.m_peer.LogWarning("Connection received unhandled library message: " + tp);
		}

		internal void ReceivedMessage(NetIncomingMessage msg)
		{
			NetMessageType tp = msg.m_receivedMessageType;
			int channelSlot = (int)(tp - NetMessageType.UserUnreliable);
			NetReceiverChannelBase chan = this.m_receiveChannels[channelSlot];
			if (chan == null)
			{
				chan = this.CreateReceiverChannel(tp);
			}
			chan.ReceiveMessage(msg);
		}

		private NetReceiverChannelBase CreateReceiverChannel(NetMessageType tp)
		{
			NetDeliveryMethod method = NetUtility.GetDeliveryMethod(tp);
			NetDeliveryMethod netDeliveryMethod = method;
			NetReceiverChannelBase chan;
			switch (netDeliveryMethod)
			{
			case NetDeliveryMethod.Unreliable:
				chan = new NetUnreliableUnorderedReceiver(this);
				break;
			case NetDeliveryMethod.UnreliableSequenced:
				chan = new NetUnreliableSequencedReceiver(this);
				break;
			default:
				switch (netDeliveryMethod)
				{
				case NetDeliveryMethod.ReliableUnordered:
					chan = new NetReliableUnorderedReceiver(this, 64);
					break;
				case NetDeliveryMethod.ReliableSequenced:
					chan = new NetReliableSequencedReceiver(this, 64);
					break;
				default:
					if (netDeliveryMethod != NetDeliveryMethod.ReliableOrdered)
					{
						throw new NetException("Unhandled NetDeliveryMethod!");
					}
					chan = new NetReliableOrderedReceiver(this, 64);
					break;
				}
				break;
			}
			int channelSlot = (int)(tp - NetMessageType.UserUnreliable);
			this.m_receiveChannels[channelSlot] = chan;
			return chan;
		}

		internal void QueueAck(NetMessageType tp, int sequenceNumber)
		{
			this.m_queuedOutgoingAcks.Enqueue(new NetTuple<NetMessageType, int>(tp, sequenceNumber));
		}

		public void GetSendQueueInfo(NetDeliveryMethod method, int sequenceChannel, out int windowSize, out int freeWindowSlots)
		{
			int channelSlot = (int)(method - NetDeliveryMethod.Unreliable) + sequenceChannel;
			NetSenderChannelBase chan = this.m_sendChannels[channelSlot];
			if (chan == null)
			{
				windowSize = NetUtility.GetWindowSize(method);
				freeWindowSlots = windowSize;
				return;
			}
			windowSize = chan.WindowSize;
			freeWindowSlots = chan.GetAllowedSends() - chan.m_queuedSends.Count;
		}

		internal void Shutdown(string reason)
		{
			this.ExecuteDisconnect(reason, true);
		}

		public override string ToString()
		{
			return "[NetConnection to " + this.m_remoteEndPoint + "]";
		}

		public NetIncomingMessage RemoteHailMessage
		{
			get
			{
				return this.m_remoteHailMessage;
			}
		}

		internal void UnconnectedHeartbeat(float now)
		{
			if (this.m_disconnectRequested)
			{
				this.ExecuteDisconnect(this.m_disconnectMessage, true);
			}
			if (this.m_connectRequested)
			{
				switch (this.m_status)
				{
				case NetConnectionStatus.InitiatedConnect:
					this.SendConnect(now);
					return;
				case NetConnectionStatus.RespondedConnect:
				case NetConnectionStatus.Connected:
					this.ExecuteDisconnect("Reconnecting", true);
					return;
				case NetConnectionStatus.Disconnecting:
					return;
				case NetConnectionStatus.Disconnected:
					throw new NetException("This connection is Disconnected; spent. A new one should have been created");
				}
				this.SendConnect(now);
				return;
			}
			if (now - this.m_lastHandshakeSendTime > this.m_peerConfiguration.m_resendHandshakeInterval)
			{
				if (this.m_handshakeAttempts >= this.m_peerConfiguration.m_maximumHandshakeAttempts)
				{
					this.ExecuteDisconnect("Failed to establish connection - no response from remote host", true);
					return;
				}
				switch (this.m_status)
				{
				case NetConnectionStatus.None:
				case NetConnectionStatus.ReceivedInitiation:
					this.m_peer.LogWarning("Time to resend handshake, but status is " + this.m_status);
					return;
				case NetConnectionStatus.InitiatedConnect:
					this.SendConnect(now);
					return;
				case NetConnectionStatus.RespondedAwaitingApproval:
					this.m_lastHandshakeSendTime = now;
					return;
				case NetConnectionStatus.RespondedConnect:
					this.SendConnectResponse(now, true);
					return;
				default:
					this.m_peer.LogWarning("Time to resend handshake, but status is " + this.m_status);
					break;
				}
			}
		}

		internal void ExecuteDisconnect(string reason, bool sendByeMessage)
		{
			for (int i = 0; i < this.m_sendChannels.Length; i++)
			{
				NetSenderChannelBase channel = this.m_sendChannels[i];
				if (channel != null)
				{
					channel.Reset();
				}
			}
			if (sendByeMessage)
			{
				this.SendDisconnect(reason, true);
			}
			this.SetStatus(NetConnectionStatus.Disconnected, reason);
			lock (this.m_peer.m_handshakes)
			{
				this.m_peer.m_handshakes.Remove(this.m_remoteEndPoint);
			}
			this.m_disconnectRequested = false;
			this.m_connectRequested = false;
			this.m_handshakeAttempts = 0;
		}

		internal void SendConnect(float now)
		{
			int preAllocate = 13 + this.m_peerConfiguration.AppIdentifier.Length;
			preAllocate += ((this.m_localHailMessage == null) ? 0 : this.m_localHailMessage.LengthBytes);
			NetOutgoingMessage om = this.m_peer.CreateMessage(preAllocate);
			om.m_messageType = NetMessageType.Connect;
			om.Write(this.m_peerConfiguration.AppIdentifier);
			om.Write(this.m_peer.m_uniqueIdentifier);
			om.Write(now);
			this.WriteLocalHail(om);
			this.m_peer.SendLibrary(om, this.m_remoteEndPoint);
			this.m_connectRequested = false;
			this.m_lastHandshakeSendTime = now;
			this.m_handshakeAttempts++;
			int handshakeAttempts = this.m_handshakeAttempts;
			this.SetStatus(NetConnectionStatus.InitiatedConnect, "Locally requested connect");
		}

		internal void SendConnectResponse(float now, bool onLibraryThread)
		{
			NetOutgoingMessage om = this.m_peer.CreateMessage(this.m_peerConfiguration.AppIdentifier.Length + 13 + ((this.m_localHailMessage == null) ? 0 : this.m_localHailMessage.LengthBytes));
			om.m_messageType = NetMessageType.ConnectResponse;
			om.Write(this.m_peerConfiguration.AppIdentifier);
			om.Write(this.m_peer.m_uniqueIdentifier);
			om.Write(now);
			this.WriteLocalHail(om);
			if (onLibraryThread)
			{
				this.m_peer.SendLibrary(om, this.m_remoteEndPoint);
			}
			else
			{
				this.m_peer.m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(this.m_remoteEndPoint, om));
			}
			this.m_lastHandshakeSendTime = now;
			this.m_handshakeAttempts++;
			int handshakeAttempts = this.m_handshakeAttempts;
			this.SetStatus(NetConnectionStatus.RespondedConnect, "Remotely requested connect");
		}

		internal void SendDisconnect(string reason, bool onLibraryThread)
		{
			NetOutgoingMessage om = this.m_peer.CreateMessage(reason);
			om.m_messageType = NetMessageType.Disconnect;
			if (onLibraryThread)
			{
				this.m_peer.SendLibrary(om, this.m_remoteEndPoint);
				return;
			}
			this.m_peer.m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(this.m_remoteEndPoint, om));
		}

		private void WriteLocalHail(NetOutgoingMessage om)
		{
			if (this.m_localHailMessage != null)
			{
				byte[] hi = this.m_localHailMessage.Data;
				if (hi != null && hi.Length >= this.m_localHailMessage.LengthBytes)
				{
					if (om.LengthBytes + this.m_localHailMessage.LengthBytes > this.m_peerConfiguration.m_maximumTransmissionUnit - 10)
					{
						throw new NetException("Hail message too large; can maximally be " + (this.m_peerConfiguration.m_maximumTransmissionUnit - 10 - om.LengthBytes));
					}
					om.Write(this.m_localHailMessage.Data, 0, this.m_localHailMessage.LengthBytes);
				}
			}
		}

		internal void SendConnectionEstablished()
		{
			NetOutgoingMessage om = this.m_peer.CreateMessage(4);
			om.m_messageType = NetMessageType.ConnectionEstablished;
			om.Write((float)NetTime.Now);
			this.m_peer.SendLibrary(om, this.m_remoteEndPoint);
			this.m_handshakeAttempts = 0;
			this.InitializePing();
			if (this.m_status != NetConnectionStatus.Connected)
			{
				this.SetStatus(NetConnectionStatus.Connected, "Connected to " + NetUtility.ToHexString(this.m_remoteUniqueIdentifier));
			}
		}

		public void Approve()
		{
			if (this.m_status != NetConnectionStatus.RespondedAwaitingApproval)
			{
				this.m_peer.LogWarning("Approve() called in wrong status; expected RespondedAwaitingApproval; got " + this.m_status);
				return;
			}
			this.m_localHailMessage = null;
			this.m_handshakeAttempts = 0;
			this.SendConnectResponse((float)NetTime.Now, false);
		}

		public void Approve(NetOutgoingMessage localHail)
		{
			if (this.m_status != NetConnectionStatus.RespondedAwaitingApproval)
			{
				this.m_peer.LogWarning("Approve() called in wrong status; expected RespondedAwaitingApproval; got " + this.m_status);
				return;
			}
			this.m_localHailMessage = localHail;
			this.m_handshakeAttempts = 0;
			this.SendConnectResponse((float)NetTime.Now, false);
		}

		public void Deny()
		{
			this.Deny(string.Empty);
		}

		public void Deny(string reason)
		{
			this.SendDisconnect(reason, false);
			this.m_peer.m_handshakes.Remove(this.m_remoteEndPoint);
		}

		internal void ReceivedHandshake(double now, NetMessageType tp, int ptr, int payloadLength)
		{
			switch (tp)
			{
			case NetMessageType.Ping:
			case NetMessageType.Pong:
			case NetMessageType.Acknowledge:
				break;
			case NetMessageType.Connect:
				if (this.m_status == NetConnectionStatus.ReceivedInitiation)
				{
					byte[] hail;
					bool ok = this.ValidateHandshakeData(ptr, payloadLength, out hail);
					if (ok)
					{
						if (hail != null)
						{
							this.m_remoteHailMessage = this.m_peer.CreateIncomingMessage(NetIncomingMessageType.Data, hail);
							this.m_remoteHailMessage.LengthBits = hail.Length * 8;
						}
						else
						{
							this.m_remoteHailMessage = null;
						}
						if (this.m_peerConfiguration.IsMessageTypeEnabled(NetIncomingMessageType.ConnectionApproval))
						{
							NetIncomingMessage appMsg = this.m_peer.CreateIncomingMessage(NetIncomingMessageType.ConnectionApproval, (this.m_remoteHailMessage == null) ? 0 : this.m_remoteHailMessage.LengthBytes);
							appMsg.m_receiveTime = now;
							appMsg.m_senderConnection = this;
							appMsg.m_senderEndPoint = this.m_remoteEndPoint;
							if (this.m_remoteHailMessage != null)
							{
								appMsg.Write(this.m_remoteHailMessage.m_data, 0, this.m_remoteHailMessage.LengthBytes);
							}
							this.SetStatus(NetConnectionStatus.RespondedAwaitingApproval, "Awaiting approval");
							this.m_peer.ReleaseMessage(appMsg);
							return;
						}
						this.SendConnectResponse((float)now, true);
					}
					return;
				}
				if (this.m_status == NetConnectionStatus.RespondedAwaitingApproval)
				{
					this.m_peer.LogWarning("Ignoring multiple Connect() most likely due to a delayed Approval");
					return;
				}
				if (this.m_status == NetConnectionStatus.RespondedConnect)
				{
					this.SendConnectResponse((float)now, true);
					return;
				}
				break;
			case NetMessageType.ConnectResponse:
				switch (this.m_status)
				{
				case NetConnectionStatus.None:
				case NetConnectionStatus.ReceivedInitiation:
				case NetConnectionStatus.RespondedAwaitingApproval:
				case NetConnectionStatus.RespondedConnect:
				case NetConnectionStatus.Disconnecting:
				case NetConnectionStatus.Disconnected:
					break;
				case NetConnectionStatus.InitiatedConnect:
				{
					byte[] hail;
					bool ok2 = this.ValidateHandshakeData(ptr, payloadLength, out hail);
					if (ok2)
					{
						if (hail != null)
						{
							this.m_remoteHailMessage = this.m_peer.CreateIncomingMessage(NetIncomingMessageType.Data, hail);
							this.m_remoteHailMessage.LengthBits = hail.Length * 8;
						}
						else
						{
							this.m_remoteHailMessage = null;
						}
						this.m_peer.AcceptConnection(this);
						this.SendConnectionEstablished();
						return;
					}
					break;
				}
				case NetConnectionStatus.Connected:
					this.SendConnectionEstablished();
					return;
				default:
					return;
				}
				break;
			case NetMessageType.ConnectionEstablished:
				switch (this.m_status)
				{
				case NetConnectionStatus.None:
				case NetConnectionStatus.InitiatedConnect:
				case NetConnectionStatus.ReceivedInitiation:
				case NetConnectionStatus.RespondedAwaitingApproval:
				case NetConnectionStatus.Connected:
				case NetConnectionStatus.Disconnecting:
				case NetConnectionStatus.Disconnected:
					break;
				case NetConnectionStatus.RespondedConnect:
				{
					NetIncomingMessage msg = this.m_peer.SetupReadHelperMessage(ptr, payloadLength);
					this.InitializeRemoteTimeOffset(msg.ReadSingle());
					this.m_peer.AcceptConnection(this);
					this.InitializePing();
					this.SetStatus(NetConnectionStatus.Connected, "Connected to " + NetUtility.ToHexString(this.m_remoteUniqueIdentifier));
					return;
				}
				default:
					return;
				}
				break;
			case NetMessageType.Disconnect:
			{
				string reason = "Ouch";
				try
				{
					NetIncomingMessage inc = this.m_peer.SetupReadHelperMessage(ptr, payloadLength);
					reason = inc.ReadString();
				}
				catch
				{
				}
				this.ExecuteDisconnect(reason, false);
				return;
			}
			case NetMessageType.Discovery:
				this.m_peer.HandleIncomingDiscoveryRequest(now, this.m_remoteEndPoint, ptr, payloadLength);
				return;
			case NetMessageType.DiscoveryResponse:
				this.m_peer.HandleIncomingDiscoveryResponse(now, this.m_remoteEndPoint, ptr, payloadLength);
				break;
			default:
				return;
			}
		}

		private bool ValidateHandshakeData(int ptr, int payloadLength, out byte[] hail)
		{
			hail = null;
			NetIncomingMessage msg = this.m_peer.SetupReadHelperMessage(ptr, payloadLength);
			try
			{
				string remoteAppIdentifier = msg.ReadString();
				long remoteUniqueIdentifier = msg.ReadInt64();
				this.InitializeRemoteTimeOffset(msg.ReadSingle());
				int remainingBytes = payloadLength - (msg.PositionInBytes - ptr);
				if (remainingBytes > 0)
				{
					hail = msg.ReadBytes(remainingBytes);
				}
				if (remoteAppIdentifier != this.m_peer.m_configuration.AppIdentifier)
				{
					this.ExecuteDisconnect("Wrong application identifier!", true);
					return false;
				}
				this.m_remoteUniqueIdentifier = remoteUniqueIdentifier;
			}
			catch (Exception ex)
			{
				this.ExecuteDisconnect("Handshake data validation failed", true);
				this.m_peer.LogWarning("ReadRemoteHandshakeData failed: " + ex.Message);
				return false;
			}
			return true;
		}

		public void Disconnect(string byeMessage)
		{
			if (this.m_status == NetConnectionStatus.None || this.m_status == NetConnectionStatus.Disconnected)
			{
				return;
			}
			this.m_disconnectMessage = byeMessage;
			if (this.m_status != NetConnectionStatus.Disconnected && this.m_status != NetConnectionStatus.None)
			{
				this.SetStatus(NetConnectionStatus.Disconnecting, byeMessage);
			}
			this.m_handshakeAttempts = 0;
			this.m_disconnectRequested = true;
		}

		private const int c_protocolMaxMTU = 8190;

		private const int m_infrequentEventsSkipFrames = 8;

		private const int m_messageCoalesceFrames = 3;

		public const string sConnectionTimeoutErrorString = "Failed to establish connection - no response from remote host";

		private NetConnection.ExpandMTUStatus m_expandMTUStatus;

		private int m_largestSuccessfulMTU;

		private int m_smallestFailedMTU;

		private int m_lastSentMTUAttemptSize;

		private double m_lastSentMTUAttemptTime;

		private int m_mtuAttemptFails;

		internal int m_currentMTU;

		private float m_sentPingTime;

		private int m_sentPingNumber;

		private float m_averageRoundtripTime;

		private float m_timeoutDeadline = float.MaxValue;

		internal double m_remoteTimeOffset;

		internal NetPeer m_peer;

		internal NetPeerConfiguration m_peerConfiguration;

		internal NetConnectionStatus m_status;

		internal NetConnectionStatus m_visibleStatus;

		internal IPEndPoint m_remoteEndPoint;

		internal NetSenderChannelBase[] m_sendChannels;

		internal NetReceiverChannelBase[] m_receiveChannels;

		internal NetOutgoingMessage m_localHailMessage;

		internal long m_remoteUniqueIdentifier;

		internal NetQueue<NetTuple<NetMessageType, int>> m_queuedOutgoingAcks;

		internal NetQueue<NetTuple<NetMessageType, int>> m_queuedIncomingAcks;

		private int m_sendBufferWritePtr;

		private int m_sendBufferNumMessages;

		private object m_tag;

		internal NetConnectionStatistics m_statistics;

		internal bool m_connectRequested;

		internal bool m_disconnectRequested;

		internal bool m_connectionInitiator;

		internal string m_disconnectMessage;

		internal NetIncomingMessage m_remoteHailMessage;

		internal float m_lastHandshakeSendTime;

		internal int m_handshakeAttempts;

		private enum ExpandMTUStatus
		{
			None,
			InProgress,
			Finished
		}
	}
}
