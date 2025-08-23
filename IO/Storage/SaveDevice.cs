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
				MD5HashProvider hasher = new MD5HashProvider();
				Hash hash = hasher.Compute(dataToSave);
				MemoryStream hashedStream = new MemoryStream(dataToSave.Length + hash.Data.Length + 8);
				BinaryWriter writer = new BinaryWriter(hashedStream);
				writer.Write(hash.Data.Length);
				writer.Write(hash.Data);
				writer.Write(dataToSave.Length);
				writer.Write(dataToSave);
				writer.Flush();
				dataToSave = hashedStream.ToArray();
			}
			using (Stream stream = this.DeviceOpenFile(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				BinaryWriter outWriter = new BinaryWriter(stream);
				outWriter.Write(1146311762);
				outWriter.Write(5);
				outWriter.Write((uint)fileOptions);
				outWriter.Write(dataToSave.Length);
				outWriter.Write(dataToSave);
				outWriter.Flush();
			}
		}

		public virtual void Save(string fileName, bool tamperProof, bool compressed, FileAction saveAction)
		{
			this.VerifyIsReady();
			lock (this)
			{
				MemoryStream memstream = new MemoryStream(1024);
				saveAction(memstream);
				byte[] dataToSave = memstream.ToArray();
				this.Save(fileName, dataToSave, tamperProof, compressed);
			}
		}

		public byte[] LoadData(string fileName)
		{
			this.VerifyIsReady();
			byte[] array;
			lock (this)
			{
				int version;
				SaveDevice.FileOptions fileOptions;
				byte[] data;
				using (Stream inputStream = this.DeviceOpenFile(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					BinaryReader sReader = new BinaryReader(inputStream);
					int ident = sReader.ReadInt32();
					if (ident != 1146311762)
					{
						throw new Exception();
					}
					version = sReader.ReadInt32();
					if (version < 3 || version > 5)
					{
						throw new Exception();
					}
					fileOptions = (SaveDevice.FileOptions)sReader.ReadUInt32();
					int dataLength = sReader.ReadInt32();
					data = sReader.ReadBytes(dataLength);
				}
				if ((SaveDevice.FileOptions.Encrypted & fileOptions) != SaveDevice.FileOptions.None)
				{
					MemoryStream mStream = new MemoryStream(data);
					BinaryReader reader = new BinaryReader(mStream);
					MD5HashProvider hasher = new MD5HashProvider();
					int hashLength = reader.ReadInt32();
					Hash hash = hasher.CreateHash(reader.ReadBytes(hashLength));
					int dataLength2 = reader.ReadInt32();
					data = reader.ReadBytes(dataLength2);
					Hash fileHash = hasher.Compute(data);
					if (!fileHash.Equals(hash))
					{
						throw new Exception();
					}
					if (this.LocalKey == null)
					{
						data = SecurityTools.DecryptData(SaveDevice.CommonKey, data);
					}
					else
					{
						data = SecurityTools.DecryptData(this.LocalKey, data);
					}
				}
				if ((SaveDevice.FileOptions.Compressesd & fileOptions) != SaveDevice.FileOptions.None)
				{
					data = this.compressionTools.Decompress(data);
				}
				if (version < 5)
				{
					this.Save(fileName, data, (SaveDevice.FileOptions.Compressesd & fileOptions) != SaveDevice.FileOptions.None, (SaveDevice.FileOptions.Encrypted & fileOptions) != SaveDevice.FileOptions.None);
				}
				array = data;
			}
			return array;
		}

		public void Load(string fileName, FileAction loadAction)
		{
			lock (this)
			{
				byte[] data = this.LoadData(fileName);
				MemoryStream stream = new MemoryStream(data);
				loadAction(stream);
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
			SaveDevice.FileOperationState state = this.GetFileOperationState();
			state.File = path;
			state.UserState = userState;
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoDeleteDirectoryAsync), state);
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
			string[] dirnames = this.DeviceGetDirectoryNames(Path.Combine(path, "*"));
			foreach (string dir in dirnames)
			{
				this.DeleteDirectoryInternal(dir);
			}
			string[] fileNames = this.DeviceGetFileNames(Path.Combine(path, "*"));
			foreach (string file in fileNames)
			{
				this.DeviceDeleteFile(file);
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
			SaveDevice.FileOperationState state = this.GetFileOperationState();
			state.File = fileName;
			state.TamperProof = tamperProof;
			state.Compressed = compressed;
			state.Action = saveAction;
			state.UserState = userState;
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoSaveAsync), state);
		}

		public void LoadAsync(string fileName, FileAction loadAction)
		{
			this.LoadAsync(fileName, loadAction, null);
		}

		public void LoadAsync(string fileName, FileAction loadAction, object userState)
		{
			this.PendingOperationsIncrement();
			SaveDevice.FileOperationState state = this.GetFileOperationState();
			state.File = fileName;
			state.Action = loadAction;
			state.UserState = userState;
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoLoadAsync), state);
		}

		public void DeleteAsync(string fileName)
		{
			this.DeleteAsync(fileName, null);
		}

		public void DeleteAsync(string fileName, object userState)
		{
			this.PendingOperationsIncrement();
			SaveDevice.FileOperationState state = this.GetFileOperationState();
			state.File = fileName;
			state.UserState = userState;
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoDeleteAsync), state);
		}

		public void FileExistsAsync(string fileName)
		{
			this.FileExistsAsync(fileName, null);
		}

		public void FileExistsAsync(string fileName, object userState)
		{
			this.PendingOperationsIncrement();
			SaveDevice.FileOperationState state = this.GetFileOperationState();
			state.File = fileName;
			state.UserState = userState;
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoFileExistsAsync), state);
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
			SaveDevice.FileOperationState state = this.GetFileOperationState();
			state.Pattern = pattern;
			state.UserState = userState;
			ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoGetFilesAsync), state);
		}

		private void SetProcessorAffinity()
		{
		}

		private void DoSaveAsync(object asyncState)
		{
			this.SetProcessorAffinity();
			SaveDevice.FileOperationState state = asyncState as SaveDevice.FileOperationState;
			Exception error = null;
			try
			{
				this.Save(state.File, state.TamperProof, state.Compressed, state.Action);
			}
			catch (Exception e)
			{
				error = e;
			}
			FileActionCompletedEventArgs args = new FileActionCompletedEventArgs(error, state.UserState);
			if (this.SaveCompleted != null)
			{
				this.SaveCompleted(this, args);
			}
			this.ReturnFileOperationState(state);
			this.PendingOperationsDecrement();
		}

		private void DoLoadAsync(object asyncState)
		{
			this.SetProcessorAffinity();
			SaveDevice.FileOperationState state = asyncState as SaveDevice.FileOperationState;
			Exception error = null;
			try
			{
				this.Load(state.File, state.Action);
			}
			catch (Exception e)
			{
				error = e;
			}
			FileActionCompletedEventArgs args = new FileActionCompletedEventArgs(error, state.UserState);
			if (this.LoadCompleted != null)
			{
				this.LoadCompleted(this, args);
			}
			this.ReturnFileOperationState(state);
			this.PendingOperationsDecrement();
		}

		private void DoDeleteDirectoryAsync(object asyncState)
		{
			this.SetProcessorAffinity();
			SaveDevice.FileOperationState state = asyncState as SaveDevice.FileOperationState;
			Exception error = null;
			try
			{
				this.DeleteDirectory(state.File);
			}
			catch (Exception e)
			{
				error = e;
			}
			FileActionCompletedEventArgs args = new FileActionCompletedEventArgs(error, state.UserState);
			if (this.DeleteDirectoryCompleted != null)
			{
				this.DeleteDirectoryCompleted(this, args);
			}
			this.ReturnFileOperationState(state);
			this.PendingOperationsDecrement();
		}

		private void DoDeleteAsync(object asyncState)
		{
			this.SetProcessorAffinity();
			SaveDevice.FileOperationState state = asyncState as SaveDevice.FileOperationState;
			Exception error = null;
			try
			{
				this.Delete(state.File);
			}
			catch (Exception e)
			{
				error = e;
			}
			FileActionCompletedEventArgs args = new FileActionCompletedEventArgs(error, state.UserState);
			if (this.DeleteCompleted != null)
			{
				this.DeleteCompleted(this, args);
			}
			this.ReturnFileOperationState(state);
			this.PendingOperationsDecrement();
		}

		private void DoFileExistsAsync(object asyncState)
		{
			this.SetProcessorAffinity();
			SaveDevice.FileOperationState state = asyncState as SaveDevice.FileOperationState;
			Exception error = null;
			bool result = false;
			try
			{
				result = this.FileExists(state.File);
			}
			catch (Exception e)
			{
				error = e;
			}
			FileExistsCompletedEventArgs args = new FileExistsCompletedEventArgs(error, result, state.UserState);
			if (this.FileExistsCompleted != null)
			{
				this.FileExistsCompleted(this, args);
			}
			this.ReturnFileOperationState(state);
			this.PendingOperationsDecrement();
		}

		private void DoGetFilesAsync(object asyncState)
		{
			this.SetProcessorAffinity();
			SaveDevice.FileOperationState state = asyncState as SaveDevice.FileOperationState;
			Exception error = null;
			string[] result = null;
			try
			{
				result = this.GetFiles(state.Pattern);
			}
			catch (Exception e)
			{
				error = e;
			}
			GetFilesCompletedEventArgs args = new GetFilesCompletedEventArgs(error, result, state.UserState);
			if (this.GetFilesCompleted != null)
			{
				this.GetFilesCompleted(this, args);
			}
			this.ReturnFileOperationState(state);
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
			SaveDevice.FileOperationState fileOperationState;
			lock (this.pendingStates)
			{
				if (this.pendingStates.Count > 0)
				{
					SaveDevice.FileOperationState state = this.pendingStates.Dequeue();
					state.Reset();
					fileOperationState = state;
				}
				else
				{
					fileOperationState = new SaveDevice.FileOperationState();
				}
			}
			return fileOperationState;
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
