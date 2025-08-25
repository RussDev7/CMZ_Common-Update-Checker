using System;

namespace DNA.Distribution
{
	public abstract class OnlineServices
	{
		public static OnlineServices Instance
		{
			get
			{
				return OnlineServices._instance;
			}
		}

		public Guid ProductID
		{
			get
			{
				return this._productID;
			}
		}

		public string Username
		{
			get
			{
				return this._username;
			}
		}

		public ulong SteamUserID
		{
			get
			{
				return this._steamUserID;
			}
		}

		public OnlineServices()
		{
			if (OnlineServices._instance != null)
			{
				throw new Exception("Instance of online service already running");
			}
			OnlineServices._instance = this;
		}

		public abstract void Update(TimeSpan elapsedTime, TimeSpan totalTime);

		private static OnlineServices _instance;

		private Guid _productID;

		protected string _username;

		protected ulong _steamUserID;
	}
}
