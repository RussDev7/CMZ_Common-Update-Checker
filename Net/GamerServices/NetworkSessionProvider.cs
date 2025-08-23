using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using DNA.Net.MatchMaking;

namespace DNA.Net.GamerServices
{
	public abstract class NetworkSessionProvider : IDisposable
	{
		public GamerCollection<NetworkGamer> AllGamers
		{
			get
			{
				return this._allGamerCollection;
			}
		}

		public GamerCollection<NetworkGamer> RemoteGamers
		{
			get
			{
				return this._remoteGamerCollection;
			}
		}

		public virtual string ExternalIPString
		{
			get
			{
				return this._externalIPString;
			}
		}

		public virtual string InternalIPString
		{
			get
			{
				return this._internalIPString;
			}
		}

		public virtual string Password
		{
			get
			{
				return this._password;
			}
			set
			{
				this._password = value;
			}
		}

		public virtual NetworkSession.ResultCode HostConnectionResult
		{
			get
			{
				return this._hostConnectionResult;
			}
		}

		public virtual string HostConnectionResultString
		{
			get
			{
				return this._hostConnectionResultString;
			}
		}

		public void ResetHostConnectionResult()
		{
			this._hostConnectionResult = NetworkSession.ResultCode.Pending;
			this._hostConnectionResultString = "";
		}

		public string ServerMessage
		{
			get
			{
				return this._serverMessage;
			}
			set
			{
				this._serverMessage = value;
			}
		}

		private event EventHandler<GamerJoinedEventArgs> _gamerJoined;

		public virtual event EventHandler<GamerJoinedEventArgs> GamerJoined
		{
			add
			{
				this._gamerJoined += value;
				GamerCollection<LocalNetworkGamer> localGamers = this.LocalGamers;
				foreach (LocalNetworkGamer lg in localGamers)
				{
					value(this, new GamerJoinedEventArgs(lg));
				}
			}
			remove
			{
				this._gamerJoined -= value;
			}
		}

		public event EventHandler<GameEndedEventArgs> GameEnded;

		public event EventHandler<GamerLeftEventArgs> GamerLeft;

		public event EventHandler<GameStartedEventArgs> GameStarted;

		public event EventHandler<HostChangedEventArgs> HostChanged;

		public event EventHandler<NetworkSessionEndedEventArgs> SessionEnded;

		public NetworkSessionProvider(NetworkSessionStaticProvider statics)
		{
			this._staticProvider = statics;
			this._localGamers = new List<LocalNetworkGamer>();
			this._allGamers = new List<NetworkGamer>();
			this._remoteGamers = new List<NetworkGamer>();
			this._localGamerCollection = new GamerCollection<LocalNetworkGamer>(this._localGamers);
			this._allGamerCollection = new GamerCollection<NetworkGamer>(this._allGamers);
			this._remoteGamerCollection = new GamerCollection<NetworkGamer>(this._remoteGamers);
			this._nextPlayerGID = 1;
		}

		protected static DNAGame CurrentGame
		{
			get
			{
				return (DNAGame)GamerServicesComponent.Instance.Game;
			}
		}

		protected void HandleDisconnection(string disconnectReason)
		{
			if (this._hostConnectionResult == NetworkSession.ResultCode.Pending)
			{
				this._hostConnectionResult = this._staticProvider.ParseResultCode(disconnectReason);
				this._hostConnectionResultString = disconnectReason;
				return;
			}
			if (this.SessionEnded != null)
			{
				NetworkSessionEndReason nser;
				if (disconnectReason != null)
				{
					if (disconnectReason == "Session Ended Normally")
					{
						nser = NetworkSessionEndReason.HostEndedSession;
						goto IL_0078;
					}
					if (disconnectReason == "Host Kicked Us")
					{
						nser = NetworkSessionEndReason.RemovedByHost;
						goto IL_0078;
					}
					if (disconnectReason == "Connection Dropped")
					{
						nser = NetworkSessionEndReason.Disconnected;
						goto IL_0078;
					}
					if (disconnectReason == "Failed to establish connection - no response from remote host")
					{
						nser = NetworkSessionEndReason.Disconnected;
						goto IL_0078;
					}
				}
				nser = NetworkSessionEndReason.Disconnected;
				IL_0078:
				this.SessionEnded(this, new NetworkSessionEndedEventArgs(nser));
			}
		}

