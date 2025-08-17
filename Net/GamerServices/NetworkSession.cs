using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using DNA.Net.MatchMaking;

namespace DNA.Net.GamerServices
{
	public sealed class NetworkSession : IDisposable
	{
		public static NetworkSessionStaticProvider StaticProvider
		{
			get
			{
				return NetworkSession._staticProvider;
			}
			set
			{
				NetworkSession._staticProvider = value;
			}
		}

		public static NetworkSessionServices NetworkSessionServices
		{
			get
			{
				return NetworkSession._staticProvider.NetworkSessionServices;
			}
			set
			{
				NetworkSession._staticProvider.NetworkSessionServices = value;
			}
		}

		public static HostDiscovery GetHostDiscoveryObject(string gamename, int version, PlayerID playerID)
		{
			return NetworkSession._staticProvider.GetHostDiscoveryObject(gamename, version, playerID);
		}

		public HostSessionInfo HostSessionInfo
		{
			get
			{
				return this._sessionProvider.HostSessionInfo;
			}
			set
			{
				this._sessionProvider.HostSessionInfo = value;
			}
		}

		public void ReportClientJoined(string username)
		{
			this._sessionProvider.ReportClientJoined(username);
		}

		public void ReportClientLeft(string username)
		{
			this._sessionProvider.ReportClientLeft(username);
		}

		public void ReportSessionAlive()
		{
			this._sessionProvider.ReportSessionAlive();
		}

		public void UpdateHostSession(string serverName, bool? passwordProtected, bool? isPublic, NetworkSessionProperties sessionProps)
		{
			this._sessionProvider.UpdateHostSession(serverName, passwordProtected, isPublic, sessionProps);
		}

		public void UpdateHostSessionJoinPolicy(JoinGamePolicy joinGamePolicy)
		{
			this._sessionProvider.UpdateHostSessionJoinPolicy(joinGamePolicy);
		}

		public void CloseNetworkSession()
		{
			this._sessionProvider.CloseNetworkSession();
		}

		public static NetworkSession CurrentNetworkSession
		{
			get
			{
				return NetworkSession._currentNetworkSession;
			}
		}

		public NetworkSession(NetworkSessionProvider provider)
		{
			this._sessionProvider = provider;
			NetworkSession._currentNetworkSession = this;
		}

		public ManualResetEvent LocalPlayerJoinedEvent
		{
			get
			{
				return this._sessionProvider.LocalPlayerJoinedEvent;
			}
		}

		public GamerCollection<NetworkGamer> AllGamers
		{
			get
			{
				return this._sessionProvider.AllGamers;
			}
		}

		public GamerCollection<NetworkGamer> RemoteGamers
		{
			get
			{
				return this._sessionProvider.RemoteGamers;
			}
		}

		public GamerCollection<LocalNetworkGamer> LocalGamers
		{
			get
			{
				return this._sessionProvider.LocalGamers;
			}
		}

		public static int DefaultPort
		{
			get
			{
				return NetworkSession._staticProvider.DefaultPort;
			}
		}

		public bool AllowHostMigration
		{
			get
			{
				return this._sessionProvider.AllowHostMigration;
			}
			set
			{
				this._sessionProvider.AllowHostMigration = value;
			}
		}

		public bool AllowJoinInProgress
		{
			get
			{
				return this._sessionProvider.AllowJoinInProgress;
			}
			set
			{
				this._sessionProvider.AllowJoinInProgress = value;
			}
		}

		public int BytesPerSecondReceived
		{
			get
			{
				return this._sessionProvider.BytesPerSecondReceived;
			}
		}

		public int BytesPerSecondSent
		{
			get
			{
				return this._sessionProvider.BytesPerSecondSent;
			}
		}

		public NetworkGamer Host
		{
			get
			{
				return this._sessionProvider.Host;
			}
		}

		public bool IsDisposed
		{
			get
			{
				return this._sessionProvider == null || this._sessionProvider.IsDisposed;
			}
		}

		public bool IsEveryoneReady
		{
			get
			{
				return this._sessionProvider.IsEveryoneReady;
			}
		}

		public bool IsHost
		{
			get
			{
				return this._sessionProvider.IsHost;
			}
		}

