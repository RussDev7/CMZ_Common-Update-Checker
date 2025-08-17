using System;
using DNA.Net.GamerServices;

namespace DNA.Net.MatchMaking
{
	public class QuerySessionInfo
	{
		public int? MaxPlayers { get; set; }

		public int? MinOpenSlots { get; set; }

		public int? MaxOpenSlots { get; set; }

		public NetworkSessionProperties SessionProperties
		{
			get
			{
				return this._props;
			}
		}

		public NetworkSessionProperties _props = new NetworkSessionProperties();
	}
}