		protected void AddLocalGamer(SignedInGamer sig, bool isHost, byte globalID, ulong steamID)
		{
			LocalNetworkGamer lng = new LocalNetworkGamer(sig, this._networkSession, true, isHost, globalID, steamID);
			this._localPlayerGID = globalID;
			if (isHost)
			{
				this._host = lng;
			}
			this._idToGamer.Add(globalID, lng);
			this._allGamers.Add(lng);
			this._localGamers.Add(lng);
			this._allGamerCollection = new GamerCollection<NetworkGamer>(this._allGamers);
			lock (this._localGamerCollection)
			{
				this._localGamerCollection = new GamerCollection<LocalNetworkGamer>(this._localGamers);
			}
			this.LocalPlayerJoinedEvent.Set();
			if (this._gamerJoined != null)
			{
				this._gamerJoined(this, new GamerJoinedEventArgs(lng));
			}
		}

		protected void AddLocalGamer(SignedInGamer sig, bool isHost, byte globalID)
		{
			this.AddLocalGamer(sig, isHost, globalID, 0UL);
		}

		protected NetworkGamer AddProxyGamer(Gamer gmr, bool isHost, byte globalID)
		{
			NetworkGamer ng = new NetworkGamer(gmr, this._networkSession, false, isHost, globalID);
			if (isHost)
			{
				this._host = ng;
			}
			this._idToGamer.Add(ng.Id, ng);
			this._allGamers.Add(ng);
			this._allGamerCollection = new GamerCollection<NetworkGamer>(this._allGamers);
			this._remoteGamers.Add(ng);
			this._remoteGamerCollection = new GamerCollection<NetworkGamer>(this._remoteGamers);
			if (this._gamerJoined != null)
			{
				this._gamerJoined(this, new GamerJoinedEventArgs(ng));
			}
			return ng;
		}

		private void FinishAddingRemoteGamer(NetworkGamer ng, bool isHost, byte playerGID)
		{
			if (isHost)
			{
				this._host = ng;
			}
			this._idToGamer.Add(playerGID, ng);
			this._allGamers.Add(ng);
			this._allGamerCollection = new GamerCollection<NetworkGamer>(this._allGamers);
			this._remoteGamers.Add(ng);
			this._remoteGamerCollection = new GamerCollection<NetworkGamer>(this._remoteGamers);
			if (this._gamerJoined != null)
			{
				this._gamerJoined(this, new GamerJoinedEventArgs(ng));
			}
		}

		protected NetworkGamer AddRemoteGamer(Gamer gmr, IPAddress endPoint, bool isHost, byte playerGID)
		{
			NetworkGamer ng = new NetworkGamer(gmr, this._networkSession, false, isHost, playerGID, endPoint);
			this.FinishAddingRemoteGamer(ng, isHost, playerGID);
			return ng;
		}

		protected NetworkGamer AddRemoteGamer(Gamer gmr, ulong steamId, bool isHost, byte playerGID)
		{
			NetworkGamer ng = new NetworkGamer(gmr, this._networkSession, false, isHost, playerGID, steamId);
			this.FinishAddingRemoteGamer(ng, isHost, playerGID);
			return ng;
		}

		protected void RemoveGamer(NetworkGamer gamer)
		{
			if (this.GamerLeft != null)
			{
				this.GamerLeft(this, new GamerLeftEventArgs(gamer));
			}
			gamer.NetConnectionObject = null;
			this._idToGamer.Remove(gamer.Id);
			if (this._allGamers.Remove(gamer))
			{
				this._allGamerCollection = new GamerCollection<NetworkGamer>(this._allGamers);
			}
			if (gamer is LocalNetworkGamer)
			{
				if (!this._localGamers.Remove(gamer as LocalNetworkGamer))
				{
					return;
				}
				lock (this._localGamerCollection)
				{
					this._localGamerCollection = new GamerCollection<LocalNetworkGamer>(this._localGamers);
					return;
				}
			}
			if (this._remoteGamers.Remove(gamer))
			{
				this._remoteGamerCollection = new GamerCollection<NetworkGamer>(this._remoteGamers);
			}
		}

		public virtual NetworkGamer FindGamerById(byte gamerId)
		{
			NetworkGamer result;
			if (this._idToGamer.TryGetValue(gamerId, out result))
			{
				return result;
			}
			return null;
		}

		public virtual bool AllowHostMigration
		{
			get
			{
				return this._allowHostMigration;
			}
			set
			{
				this._allowHostMigration = value;
			}
		}

		public virtual bool AllowJoinInProgress
		{
			get
			{
				return this._allowJoinInProgress;
			}
			set
			{
				this._allowJoinInProgress = value;
			}
		}

		public virtual NetworkGamer Host
		{
			get
			{
				return this._host;
			}
		}

		public virtual bool IsDisposed
		{
			get
			{
				return this._disposed;
			}
		}

		public virtual bool IsHost
		{
			get
			{
				return this._isHost;
			}
		}

