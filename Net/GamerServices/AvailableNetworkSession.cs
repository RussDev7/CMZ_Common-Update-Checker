using System;
using System.Net;
using System.Net.Sockets;
using DNA.Net.MatchMaking;

namespace DNA.Net.GamerServices
{
	public sealed class AvailableNetworkSession
	{
		public IPEndPoint IPEndPoint
		{
			get
			{
				return this.HostEndPoint;
			}
		}

		internal IPEndPoint HostEndPoint
		{
			get
			{
				return this._endPoint;
			}
		}

		public void ConvertToIPV4()
		{
			if (this.IPEndPoint != null && this.IPEndPoint.Address.AddressFamily == AddressFamily.InterNetworkV6)
			{
				IPAddress ipaddress = IPAddress.Any;
				ipaddress = this.IPEndPoint.Address.MaptoIPV4();
				this._endPoint = new IPEndPoint(ipaddress, this._endPoint.Port);
			}
		}

		internal AvailableNetworkSession(IPEndPoint endPoint, string hostGamerTag, string hostMessage, int sessionID, NetworkSessionProperties props, int maxplayers, int currentPlayers, bool passwordProtected)
		{
			this._endPoint = endPoint;
			this._hostGamerTag = hostGamerTag;
			this._hostMessage = hostMessage;
			this._sessionID = sessionID;
			this._properties = props;
			this._maxPlayers = maxplayers;
			this._passwordProtected = passwordProtected;
			this._currentPlayers = currentPlayers;
			this._friendCount = 0;
			this._proximity = 0;
		}

		public AvailableNetworkSession(ClientSessionInfo clientSessionInfo)
		{
			if (clientSessionInfo.IPAddress != null)
			{
				this._endPoint = new IPEndPoint(clientSessionInfo.IPAddress, clientSessionInfo.NetworkPort);
			}
			else
			{
				this._endPoint = null;
			}
			this.LobbySteamID = clientSessionInfo.SteamLobbyID;
			this.HostSteamID = clientSessionInfo.SteamHostID;
			this._hostGamerTag = clientSessionInfo.HostUserName;
			this._hostMessage = clientSessionInfo.Name;
			this._sessionID = 0;
			this._properties = clientSessionInfo.SessionProperties;
			this._maxPlayers = clientSessionInfo.MaxPlayers;
			this._passwordProtected = clientSessionInfo.PasswordProtected;
			this._currentPlayers = clientSessionInfo.CurrentPlayers;
			this._dateCreated = clientSessionInfo.DateCreated;
			this._friendCount = clientSessionInfo.NumFriends;
			this._proximity = clientSessionInfo.Proximity;
		}

		public DateTime DateCreated
		{
			get
			{
				return this._dateCreated;
			}
		}

		public int MaxGamerCount
		{
			get
			{
				return this._maxPlayers;
			}
		}

		public int CurrentGamerCount
		{
			get
			{
				return this._currentPlayers;
			}
		}

		public int FriendCount
		{
			get
			{
				return this._friendCount;
			}
		}

		public int Proximity
		{
			get
			{
				return this._proximity;
			}
		}

		public string HostGamertag
		{
			get
			{
				return this._hostGamerTag;
			}
		}

		public string ServerMessage
		{
			get
			{
				return this._hostMessage;
			}
		}

		public int SessionID
		{
			get
			{
				return this._sessionID;
			}
		}

		public int OpenPrivateGamerSlots
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int OpenPublicGamerSlots
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool PasswordProtected
		{
			get
			{
				return this._passwordProtected;
			}
		}

		public QualityOfService QualityOfService
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public NetworkSessionProperties SessionProperties
		{
			get
			{
				return this._properties;
			}
		}

		private IPEndPoint _endPoint;

		private string _hostGamerTag;

		private string _hostMessage;

		private int _sessionID;

		private int _maxPlayers;

		private int _currentPlayers;

		private bool _passwordProtected;

		private int _friendCount;

		private int _proximity;

		private NetworkSessionProperties _properties;

		private DateTime _dateCreated;

		public ulong LobbySteamID;

		public ulong HostSteamID;
	}
}
