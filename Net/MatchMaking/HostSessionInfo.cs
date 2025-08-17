using System;
using DNA.Net.GamerServices;

namespace DNA.Net.MatchMaking
{
	public class HostSessionInfo
	{
		public string Name { get; set; }

		public Guid SessionID { get; set; }

		public ulong AltSessionID { get; set; }

		public bool PasswordProtected { get; set; }

		public JoinGamePolicy JoinGamePolicy { get; set; }

		public bool IsPublic
		{
			get
			{
				return this.JoinGamePolicy == JoinGamePolicy.Anyone;
			}
			set
			{
				this.JoinGamePolicy = (value ? JoinGamePolicy.Anyone : JoinGamePolicy.FriendsOnly);
			}
		}

		public NetworkSessionProperties SessionProperties
		{
			get
			{
				return this._props;
			}
			set
			{
				this._props = value;
			}
		}

		private NetworkSessionProperties _props = new NetworkSessionProperties();
	}
}