		public virtual int MaxGamers
		{
			get
			{
				return this._maxPlayers;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public virtual NetworkSessionProperties SessionProperties
		{
			get
			{
				return this._properties;
			}
		}

		public virtual NetworkSessionType SessionType
		{
			get
			{
				return this._sessionType;
			}
		}

		public abstract void StartHost(NetworkSessionStaticProvider.BeginCreateSessionState sqs);

		public abstract void StartClient(NetworkSessionStaticProvider.BeginJoinSessionState sqs);

		public virtual void StartClientInvited(ulong lobbyId, NetworkSessionStaticProvider.BeginJoinSessionState sqs, GetPasswordForInvitedGameCallback getPasswordCallback)
		{
			throw new NotImplementedException();
		}

		public abstract void BroadcastRemoteData(byte[] data, SendDataOptions options);

		public abstract void BroadcastRemoteData(byte[] data, int offset, int length, SendDataOptions options);

		public void Dispose()
		{
			if (!this._disposed)
			{
				this.Dispose(true);
			}
		}

		public virtual GamerCollection<LocalNetworkGamer> LocalGamers
		{
			get
			{
				GamerCollection<LocalNetworkGamer> result;
				lock (this._localGamerCollection)
				{
					result = this._localGamerCollection;
				}
				return result;
			}
		}

		public abstract void SendRemoteData(byte[] data, SendDataOptions options, NetworkGamer recipient);

		public abstract void SendRemoteData(byte[] data, int offset, int length, SendDataOptions options, NetworkGamer recipient);

		public abstract void ReportClientJoined(string username);

		public abstract void ReportClientLeft(string username);

		public abstract void ReportSessionAlive();

		public abstract void UpdateHostSession(string serverName, bool? passwordProtected, bool? isPublic, NetworkSessionProperties sessionProps);

		public abstract void UpdateHostSessionJoinPolicy(JoinGamePolicy joinGamePolicy);

		public abstract void CloseNetworkSession();

		public abstract void Update();

		public virtual void AddLocalGamer(SignedInGamer gamer)
		{
			throw new NotImplementedException();
		}

		public virtual int BytesPerSecondReceived
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual int BytesPerSecondSent
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual void Dispose(bool disposeManagedObjects)
		{
			this._disposed = true;
		}

		public virtual void EndGame()
		{
			throw new NotImplementedException();
		}

		public virtual bool IsEveryoneReady
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual GamerCollection<NetworkGamer> PreviousGamers
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual int PrivateGamerSlots
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public virtual NetworkSessionState SessionState
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public virtual TimeSpan SimulatedLatency
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public virtual float SimulatedPacketLoss
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public virtual void StartGame()
		{
			throw new NotImplementedException();
		}

		public virtual void ResetReady()
		{
			throw new NotImplementedException();
		}

		protected const string sDisconnectReasonShutdown = "Session Ended Normally";

		protected const string sDisconnectReasonKicked = "Host Kicked Us";

		protected const string sDisconnectReasonDropped = "Connection Dropped";

		protected NetworkSessionStaticProvider _staticProvider;

		protected NetworkSession _networkSession;

		public ManualResetEvent LocalPlayerJoinedEvent = new ManualResetEvent(false);

		protected Dictionary<byte, NetworkGamer> _idToGamer = new Dictionary<byte, NetworkGamer>();

		protected List<NetworkGamer> _allGamers;

		protected GamerCollection<NetworkGamer> _allGamerCollection;

		protected List<NetworkGamer> _remoteGamers;

		protected GamerCollection<NetworkGamer> _remoteGamerCollection = new GamerCollection<NetworkGamer>();

		protected List<LocalNetworkGamer> _localGamers;

		protected GamerCollection<LocalNetworkGamer> _localGamerCollection;

		protected List<SignedInGamer> _signedInGamers;

		protected NetworkSessionProperties _properties = new NetworkSessionProperties();

		protected int _maxPlayers;

		protected string _gameName;

		protected string _password;

		protected string _serverMessage;

		protected string _externalIPString;

		protected string _internalIPString;

		protected int _version;

		protected NetworkSession.ResultCode _hostConnectionResult;

		protected string _hostConnectionResultString;

		public NetworkSession.AllowConnectionCallbackDelegate AllowConnectionCallback;

		public NetworkSession.AllowConnectionCallbackDelegateAlt AllowConnectionCallbackAlt;

		protected bool _isHost;

		protected byte _nextPlayerGID = 1;

		protected byte _localPlayerGID;

		protected bool _disposed;

		protected bool _allowHostMigration;

		protected bool _allowJoinInProgress;

		protected NetworkGamer _host;

		protected NetworkSessionType _sessionType;

		public HostSessionInfo HostSessionInfo;
	}
}
