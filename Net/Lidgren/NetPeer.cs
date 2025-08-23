using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace DNA.Net.Lidgren
{
	public class NetPeer
	{
		public NetSendResult SendMessage(NetOutgoingMessage msg, NetConnection recipient, NetDeliveryMethod method)
		{
			return this.SendMessage(msg, recipient, method, 0);
		}

		public NetSendResult SendMessage(NetOutgoingMessage msg, NetConnection recipient, NetDeliveryMethod method, int sequenceChannel)
		{
			if (msg == null)
			{
				throw new ArgumentNullException("msg");
			}
			if (recipient == null)
			{
				throw new ArgumentNullException("recipient");
			}
			if (sequenceChannel >= 32)
			{
				throw new ArgumentOutOfRangeException("sequenceChannel");
			}
			if (msg.m_isSent)
			{
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			}
			msg.m_isSent = true;
			int len = 5 + msg.LengthBytes;
			if (len <= recipient.m_currentMTU)
			{
				Interlocked.Increment(ref msg.m_recyclingCount);
				return recipient.EnqueueMessage(msg, method, sequenceChannel);
			}
			if (recipient.m_status != NetConnectionStatus.Connected)
			{
				return NetSendResult.FailedNotConnected;
			}
			this.SendFragmentedMessage(msg, new NetConnection[] { recipient }, method, sequenceChannel);
			return NetSendResult.Queued;
		}

		internal int GetMTU(IList<NetConnection> recipients)
		{
			int count = recipients.Count;
			int mtu = int.MaxValue;
			for (int i = 0; i < count; i++)
			{
				NetConnection conn = recipients[i];
				int cmtu = conn.m_currentMTU;
				if (cmtu < mtu)
				{
					mtu = cmtu;
				}
			}
			return mtu;
		}

		public void SendMessage(NetOutgoingMessage msg, List<NetConnection> recipients, NetDeliveryMethod method, int sequenceChannel)
		{
			if (msg == null)
			{
				throw new ArgumentNullException("msg");
			}
			if (recipients == null)
			{
				throw new ArgumentNullException("recipients");
			}
			if (recipients.Count < 1)
			{
				throw new NetException("recipients must contain at least one item");
			}
			if (method != NetDeliveryMethod.Unreliable)
			{
			}
			if (msg.m_isSent)
			{
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			}
			int mtu = this.GetMTU(recipients);
			msg.m_isSent = true;
			int len = msg.GetEncodedSize();
			if (len <= mtu)
			{
				Interlocked.Add(ref msg.m_recyclingCount, recipients.Count);
				using (List<NetConnection>.Enumerator enumerator = recipients.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						NetConnection conn = enumerator.Current;
						if (conn == null)
						{
							Interlocked.Decrement(ref msg.m_recyclingCount);
						}
						else
						{
							NetSendResult res = conn.EnqueueMessage(msg, method, sequenceChannel);
							if (res != NetSendResult.Queued && res != NetSendResult.Sent)
							{
								Interlocked.Decrement(ref msg.m_recyclingCount);
							}
						}
					}
					return;
				}
			}
			this.SendFragmentedMessage(msg, recipients, method, sequenceChannel);
		}

		public void SendUnconnectedMessage(NetOutgoingMessage msg, string host, int port)
		{
			if (msg == null)
			{
				throw new ArgumentNullException("msg");
			}
			if (host == null)
			{
				throw new ArgumentNullException("host");
			}
			if (msg.m_isSent)
			{
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			}
			if (msg.LengthBytes > this.m_configuration.MaximumTransmissionUnit)
			{
				throw new NetException("Unconnected messages too long! Must be shorter than NetConfiguration.MaximumTransmissionUnit (currently " + this.m_configuration.MaximumTransmissionUnit + ")");
			}
			IPAddress adr = NetUtility.Resolve(host);
			if (adr == null)
			{
				throw new NetException("Failed to resolve " + host);
			}
			msg.m_messageType = NetMessageType.Unconnected;
			msg.m_isSent = true;
			Interlocked.Increment(ref msg.m_recyclingCount);
			this.m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(new IPEndPoint(adr, port), msg));
		}

		public void SendUnconnectedMessage(NetOutgoingMessage msg, IPEndPoint recipient)
		{
			if (msg == null)
			{
				throw new ArgumentNullException("msg");
			}
			if (recipient == null)
			{
				throw new ArgumentNullException("recipient");
			}
			if (msg.m_isSent)
			{
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			}
			if (msg.LengthBytes > this.m_configuration.MaximumTransmissionUnit)
			{
				throw new NetException("Unconnected messages too long! Must be shorter than NetConfiguration.MaximumTransmissionUnit (currently " + this.m_configuration.MaximumTransmissionUnit + ")");
			}
			msg.m_messageType = NetMessageType.Unconnected;
			msg.m_isSent = true;
			Interlocked.Increment(ref msg.m_recyclingCount);
			this.m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(recipient, msg));
		}

		public void SendUnconnectedMessage(NetOutgoingMessage msg, IList<IPEndPoint> recipients)
		{
			if (msg == null)
			{
				throw new ArgumentNullException("msg");
			}
			if (recipients == null)
			{
				throw new ArgumentNullException("recipients");
			}
			if (recipients.Count < 1)
			{
				throw new NetException("recipients must contain at least one item");
			}
			if (msg.m_isSent)
			{
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			}
			if (msg.LengthBytes > this.m_configuration.MaximumTransmissionUnit)
			{
				throw new NetException("Unconnected messages too long! Must be shorter than NetConfiguration.MaximumTransmissionUnit (currently " + this.m_configuration.MaximumTransmissionUnit + ")");
			}
			msg.m_messageType = NetMessageType.Unconnected;
			msg.m_isSent = true;
			Interlocked.Add(ref msg.m_recyclingCount, recipients.Count);
			foreach (IPEndPoint ep in recipients)
			{
				this.m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(ep, msg));
			}
		}

		public void SendUnconnectedToSelf(NetOutgoingMessage msg)
		{
			if (msg == null)
			{
				throw new ArgumentNullException("msg");
			}
			if (msg.m_isSent)
			{
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			}
			msg.m_messageType = NetMessageType.Unconnected;
			msg.m_isSent = true;
			if (!this.m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.UnconnectedData))
			{
				return;
			}
			NetIncomingMessage om = this.CreateIncomingMessage(NetIncomingMessageType.UnconnectedData, msg.LengthBytes);
			om.m_isFragment = false;
			om.m_receiveTime = NetTime.Now;
			om.m_senderConnection = null;
			om.m_senderEndPoint = this.m_socket.LocalEndPoint as IPEndPoint;
			om.m_bitLength = msg.LengthBits;
			this.ReleaseMessage(om);
		}

		public Socket Socket
		{
			get
			{
				return this.m_socket;
			}
		}

		public void RegisterReceivedCallback(SendOrPostCallback callback)
		{
			if (SynchronizationContext.Current == null)
			{
				throw new NetException("Need a SynchronizationContext to register callback on correct thread!");
			}
			if (this.m_receiveCallbacks == null)
			{
				this.m_receiveCallbacks = new List<NetTuple<SynchronizationContext, SendOrPostCallback>>();
			}
			this.m_receiveCallbacks.Add(new NetTuple<SynchronizationContext, SendOrPostCallback>(SynchronizationContext.Current, callback));
		}

		public void UnregisterReceivedCallback(SendOrPostCallback callback)
		{
			if (this.m_receiveCallbacks == null)
			{
				return;
			}
			this.m_receiveCallbacks.Remove(new NetTuple<SynchronizationContext, SendOrPostCallback>(SynchronizationContext.Current, callback));
			if (this.m_receiveCallbacks.Count < 1)
			{
				this.m_receiveCallbacks = null;
			}
		}

		internal void ReleaseMessage(NetIncomingMessage msg)
		{
			if (msg.m_isFragment)
			{
				this.HandleReleasedFragment(msg);
				return;
			}
			this.m_releasedIncomingMessages.Enqueue(msg);
			if (this.m_messageReceivedEvent != null)
			{
				this.m_messageReceivedEvent.Set();
			}
			if (this.m_receiveCallbacks != null)
			{
				foreach (NetTuple<SynchronizationContext, SendOrPostCallback> tuple in this.m_receiveCallbacks)
				{
					tuple.Item1.Post(tuple.Item2, this);
				}
			}
		}

		private void InitializeNetwork()
		{
			lock (this.m_initializeLock)
			{
				this.m_configuration.Lock();
				if (this.m_status != NetPeerStatus.Running)
				{
					if (this.m_configuration.m_enableUPnP)
					{
						this.m_upnp = new NetUPnP(this);
					}
					this.InitializePools();
					this.m_releasedIncomingMessages.Clear();
					this.m_unsentUnconnectedMessages.Clear();
					this.m_handshakes.Clear();
					IPEndPoint iep = new IPEndPoint(this.m_configuration.LocalAddress, this.m_configuration.Port);
					EndPoint ep = iep;
					this.m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					this.m_socket.ReceiveBufferSize = this.m_configuration.ReceiveBufferSize;
					this.m_socket.SendBufferSize = this.m_configuration.SendBufferSize;
					this.m_socket.Blocking = false;
					this.m_socket.Bind(ep);
					try
					{
						uint SIO_UDP_CONNRESET = 2550136844U;
						this.m_socket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
					}
					catch
					{
					}
					IPEndPoint boundEp = this.m_socket.LocalEndPoint as IPEndPoint;
					this.m_listenPort = boundEp.Port;
					this.m_receiveBuffer = new byte[this.m_configuration.ReceiveBufferSize];
					this.m_sendBuffer = new byte[this.m_configuration.SendBufferSize];
					this.m_readHelperMessage = new NetIncomingMessage(NetIncomingMessageType.Error);
					this.m_readHelperMessage.m_data = this.m_receiveBuffer;
					byte[] macBytes = new byte[8];
					NetRandom.Instance.NextBytes(macBytes);
					try
					{
						PhysicalAddress pa = NetUtility.GetMacAddress();
						if (pa != null)
						{
							macBytes = pa.GetAddressBytes();
						}
						else
						{
							this.LogWarning("Failed to get Mac address");
						}
					}
					catch (NotSupportedException)
					{
					}
					byte[] epBytes = BitConverter.GetBytes(boundEp.GetHashCode());
					byte[] combined = new byte[epBytes.Length + macBytes.Length];
					Array.Copy(epBytes, 0, combined, 0, epBytes.Length);
					Array.Copy(macBytes, 0, combined, epBytes.Length, macBytes.Length);
					this.m_uniqueIdentifier = BitConverter.ToInt64(SHA1.Create().ComputeHash(combined), 0);
					this.m_status = NetPeerStatus.Running;
				}
			}
		}

		private void NetworkLoop()
		{
			do
			{
				try
				{
					this.Heartbeat();
				}
				catch (Exception ex)
				{
					this.LogWarning(ex.ToString());
				}
			}
			while (this.m_status == NetPeerStatus.Running);
			this.ExecutePeerShutdown();
		}

		private void ExecutePeerShutdown()
		{
			List<NetConnection> list = new List<NetConnection>(this.m_handshakes.Count + this.m_connections.Count);
			lock (this.m_connections)
			{
				foreach (NetConnection conn in this.m_connections)
				{
					if (conn != null)
					{
						list.Add(conn);
					}
				}
				lock (this.m_handshakes)
				{
					foreach (NetConnection hs in this.m_handshakes.Values)
					{
						if (hs != null)
						{
							list.Add(hs);
						}
					}
					foreach (NetConnection conn2 in list)
					{
						conn2.Shutdown(this.m_shutdownReason);
					}
				}
			}
			this.FlushDelayedPackets();
			this.Heartbeat();
			Thread.Sleep(10);
			lock (this.m_initializeLock)
			{
				try
				{
					if (this.m_socket != null)
					{
						try
						{
							this.m_socket.Shutdown(SocketShutdown.Receive);
						}
						catch
						{
						}
						this.m_socket.Close(2);
					}
					if (this.m_messageReceivedEvent != null)
					{
						this.m_messageReceivedEvent.Set();
						this.m_messageReceivedEvent.Close();
						this.m_messageReceivedEvent = null;
					}
				}
				finally
				{
					this.m_socket = null;
					this.m_status = NetPeerStatus.NotRunning;
				}
				this.m_receiveBuffer = null;
				this.m_sendBuffer = null;
				this.m_unsentUnconnectedMessages.Clear();
				this.m_connections.Clear();
				this.m_connectionLookup.Clear();
				this.m_handshakes.Clear();
			}
		}

		private void Heartbeat()
		{
			double dnow = NetTime.Now;
			float now = (float)dnow;
			double delta = dnow - this.m_lastHeartbeat;
			int maxCHBpS = 1250 - this.m_connections.Count;
			if (maxCHBpS < 250)
			{
				maxCHBpS = 250;
			}
			if (delta > 1.0 / (double)maxCHBpS || delta < 0.0)
			{
				this.m_frameCounter += 1U;
				this.m_lastHeartbeat = dnow;
				if (this.m_frameCounter % 3U == 0U)
				{
					foreach (KeyValuePair<IPEndPoint, NetConnection> kvp in this.m_handshakes)
					{
						NetConnection conn = kvp.Value;
						conn.UnconnectedHeartbeat(now);
						if (conn.m_status == NetConnectionStatus.Connected || conn.m_status == NetConnectionStatus.Disconnected)
						{
							break;
						}
					}
				}
				if (this.m_configuration.m_autoFlushSendQueue)
				{
					this.m_executeFlushSendQueue = true;
				}
				lock (this.m_connections)
				{
					foreach (NetConnection conn2 in this.m_connections)
					{
						conn2.Heartbeat(now, this.m_frameCounter);
						if (conn2.m_status == NetConnectionStatus.Disconnected)
						{
							this.m_connections.Remove(conn2);
							this.m_connectionLookup.Remove(conn2.RemoteEndPoint);
							break;
						}
					}
				}
				this.m_executeFlushSendQueue = false;
				NetTuple<IPEndPoint, NetOutgoingMessage> unsent;
				while (this.m_unsentUnconnectedMessages.TryDequeue(out unsent))
				{
					NetOutgoingMessage om = unsent.Item2;
					int len = om.Encode(this.m_sendBuffer, 0, 0);
					bool connReset;
					this.SendPacket(len, unsent.Item1, 1, out connReset);
					Interlocked.Decrement(ref om.m_recyclingCount);
					if (om.m_recyclingCount <= 0)
					{
						this.Recycle(om);
					}
				}
			}
			if (this.m_socket == null)
			{
				return;
			}
			if (!this.m_socket.Poll(1000, SelectMode.SelectRead))
			{
				return;
			}
			dnow = NetTime.Now;
			now = (float)dnow;
			int bytesReceived;
			int ptr;
			int payloadByteLength;
			for (;;)
			{
				bytesReceived = 0;
				try
				{
					bytesReceived = this.m_socket.ReceiveFrom(this.m_receiveBuffer, 0, this.m_receiveBuffer.Length, SocketFlags.None, ref this.m_senderRemote);
				}
				catch (SocketException sx)
				{
					if (sx.SocketErrorCode == SocketError.ConnectionReset)
					{
						this.LogWarning("ConnectionReset");
						break;
					}
					this.LogWarning(sx.ToString());
					break;
				}
				if (bytesReceived < 5)
				{
					break;
				}
				IPEndPoint ipsender = (IPEndPoint)this.m_senderRemote;
				if (this.m_upnp != null && now < this.m_upnp.m_discoveryResponseDeadline)
				{
					try
					{
						string resp = Encoding.ASCII.GetString(this.m_receiveBuffer, 0, bytesReceived);
						if (resp.Contains("upnp:rootdevice") || resp.Contains("UPnP/1.0"))
						{
							resp = resp.Substring(resp.ToLower().IndexOf("location:") + 9);
							resp = resp.Substring(0, resp.IndexOf("\r")).Trim();
							this.m_upnp.ExtractServiceUrl(resp);
							break;
						}
					}
					catch
					{
					}
				}
				NetConnection sender = null;
				this.m_connectionLookup.TryGetValue(ipsender, out sender);
				int numMessages = 0;
				ptr = 0;
				while (bytesReceived - ptr >= 5)
				{
					numMessages++;
					NetMessageType tp = (NetMessageType)this.m_receiveBuffer[ptr++];
					byte low = this.m_receiveBuffer[ptr++];
					byte high = this.m_receiveBuffer[ptr++];
					bool isFragment = (low & 1) == 1;
					ushort sequenceNumber = (ushort)((low >> 1) | ((int)high << 7));
					ushort payloadBitLength = (ushort)((int)this.m_receiveBuffer[ptr++] | ((int)this.m_receiveBuffer[ptr++] << 8));
					payloadByteLength = NetUtility.BytesToHoldBits((int)payloadBitLength);
					if (bytesReceived - ptr < payloadByteLength)
					{
						goto Block_13;
					}
					try
					{
						if (tp >= NetMessageType.LibraryError)
						{
							if (sender != null)
							{
								sender.ReceivedLibraryMessage(tp, ptr, payloadByteLength);
							}
							else
							{
								this.ReceivedUnconnectedLibraryMessage(dnow, ipsender, tp, ptr, payloadByteLength);
							}
						}
						else
						{
							if (sender == null && !this.m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.UnconnectedData))
							{
								return;
							}
							NetIncomingMessage msg = this.CreateIncomingMessage(NetIncomingMessageType.Data, payloadByteLength);
							msg.m_isFragment = isFragment;
							msg.m_receiveTime = dnow;
							msg.m_sequenceNumber = (int)sequenceNumber;
							msg.m_receivedMessageType = tp;
							msg.m_senderConnection = sender;
							msg.m_senderEndPoint = ipsender;
							msg.m_bitLength = (int)payloadBitLength;
							Buffer.BlockCopy(this.m_receiveBuffer, ptr, msg.m_data, 0, payloadByteLength);
							if (sender != null)
							{
								if (tp == NetMessageType.Unconnected)
								{
									msg.m_incomingMessageType = NetIncomingMessageType.UnconnectedData;
									this.ReleaseMessage(msg);
								}
								else
								{
									sender.ReceivedMessage(msg);
								}
							}
							else
							{
								msg.m_incomingMessageType = NetIncomingMessageType.UnconnectedData;
								this.ReleaseMessage(msg);
							}
						}
					}
					catch (Exception ex)
					{
						this.LogError(string.Concat(new object[] { "Packet parsing error: ", ex.Message, " from ", ipsender }));
					}
					ptr += payloadByteLength;
				}
				if (this.m_socket.Available <= 0)
				{
					return;
				}
			}
			return;
			Block_13:
			this.LogWarning(string.Concat(new object[]
			{
				"Malformed packet; stated payload length ",
				payloadByteLength,
				", remaining bytes ",
				bytesReceived - ptr
			}));
		}

		public void FlushSendQueue()
		{
			this.m_executeFlushSendQueue = true;
		}

		internal void HandleIncomingDiscoveryRequest(double now, IPEndPoint senderEndPoint, int ptr, int payloadByteLength)
		{
			if (this.m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.DiscoveryRequest))
			{
				NetIncomingMessage dm = this.CreateIncomingMessage(NetIncomingMessageType.DiscoveryRequest, payloadByteLength);
				if (payloadByteLength > 0)
				{
					Buffer.BlockCopy(this.m_receiveBuffer, ptr, dm.m_data, 0, payloadByteLength);
				}
				dm.m_receiveTime = now;
				dm.m_bitLength = payloadByteLength * 8;
				dm.m_senderEndPoint = senderEndPoint;
				this.ReleaseMessage(dm);
			}
		}

		internal void HandleIncomingDiscoveryResponse(double now, IPEndPoint senderEndPoint, int ptr, int payloadByteLength)
		{
			if (this.m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.DiscoveryResponse))
			{
				NetIncomingMessage dr = this.CreateIncomingMessage(NetIncomingMessageType.DiscoveryResponse, payloadByteLength);
				if (payloadByteLength > 0)
				{
					Buffer.BlockCopy(this.m_receiveBuffer, ptr, dr.m_data, 0, payloadByteLength);
				}
				dr.m_receiveTime = now;
				dr.m_bitLength = payloadByteLength * 8;
				dr.m_senderEndPoint = senderEndPoint;
				this.ReleaseMessage(dr);
			}
		}

		private void ReceivedUnconnectedLibraryMessage(double now, IPEndPoint senderEndPoint, NetMessageType tp, int ptr, int payloadByteLength)
		{
			NetConnection shake;
			if (this.m_handshakes.TryGetValue(senderEndPoint, out shake))
			{
				shake.ReceivedHandshake(now, tp, ptr, payloadByteLength);
				return;
			}
			switch (tp)
			{
			case NetMessageType.Connect:
			{
				int reservedSlots = this.m_handshakes.Count + this.m_connections.Count;
				if (reservedSlots >= this.m_configuration.m_maximumConnections)
				{
					NetOutgoingMessage full = this.CreateMessage("Server full");
					full.m_messageType = NetMessageType.Disconnect;
					this.SendLibrary(full, senderEndPoint);
					return;
				}
				NetConnection conn = new NetConnection(this, senderEndPoint);
				conn.m_status = NetConnectionStatus.ReceivedInitiation;
				this.m_handshakes.Add(senderEndPoint, conn);
				conn.ReceivedHandshake(now, tp, ptr, payloadByteLength);
				return;
			}
			case NetMessageType.ConnectResponse:
				lock (this.m_handshakes)
				{
					foreach (KeyValuePair<IPEndPoint, NetConnection> hs in this.m_handshakes)
					{
						if (hs.Key.Address.Equals(senderEndPoint.Address) && hs.Value.m_connectionInitiator)
						{
							NetConnection hsconn = hs.Value;
							this.m_connectionLookup.Remove(hs.Key);
							this.m_handshakes.Remove(hs.Key);
							hsconn.MutateEndPoint(senderEndPoint);
							this.m_connectionLookup.Add(senderEndPoint, hsconn);
							this.m_handshakes.Add(senderEndPoint, hsconn);
							hsconn.ReceivedHandshake(now, tp, ptr, payloadByteLength);
							return;
						}
					}
				}
				this.LogWarning(string.Concat(new object[] { "Received unhandled library message ", tp, " from ", senderEndPoint }));
				break;
			case NetMessageType.ConnectionEstablished:
			case NetMessageType.Acknowledge:
				goto IL_0193;
			case NetMessageType.Disconnect:
				break;
			case NetMessageType.Discovery:
				this.HandleIncomingDiscoveryRequest(now, senderEndPoint, ptr, payloadByteLength);
				return;
			case NetMessageType.DiscoveryResponse:
				this.HandleIncomingDiscoveryResponse(now, senderEndPoint, ptr, payloadByteLength);
				return;
			case NetMessageType.NatPunchMessage:
				this.HandleNatPunch(ptr, senderEndPoint);
				return;
			case NetMessageType.NatIntroduction:
				this.HandleNatIntroduction(ptr);
				return;
			default:
				goto IL_0193;
			}
			return;
			IL_0193:
			this.LogWarning(string.Concat(new object[] { "Received unhandled library message ", tp, " from ", senderEndPoint }));
		}

		internal void AcceptConnection(NetConnection conn)
		{
			conn.InitExpandMTU(NetTime.Now);
			if (!this.m_handshakes.Remove(conn.m_remoteEndPoint))
			{
				this.LogWarning("AcceptConnection called but m_handshakes did not contain it!");
			}
			lock (this.m_connections)
			{
				if (this.m_connections.Contains(conn))
				{
					this.LogWarning("AcceptConnection called but m_connection already contains it!");
				}
				else
				{
					this.m_connections.Add(conn);
					this.m_connectionLookup.Add(conn.m_remoteEndPoint, conn);
				}
			}
		}

		[Conditional("DEBUG")]
		internal void VerifyNetworkThread()
		{
			Thread ct = Thread.CurrentThread;
			if (Thread.CurrentThread != this.m_networkThread)
			{
				throw new NetException(string.Concat(new object[] { "Executing on wrong thread! Should be library system thread (is ", ct.Name, " mId ", ct.ManagedThreadId, ")" }));
			}
		}

		internal NetIncomingMessage SetupReadHelperMessage(int ptr, int payloadLength)
		{
			this.m_readHelperMessage.m_bitLength = (ptr + payloadLength) * 8;
			this.m_readHelperMessage.m_readPosition = ptr * 8;
			return this.m_readHelperMessage;
		}

		public NetPeerStatus Status
		{
			get
			{
				return this.m_status;
			}
		}

		public AutoResetEvent MessageReceivedEvent
		{
			get
			{
				if (this.m_messageReceivedEvent == null)
				{
					this.m_messageReceivedEvent = new AutoResetEvent(false);
				}
				return this.m_messageReceivedEvent;
			}
		}

		public long UniqueIdentifier
		{
			get
			{
				return this.m_uniqueIdentifier;
			}
		}

		public int Port
		{
			get
			{
				return this.m_listenPort;
			}
		}

		public NetUPnP UPnP
		{
			get
			{
				return this.m_upnp;
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

		public List<NetConnection> Connections
		{
			get
			{
				List<NetConnection> list;
				lock (this.m_connections)
				{
					list = new List<NetConnection>(this.m_connections);
				}
				return list;
			}
		}

		public int ConnectionsCount
		{
			get
			{
				return this.m_connections.Count;
			}
		}

		public NetPeerStatistics Statistics
		{
			get
			{
				return this.m_statistics;
			}
		}

		public NetPeerConfiguration Configuration
		{
			get
			{
				return this.m_configuration;
			}
		}

		public NetPeer(NetPeerConfiguration config)
		{
			this.m_configuration = config;
			this.m_statistics = new NetPeerStatistics(this);
			this.m_releasedIncomingMessages = new NetQueue<NetIncomingMessage>(4);
			this.m_unsentUnconnectedMessages = new NetQueue<NetTuple<IPEndPoint, NetOutgoingMessage>>(2);
			this.m_connections = new List<NetConnection>();
			this.m_connectionLookup = new Dictionary<IPEndPoint, NetConnection>();
			this.m_handshakes = new Dictionary<IPEndPoint, NetConnection>();
			this.m_senderRemote = new IPEndPoint(IPAddress.Any, 0);
			this.m_status = NetPeerStatus.NotRunning;
			this.m_receivedFragmentGroups = new Dictionary<NetConnection, Dictionary<int, ReceivedFragmentGroup>>();
		}

		public void Start()
		{
			if (this.m_status != NetPeerStatus.NotRunning)
			{
				this.LogWarning("Start() called on already running NetPeer - ignoring.");
				return;
			}
			this.m_status = NetPeerStatus.Starting;
			if (this.m_configuration.NetworkThreadName == "Lidgren network thread")
			{
				int pc = Interlocked.Increment(ref NetPeer.s_initializedPeersCount);
				this.m_configuration.NetworkThreadName = "Lidgren network thread " + pc.ToString();
			}
			this.InitializeNetwork();
			this.m_networkThread = new Thread(new ThreadStart(this.NetworkLoop));
			this.m_networkThread.Name = this.m_configuration.NetworkThreadName;
			this.m_networkThread.IsBackground = true;
			this.m_networkThread.Priority = ThreadPriority.BelowNormal;
			this.m_networkThread.Start();
			if (this.m_upnp != null)
			{
				this.m_upnp.Discover(this);
			}
			Thread.Sleep(50);
		}

		public NetConnection GetConnection(IPEndPoint ep)
		{
			NetConnection retval;
			this.m_connectionLookup.TryGetValue(ep, out retval);
			return retval;
		}

		public NetIncomingMessage WaitMessage(int maxMillis)
		{
			NetIncomingMessage msg = this.ReadMessage();
			if (msg != null)
			{
				return msg;
			}
			if (this.m_messageReceivedEvent != null)
			{
				this.m_messageReceivedEvent.WaitOne(maxMillis);
			}
			return this.ReadMessage();
		}

		public NetIncomingMessage ReadMessage()
		{
			NetIncomingMessage retval;
			if (this.m_releasedIncomingMessages.TryDequeue(out retval) && retval.MessageType == NetIncomingMessageType.StatusChanged)
			{
				NetConnectionStatus status = (NetConnectionStatus)retval.PeekByte();
				retval.SenderConnection.m_visibleStatus = status;
			}
			return retval;
		}

		public int ReadMessages(IList<NetIncomingMessage> addTo)
		{
			int added = this.m_releasedIncomingMessages.TryDrain(addTo);
			if (added > 0)
			{
				for (int i = 0; i < added; i++)
				{
					int index = addTo.Count - added + i;
					NetIncomingMessage nim = addTo[index];
					if (nim.MessageType == NetIncomingMessageType.StatusChanged)
					{
						NetConnectionStatus status = (NetConnectionStatus)nim.PeekByte();
						nim.SenderConnection.m_visibleStatus = status;
					}
				}
			}
			return added;
		}

		internal void SendLibrary(NetOutgoingMessage msg, IPEndPoint recipient)
		{
			int len = msg.Encode(this.m_sendBuffer, 0, 0);
			bool connReset;
			this.SendPacket(len, recipient, 1, out connReset);
		}

		public NetConnection Connect(string host, int port)
		{
			return this.Connect(new IPEndPoint(NetUtility.Resolve(host), port), null);
		}

		public NetConnection Connect(string host, int port, NetOutgoingMessage hailMessage)
		{
			return this.Connect(new IPEndPoint(NetUtility.Resolve(host), port), hailMessage);
		}

		public NetConnection Connect(IPEndPoint remoteEndPoint)
		{
			return this.Connect(remoteEndPoint, null);
		}

		public virtual NetConnection Connect(IPEndPoint remoteEndPoint, NetOutgoingMessage hailMessage)
		{
			if (remoteEndPoint == null)
			{
				throw new ArgumentNullException("remoteEndPoint");
			}
			NetConnection netConnection;
			lock (this.m_connections)
			{
				if (this.m_status == NetPeerStatus.NotRunning)
				{
					throw new NetException("Must call Start() first");
				}
				if (this.m_connectionLookup.ContainsKey(remoteEndPoint))
				{
					throw new NetException("Already connected to that endpoint!");
				}
				NetConnection hs;
				if (this.m_handshakes.TryGetValue(remoteEndPoint, out hs))
				{
					NetConnectionStatus status = hs.m_status;
					if (status != NetConnectionStatus.InitiatedConnect)
					{
						if (status != NetConnectionStatus.RespondedConnect)
						{
							this.LogWarning("Weird situation; Connect() already in progress to remote endpoint; but hs status is " + hs.m_status);
						}
						else
						{
							hs.SendConnectResponse((float)NetTime.Now, false);
						}
					}
					else
					{
						hs.m_connectRequested = true;
					}
					netConnection = hs;
				}
				else
				{
					NetConnection conn = new NetConnection(this, remoteEndPoint);
					conn.m_status = NetConnectionStatus.InitiatedConnect;
					conn.m_localHailMessage = hailMessage;
					conn.m_connectRequested = true;
					conn.m_connectionInitiator = true;
					this.m_handshakes.Add(remoteEndPoint, conn);
					netConnection = conn;
				}
			}
			return netConnection;
		}

		internal void RawSend(byte[] arr, int offset, int length, IPEndPoint destination)
		{
			Array.Copy(arr, offset, this.m_sendBuffer, 0, length);
			bool unused;
			this.SendPacket(length, destination, 1, out unused);
		}

		public void Shutdown(string bye)
		{
			if (this.m_socket == null)
			{
				return;
			}
			this.m_shutdownReason = bye;
			this.m_status = NetPeerStatus.ShutdownRequested;
		}

		private void SendFragmentedMessage(NetOutgoingMessage msg, IList<NetConnection> recipients, NetDeliveryMethod method, int sequenceChannel)
		{
			int group = Interlocked.Increment(ref this.m_lastUsedFragmentGroup);
			if (group >= 65534)
			{
				this.m_lastUsedFragmentGroup = 1;
				group = 1;
			}
			msg.m_fragmentGroup = group;
			int totalBytes = msg.LengthBytes;
			int mtu = this.GetMTU(recipients);
			int bytesPerChunk = NetFragmentationHelper.GetBestChunkSize(group, totalBytes, mtu);
			int numChunks = totalBytes / bytesPerChunk;
			if (numChunks * bytesPerChunk < totalBytes)
			{
				numChunks++;
			}
			int bitsPerChunk = bytesPerChunk * 8;
			int bitsLeft = msg.LengthBits;
			for (int i = 0; i < numChunks; i++)
			{
				NetOutgoingMessage chunk = this.CreateMessage(mtu);
				chunk.m_bitLength = ((bitsLeft > bitsPerChunk) ? bitsPerChunk : bitsLeft);
				chunk.m_data = msg.m_data;
				chunk.m_fragmentGroup = group;
				chunk.m_fragmentGroupTotalBits = totalBytes * 8;
				chunk.m_fragmentChunkByteSize = bytesPerChunk;
				chunk.m_fragmentChunkNumber = i;
				Interlocked.Add(ref chunk.m_recyclingCount, recipients.Count);
				foreach (NetConnection recipient in recipients)
				{
					recipient.EnqueueMessage(chunk, method, sequenceChannel);
				}
				bitsLeft -= bitsPerChunk;
			}
		}

		private void HandleReleasedFragment(NetIncomingMessage im)
		{
			int group;
			int totalBits;
			int chunkByteSize;
			int chunkNumber;
			int ptr = NetFragmentationHelper.ReadHeader(im.m_data, 0, out group, out totalBits, out chunkByteSize, out chunkNumber);
			int totalBytes = NetUtility.BytesToHoldBits(totalBits);
			int totalNumChunks = totalBytes / chunkByteSize;
			if (totalNumChunks * chunkByteSize < totalBytes)
			{
				totalNumChunks++;
			}
			if (chunkNumber >= totalNumChunks)
			{
				this.LogWarning(string.Concat(new object[] { "Index out of bounds for chunk ", chunkNumber, " (total chunks ", totalNumChunks, ")" }));
				return;
			}
			Dictionary<int, ReceivedFragmentGroup> groups;
			if (!this.m_receivedFragmentGroups.TryGetValue(im.SenderConnection, out groups))
			{
				groups = new Dictionary<int, ReceivedFragmentGroup>();
				this.m_receivedFragmentGroups[im.SenderConnection] = groups;
			}
			ReceivedFragmentGroup info;
			if (!groups.TryGetValue(group, out info))
			{
				info = new ReceivedFragmentGroup();
				info.Data = new byte[totalBytes];
				info.ReceivedChunks = new NetBitVector(totalNumChunks);
				groups[group] = info;
			}
			info.ReceivedChunks[chunkNumber] = true;
			info.LastReceived = (float)NetTime.Now;
			int offset = chunkNumber * chunkByteSize;
			Buffer.BlockCopy(im.m_data, ptr, info.Data, offset, im.LengthBytes - ptr);
			info.ReceivedChunks.Count();
			if (info.ReceivedChunks.Count() == totalNumChunks)
			{
				im.m_data = info.Data;
				im.m_bitLength = totalBits;
				im.m_isFragment = false;
				groups.Remove(group);
				this.ReleaseMessage(im);
				return;
			}
			this.Recycle(im);
		}

		public void DiscoverLocalPeers(int serverPort)
		{
			NetOutgoingMessage om = this.CreateMessage(0);
			om.m_messageType = NetMessageType.Discovery;
			this.m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(new IPEndPoint(IPAddress.Broadcast, serverPort), om));
		}

		public bool DiscoverKnownPeer(string host, int serverPort)
		{
			IPAddress address = NetUtility.Resolve(host);
			if (address == null)
			{
				return false;
			}
			this.DiscoverKnownPeer(new IPEndPoint(address, serverPort));
			return true;
		}

		public void DiscoverKnownPeer(IPEndPoint endPoint)
		{
			NetOutgoingMessage om = this.CreateMessage(0);
			om.m_messageType = NetMessageType.Discovery;
			this.m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(endPoint, om));
		}

		public void DiscoverKnownPeer(IPEndPoint endPoint, NetOutgoingMessage msg)
		{
			msg.m_messageType = NetMessageType.Discovery;
			this.m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(endPoint, msg));
		}

		public void SendDiscoveryResponse(NetOutgoingMessage msg, IPEndPoint recipient)
		{
			if (recipient == null)
			{
				throw new ArgumentNullException("recipient");
			}
			if (msg == null)
			{
				msg = this.CreateMessage(0);
			}
			else if (msg.m_isSent)
			{
				throw new NetException("Message has already been sent!");
			}
			if (msg.LengthBytes >= this.m_configuration.MaximumTransmissionUnit)
			{
				throw new NetException("Cannot send discovery message larger than MTU (currently " + this.m_configuration.MaximumTransmissionUnit + " bytes)");
			}
			msg.m_messageType = NetMessageType.DiscoveryResponse;
			this.m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(recipient, msg));
		}

		private void InitializePools()
		{
			if (this.m_configuration.UseMessageRecycling)
			{
				this.m_storagePool = new List<byte[]>(16);
				this.m_outgoingMessagesPool = new NetQueue<NetOutgoingMessage>(4);
				this.m_incomingMessagesPool = new NetQueue<NetIncomingMessage>(4);
				return;
			}
			this.m_storagePool = null;
			this.m_outgoingMessagesPool = null;
			this.m_incomingMessagesPool = null;
		}

		internal byte[] GetStorage(int minimumCapacityInBytes)
		{
			if (this.m_storagePool == null)
			{
				return new byte[minimumCapacityInBytes];
			}
			lock (this.m_storagePool)
			{
				for (int i = 0; i < this.m_storagePool.Count; i++)
				{
					byte[] retval = this.m_storagePool[i];
					if (retval != null && retval.Length >= minimumCapacityInBytes)
					{
						this.m_storagePool[i] = null;
						this.m_storagePoolBytes -= retval.Length;
						return retval;
					}
				}
			}
			this.m_statistics.m_bytesAllocated += (long)minimumCapacityInBytes;
			return new byte[minimumCapacityInBytes];
		}

		internal void Recycle(byte[] storage)
		{
			if (this.m_storagePool == null)
			{
				return;
			}
			lock (this.m_storagePool)
			{
				this.m_storagePoolBytes += storage.Length;
				int cnt = this.m_storagePool.Count;
				for (int i = 0; i < cnt; i++)
				{
					if (this.m_storagePool[i] == null)
					{
						this.m_storagePool[i] = storage;
						return;
					}
				}
				this.m_storagePool.Add(storage);
			}
		}

		public NetOutgoingMessage CreateMessage()
		{
			return this.CreateMessage(this.m_configuration.m_defaultOutgoingMessageCapacity);
		}

		public NetOutgoingMessage CreateMessage(string content)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			NetOutgoingMessage om = this.CreateMessage(2 + bytes.Length);
			om.WriteVariableUInt32((uint)bytes.Length);
			om.Write(bytes);
			return om;
		}

		public NetOutgoingMessage CreateMessage(int initialCapacity)
		{
			NetOutgoingMessage retval;
			if (this.m_outgoingMessagesPool == null || !this.m_outgoingMessagesPool.TryDequeue(out retval))
			{
				retval = new NetOutgoingMessage();
			}
			byte[] storage = this.GetStorage(initialCapacity);
			retval.m_data = storage;
			return retval;
		}

		internal NetIncomingMessage CreateIncomingMessage(NetIncomingMessageType tp, byte[] useStorageData)
		{
			NetIncomingMessage retval;
			if (this.m_incomingMessagesPool == null || !this.m_incomingMessagesPool.TryDequeue(out retval))
			{
				retval = new NetIncomingMessage(tp);
			}
			else
			{
				retval.m_incomingMessageType = tp;
			}
			retval.m_data = useStorageData;
			return retval;
		}

		internal NetIncomingMessage CreateIncomingMessage(NetIncomingMessageType tp, int minimumByteSize)
		{
			NetIncomingMessage retval;
			if (this.m_incomingMessagesPool == null || !this.m_incomingMessagesPool.TryDequeue(out retval))
			{
				retval = new NetIncomingMessage(tp);
			}
			else
			{
				retval.m_incomingMessageType = tp;
			}
			retval.m_data = this.GetStorage(minimumByteSize);
			return retval;
		}

		public void Recycle(NetIncomingMessage msg)
		{
			if (this.m_incomingMessagesPool == null)
			{
				return;
			}
			byte[] storage = msg.m_data;
			msg.m_data = null;
			this.Recycle(storage);
			msg.Reset();
			this.m_incomingMessagesPool.Enqueue(msg);
		}

		public void Recycle(IEnumerable<NetIncomingMessage> toRecycle)
		{
			if (this.m_incomingMessagesPool == null)
			{
				return;
			}
			if (this.m_storagePool != null)
			{
				lock (this.m_storagePool)
				{
					foreach (NetIncomingMessage msg in toRecycle)
					{
						byte[] storage = msg.m_data;
						msg.m_data = null;
						this.m_storagePoolBytes += storage.Length;
						int cnt = this.m_storagePool.Count;
						for (int i = 0; i < cnt; i++)
						{
							if (this.m_storagePool[i] == null)
							{
								this.m_storagePool[i] = storage;
								return;
							}
						}
						msg.Reset();
						this.m_storagePool.Add(storage);
					}
				}
			}
			this.m_incomingMessagesPool.Enqueue(toRecycle);
		}

		internal void Recycle(NetOutgoingMessage msg)
		{
			if (this.m_outgoingMessagesPool == null)
			{
				return;
			}
			byte[] storage = msg.m_data;
			msg.m_data = null;
			if (msg.m_fragmentGroup == 0)
			{
				this.Recycle(storage);
			}
			msg.Reset();
			this.m_outgoingMessagesPool.Enqueue(msg);
		}

		internal NetIncomingMessage CreateIncomingMessage(NetIncomingMessageType tp, string text)
		{
			NetIncomingMessage retval;
			if (string.IsNullOrEmpty(text))
			{
				retval = this.CreateIncomingMessage(tp, 1);
				retval.Write(string.Empty);
				return retval;
			}
			int numBytes = Encoding.UTF8.GetByteCount(text);
			retval = this.CreateIncomingMessage(tp, numBytes + ((numBytes > 127) ? 2 : 1));
			retval.Write(text);
			return retval;
		}

		internal bool SendMTUPacket(int numBytes, IPEndPoint target)
		{
			try
			{
				this.m_socket.DontFragment = true;
				int bytesSent = this.m_socket.SendTo(this.m_sendBuffer, 0, numBytes, SocketFlags.None, target);
				if (numBytes != bytesSent)
				{
					this.LogWarning(string.Concat(new object[] { "Failed to send the full ", numBytes, "; only ", bytesSent, " bytes sent in packet!" }));
				}
			}
			catch (SocketException sx)
			{
				if (sx.SocketErrorCode == SocketError.MessageSize)
				{
					return false;
				}
				if (sx.SocketErrorCode == SocketError.WouldBlock)
				{
					this.LogWarning("Socket threw exception; would block - send buffer full? Increase in NetPeerConfiguration");
					return true;
				}
				if (sx.SocketErrorCode == SocketError.ConnectionReset)
				{
					return true;
				}
				this.LogError(string.Concat(new object[] { "Failed to send packet: (", sx.SocketErrorCode, ") ", sx }));
			}
			catch (Exception ex)
			{
				this.LogError("Failed to send packet: " + ex);
			}
			finally
			{
				this.m_socket.DontFragment = false;
			}
			return true;
		}

		internal void SendPacket(int numBytes, IPEndPoint target, int numMessages, out bool connectionReset)
		{
			connectionReset = false;
			try
			{
				if (target.Address == IPAddress.Broadcast)
				{
					this.m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
				}
				int bytesSent = this.m_socket.SendTo(this.m_sendBuffer, 0, numBytes, SocketFlags.None, target);
				if (numBytes != bytesSent)
				{
					this.LogWarning(string.Concat(new object[] { "Failed to send the full ", numBytes, "; only ", bytesSent, " bytes sent in packet!" }));
				}
			}
			catch (SocketException sx)
			{
				if (sx.SocketErrorCode == SocketError.WouldBlock)
				{
					this.LogWarning("Socket threw exception; would block - send buffer full? Increase in NetPeerConfiguration");
				}
				else if (sx.SocketErrorCode == SocketError.ConnectionReset)
				{
					connectionReset = true;
				}
				else
				{
					this.LogError("Failed to send packet: " + sx);
				}
			}
			catch (Exception ex)
			{
				this.LogError("Failed to send packet: " + ex);
			}
			finally
			{
				if (target.Address == IPAddress.Broadcast)
				{
					this.m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, false);
				}
			}
		}

		private void FlushDelayedPackets()
		{
		}

		private void SendCallBack(IAsyncResult res)
		{
			this.m_socket.EndSendTo(res);
		}

		public void Introduce(IPEndPoint hostInternal, IPEndPoint hostExternal, IPEndPoint clientInternal, IPEndPoint clientExternal, string token)
		{
			NetOutgoingMessage msg = this.CreateMessage(10 + token.Length + 1);
			msg.m_messageType = NetMessageType.NatIntroduction;
			msg.Write(0);
			msg.Write(hostInternal);
			msg.Write(hostExternal);
			msg.Write(token);
			this.m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(clientExternal, msg));
			msg = this.CreateMessage(10 + token.Length + 1);
			msg.m_messageType = NetMessageType.NatIntroduction;
			msg.Write(1);
			msg.Write(clientInternal);
			msg.Write(clientExternal);
			msg.Write(token);
			this.m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(hostExternal, msg));
		}

		internal void HandleNatIntroduction(int ptr)
		{
			NetIncomingMessage tmp = this.SetupReadHelperMessage(ptr, 1000);
			byte hostByte = tmp.ReadByte();
			IPEndPoint remoteInternal = tmp.ReadIPEndPoint();
			IPEndPoint remoteExternal = tmp.ReadIPEndPoint();
			string token = tmp.ReadString();
			if (hostByte == 0 && !this.m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.NatIntroductionSuccess))
			{
				return;
			}
			NetOutgoingMessage punch = this.CreateMessage(1);
			punch.m_messageType = NetMessageType.NatPunchMessage;
			punch.Write(hostByte);
			punch.Write(token);
			this.m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(remoteInternal, punch));
			punch = this.CreateMessage(1);
			punch.m_messageType = NetMessageType.NatPunchMessage;
			punch.Write(hostByte);
			punch.Write(token);
			this.m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(remoteExternal, punch));
		}

		private void HandleNatPunch(int ptr, IPEndPoint senderEndPoint)
		{
			NetIncomingMessage tmp = this.SetupReadHelperMessage(ptr, 1000);
			if (tmp.ReadByte() == 0)
			{
				return;
			}
			string token = tmp.ReadString();
			NetIncomingMessage punchSuccess = this.CreateIncomingMessage(NetIncomingMessageType.NatIntroductionSuccess, 10);
			punchSuccess.m_senderEndPoint = senderEndPoint;
			punchSuccess.Write(token);
			this.ReleaseMessage(punchSuccess);
			NetOutgoingMessage punch = this.CreateMessage(1);
			punch.m_messageType = NetMessageType.NatPunchMessage;
			punch.Write(0);
			punch.Write(token);
			this.m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(senderEndPoint, punch));
		}

		[Conditional("DEBUG")]
		internal void LogVerbose(string message)
		{
			if (this.m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.VerboseDebugMessage))
			{
				this.ReleaseMessage(this.CreateIncomingMessage(NetIncomingMessageType.VerboseDebugMessage, message));
			}
		}

		[Conditional("DEBUG")]
		internal void LogDebug(string message)
		{
			if (this.m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.DebugMessage))
			{
				this.ReleaseMessage(this.CreateIncomingMessage(NetIncomingMessageType.DebugMessage, message));
			}
		}

		internal void LogWarning(string message)
		{
			if (this.m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.WarningMessage))
			{
				this.ReleaseMessage(this.CreateIncomingMessage(NetIncomingMessageType.WarningMessage, message));
			}
		}

		internal void LogError(string message)
		{
			if (this.m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.ErrorMessage))
			{
				this.ReleaseMessage(this.CreateIncomingMessage(NetIncomingMessageType.ErrorMessage, message));
			}
		}

		private NetPeerStatus m_status;

		private Thread m_networkThread;

		private Socket m_socket;

		internal byte[] m_sendBuffer;

		internal byte[] m_receiveBuffer;

		internal NetIncomingMessage m_readHelperMessage;

		private EndPoint m_senderRemote;

		private object m_initializeLock = new object();

		private uint m_frameCounter;

		private double m_lastHeartbeat;

		private NetUPnP m_upnp;

		internal readonly NetPeerConfiguration m_configuration;

		private readonly NetQueue<NetIncomingMessage> m_releasedIncomingMessages;

		internal readonly NetQueue<NetTuple<IPEndPoint, NetOutgoingMessage>> m_unsentUnconnectedMessages;

		internal Dictionary<IPEndPoint, NetConnection> m_handshakes;

		internal readonly NetPeerStatistics m_statistics;

		internal long m_uniqueIdentifier;

		internal bool m_executeFlushSendQueue;

		private AutoResetEvent m_messageReceivedEvent;

		private List<NetTuple<SynchronizationContext, SendOrPostCallback>> m_receiveCallbacks;

		private static int s_initializedPeersCount;

		private int m_listenPort;

		private object m_tag;

		internal readonly List<NetConnection> m_connections;

		private readonly Dictionary<IPEndPoint, NetConnection> m_connectionLookup;

		private string m_shutdownReason;

		private int m_lastUsedFragmentGroup;

		private Dictionary<NetConnection, Dictionary<int, ReceivedFragmentGroup>> m_receivedFragmentGroups;

		private List<byte[]> m_storagePool;

		private NetQueue<NetOutgoingMessage> m_outgoingMessagesPool;

		private NetQueue<NetIncomingMessage> m_incomingMessagesPool;

		internal int m_storagePoolBytes;
	}
}
