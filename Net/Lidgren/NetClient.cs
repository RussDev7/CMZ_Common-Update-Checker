using System;
using System.Collections.Generic;
using System.Net;

namespace DNA.Net.Lidgren
{
	public class NetClient : NetPeer
	{
		public NetConnection ServerConnection
		{
			get
			{
				NetConnection retval = null;
				if (this.m_connections.Count > 0)
				{
					try
					{
						retval = this.m_connections[0];
					}
					catch
					{
						return null;
					}
					return retval;
				}
				return retval;
			}
		}

		public NetConnectionStatus ConnectionStatus
		{
			get
			{
				NetConnection conn = this.ServerConnection;
				if (conn == null)
				{
					return NetConnectionStatus.Disconnected;
				}
				return conn.Status;
			}
		}

		public NetClient(NetPeerConfiguration config)
			: base(config)
		{
			config.AcceptIncomingConnections = false;
		}

		public override NetConnection Connect(IPEndPoint remoteEndPoint, NetOutgoingMessage hailMessage)
		{
			lock (this.m_connections)
			{
				if (this.m_connections.Count > 0)
				{
					base.LogWarning("Connect attempt failed; Already connected");
					return null;
				}
			}
			lock (this.m_handshakes)
			{
				if (this.m_handshakes.Count > 0)
				{
					base.LogWarning("Connect attempt failed; Handshake already in progress");
					return null;
				}
			}
			return base.Connect(remoteEndPoint, hailMessage);
		}

		public void Disconnect(string byeMessage)
		{
			NetConnection serverConnection = this.ServerConnection;
			if (serverConnection == null)
			{
				lock (this.m_handshakes)
				{
					if (this.m_handshakes.Count > 0)
					{
						foreach (KeyValuePair<IPEndPoint, NetConnection> hs in this.m_handshakes)
						{
							hs.Value.Disconnect(byeMessage);
						}
						return;
					}
				}
				base.LogWarning("Disconnect requested when not connected!");
				return;
			}
			serverConnection.Disconnect(byeMessage);
		}

		public NetSendResult SendMessage(NetOutgoingMessage msg, NetDeliveryMethod method)
		{
			NetConnection serverConnection = this.ServerConnection;
			if (serverConnection == null)
			{
				base.LogWarning("Cannot send message, no server connection!");
				return NetSendResult.FailedNotConnected;
			}
			return serverConnection.SendMessage(msg, method, 0);
		}

		public NetSendResult SendMessage(NetOutgoingMessage msg, NetDeliveryMethod method, int sequenceChannel)
		{
			NetConnection serverConnection = this.ServerConnection;
			if (serverConnection == null)
			{
				base.LogWarning("Cannot send message, no server connection!");
				return NetSendResult.FailedNotConnected;
			}
			return serverConnection.SendMessage(msg, method, sequenceChannel);
		}

		public override string ToString()
		{
			return "[NetClient " + this.ServerConnection + "]";
		}
	}
}
