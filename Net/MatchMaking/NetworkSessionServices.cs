﻿using System;

namespace DNA.Net.MatchMaking
{
	public abstract class NetworkSessionServices
	{
		public Guid ProductID
		{
			get
			{
				return this._productID;
			}
		}

		public int Version
		{
			get
			{
				return this._version;
			}
		}

		public NetworkSessionServices(Guid productID, int networkVersion)
		{
			this._productID = productID;
			this._version = networkVersion;
		}

		public abstract HostSessionInfo CreateNetworkSession(CreateSessionInfo sessionInfo);

		public abstract void CloseNetworkSession(HostSessionInfo hostSession);

		public abstract void UpdateHostSession(HostSessionInfo hostSession);

		public abstract void ReportSessionAlive(HostSessionInfo hostSession);

		public abstract void UpdateClientInfo(ClientSessionInfo clientSession);

		public abstract void ReportClientJoined(HostSessionInfo hostSession, string userName);

		public abstract void ReportClientLeft(HostSessionInfo hostSession, string userName);

		public abstract ClientSessionInfo[] QueryClientInfo(QuerySessionInfo queryInfo);

		private Guid _productID;

		private int _version;
	}
}
