using System;
using System.IO;

namespace DNA.Net.GamerServices
{
	public sealed class GamerProfile : IDisposable
	{
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

		public Stream GetGamerPicture()
		{
			throw new NotImplementedException();
		}
	}
}