		public NetworkSession.ResultCode HostConnectionResult
		{
			get
			{
				return this._sessionProvider.HostConnectionResult;
			}
		}

		public string HostConnectionResultString
		{
			get
			{
				return this._sessionProvider.HostConnectionResultString;
			}
		}

		public void ResetHostConnectionResult()
		{
			this._sessionProvider.ResetHostConnectionResult();
		}

		public int MaxGamers
		{
			get
			{
				return this._sessionProvider.MaxGamers;
			}
			set
			{
				this._sessionProvider.MaxGamers = value;
			}
		}

		public GamerCollection<NetworkGamer> PreviousGamers
		{
			get
			{
				return this._sessionProvider.PreviousGamers;
			}
		}

		public int PrivateGamerSlots
		{
			get
			{
				return this._sessionProvider.PrivateGamerSlots;
			}
			set
			{
				this._sessionProvider.PrivateGamerSlots = value;
			}
		}

		public NetworkSessionProperties SessionProperties
		{
			get
			{
				return this._sessionProvider.SessionProperties;
			}
		}

		public NetworkSessionState SessionState
		{
			get
			{
				return this._sessionProvider.SessionState;
			}
		}

		public NetworkSessionType SessionType
		{
			get
			{
				return this._sessionProvider.SessionType;
			}
		}

		public TimeSpan SimulatedLatency
		{
			get
			{
				return this._sessionProvider.SimulatedLatency;
			}
			set
			{
				this._sessionProvider.SimulatedLatency = value;
			}
		}

		public float SimulatedPacketLoss
		{
			get
			{
				return this._sessionProvider.SimulatedPacketLoss;
			}
			set
			{
				this._sessionProvider.SimulatedPacketLoss = value;
			}
		}

		public event EventHandler<GameEndedEventArgs> GameEnded
		{
			add
			{
				this._sessionProvider.GameEnded += value;
			}
			remove
			{
				this._sessionProvider.GameEnded -= value;
			}
		}

		public event EventHandler<GamerJoinedEventArgs> GamerJoined
		{
			add
			{
				this._sessionProvider.GamerJoined += value;
			}
			remove
			{
				this._sessionProvider.GamerJoined -= value;
			}
		}

		public event EventHandler<GamerLeftEventArgs> GamerLeft
		{
			add
			{
				this._sessionProvider.GamerLeft += value;
			}
			remove
			{
				this._sessionProvider.GamerLeft -= value;
			}
		}

		public event EventHandler<GameStartedEventArgs> GameStarted
		{
			add
			{
				this._sessionProvider.GameStarted += value;
			}
			remove
			{
				this._sessionProvider.GameStarted -= value;
			}
		}

		public event EventHandler<HostChangedEventArgs> HostChanged
		{
			add
			{
				this._sessionProvider.HostChanged += value;
			}
			remove
			{
				this._sessionProvider.HostChanged -= value;
			}
		}

		public static event EventHandler<InviteAcceptedEventArgs> InviteAccepted
		{
			add
			{
				NetworkSession._staticProvider.InviteAccepted += value;
			}
			remove
			{
				NetworkSession._staticProvider.InviteAccepted -= value;
			}
		}

		public event EventHandler<NetworkSessionEndedEventArgs> SessionEnded
		{
			add
			{
				this._sessionProvider.SessionEnded += value;
			}
			remove
			{
				this._sessionProvider.SessionEnded -= value;
			}
		}

		public static IAsyncResult BeginCreate(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers, AsyncCallback callback, object asyncState)
		{
			return NetworkSession._staticProvider.BeginCreate(sessionType, maxLocalGamers, maxGamers, callback, asyncState);
		}

		public static IAsyncResult BeginCreate(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties, string gameName, int networkVersion, string serverMessage, string password, AsyncCallback callback, object asyncState)
		{
			return NetworkSession._staticProvider.BeginCreate(sessionType, localGamers, maxGamers, privateGamerSlots, sessionProperties, gameName, networkVersion, serverMessage, password, callback, asyncState);
		}

		public static IAsyncResult BeginCreate(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties, AsyncCallback callback, object asyncState)
		{
			return NetworkSession._staticProvider.BeginCreate(sessionType, maxLocalGamers, maxGamers, privateGamerSlots, sessionProperties, callback, asyncState);
		}

