using System;

namespace DNA.Distribution
{
	public class NullOnlineServices : OnlineServices
	{
		public NullOnlineServices()
		{
			this._username = "Player";
		}

		public override void Update(TimeSpan elapsedTime, TimeSpan totalTime)
		{
		}
	}
}
