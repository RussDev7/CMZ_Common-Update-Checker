using System;

namespace DNA.Net.GamerServices
{
	public class HostChangedEventArgs : EventArgs
	{
		public HostChangedEventArgs(NetworkGamer oldHost, NetworkGamer newHost)
		{
			throw new NotImplementedException();
		}

		public NetworkGamer NewHost
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public NetworkGamer OldHost
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
}
