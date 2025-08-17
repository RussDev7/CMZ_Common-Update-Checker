using System;
using System.Collections.Generic;

namespace DNA.Net.GamerServices
{
	public sealed class FriendCollection : GamerCollection<FriendGamer>, IDisposable
	{
		internal FriendCollection(IList<FriendGamer> list)
			: base(list)
		{
		}

		public bool IsDisposed
		{
			get
			{
				return false;
			}
		}

		public void Dispose()
		{
		}
	}
}
