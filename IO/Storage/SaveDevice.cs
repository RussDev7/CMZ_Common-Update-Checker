using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using DNA.IO.Compression;
using DNA.Security;
using DNA.Security.Cryptography;

namespace DNA.IO.Storage
{
	public abstract class SaveDevice : IDisposable
	{
		public SaveDevice(byte[] key)
		{
			this.LocalKey = key;
		}

		protected abstract Stream DeviceOpenFile(string fileName, FileMode mode, FileAccess access, FileShare share);

		protected abstract void DeviceDeleteFile(string fileName);

		protected abstract bool DeviceFileExists(string fileName);

		protected abstract bool DeviceDirectoryExists(string dirName);

		protected abstract string[] DeviceGetDirectoryNames();

		protected abstract string[] DeviceGetDirectoryNames(string pattern);

		protected abstract string[] DeviceGetFileNames();

		protected abstract string[] DeviceGetFileNames(string pattern);

		protected abstract void DeviceCreateDirectory(string path);

		protected abstract void DeviceDeleteDirectory(string path);

		public abstract void Flush();

		public abstract void DeleteStorage();

		public abstract void DeviceDispose();

		protected virtual void VerifyIsReady()
		{
		}

		public void Save(string fileName, byte[] dataToSave, bool tamperProof, bool compressed)
		{
			SaveDevice.FileOptions fileOptions = SaveDevice.FileOptions.None;
			if (compressed)
			{
				fileOptions |= SaveDevice.FileOptions.Compressesd;
				dataToSave = this.compressionTools.Compress(dataToSave);
			}
			if (tamperProof)
			{
				fileOptions |= SaveDevice.FileOptions.Encrypted;
				if (this.LocalKey == null)
				{
					dataToSave = SecurityTools.EncryptData(SaveDevice.CommonKey, dataToSave);
				}
				else
				{
					dataToSave = SecurityTools.EncryptData(this.LocalKey, dataToSave);
				}
				MD5HashProvider md5HashProvider = new MD5HashProvider();
				Hash hash = md5HashProvider.Compute(dataToSave);
				MemoryStream memoryStream = new MemoryStream(dataToSave.Length + hash.Data.Length + 8);
				BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write(hash.Data.Length);
				binaryWriter.Write(hash.Data);
				binaryWriter.Write(dataToSave.Length);
				binaryWriter.Write(dataToSave);
				binaryWriter.Flush();
				dataToSave = memoryStream.ToArray();
			}
			using (Stream stream = this.DeviceOpenFile(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				BinaryWriter binaryWriter2 = new BinaryWriter(stream);
				binaryWriter2.Write(1146311762);
				binaryWriter2.Write(5);
				binaryWriter2.Write((uint)fileOptions);
				binaryWriter2.Write(dataToSave.Length);
				binaryWriter2.Write(dataToSave);
				binaryWriter2.Flush();
			}
		}

		public virtual void Save(string fileName, bool tamperProof, bool compressed, FileAction saveAction)
		{
			this.VerifyIsReady();
			lock (this)
			{
				MemoryStream memoryStream = new MemoryStream(1024);
				saveAction(memoryStream);
				byte[] array = memoryStream.ToArray();
				this.Save(fileName, array, tamperProof, compressed);
			}
		}

		public byte[] LoadData(string fileName)
		{
			this.VerifyIsReady();
			byte[] array2;
			lock (this)
			{
				int num2;
				SaveDevice.FileOptions fileOptions;
				byte[] array;
				using (Stream stream = this.DeviceOpenFile(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					BinaryReader binaryReader = new BinaryReader(stream);
					int num = binaryReader.ReadInt32();
					if (num != 1146311762)
					{
						throw new Exception();
					}
					num2 = binaryReader.ReadInt32();
					if (num2 < 3 || num2 > 5)
					{
						throw new Exception();
					}
					fileOptions = (SaveDevice.FileOptions)binaryReader.ReadUInt32();
					int num3 = binaryReader.ReadInt32();
					array = binaryReader.ReadBytes(num3);
				}
				if ((SaveDevice.FileOptions.Encrypted & fileOptions) != SaveDevice.FileOptions.None)
				{
					MemoryStream memoryStream = new MemoryStream(array);
					BinaryReader binaryReader2 = new BinaryReader(memoryStream);
					MD5HashProvider md5HashProvider = new MD5HashProvider();
					int num4 = binaryReader2.ReadInt32();
					Hash hash = md5HashProvider.CreateHash(binaryReader2.ReadBytes(num4));
					int num5 = binaryReader2.ReadInt32();
					array = binaryReader2.ReadBytes(num5);
					Hash hash2 = md5HashProvider.Compute(array);
					if (!hash2.Equals(hash))
					{
						throw new Exception();
					}
					if (this.LocalKey == null)
					{
						array = SecurityTools.DecryptData(SaveDevice.CommonKey, array);
					}
					else
					{
						array = SecurityTools.DecryptData(this.LocalKey, array);
					}
				}
				if ((SaveDevice.FileOptions.Compressesd & fileOptions) != SaveDevice.FileOptions.None)
				{
					array = this.compressionTools.Decompress(array);
				}
				if (num2 < 5)
				{
					this.Save(fileName, array, (SaveDevice.FileOptions.Compressesd & fileOptions) != SaveDevice.FileOptions.None, (SaveDevice.FileOptions.Encrypted & fileOptions) != SaveDevice.FileOptions.None);
				}
				array2 = array;
			}
			return array2;
		}

		public void Load(string fileName, FileAction loadAction)
		{
			lock (this)
			{
				byte[] array = this.LoadData(fileName);
				MemoryStream memoryStream = new MemoryStream(array);
				loadAction(memoryStream);
			}
		}

		public void Delete(string fileName)
		{
			this.VerifyIsReady();
			lock (this)
			{
				if (this.DeviceFileExists(fileName))
				{
					this.DeviceDeleteFile(fileName);
				}
			}
		}

		public bool FileExists(string fileName)
		{
			this.VerifyIsReady();
			bool flag2;
			lock (this)
			{
				flag2 = this.DeviceFileExists(fileName);
			}
			return flag2;
		}

		public string[] GetFiles()
		{
			return this.GetFiles(null);
		}

		public string[] GetFiles(string pattern)
		{
			this.VerifyIsReady();
			string[] array;
			lock (this)
			{
				array = (string.IsNullOrEmpty(pattern) ? this.DeviceGetFileNames() : this.DeviceGetFileNames(pattern));
			}
			return array;
		}

		public void GetDirectoriesAsync(string path)
		{
			throw new NotImplementedException();
		}

		public void GetDirectoriesAsync(string path, string pattern)
		{
			throw new NotImplementedException();
		}

		public void CreateDirectoryAsync(string path)
		{
			throw new NotImplementedException();
		}

		public void DeleteDirectoryAsync(string path)
		{
			this.DeleteDirectoryAsync(path, null);
		}

		public void DeleteDirectoryAsync(string path, object userState)
		{
			this.PendingOperationsIncrement();
			SaveDevice.FileOperationState fileOperationState = this.GetFileOperationState();
			fileOperationState.File = path;
			fileOperationState.UserState = userState;
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoDeleteDirectoryAsync), fileOperationState);
		}

