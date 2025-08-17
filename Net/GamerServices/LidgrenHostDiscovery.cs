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
			NetPeerConfiguration netPeerConfiguration = new NetPeerConfiguration(this._gameName + "Discovery");
			netPeerConfiguration.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
			this._client = new NetClient(netPeerConfiguration);
			this._client.Start();
		}

		private void GetHostInfo(HostDiscovery.WaitingForResponse wc, IPEndPoint endpoint)
		{
			wc.HostEndPoint = endpoint;
			this._awaitingResponse.Add(wc);
			HostDiscoveryRequestMessage hostDiscoveryRequestMessage = new HostDiscoveryRequestMessage();
			hostDiscoveryRequestMessage.GameName = this._gameName;
			hostDiscoveryRequestMessage.RequestID = wc.WaitingID;
			hostDiscoveryRequestMessage.NetworkVersion = this._version;
			hostDiscoveryRequestMessage.PlayerID = this._playerID;
			NetOutgoingMessage netOutgoingMessage = this._client.CreateMessage();
			netOutgoingMessage.Write(hostDiscoveryRequestMessage, this._gameName, this._version);
			wc.Timer = Stopwatch.StartNew();
			this._client.DiscoverKnownPeer(endpoint, netOutgoingMessage);
		}

		public override int GetHostInfo(IPEndPoint endpoint, HostDiscovery.HostDiscoveryCallback callback, object context)
		{
			int num = this._nextWaitingID++;
			this.GetHostInfo(new HostDiscovery.WaitingForResponse
			{
				Callback = callback,
				Context = context,
				WaitingID = num
			}, endpoint);
			return num;
		}

		public override int GetHostInfo(string nameOrIP, int port, HostDiscovery.HostDiscoveryCallback callback, object context)
		{
			int num = this._nextWaitingID++;
			HostDiscovery.WaitingForResponse waitingForResponse = new HostDiscovery.WaitingForResponse();
			waitingForResponse.Callback = callback;
			waitingForResponse.Context = context;
			waitingForResponse.WaitingID = num;
			LidgrenHostDiscovery.WaitingForHostResolution wfhr = new LidgrenHostDiscovery.WaitingForHostResolution(this._awaitingResolution, this._waitingToSubmitDiscoveryRequest);
			wfhr.ResponseWait = waitingForResponse;
			lock (this._awaitingResolution)
			{
				this._awaitingResolution.Add(wfhr);
			}
			ThreadPool.QueueUserWorkItem(delegate(object state)
			{
				NetUtility.ResolveAsync(nameOrIP, port, new NetUtility.ResolveEndPointCallback(wfhr.ResolvedCallback));
			});
			return num;
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
			string text;
			int num;
			if (this.SplitDomainName(nameOrIPIncludingPort, out text, out num))
			{
				return this.GetHostInfo(text, num, callback, context);
			}
			callback(HostDiscovery.ResultCode.HostNameInvalid, null, context);
			return 0;
		}

		private bool SplitDomainName(string name, out string result, out int port)
		{
			port = NetworkSession.DefaultPort;
			result = name.Trim();
			int num = result.LastIndexOf(':');
			if (num < result.Length - 1 && num > 0)
			{
				string text = result.Substring(num + 1);
				result = result.Substring(0, num);
				if (!int.TryParse(text, out port) || port <= 0 || port >= 65536)
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
						HostDiscovery.WaitingForResponse waitingForResponse = this._awaitingResponse[i];
						this._awaitingResponse.RemoveAt(i);
						return waitingForResponse;
					}
				}
			}
			return null;
		}

		public new virtual void Update()
		{
			NetIncomingMessage netIncomingMessage;
			while ((netIncomingMessage = this._client.ReadMessage()) != null)
			{
				NetIncomingMessageType messageType = netIncomingMessage.MessageType;
				if (messageType == NetIncomingMessageType.DiscoveryResponse)
				{
					HostDiscoveryResponseMessage hostDiscoveryResponseMessage = netIncomingMessage.ReadDiscoveryResponseMessage(this._gameName, this._version);
					HostDiscovery.WaitingForResponse waitingForResponse;
					do
					{
						if (hostDiscoveryResponseMessage.ReadResult == VersionCheckedMessage.ReadResultCode.Success && hostDiscoveryResponseMessage.Result == NetworkSession.ResultCode.Succeeded)
						{
							waitingForResponse = base.FindWaiterByRequestID(hostDiscoveryResponseMessage.RequestID);
						}
						else
						{
							waitingForResponse = this.FindWaiterByEndPoint(netIncomingMessage.SenderEndPoint);
						}
						if (waitingForResponse != null)
						{
							AvailableNetworkSession availableNetworkSession = new AvailableNetworkSession(netIncomingMessage.SenderEndPoint, hostDiscoveryResponseMessage.HostUsername, hostDiscoveryResponseMessage.Message, hostDiscoveryResponseMessage.SessionID, hostDiscoveryResponseMessage.SessionProperties, hostDiscoveryResponseMessage.MaxPlayers, hostDiscoveryResponseMessage.CurrentPlayers, hostDiscoveryResponseMessage.PasswordProtected);
							HostDiscovery.ResultCode resultCode = HostDiscovery.ResultCode.Success;
							switch (hostDiscoveryResponseMessage.ReadResult)
							{
							case VersionCheckedMessage.ReadResultCode.Success:
								switch (hostDiscoveryResponseMessage.Result)
								{
								case NetworkSession.ResultCode.GameNamesDontMatch:
									resultCode = HostDiscovery.ResultCode.WrongGameName;
									break;
								case NetworkSession.ResultCode.VersionIsInvalid:
									resultCode = HostDiscovery.ResultCode.VersionIsInvalid;
									break;
								case NetworkSession.ResultCode.ServerHasNewerVersion:
									resultCode = HostDiscovery.ResultCode.ServerHasNewerVersion;
									break;
								case NetworkSession.ResultCode.ServerHasOlderVersion:
									resultCode = HostDiscovery.ResultCode.ServerHasOlderVersion;
									break;
								case NetworkSession.ResultCode.ConnectionDenied:
									resultCode = HostDiscovery.ResultCode.ConnectionDenied;
									break;
								case NetworkSession.ResultCode.GamerAlreadyConnected:
									resultCode = HostDiscovery.ResultCode.GamerAlreadyConnected;
									break;
								}
								break;
							case VersionCheckedMessage.ReadResultCode.GameNameInvalid:
								resultCode = HostDiscovery.ResultCode.WrongGameName;
								break;
							case VersionCheckedMessage.ReadResultCode.VersionInvalid:
								resultCode = HostDiscovery.ResultCode.VersionIsInvalid;
								break;
							case VersionCheckedMessage.ReadResultCode.LocalVersionIsHIgher:
								resultCode = HostDiscovery.ResultCode.ServerHasOlderVersion;
								break;
							case VersionCheckedMessage.ReadResultCode.LocalVersionIsLower:
								resultCode = HostDiscovery.ResultCode.ServerHasNewerVersion;
								break;
							}
							waitingForResponse.Callback(resultCode, availableNetworkSession, waitingForResponse.Context);
						}
					}
					while (waitingForResponse != null);
				}
				this._client.Recycle(netIncomingMessage);
			}
			lock (this._waitingToSubmitDiscoveryRequest)
			{
				for (int i = 0; i < this._waitingToSubmitDiscoveryRequest.Count; i++)
				{
					LidgrenHostDiscovery.WaitingForHostResolution waitingForHostResolution = this._waitingToSubmitDiscoveryRequest[i];
					if (waitingForHostResolution.ResponseWait.Callback != null)
					{
						if (waitingForHostResolution.EndPoint == null)
						{
							waitingForHostResolution.ResponseWait.Callback(HostDiscovery.ResultCode.FailedToResolveHostName, null, waitingForHostResolution.ResponseWait.Context);
						}
						else
						{
							this.GetHostInfo(waitingForHostResolution.ResponseWait, waitingForHostResolution.EndPoint);
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
