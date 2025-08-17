using System;

namespace DNA.Net.GamerServices
{
	public class GamerLeftEventArgs : EventArgs
	{
		public GamerLeftEventArgs(NetworkGamer gamer)
		{
			this._networkGamer = gamer;
		}

		public NetworkGamer Gamer
		{
			get
			{
				return this._networkGamer;
			}
		}

		private NetworkGamer _networkGamer;
	}
}