		public static NetworkSession EndCreate(IAsyncResult result)
		{
			return NetworkSession._staticProvider.EndCreate(result);
		}

		public void StartHost(NetworkSessionStaticProvider.BeginCreateSessionState sqs)
		{
			this._sessionProvider.StartHost(sqs);
		}

		public void StartClient(NetworkSessionStaticProvider.BeginJoinSessionState sqs)
		{
			this._sessionProvider.StartClient(sqs);
		}

		public void StartClientInvited(ulong lobbyId, NetworkSessionStaticProvider.BeginJoinSessionState sqs, GetPasswordForInvitedGameCallback getPasswordCallback)
		{
			this._sessionProvider.StartClientInvited(lobbyId, sqs, getPasswordCallback);
		}

		public static IAsyncResult BeginFind(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, QuerySessionInfo searchProperties, AsyncCallback callback, object asyncState)
		{
			return NetworkSession._staticProvider.BeginFind(sessionType, localGamers, searchProperties, callback, asyncState);
		}

		public static IAsyncResult BeginFind(NetworkSessionType sessionType, int maxLocalGamers, NetworkSessionProperties searchProperties, AsyncCallback callback, object asyncState)
		{
			throw new NotImplementedException();
		}

		public static AvailableNetworkSessionCollection EndFind(IAsyncResult result)
		{
			return NetworkSession._staticProvider.EndFind(result);
		}

		public static IAsyncResult BeginJoin(AvailableNetworkSession availableSession, string gameName, int version, string password, IEnumerable<SignedInGamer> localGamers, AsyncCallback callback, object asyncState)
		{
			return NetworkSession._staticProvider.BeginJoin(availableSession, gameName, version, password, localGamers, callback, asyncState);
		}

		public static NetworkSession EndJoin(IAsyncResult result)
		{
			return NetworkSession._staticProvider.EndJoin(result);
		}

		public static IAsyncResult BeginJoinInvited(ulong lobbyId, int version, string gameName, IEnumerable<SignedInGamer> localGamers, AsyncCallback callback, object asyncState, GetPasswordForInvitedGameCallback getPasswordCallback)
		{
			return NetworkSession._staticProvider.BeginJoinInvited(lobbyId, version, gameName, localGamers, callback, asyncState, getPasswordCallback);
		}

		public static IAsyncResult BeginJoinInvited(int maxLocalGamers, AsyncCallback callback, object asyncState)
		{
			return NetworkSession._staticProvider.BeginJoinInvited(maxLocalGamers, callback, asyncState);
		}

		public static NetworkSession Create(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers)
		{
			return NetworkSession._staticProvider.Create(sessionType, maxLocalGamers, maxGamers);
		}

		public static NetworkSession Create(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
		{
			return NetworkSession._staticProvider.Create(sessionType, localGamers, maxGamers, privateGamerSlots, sessionProperties);
		}

		public static NetworkSession Create(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
		{
			return NetworkSession._staticProvider.Create(sessionType, maxLocalGamers, maxGamers, privateGamerSlots, sessionProperties);
		}

		public void EndGame()
		{
			this._sessionProvider.EndGame();
		}

		public static NetworkSession EndJoinInvited(IAsyncResult result)
		{
			return NetworkSession._staticProvider.EndJoin(result);
		}

		public static AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties)
		{
			return NetworkSession._staticProvider.Find(sessionType, localGamers, searchProperties);
		}

		public static AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, int maxLocalGamers, NetworkSessionProperties searchProperties)
		{
			return NetworkSession.Find(sessionType, maxLocalGamers, searchProperties);
		}

		public NetworkGamer FindGamerById(byte gamerId)
		{
			return this._sessionProvider.FindGamerById(gamerId);
		}

		public void SendRemoteData(byte[] data, SendDataOptions options, NetworkGamer recipient)
		{
			this._sessionProvider.SendRemoteData(data, options, recipient);
		}

		public void SendRemoteData(byte[] data, int offset, int length, SendDataOptions options, NetworkGamer recipient)
		{
			this._sessionProvider.SendRemoteData(data, offset, length, options, recipient);
		}

		public void BroadcastRemoteData(byte[] data, SendDataOptions options)
		{
			this._sessionProvider.BroadcastRemoteData(data, options);
		}

