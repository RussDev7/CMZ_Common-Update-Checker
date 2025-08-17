﻿using System;

namespace DNA.Net.Lidgren
{
	internal abstract class NetReceiverChannelBase
	{
		public NetReceiverChannelBase(NetConnection connection)
		{
			this.m_connection = connection;
			this.m_peer = connection.m_peer;
		}

		internal abstract void ReceiveMessage(NetIncomingMessage msg);

		internal NetPeer m_peer;

		internal NetConnection m_connection;
	}
}
