using System;

namespace DNA.Net.GamerServices
{
	public class NetworkSessionEndedEventArgs : EventArgs
	{
		public NetworkSessionEndedEventArgs(NetworkSessionEndReason endReason)
		{
			this._endReason = endReason;
		}

		public NetworkSessionEndReason EndReason
		{
			get
			{
				return this._endReason;
			}
		}

		private NetworkSessionEndReason _endReason;
	}
}
