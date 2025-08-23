using System;
using System.Collections.Generic;

namespace DNA.Net.Lidgren
{
	public class NetServer : NetPeer
	{
		public NetServer(NetPeerConfiguration config)
			: base(config)
		{
			config.AcceptIncomingConnections = true;
		}

		public void SendToAll(NetOutgoingMessage msg, NetDeliveryMethod method)
		{
			List<NetConnection> all = base.Connections;
			if (all.Count <= 0)
			{
				return;
			}
			base.SendMessage(msg, all, method, 0);
		}

		public void SendToAll(NetOutgoingMessage msg, NetConnection except, NetDeliveryMethod method, int sequenceChannel)
		{
			List<NetConnection> all = base.Connections;
			if (all.Count <= 0)
			{
				return;
			}
			if (except == null)
			{
				base.SendMessage(msg, all, method, sequenceChannel);
				return;
			}
			List<NetConnection> recipients = new List<NetConnection>(all.Count - 1);
			foreach (NetConnection conn in all)
			{
				if (conn != except)
				{
					recipients.Add(conn);
				}
			}
			if (recipients.Count > 0)
			{
				base.SendMessage(msg, recipients, method, sequenceChannel);
			}
		}

		public override string ToString()
		{
			return "[NetServer " + base.ConnectionsCount + " connections]";
		}
	}
}
