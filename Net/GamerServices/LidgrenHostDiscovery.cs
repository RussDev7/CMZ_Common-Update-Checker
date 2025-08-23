using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using DNA.Net.Lidgren;

namespace DNA.Net.GamerServices
{
	internal class LidgrenHostDiscovery : HostDiscovery
	{
		public LidgrenHostDiscovery(string gamename, int version, PlayerID playerID)
			: base(gamename, version, playerID)
		{
			NetPeerConfiguration config = new NetPeerConfiguration(this._gameName + "Discovery");
			config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
			this._client = new NetClient(config);
			this._client.Start();
		}

		private void GetHostInfo(HostDiscovery.WaitingForResponse wc, IPEndPoint endpoint)
		{
			wc.HostEndPoint = endpoint;
			this._awaitingResponse.Add(wc);
			HostDiscoveryRequestMessage hdrm = new HostDiscoveryRequestMessage();
			hdrm.GameName = this._gameName;
			hdrm.RequestID = wc.WaitingID;
			hdrm.NetworkVersion = this._version;
			hdrm.PlayerID = this._playerID;
			NetOutgoingMessage msg = this._client.CreateMessage();
			msg.Write(hdrm, this._gameName, this._version);
			wc.Timer = Stopwatch.StartNew();
			this._client.DiscoverKnownPeer(endpoint, msg);
		}

		public override int GetHostInfo(IPEndPoint endpoint, HostDiscovery.HostDiscoveryCallback callback, object context)
		{
			int result = this._nextWaitingID++;
			this.GetHostInfo(new HostDiscovery.WaitingForResponse
			{
				Callback = callback,
				Context = context,
				WaitingID = result
			}, endpoint);
			return result;
		}

		public override int GetHostInfo(string nameOrIP, int port, HostDiscovery.HostDiscoveryCallback callback, object context)
		{
			int result = this._nextWaitingID++;
			HostDiscovery.WaitingForResponse wc = new HostDiscovery.WaitingForResponse();
			wc.Callback = callback;
			wc.Context = context;
			wc.WaitingID = result;
			LidgrenHostDiscovery.WaitingForHostResolution wfhr = new LidgrenHostDiscovery.WaitingForHostResolution(this._awaitingResolution, this._waitingToSubmitDiscoveryRequest);
			wfhr.ResponseWait = wc;
			lock (this._awaitingResolution)
			{
				this._awaitingResolution.Add(wfhr);
			}
			ThreadPool.QueueUserWorkItem(delegate(object state)
			{
				NetUtility.ResolveAsync(nameOrIP, port, new NetUtility.ResolveEndPointCallback(wfhr.ResolvedCallback));
			});
			return result;
		}

		public override void RemovePendingRequest(int id)
		{
			base.RemovePendingRequest(id);
			lock (this._awaitingResolution)
			{
				for (int i = 0; i < this._awaitingResolution.Count; i++)
				{
					if (this._awaitingResolution[i].ResponseWait.WaitingID == id)
					{
						this._awaitingResolution[i].ResponseWait.Callback = null;
						return;
					}
				}
			}
			lock (this._waitingToSubmitDiscoveryRequest)
			{
				for (int j = 0; j < this._waitingToSubmitDiscoveryRequest.Count; j++)
				{
					if (this._waitingToSubmitDiscoveryRequest[j].ResponseWait.WaitingID == id)
					{
						this._waitingToSubmitDiscoveryRequest[j].ResponseWait.Callback = null;
						break;
					}
				}
			}
		}

		public override int GetHostInfo(string nameOrIPIncludingPort, HostDiscovery.HostDiscoveryCallback callback, object context)
		{
			string domain;
			int port;
			if (this.SplitDomainName(nameOrIPIncludingPort, out domain, out port))
			{
				return this.GetHostInfo(domain, port, callback, context);
			}
			callback(HostDiscovery.ResultCode.HostNameInvalid, null, context);
			return 0;
		}

		private bool SplitDomainName(string name, out string result, out int port)
		{
			port = NetworkSession.DefaultPort;
			result = name.Trim();
			int lastColon = result.LastIndexOf(':');
			if (lastColon < result.Length - 1 && lastColon > 0)
			{
				string portString = result.Substring(lastColon + 1);
				result = result.Substring(0, lastColon);
				if (!int.TryParse(portString, out port) || port <= 0 || port >= 65536)
				{
					return false;
				}
			}
			return true;
		}

		private HostDiscovery.WaitingForResponse FindWaiterByEndPoint(IPEndPoint endpoint)
		{
			lock (this._awaitingResponse)
			{
				for (int i = 0; i < this._awaitingResponse.Count; i++)
				{
					if (this._awaitingResponse[i].HostEndPoint.Equals(endpoint))
					{
						HostDiscovery.WaitingForResponse result = this._awaitingResponse[i];
						this._awaitingResponse.RemoveAt(i);
						return result;
					}
				}
			}
			return null;
		}

