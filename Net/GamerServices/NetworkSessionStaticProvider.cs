using System;
using System.Collections.Generic;
using System.Threading;
using DNA.Net.MatchMaking;
using DNA.Threading;

namespace DNA.Net.GamerServices
{
	public abstract class NetworkSessionStaticProvider
	{
		public TaskScheduler TaskScheduler
		{
			get
			{
				return this._taskScheduler;
			}
		}

		protected static DNAGame CurrentGame
		{
			get
			{
				return (DNAGame)GamerServicesComponent.Instance.Game;
			}
		}

		public virtual int DefaultPort
		{
			get
			{
				return this._defaultPort;
			}
			set
			{
				this._defaultPort = value;
			}
		}

		protected event EventHandler<InviteAcceptedEventArgs> _inviteAccepted;

		public virtual event EventHandler<InviteAcceptedEventArgs> InviteAccepted
		{
			add
			{
				this._inviteAccepted += value;
			}
			remove
			{
				this._inviteAccepted -= value;
			}
		}

		protected void CallInviteAccepted(InviteAcceptedEventArgs args)
		{
			if (this._inviteAccepted != null)
			{
				this._inviteAccepted(this, args);
			}
		}

		private void _taskScheduler_ThreadException(object sender, TaskScheduler.ExceptionEventArgs e)
		{
			NetworkSessionStaticProvider.CurrentGame.CrashGame(e.InnerException);
		}

		public NetworkSessionStaticProvider()
		{
			for (NetworkSession.ResultCode i = NetworkSession.ResultCode.Succeeded; i < NetworkSession.ResultCode.Count; i++)
			{
				this._sResultCodeNames[(int)i] = i.ToString();
			}
			this._taskScheduler = new TaskScheduler();
			this._taskScheduler.ThreadException += this._taskScheduler_ThreadException;
		}

		public NetworkSession.ResultCode ParseResultCode(string name)
		{
			for (int i = 0; i < 27; i++)
			{
				if (this._sResultCodeNames[i] == name)
				{
					return (NetworkSession.ResultCode)i;
				}
			}
			return NetworkSession.ResultCode.UnknownResult;
		}

		public virtual IAsyncResult BeginCreate(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties, string gameName, int networkVersion, string serverMessage, string password, AsyncCallback callback, object asyncState)
		{
			NetworkSessionStaticProvider.BeginCreateSessionState state = new NetworkSessionStaticProvider.BeginCreateSessionState(callback, asyncState);
			state.LocalGamers = localGamers;
			state.Properties = sessionProperties;
			state.MaxPlayers = maxGamers;
			state.SessionType = sessionType;
			state.NetworkGameName = gameName;
			state.ServerMessage = serverMessage;
			state.Version = networkVersion;
			state.Password = password;
			state.Session = this.CreateSession();
			this.FinishBeginCreate(state);
			return state;
		}

		public virtual NetworkSession EndCreate(IAsyncResult result)
		{
			NetworkSessionStaticProvider.BeginCreateSessionState sqs = (NetworkSessionStaticProvider.BeginCreateSessionState)result;
			sqs.AsyncWaitHandle.WaitOne();
			return sqs.Session;
		}

		public virtual IAsyncResult BeginFind(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, QuerySessionInfo searchProperties, AsyncCallback callback, object asyncState)
		{
			NetworkSessionStaticProvider.SessionQueryState state = new NetworkSessionStaticProvider.SessionQueryState(searchProperties, callback, asyncState);
			this.FinishBeginFind(state);
			return state;
		}

		public virtual AvailableNetworkSessionCollection EndFind(IAsyncResult result)
		{
			NetworkSessionStaticProvider.SessionQueryState sqs = (NetworkSessionStaticProvider.SessionQueryState)result;
			sqs.AsyncWaitHandle.WaitOne();
			return sqs.Sessions;
		}

		public virtual IAsyncResult BeginJoinInvited(ulong lobbyId, int version, string gameName, IEnumerable<SignedInGamer> localGamers, AsyncCallback callback, object asyncState, GetPasswordForInvitedGameCallback getPasswordCallback)
		{
			NetworkSessionStaticProvider.BeginJoinSessionState state = new NetworkSessionStaticProvider.BeginJoinSessionState(callback, asyncState);
			state.AvailableSession = null;
			state.SessionType = NetworkSessionType.PlayerMatch;
			state.LocalGamers = localGamers;
			state.Version = version;
			state.NetworkGameName = gameName;
			state.Session = this.CreateSession();
			this.FinishBeginJoinInvited(lobbyId, state, getPasswordCallback);
			return state;
		}

		public virtual IAsyncResult BeginJoin(AvailableNetworkSession availableSession, string gameName, int version, string password, IEnumerable<SignedInGamer> localGamers, AsyncCallback callback, object asyncState)
		{
			NetworkSessionStaticProvider.BeginJoinSessionState state = new NetworkSessionStaticProvider.BeginJoinSessionState(callback, asyncState);
			state.AvailableSession = availableSession;
			state.SessionType = NetworkSessionType.PlayerMatch;
			state.LocalGamers = localGamers;
			state.NetworkGameName = gameName;
			state.Version = version;
			state.Password = password;
			state.Session = this.CreateSession();
			this.FinishBeginJoin(state);
			return state;
		}