		public string[] GetDirectories(string path)
		{
			return this.GetDirectories(path, null);
		}

		public string[] GetDirectories(string path, string pattern)
		{
			this.VerifyIsReady();
			string[] array;
			lock (this)
			{
				if (string.IsNullOrEmpty(pattern))
				{
					pattern = "*";
				}
				if (string.IsNullOrEmpty(path))
				{
					path = pattern;
				}
				else
				{
					path = Path.Combine(path, pattern);
				}
				array = this.DeviceGetDirectoryNames(path);
			}
			return array;
		}

		public void CreateDirectory(string path)
		{
			this.VerifyIsReady();
			lock (this)
			{
				this.DeviceCreateDirectory(path);
			}
		}

		public void DeleteDirectory(string path)
		{
			this.VerifyIsReady();
			lock (this)
			{
				this.DeleteDirectoryInternal(path);
			}
		}

		private void DeleteDirectoryInternal(string path)
		{
			string[] array = this.DeviceGetDirectoryNames(Path.Combine(path, "*"));
			foreach (string text in array)
			{
				this.DeleteDirectoryInternal(text);
			}
			string[] array3 = this.DeviceGetFileNames(Path.Combine(path, "*"));
			foreach (string text2 in array3)
			{
				this.DeviceDeleteFile(text2);
			}
			this.DeviceDeleteDirectory(path);
		}

