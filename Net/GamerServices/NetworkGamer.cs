using System;
using System.Net;

namespace DNA.Net.GamerServices
{
	public class NetworkGamer : Gamer
	{
		public NetworkGamer(Gamer gmr, NetworkSession session, bool isLocal, bool isHost, byte globalID, IPAddress publicAddress)
		{
			this._isHost = isHost;
			this._isLocal = isLocal;
			this._globalId = globalID;
			this._publicAddress = publicAddress;
			this._networkSession = new WeakReference<NetworkSession>(session);
			this.PlayerID = gmr.PlayerID;
			base.Gamertag = gmr.Gamertag;
			this._alternateAddress = 0UL;
		}

		public NetworkGamer(Gamer gmr, NetworkSession session, bool isLocal, bool isHost, byte globalID)
		{
			this._isHost = isHost;
			this._isLocal = isLocal;
			this._globalId = globalID;
			this._publicAddress = null;
			this.NetProxyObject = true;
			this.NetConnectionObject = null;
			this._networkSession = new WeakReference<NetworkSession>(session);
			this.PlayerID = gmr.PlayerID;
			base.Gamertag = gmr.Gamertag;
			this._alternateAddress = 0UL;
		}

		public NetworkGamer(Gamer gmr, NetworkSession session, bool isLocal, bool isHost, byte globalID, ulong altAddress)
		{
			this._isHost = isHost;
			this._isLocal = isLocal;
			this._globalId = globalID;
			this._publicAddress = null;
			this.NetProxyObject = false;
			this.NetConnectionObject = null;
			this._networkSession = new WeakReference<NetworkSession>(session);
			this.PlayerID = gmr.PlayerID;
			base.Gamertag = gmr.Gamertag;
			this._alternateAddress = altAddress;
		}

		public IPAddress PublicAddress
		{
			get
			{
				return this._publicAddress;
			}
		}

		public ulong AlternateAddress
		{
			get
			{
				return this._alternateAddress;
			}
		}

		public bool HasLeftSession
		{
			get
			{
				return this._hasLeftSession;
			}
		}

		public bool HasVoice
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public byte Id
		{
			get
			{
				return this._globalId;
			}
		}

		public bool IsGuest
		{
			get
			{
				return false;
			}
		}

		public bool IsHost
		{
			get
			{
				return this._isHost;
			}
		}

		public bool IsLocal
		{
			get
			{
				return this._isLocal;
			}
		}

		public bool IsPrivateSlot
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool IsReady
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

		public bool IsTalking
		{
			get
			{
				return false;
			}
		}

		public NetworkMachine Machine
		{
			get
			{
				throw new NotImplementedException();
			}
			internal set
			{
				throw new NotImplementedException();
			}
		}

		public TimeSpan RoundtripTime
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public NetworkSession Session
		{
			get
			{
				return this._networkSession.Target;
			}
		}

		private WeakReference<NetworkSession> _networkSession;

		public object NetConnectionObject;

		public bool NetProxyObject;

		private IPAddress _publicAddress;

		private ulong _alternateAddress;

		private bool _hasLeftSession;

		protected byte _globalId = 1;

		private bool _isHost;

		private bool _isLocal;
	}
}
