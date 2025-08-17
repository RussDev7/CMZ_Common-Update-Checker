using System;

namespace DNA.Net.GamerServices
{
	public class GamerJoinedEventArgs : EventArgs
	{
		public GamerJoinedEventArgs(NetworkGamer gamer)
		{
			this._gamer = gamer;
		}

		public NetworkGamer Gamer
		{
			get
			{
				return this._gamer;
			}
		}

		private NetworkGamer _gamer;
	}
}
