using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace DNA.Net.GamerServices
{
	public sealed class SignedInGamer : Gamer
	{
		public SignedInGamer(PlayerIndex pindex, PlayerID id, string playerName)
		{
			this._playerIndex = pindex;
			this.PlayerID = id;
			base.Gamertag = playerName;
		}

		public GameDefaults GameDefaults
		{
			get
			{
				return this._gameDefaults;
			}
		}

		public bool IsGuest
		{
			get
			{
				return false;
			}
		}

		public bool IsSignedInToLive
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int PartySize
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

		public PlayerIndex PlayerIndex
		{
			get
			{
				return PlayerIndex.One;
			}
		}

		public GamerPresence Presence
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public GamerPrivileges Privileges
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public static event EventHandler<SignedInEventArgs> SignedIn;

		public static event EventHandler<SignedOutEventArgs> SignedOut;

		public FriendCollection GetFriends()
		{
			return new FriendCollection(new FriendGamer[0]);
		}

		public bool IsFriend(Gamer gamer)
		{
			return false;
		}

		public bool IsHeadset(Microphone microphone)
		{
			return true;
		}

		private GameDefaults _gameDefaults = new GameDefaults();

		private PlayerIndex _playerIndex;
	}
}
