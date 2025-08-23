using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	public class GraphicsDeviceLocker
	{
		public static void Create(GraphicsDeviceManager gdm)
		{
			if (GraphicsDeviceLocker.Instance == null)
			{
				GraphicsDeviceLocker.Instance = new GraphicsDeviceLocker();
			}
			gdm.DeviceResetting += GraphicsDeviceLocker.Instance.OnDeviceResetting;
			gdm.DeviceReset += GraphicsDeviceLocker.Instance.OnDeviceReset;
			gdm.DeviceCreated += GraphicsDeviceLocker.Instance.OnDeviceReset;
		}

		private void OnDeviceResetting(object sender, EventArgs args)
		{
			lock (this._lockObject)
			{
				this._resetting = true;
			}
		}

		private void OnDeviceReset(object sender, EventArgs args)
		{
			lock (this._lockObject)
			{
				this._resetting = false;
			}
		}

		public bool TryLockDeviceTimed(ref Stopwatch sw)
		{
			bool result = false;
			if (sw == null)
			{
				sw = Stopwatch.StartNew();
			}
			if (Monitor.TryEnter(this._lockObject))
			{
				if (!this._resetting || sw.ElapsedMilliseconds > 10000L)
				{
					if (sw.ElapsedMilliseconds > 10000L)
					{
						this._resetting = false;
					}
					result = true;
				}
				else
				{
					Monitor.Exit(this._lockObject);
				}
			}
			return result;
		}

		public bool TryLockDevice()
		{
			bool result = false;
			if (Monitor.TryEnter(this._lockObject))
			{
				if (!this._resetting)
				{
					result = true;
				}
				else
				{
					Monitor.Exit(this._lockObject);
				}
			}
			return result;
		}

		public void UnlockDevice()
		{
			Monitor.Exit(this._lockObject);
		}

		public static void CallbackInProtectedEnvironment(GraphicsDeviceLocker.ProtectedCallbackDelegate callback)
		{
			bool created = false;
			do
			{
				if (GraphicsDeviceLocker.Instance.TryLockDevice())
				{
					try
					{
						callback();
					}
					finally
					{
						GraphicsDeviceLocker.Instance.UnlockDevice();
					}
					created = true;
				}
				if (!created)
				{
					Thread.Sleep(10);
				}
			}
			while (!created);
		}

		public static GraphicsDeviceLocker Instance;

		private volatile bool _resetting;

		private object _lockObject = new object();

		public delegate void ProtectedCallbackDelegate();
	}
}
