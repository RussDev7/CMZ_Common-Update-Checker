using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace DNA.Net.GamerServices
{
	public class HostDiscovery
	{
		public HostDiscovery(string gamename, int version, PlayerID playerID)
		{
			this._gameName = gamename;
			this._version = version;
			this._playerID = playerID;
		}

		public virtual void RemovePendingRequest(int id)
		{
			for (int i = 0; i < this._awaitingResponse.Count; i++)
			{
				if (this._awaitingResponse[i].WaitingID == id)
				{
					HostDiscovery.WaitingForResponse result = this._awaitingResponse[i];
					result.Callback = null;
					this._awaitingResponse.RemoveAt(i);
					return;
				}
			}
		}

		public virtual int GetHostInfo(ulong lobbyid, HostDiscovery.HostDiscoveryCallback callback, object context)
		{
			throw new NotImplementedException("HostDiscovery::GetHostInfo does not have a default implementation");
		}

		public virtual int GetHostInfo(IPEndPoint endpoint, HostDiscovery.HostDiscoveryCallback callback, object context)
		{
			throw new NotImplementedException("HostDiscovery::GetHostInfo does not have a default implementation");
		}

		public virtual int GetHostInfo(string nameOrIP, int port, HostDiscovery.HostDiscoveryCallback callback, object context)
		{
			throw new NotImplementedException("HostDiscovery::GetHostInfo does not have a default implementation");
		}

		public virtual int GetHostInfo(string nameOrIPIncludingPort, HostDiscovery.HostDiscoveryCallback callback, object context)
		{
			throw new NotImplementedException("HostDiscovery::GetHostInfo does not have a default implementation");
		}

		protected HostDiscovery.WaitingForResponse FindWaiterByRequestID(int rid)
		{
			lock (this._awaitingResponse)
			{
				for (int i = 0; i < this._awaitingResponse.Count; i++)
				{
					if (this._awaitingResponse[i].WaitingID == rid)
					{
						HostDiscovery.WaitingForResponse result = this._awaitingResponse[i];
						this._awaitingResponse.RemoveAt(i);
						return result;
					}
				}
			}
			return null;
		}

		public virtual void Update()
		{
			int i = 0;
			while (i < this._awaitingResponse.Count)
			{
				HostDiscovery.WaitingForResponse w = this._awaitingResponse[i];
				if (w != null && w.Timer.Elapsed.TotalSeconds > (double)this.Timeout)
				{
					this._awaitingResponse.RemoveAt(i);
					w.Callback(HostDiscovery.ResultCode.TimedOut, null, w.Context);
				}
				else
				{
					i++;
				}
			}
		}

		public virtual void Shutdown()
		{
			throw new NotImplementedException("HostDiscovery::Shutdown does not have a default implementation");
		}

		protected string _gameName;

		protected int _nextWaitingID;

		protected int _version;

		protected float Timeout = 1.5f;

		protected PlayerID _playerID;

		protected List<HostDiscovery.WaitingForResponse> _awaitingResponse = new List<HostDiscovery.WaitingForResponse>();

		public enum ResultCode
		{
			Pending = -1,
			Success,
			TimedOut,
			FailedToResolveHostName,
			HostNameInvalid,
			WrongGameName,
			ServerHasNewerVersion,
			ServerHasOlderVersion,
			VersionIsInvalid,
			GamerAlreadyConnected,
			ConnectionDenied
		}

		protected class WaitingForResponse
		{
			public Stopwatch Timer;

			public HostDiscovery.HostDiscoveryCallback Callback;

			public object Context;

			public int WaitingID;

			public IPEndPoint HostEndPoint;

			public ulong SteamLobbyID;
		}

		public delegate void HostDiscoveryCallback(HostDiscovery.ResultCode result, AvailableNetworkSession session, object context);
	}
}