		public void BroadcastRemoteData(byte[] data, int offset, int length, SendDataOptions options)
		{
			this._sessionProvider.BroadcastRemoteData(data, offset, length, options);
		}

		public static NetworkSession Join(AvailableNetworkSession availableSession)
		{
			return NetworkSession._staticProvider.Join(availableSession);
		}

		public static NetworkSession JoinInvited(IEnumerable<SignedInGamer> localGamers)
		{
			return NetworkSession._staticProvider.JoinInvited(localGamers);
		}

		public static NetworkSession JoinInvited(int maxLocalGamers)
		{
			return NetworkSession._staticProvider.JoinInvited(maxLocalGamers);
		}

		public void ResetReady()
		{
			this._sessionProvider.ResetReady();
		}

		public void StartGame()
		{
			this._sessionProvider.StartGame();
		}

		~NetworkSession()
		{
			if (this._sessionProvider != null && !this._sessionProvider.IsDisposed)
			{
				this._sessionProvider.Dispose();
				this._sessionProvider = null;
			}
		}

		public void Dispose(bool disposeManagedObjects)
		{
			if (this._sessionProvider != null)
			{
				this._sessionProvider.Dispose(disposeManagedObjects);
				this._sessionProvider = null;
				NetworkSession._currentNetworkSession = null;
			}
		}

		public void Dispose()
		{
			if (this._sessionProvider != null && !this._sessionProvider.IsDisposed)
			{
				this.Dispose(true);
			}
		}

		public string ExternalIPString
		{
			get
			{
				return this._sessionProvider.ExternalIPString;
			}
		}

		public string InternalIPString
		{
			get
			{
				return this._sessionProvider.InternalIPString;
			}
		}

		public string ServerMessage
		{
			get
			{
				return this._sessionProvider.ServerMessage;
			}
			set
			{
				this._sessionProvider.ServerMessage = value;
			}
		}

		public string Password
		{
			get
			{
				return this._sessionProvider.Password;
			}
			set
			{
				this._sessionProvider.Password = value;
			}
		}

		public void Update()
		{
			this._sessionProvider.Update();
		}

		public NetworkSession.AllowConnectionCallbackDelegate AllowConnectionCallback
		{
			get
			{
				return this._sessionProvider.AllowConnectionCallback;
			}
			set
			{
				this._sessionProvider.AllowConnectionCallback = value;
			}
		}

		public NetworkSession.AllowConnectionCallbackDelegateAlt AllowConnectionCallbackAlt
		{
			get
			{
				return this._sessionProvider.AllowConnectionCallbackAlt;
			}
			set
			{
				this._sessionProvider.AllowConnectionCallbackAlt = value;
			}
		}

		public const int MaxPreviousGamers = 100;

		public const int MaxSupportedGamers = 31;

		private static NetworkSessionStaticProvider _staticProvider;

		private static NetworkSession _currentNetworkSession = null;

		private NetworkSessionProvider _sessionProvider;

		public delegate bool AllowConnectionCallbackDelegate(PlayerID playerID, IPAddress endpoint);

		public delegate bool AllowConnectionCallbackDelegateAlt(PlayerID playerID, ulong alternateid);

		public enum ResultCode
		{
			Pending = -1,
			Succeeded,
			Timeout,
			GameNamesDontMatch,
			VersionIsInvalid,
			ServerHasNewerVersion,
			ServerHasOlderVersion,
			ConnectionDenied,
			IncorrectSessionId,
			SessionPropertiesDontMatch,
			IncorrectPassword,
			ExceptionThrown,
			GamerAlreadyConnected,
			HostDeniedConnection,
			CouldNotCreateLobby,
			HostDisconnected,
			GameNoLongerExists,
			YouLackTheNeededPermissions,
			GameIsFull,
			UnknownSteamErrorJoiningGame,
			BannedBySteam,
			YouAreALimitedUserAndCannotJoin,
			YourClanIsLockedOrDisabled,
			YouHaveACommunityLockOnYourAccount,
			AnExistingPlayerOnThisServerHasBlockedYou,
			YouHaveBlockedAnExistingPlayerOnThisServer,
			SteamReportedIOError,
			UnknownResult,
			Count
		}
	}
}