		public void DirectoryExistsAsync(string path)
		{
			throw new NotImplementedException();
		}

		public bool DirectoryExists(string path)
		{
			this.VerifyIsReady();
			bool flag2;
			lock (this)
			{
				flag2 = this.DeviceDirectoryExists(path);
			}
			return flag2;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					this.DeviceDispose();
				}
				this.disposed = true;
			}
		}

		public void SaveRaw(string fileName, FileAction saveAction)
		{
			this.VerifyIsReady();
			lock (this)
			{
				using (Stream stream = this.DeviceOpenFile(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					saveAction(stream);
				}
			}
		}

		public void LoadRaw(string fileName, FileAction loadAction)
		{
			this.VerifyIsReady();
			lock (this)
			{
				using (Stream stream = this.DeviceOpenFile(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					loadAction(stream);
				}
			}
		}

		public bool IsBusy
		{
			get
			{
				bool flag2;
				lock (this.pendingOperationCountLock)
				{
					flag2 = this.pendingOperations > 0;
				}
				return flag2;
			}
		}

		public event SaveCompletedEventHandler SaveCompleted;

		public event LoadCompletedEventHandler LoadCompleted;

		public event DeleteDirectoryCompletedEventHandler DeleteDirectoryCompleted;

		public event DeleteCompletedEventHandler DeleteCompleted;

		public event FileExistsCompletedEventHandler FileExistsCompleted;

		public event GetFilesCompletedEventHandler GetFilesCompleted;

		public void SaveAsync(string fileName, bool tamperProof, bool compressed, FileAction saveAction)
		{
			this.SaveAsync(fileName, tamperProof, compressed, saveAction, null);
		}

		public void SaveAsync(string fileName, bool tamperProof, bool compressed, FileAction saveAction, object userState)
		{
			this.PendingOperationsIncrement();
			SaveDevice.FileOperationState fileOperationState = this.GetFileOperationState();
			fileOperationState.File = fileName;
			fileOperationState.TamperProof = tamperProof;
			fileOperationState.Compressed = compressed;
			fileOperationState.Action = saveAction;
			fileOperationState.UserState = userState;
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoSaveAsync), fileOperationState);
		}

		public void LoadAsync(string fileName, FileAction loadAction)
		{
			this.LoadAsync(fileName, loadAction, null);
		}

		public void LoadAsync(string fileName, FileAction loadAction, object userState)
		{
			this.PendingOperationsIncrement();
			SaveDevice.FileOperationState fileOperationState = this.GetFileOperationState();
			fileOperationState.File = fileName;
			fileOperationState.Action = loadAction;
			fileOperationState.UserState = userState;
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoLoadAsync), fileOperationState);
		}

		public void DeleteAsync(string fileName)
		{
			this.DeleteAsync(fileName, null);
		}

		public void DeleteAsync(string fileName, object userState)
		{
			this.PendingOperationsIncrement();
			SaveDevice.FileOperationState fileOperationState = this.GetFileOperationState();
			fileOperationState.File = fileName;
			fileOperationState.UserState = userState;
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoDeleteAsync), fileOperationState);
		}

		public void FileExistsAsync(string fileName)
		{
			this.FileExistsAsync(fileName, null);
		}

		public void FileExistsAsync(string fileName, object userState)
		{
			this.PendingOperationsIncrement();
			SaveDevice.FileOperationState fileOperationState = this.GetFileOperationState();
			fileOperationState.File = fileName;
			fileOperationState.UserState = userState;
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoFileExistsAsync), fileOperationState);
		}

		public void GetFilesAsync()
		{
			this.GetFilesAsync(null);
		}

		public void GetFilesAsync(object userState)
		{
			this.GetFilesAsync("*", userState);
		}

		public void GetFilesAsync(string pattern)
		{
			this.GetFilesAsync(pattern, null);
		}

		public void GetFilesAsync(string pattern, object userState)
		{
			this.PendingOperationsIncrement();
			SaveDevice.FileOperationState fileOperationState = this.GetFileOperationState();
			fileOperationState.Pattern = pattern;
			fileOperationState.UserState = userState;
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoGetFilesAsync), fileOperationState);
		}

		private void SetProcessorAffinity()
		{
		}

		private void DoSaveAsync(object asyncState)
		{
			this.SetProcessorAffinity();
			SaveDevice.FileOperationState fileOperationState = asyncState as SaveDevice.FileOperationState;
			Exception ex = null;
			try
			{
				this.Save(fileOperationState.File, fileOperationState.TamperProof, fileOperationState.Compressed, fileOperationState.Action);
			}
			catch (Exception ex2)
			{
				ex = ex2;
			}
			FileActionCompletedEventArgs fileActionCompletedEventArgs = new FileActionCompletedEventArgs(ex, fileOperationState.UserState);
			if (this.SaveCompleted != null)
			{
				this.SaveCompleted(this, fileActionCompletedEventArgs);
			}
			this.ReturnFileOperationState(fileOperationState);
			this.PendingOperationsDecrement();
		}

		private void DoLoadAsync(object asyncState)
		{
			this.SetProcessorAffinity();
			SaveDevice.FileOperationState fileOperationState = asyncState as SaveDevice.FileOperationState;
			Exception ex = null;
			try
			{
				this.Load(fileOperationState.File, fileOperationState.Action);
			}
			catch (Exception ex2)
			{
				ex = ex2;
			}
			FileActionCompletedEventArgs fileActionCompletedEventArgs = new FileActionCompletedEventArgs(ex, fileOperationState.UserState);
			if (this.LoadCompleted != null)
			{
				this.LoadCompleted(this, fileActionCompletedEventArgs);
			}
			this.ReturnFileOperationState(fileOperationState);
			this.PendingOperationsDecrement();
		}

		private void DoDeleteDirectoryAsync(object asyncState)
		{
			this.SetProcessorAffinity();
			SaveDevice.FileOperationState fileOperationState = asyncState as SaveDevice.FileOperationState;
			Exception ex = null;
			try
			{
				this.DeleteDirectory(fileOperationState.File);
			}
			catch (Exception ex2)
			{
				ex = ex2;
			}
			FileActionCompletedEventArgs fileActionCompletedEventArgs = new FileActionCompletedEventArgs(ex, fileOperationState.UserState);
			if (this.DeleteDirectoryCompleted != null)
			{
				this.DeleteDirectoryCompleted(this, fileActionCompletedEventArgs);
			}
			this.ReturnFileOperationState(fileOperationState);
			this.PendingOperationsDecrement();
		}

		private void DoDeleteAsync(object asyncState)
		{
			this.SetProcessorAffinity();
			SaveDevice.FileOperationState fileOperationState = asyncState as SaveDevice.FileOperationState;
			Exception ex = null;
			try
			{
				this.Delete(fileOperationState.File);
			}
			catch (Exception ex2)
			{
				ex = ex2;
			}
			FileActionCompletedEventArgs fileActionCompletedEventArgs = new FileActionCompletedEventArgs(ex, fileOperationState.UserState);
			if (this.DeleteCompleted != null)
			{
				this.DeleteCompleted(this, fileActionCompletedEventArgs);
			}
			this.ReturnFileOperationState(fileOperationState);
			this.PendingOperationsDecrement();
		}

		private void DoFileExistsAsync(object asyncState)
		{
			this.SetProcessorAffinity();
			SaveDevice.FileOperationState fileOperationState = asyncState as SaveDevice.FileOperationState;
			Exception ex = null;
			bool flag = false;
			try
			{
				flag = this.FileExists(fileOperationState.File);
			}
			catch (Exception ex2)
			{
				ex = ex2;
			}
			FileExistsCompletedEventArgs fileExistsCompletedEventArgs = new FileExistsCompletedEventArgs(ex, flag, fileOperationState.UserState);
			if (this.FileExistsCompleted != null)
			{
				this.FileExistsCompleted(this, fileExistsCompletedEventArgs);
			}
			this.ReturnFileOperationState(fileOperationState);
			this.PendingOperationsDecrement();
		}

		private void DoGetFilesAsync(object asyncState)
		{
			this.SetProcessorAffinity();
			SaveDevice.FileOperationState fileOperationState = asyncState as SaveDevice.FileOperationState;
			Exception ex = null;
			string[] array = null;
			try
			{
				array = this.GetFiles(fileOperationState.Pattern);
			}
			catch (Exception ex2)
			{
				ex = ex2;
			}
			GetFilesCompletedEventArgs getFilesCompletedEventArgs = new GetFilesCompletedEventArgs(ex, array, fileOperationState.UserState);
			if (this.GetFilesCompleted != null)
			{
				this.GetFilesCompleted(this, getFilesCompletedEventArgs);
			}
			this.ReturnFileOperationState(fileOperationState);
			this.PendingOperationsDecrement();
		}

		private void PendingOperationsIncrement()
		{
			lock (this.pendingOperationCountLock)
			{
				this.pendingOperations++;
			}
		}

		private void PendingOperationsDecrement()
		{
			lock (this.pendingOperationCountLock)
			{
				this.pendingOperations--;
			}
		}

		private SaveDevice.FileOperationState GetFileOperationState()
		{
			SaveDevice.FileOperationState fileOperationState2;
			lock (this.pendingStates)
			{
				if (this.pendingStates.Count > 0)
				{
					SaveDevice.FileOperationState fileOperationState = this.pendingStates.Dequeue();
					fileOperationState.Reset();
					fileOperationState2 = fileOperationState;
				}
				else
				{
					fileOperationState2 = new SaveDevice.FileOperationState();
				}
			}
			return fileOperationState2;
		}

		private void ReturnFileOperationState(SaveDevice.FileOperationState state)
		{
			lock (this.pendingStates)
			{
				this.pendingStates.Enqueue(state);
			}
		}

		~SaveDevice()
		{
			this.Dispose(false);
		}

		private const int FileIdent = 1146311762;

		private const int FileVersion = 5;

		private static byte[] CommonKey = new byte[]
		{
			236, 34, 252, 119, 2, 225, 246, 242, 214, 172,
			157, 191, 175, 246, 57, 246
		};

		private byte[] LocalKey;

		public static readonly int[] ProcessorAffinity = new int[] { 5 };

		private CompressionTools compressionTools = new CompressionTools();

		private bool disposed;

		private Queue<SaveDevice.FileOperationState> pendingStates = new Queue<SaveDevice.FileOperationState>(100);

		private readonly object pendingOperationCountLock = new object();

		private int pendingOperations;

		[Flags]
		private enum FileOptions : uint
		{
			None = 0U,
			Compressesd = 1U,
			Encrypted = 2U
		}

		private class FileOperationState
		{
			public void Reset()
			{
				this.TamperProof = false;
				this.Compressed = false;
				this.File = null;
				this.Pattern = null;
				this.Action = null;
				this.UserState = null;
			}

			public string File;

			public string Pattern;

			public bool TamperProof;

			public bool Compressed;

			public FileAction Action;

			public object UserState;
		}
	}
}
