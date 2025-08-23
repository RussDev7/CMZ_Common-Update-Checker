using System;
using System.Threading;

namespace DNA.Net.GamerServices
{
	public abstract class Gamer
	{
		public string Gamertag
		{
			get
			{
				return this._gamerTag;
			}
			set
			{
				this._gamerTag = value;
			}
		}

		public bool IsDisposed
		{
			get
			{
				return this._isDisposed;
			}
		}

		public static SignedInGamerCollection SignedInGamers
		{
			get
			{
				return GamerServicesComponent.Instance.SignedInGamers;
			}
		}

		public object Tag
		{
			get
			{
				return this._tag;
			}
			set
			{
				this._tag = value;
			}
		}

		public static IAsyncResult BeginGetFromGamertag(string gamertag, AsyncCallback callback, object asyncState)
		{
			throw new NotImplementedException();
		}

		public IAsyncResult BeginGetProfile(AsyncCallback callback, object asyncState)
		{
			IAsyncResult result = new Gamer.BaseAsyncResult(callback, asyncState);
			callback(result);
			return result;
		}

		public static Gamer EndGetFromGamertag(IAsyncResult result)
		{
			throw new NotImplementedException();
		}

		public GamerProfile EndGetProfile(IAsyncResult result)
		{
			return null;
		}

		public static Gamer GetFromGamertag(string gamertag)
		{
			throw new NotImplementedException();
		}

		public GamerProfile GetProfile()
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return this._gamerTag;
		}

		private string _gamerTag = "User";

		private object _tag;

		private bool _isDisposed;

		public PlayerID PlayerID = PlayerID.Null;

		private class BaseAsyncResult : IAsyncResult
		{
			public BaseAsyncResult(AsyncCallback callback, object state)
			{
				this.Callback = callback;
				this._state = state;
			}

			public object AsyncState
			{
				get
				{
					return this._state;
				}
			}

			public WaitHandle AsyncWaitHandle
			{
				get
				{
					return this.Event;
				}
			}

			public bool CompletedSynchronously
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public bool IsCompleted
			{
				get
				{
					return true;
				}
			}

			private object _state;

			public ManualResetEvent Event = new ManualResetEvent(false);

			public AsyncCallback Callback;
		}
	}
}