		public new virtual void Update()
		{
			NetIncomingMessage msg;
			while ((msg = this._client.ReadMessage()) != null)
			{
				NetIncomingMessageType messageType = msg.MessageType;
				if (messageType == NetIncomingMessageType.DiscoveryResponse)
				{
					HostDiscoveryResponseMessage hdrm = msg.ReadDiscoveryResponseMessage(this._gameName, this._version);
					HostDiscovery.WaitingForResponse waiter;
					do
					{
						if (hdrm.ReadResult == VersionCheckedMessage.ReadResultCode.Success && hdrm.Result == NetworkSession.ResultCode.Succeeded)
						{
							waiter = base.FindWaiterByRequestID(hdrm.RequestID);
						}
						else
						{
							waiter = this.FindWaiterByEndPoint(msg.SenderEndPoint);
						}
						if (waiter != null)
						{
							AvailableNetworkSession session = new AvailableNetworkSession(msg.SenderEndPoint, hdrm.HostUsername, hdrm.Message, hdrm.SessionID, hdrm.SessionProperties, hdrm.MaxPlayers, hdrm.CurrentPlayers, hdrm.PasswordProtected);
							HostDiscovery.ResultCode result = HostDiscovery.ResultCode.Success;
							switch (hdrm.ReadResult)
							{
							case VersionCheckedMessage.ReadResultCode.Success:
								switch (hdrm.Result)
								{
								case NetworkSession.ResultCode.GameNamesDontMatch:
									result = HostDiscovery.ResultCode.WrongGameName;
									break;
								case NetworkSession.ResultCode.VersionIsInvalid:
									result = HostDiscovery.ResultCode.VersionIsInvalid;
									break;
								case NetworkSession.ResultCode.ServerHasNewerVersion:
									result = HostDiscovery.ResultCode.ServerHasNewerVersion;
									break;
								case NetworkSession.ResultCode.ServerHasOlderVersion:
									result = HostDiscovery.ResultCode.ServerHasOlderVersion;
									break;
								case NetworkSession.ResultCode.ConnectionDenied:
									result = HostDiscovery.ResultCode.ConnectionDenied;
									break;
								case NetworkSession.ResultCode.GamerAlreadyConnected:
									result = HostDiscovery.ResultCode.GamerAlreadyConnected;
									break;
								}
								break;
							case VersionCheckedMessage.ReadResultCode.GameNameInvalid:
								result = HostDiscovery.ResultCode.WrongGameName;
								break;
							case VersionCheckedMessage.ReadResultCode.VersionInvalid:
								result = HostDiscovery.ResultCode.VersionIsInvalid;
								break;
							case VersionCheckedMessage.ReadResultCode.LocalVersionIsHIgher:
								result = HostDiscovery.ResultCode.ServerHasOlderVersion;
								break;
							case VersionCheckedMessage.ReadResultCode.LocalVersionIsLower:
								result = HostDiscovery.ResultCode.ServerHasNewerVersion;
								break;
							}
							waiter.Callback(result, session, waiter.Context);
						}
					}
					while (waiter != null);
				}
				this._client.Recycle(msg);
			}
			lock (this._waitingToSubmitDiscoveryRequest)
			{
				for (int i = 0; i < this._waitingToSubmitDiscoveryRequest.Count; i++)
				{
					LidgrenHostDiscovery.WaitingForHostResolution w = this._waitingToSubmitDiscoveryRequest[i];
					if (w.ResponseWait.Callback != null)
					{
						if (w.EndPoint == null)
						{
							w.ResponseWait.Callback(HostDiscovery.ResultCode.FailedToResolveHostName, null, w.ResponseWait.Context);
						}
						else
						{
							this.GetHostInfo(w.ResponseWait, w.EndPoint);
						}
					}
				}
				this._waitingToSubmitDiscoveryRequest.Clear();
			}
			base.Update();
		}

		public override void Shutdown()
		{
			this._client.Shutdown("shutting down");
		}

		private NetClient _client;

		protected List<LidgrenHostDiscovery.WaitingForHostResolution> _awaitingResolution = new List<LidgrenHostDiscovery.WaitingForHostResolution>();

		protected List<LidgrenHostDiscovery.WaitingForHostResolution> _waitingToSubmitDiscoveryRequest = new List<LidgrenHostDiscovery.WaitingForHostResolution>();

		protected class WaitingForHostResolution
		{
			public WaitingForHostResolution(List<LidgrenHostDiscovery.WaitingForHostResolution> preList, List<LidgrenHostDiscovery.WaitingForHostResolution> postList)
			{
				this.PreResolutionList = preList;
				this.PostResolutionList = postList;
			}

			public void ResolvedCallback(IPEndPoint endPoint)
			{
				this.EndPoint = endPoint;
				lock (this.PostResolutionList)
				{
					this.PostResolutionList.Add(this);
				}
				lock (this.PreResolutionList)
				{
					this.PreResolutionList.Remove(this);
				}
			}

			public void ResolvedCallback(ulong lobbyid)
			{
				this.SteamLobbyID = lobbyid;
				lock (this.PostResolutionList)
				{
					this.PostResolutionList.Add(this);
				}
				lock (this.PreResolutionList)
				{
					this.PreResolutionList.Remove(this);
				}
			}

			public HostDiscovery.WaitingForResponse ResponseWait;

			public IPEndPoint EndPoint;

			public ulong SteamLobbyID;

			public List<LidgrenHostDiscovery.WaitingForHostResolution> PreResolutionList;

			public List<LidgrenHostDiscovery.WaitingForHostResolution> PostResolutionList;
		}
	}
}