		public virtual NetworkSession EndJoin(IAsyncResult result)
		{
			NetworkSessionStaticProvider.BeginJoinSessionState sqs = (NetworkSessionStaticProvider.BeginJoinSessionState)result;
			if (sqs.HostConnectionResult != NetworkSession.ResultCode.Succeeded)
			{
				string empty = string.Empty;
				sqs.Session.Dispose();
				sqs.Session = null;
				throw new Exception("Connection failed: " + sqs.HostConnectionResultString);
			}
			sqs.AsyncWaitHandle.WaitOne();
			return sqs.Session;
		}

		protected abstract NetworkSession CreateSession();

		protected abstract void FinishBeginCreate(NetworkSessionStaticProvider.BeginCreateSessionState state);

		protected abstract void FinishBeginJoin(NetworkSessionStaticProvider.BeginJoinSessionState state);

		protected abstract void FinishBeginFind(NetworkSessionStaticProvider.SessionQueryState state);

		protected virtual void FinishBeginJoinInvited(ulong lobbyId, NetworkSessionStaticProvider.BeginJoinSessionState state, GetPasswordForInvitedGameCallback getPasswordCallback)
		{
			throw new NotImplementedException();
		}

		public abstract HostDiscovery GetHostDiscoveryObject(string gamename, int version, PlayerID playerID);

		public virtual IAsyncResult BeginJoinInvited(int maxLocalGamers, AsyncCallback callback, object asyncState)
		{
			throw new NotImplementedException();
		}

		public virtual NetworkSession Create(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers)
		{
			throw new NotImplementedException();
		}

		public virtual NetworkSession Create(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
		{
			throw new NotImplementedException();
		}

		public virtual NetworkSession Create(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
		{
			throw new NotImplementedException();
		}

		public virtual NetworkSession EndJoinInvited(IAsyncResult result)
		{
			throw new NotImplementedException();
		}

		public virtual AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties)
		{
			throw new NotImplementedException();
		}

		public virtual AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, int maxLocalGamers, NetworkSessionProperties searchProperties)
		{
			throw new NotImplementedException();
		}

		public virtual NetworkSession Join(AvailableNetworkSession availableSession)
		{
			throw new NotImplementedException();
		}

		public virtual NetworkSession JoinInvited(IEnumerable<SignedInGamer> localGamers)
		{
			throw new NotImplementedException();
		}

		public virtual NetworkSession JoinInvited(int maxLocalGamers)
		{
			throw new NotImplementedException();
		}

		public virtual IAsyncResult BeginCreate(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers, AsyncCallback callback, object asyncState)
		{
			throw new NotImplementedException();
		}

		public virtual IAsyncResult BeginCreate(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties, AsyncCallback callback, object asyncState)
		{
			throw new NotImplementedException();
		}

		public virtual IAsyncResult BeginFind(NetworkSessionType sessionType, int maxLocalGamers, NetworkSessionProperties searchProperties, AsyncCallback callback, object asyncState)
		{
			throw new NotImplementedException();
		}

		private TaskScheduler _taskScheduler;

		private string[] _sResultCodeNames = new string[27];

		protected int _defaultPort = 61903;

		public NetworkSessionServices NetworkSessionServices;

		public abstract class BaseAsyncResult : IAsyncResult
		{
			public BaseAsyncResult(AsyncCallback callback, object state)
			{
				this.Callback = callback;
				this._state = state;
			}

			public object AsyncState
			{
				get
				{
					return this._state;
				}
			}

			public WaitHandle AsyncWaitHandle
			{
				get
				{
					return this.Event;
				}
			}

			public bool CompletedSynchronously
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public virtual bool IsCompleted
			{
				get
				{
					return true;
				}
			}

			private object _state;

			public ManualResetEvent Event = new ManualResetEvent(false);

			public AsyncCallback Callback;
		}

		public class BeginCreateSessionState : NetworkSessionStaticProvider.BaseAsyncResult
		{
			public BeginCreateSessionState(AsyncCallback callback, object state)
				: base(callback, state)
			{
			}

			public override bool IsCompleted
			{
				get
				{
					return this.ExceptionEncountered == null;
				}
			}

			public NetworkSession Session;

			public NetworkSessionType SessionType;

			public int MaxPlayers;

			public IEnumerable<SignedInGamer> LocalGamers;

			public NetworkSessionProperties Properties;

			public string NetworkGameName;

			public int Version;

			public string ServerMessage;

			public string Password;

			public Exception ExceptionEncountered;
		}

		public class BeginJoinSessionState : NetworkSessionStaticProvider.BaseAsyncResult
		{
			public BeginJoinSessionState(AsyncCallback callback, object state)
				: base(callback, state)
			{
			}

			public NetworkSession Session;

			public NetworkSessionType SessionType;

			public AvailableNetworkSession AvailableSession;

			public IEnumerable<SignedInGamer> LocalGamers;

			public string NetworkGameName;

			public int Version;

			public NetworkSession.ResultCode HostConnectionResult;

			public string HostConnectionResultString;

			public string Password;
		}

		public class SessionQueryState : NetworkSessionStaticProvider.BaseAsyncResult
		{
			public SessionQueryState(QuerySessionInfo searchProps, AsyncCallback callback, object state)
				: base(callback, state)
			{
				this.SearchProperties = searchProps;
			}

			public AvailableNetworkSessionCollection Sessions;

			public QuerySessionInfo SearchProperties;

			public List<ClientSessionInfo> ClientSessionsFound;
		}

		public class JoinSessionState : NetworkSessionStaticProvider.BaseAsyncResult
		{
			public JoinSessionState(AsyncCallback callback, object state)
				: base(callback, state)
			{
			}

			public AvailableNetworkSession SessionToJoin;

			public NetworkSession JoinedSession;
		}
	}
}
