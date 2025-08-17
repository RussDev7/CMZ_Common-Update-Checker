using System;
using DNA.Net.GamerServices;

namespace DNA.Net
{
	public class InvalidMessageException : Exception
	{
		public InvalidMessageException(NetworkGamer sender, string message)
			: base(message)
		{
			this.Sender = sender;
		}

		public InvalidMessageException(NetworkGamer sender, Exception innerException)
			: base("Invalid Message", innerException)
		{
			this.Sender = sender;
		}

		public NetworkGamer Sender;
	}
}
