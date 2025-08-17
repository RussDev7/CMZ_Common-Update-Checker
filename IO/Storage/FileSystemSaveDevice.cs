using System;
using System.IO;

namespace DNA.IO.Storage
{
	public class FileSystemSaveDevice : SaveDevice
	{
		public FileSystemSaveDevice(string rootPath, byte[] key)
			: base(key)
		{
			this._rootPath = Path.GetFullPath(rootPath);
			if (this._rootPath[this._rootPath.Length - 1] != Path.DirectorySeparatorChar)
			{
				this._rootPath += Path.DirectorySeparatorChar;
			}
			if (!Directory.Exists(this._rootPath))
			{
				Directory.CreateDirectory(this._rootPath);
			}
		}

		private string MakeRootRelative(string path)
		{
			return Path.Combine(this._rootPath, path);
		}

		protected override Stream DeviceOpenFile(string fileName, FileMode mode, FileAccess access, FileShare share)
		{
			fileName = this.MakeRootRelative(fileName);
			return File.Open(fileName, mode, access, share);
		}

		protected override void DeviceDeleteFile(string fileName)
		{
			File.Delete(this.MakeRootRelative(fileName));
		}

		protected override bool DeviceFileExists(string fileName)
		{
			return File.Exists(this.MakeRootRelative(fileName));
		}

		protected override bool DeviceDirectoryExists(string dirName)
		{
			return Directory.Exists(this.MakeRootRelative(dirName));
		}

		protected override string[] DeviceGetDirectoryNames()
		{
			string[] directories = Directory.GetDirectories(this._rootPath);
			for (int i = 0; i < directories.Length; i++)
			{
				directories[i] = directories[i].Substring(this._rootPath.Length);
			}
			return directories;
		}

		protected override string[] DeviceGetDirectoryNames(string pattern)
		{
			pattern = this.MakeRootRelative(pattern);
			string directoryName = Path.GetDirectoryName(pattern);
			string fileName = Path.GetFileName(pattern);
			string[] array;
			if (string.IsNullOrEmpty(fileName))
			{
				array = Directory.GetDirectories(directoryName);
			}
			else
			{
				array = Directory.GetDirectories(directoryName, fileName);
			}
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].Substring(this._rootPath.Length);
			}
			return array;
		}

		protected override string[] DeviceGetFileNames()
		{
			string[] files = Directory.GetFiles(this._rootPath);
			for (int i = 0; i < files.Length; i++)
			{
				files[i] = files[i].Substring(this._rootPath.Length);
			}
			return files;
		}

		protected override string[] DeviceGetFileNames(string pattern)
		{
			pattern = this.MakeRootRelative(pattern);
			string directoryName = Path.GetDirectoryName(pattern);
			string fileName = Path.GetFileName(pattern);
			string[] array;
			if (string.IsNullOrEmpty(fileName))
			{
				array = Directory.GetFiles(directoryName);
			}
			else
			{
				array = Directory.GetFiles(directoryName, fileName);
			}
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].Substring(this._rootPath.Length);
			}
			return array;
		}

		protected override void DeviceCreateDirectory(string path)
		{
			Directory.CreateDirectory(this.MakeRootRelative(path));
		}

		protected override void DeviceDeleteDirectory(string path)
		{
			Directory.Delete(this.MakeRootRelative(path), true);
		}

		public override void Flush()
		{
		}

		public override void DeleteStorage()
		{
			try
			{
				Directory.Delete(this._rootPath, true);
				Directory.CreateDirectory(this._rootPath);
			}
			catch
			{
			}
		}

		public override void DeviceDispose()
		{
		}

		private string _rootPath;
	}
}
