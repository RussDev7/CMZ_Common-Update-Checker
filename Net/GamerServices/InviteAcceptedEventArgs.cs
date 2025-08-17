using System;

namespace DNA.Net.GamerServices
{
	public class InviteAcceptedEventArgs : EventArgs
	{
		public InviteAcceptedEventArgs(SignedInGamer gamer, bool isCurrentSession)
		{
			this._gamer = gamer;
		}

		public SignedInGamer Gamer
		{
			get
			{
				return this._gamer;
			}
		}

		public bool IsCurrentSession
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public ulong LobbyId;

		public ulong InviterId;

		private SignedInGamer _gamer;
	}
}
