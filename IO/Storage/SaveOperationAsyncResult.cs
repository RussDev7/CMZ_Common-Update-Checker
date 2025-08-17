using System;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework.Storage;

namespace DNA.IO.Storage
{
	internal class SaveOperationAsyncResult : IAsyncResult
	{
		public object AsyncState { get; set; }

		public WaitHandle AsyncWaitHandle { get; private set; }

		public bool CompletedSynchronously
		{
			get
			{
				return false;
			}
		}

		public bool IsCompleted
		{
			get
			{
				bool flag2;
				lock (this.accessLock)
				{
					flag2 = this.isCompleted;
				}
				return flag2;
			}
		}

		internal SaveOperationAsyncResult(StorageDevice device, string container, string file, FileAction action, FileMode mode)
		{
			this.storageDevice = device;
			this.containerName = container;
			this.fileName = file;
			this.fileAction = action;
			this.fileMode = mode;
		}

		private void EndOpenContainer(IAsyncResult result)
		{
			using (this.storageDevice.EndOpenContainer(result))
			{
				if (this.fileMode != FileMode.Create)
				{
					FileMode fileMode = this.fileMode;
				}
			}
			lock (this.accessLock)
			{
				this.isCompleted = true;
			}
		}

		private readonly object accessLock = new object();

		private bool isCompleted;

		private readonly StorageDevice storageDevice;

		private readonly string containerName;

		private readonly string fileName;

		private readonly FileAction fileAction;

		private readonly FileMode fileMode;
	}
}
