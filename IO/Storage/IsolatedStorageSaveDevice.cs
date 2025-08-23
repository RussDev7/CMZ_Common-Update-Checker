using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text.RegularExpressions;
using DNA.Net.GamerServices;

namespace DNA.IO.Storage
{
	public class IsolatedStorageSaveDevice : SaveDevice
	{
		public PlayerISOSaveDevice GetPlayerISOSaveDevice(SignedInGamer gamer, byte[] key)
		{
			return new PlayerISOSaveDevice(gamer, key, this._currentContainer);
		}

		public IsolatedStorageSaveDevice(byte[] key)
			: base(key)
		{
			this._currentContainer = IsolatedStorageSaveDevice.GetIsolatedStorage();
		}

		public bool DoesPlayerStorageExist(SignedInGamer gamer)
		{
			return this._currentContainer.DirectoryExists(gamer.Gamertag);
		}

		private static IsolatedStorageFile GetIsolatedStorage()
		{
			return IsolatedStorageFile.GetUserStoreForDomain();
		}

		public override void Flush()
		{
			lock (this)
			{
				if (this._currentContainer != null)
				{
					this._currentContainer.Dispose();
					try
					{
						this._currentContainer = IsolatedStorageSaveDevice.GetIsolatedStorage();
					}
					catch
					{
						this._currentContainer = null;
					}
				}
			}
		}

		public override void DeviceDispose()
		{
			if (this._currentContainer != null)
			{
				this._currentContainer.Dispose();
				this._currentContainer = null;
			}
		}

		public override void DeleteStorage()
		{
			lock (this)
			{
				if (this._currentContainer != null)
				{
					try
					{
						this._currentContainer.Remove();
						this._currentContainer.Dispose();
						this._currentContainer = IsolatedStorageSaveDevice.GetIsolatedStorage();
					}
					catch
					{
						this._currentContainer = null;
					}
				}
			}
		}

		protected override Stream DeviceOpenFile(string fileName, FileMode mode, FileAccess access, FileShare share)
		{
			return this._currentContainer.OpenFile(fileName, mode);
		}

		protected override void DeviceDeleteFile(string fileName)
		{
			this._currentContainer.DeleteFile(fileName);
		}

		protected override bool DeviceFileExists(string fileName)
		{
			return this._currentContainer.FileExists(fileName);
		}

		protected override bool DeviceDirectoryExists(string dirName)
		{
			return this._currentContainer.DirectoryExists(dirName);
		}

		protected override string[] DeviceGetDirectoryNames()
		{
			return this._currentContainer.GetDirectoryNames();
		}

		protected override string[] DeviceGetFileNames()
		{
			return this._currentContainer.GetFileNames();
		}

		protected override string[] DeviceGetFileNames(string pattern)
		{
			string[] paths = this._currentContainer.GetFileNames(pattern);
			return IsolatedStorageSaveDevice.FilterAndAppend(pattern, paths);
		}

		private static string[] FilterAndAppend(string pattern, string[] paths)
		{
			string dir = Path.GetDirectoryName(pattern);
			string fileName = Path.GetFileName(pattern);
			Regex search = null;
			if (!string.IsNullOrEmpty(fileName))
			{
				search = PathTools.FilePatternToRegex(fileName);
			}
			List<string> results = new List<string>();
			for (int i = 0; i < paths.Length; i++)
			{
				paths[i] = Path.Combine(dir, paths[i]);
				string file = Path.GetFileName(paths[i]);
				if (search == null || search.IsMatch(file))
				{
					results.Add(Path.Combine(dir, file));
				}
			}
			return results.ToArray();
		}

		protected override string[] DeviceGetDirectoryNames(string pattern)
		{
			string[] paths = this._currentContainer.GetDirectoryNames(pattern);
			return IsolatedStorageSaveDevice.FilterAndAppend(pattern, paths);
		}

		protected override void DeviceCreateDirectory(string path)
		{
			this._currentContainer.CreateDirectory(path);
		}

		protected override void DeviceDeleteDirectory(string path)
		{
			this._currentContainer.DeleteDirectory(path);
		}

		protected IsolatedStorageFile _currentContainer;
	}
}
