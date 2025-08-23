using System;
using System.IO;
using DNA.Threading;

namespace DNA.IO
{
	public static class StreamTools
	{
		public static void ReadFile(this Stream destination, string sourcePath)
		{
			using (FileStream stream = File.Open(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				destination.CopyStream(stream);
			}
		}

		public static void CopyStream(this Stream destination, Stream source)
		{
			destination.CopyStream(source, source.Position, source.Length - source.Position, null);
		}

		public static void CopyStream(this Stream destination, Stream source, IProgressMonitor monitor)
		{
			destination.CopyStream(source, source.Position, source.Length - source.Position, monitor);
		}

		public static void CopyStream(this Stream destination, Stream source, long length)
		{
			destination.CopyStream(source, source.Position, length, null);
		}

		public static void CopyStream(this Stream destination, Stream source, long length, IProgressMonitor monitor)
		{
			destination.CopyStream(source, source.Position, length, monitor);
		}

		public static void ShiftStream(this Stream stream, int shiftBytes)
		{
			if (shiftBytes == 0)
			{
				return;
			}
			byte[] buffer = new byte[4096];
			int bytesLeft = (int)(stream.Length - stream.Position);
			if (shiftBytes > 0)
			{
				long position = stream.Length;
				stream.SetLength(stream.Length + (long)shiftBytes);
				while (bytesLeft > 4096)
				{
					stream.Position = position - 4096L;
					stream.Read(buffer, 0, 4096);
					stream.Position = position + (long)shiftBytes;
					stream.Write(buffer, 0, 4096);
					position -= 4096L;
					bytesLeft -= bytesLeft;
				}
				stream.Position = position - (long)bytesLeft;
				stream.Read(buffer, 0, bytesLeft);
				stream.Position = position + (long)shiftBytes;
				stream.Write(buffer, 0, bytesLeft);
				return;
			}
			long position2 = stream.Position;
			while (bytesLeft > 4096)
			{
				stream.Position = position2 - 4096L;
				stream.Read(buffer, 0, 4096);
				stream.Position = position2 + (long)shiftBytes;
				stream.Write(buffer, 0, 4096);
				position2 += 4096L;
				bytesLeft -= bytesLeft;
			}
			stream.Position = position2 - (long)bytesLeft;
			stream.Read(buffer, 0, bytesLeft);
			stream.Position = position2 + (long)shiftBytes;
			stream.Write(buffer, 0, bytesLeft);
			stream.SetLength(stream.Length + (long)shiftBytes);
		}

		public static void CopyStream(this Stream destination, Stream source, long startPosition, long length, IProgressMonitor progress)
		{
			if (progress != null)
			{
				progress.StatusText = "Copying Streams";
			}
			byte[] buffer = new byte[4096];
			long bytesLeft = length;
			long position = 0L;
			int count = 0;
			int chunksize = (int)((bytesLeft < 4096L) ? bytesLeft : 4096L);
			source.Position = startPosition;
			while (bytesLeft > 0L)
			{
				int currentLength = source.Read(buffer, 0, chunksize);
				if (currentLength == 0)
				{
					throw new Exception("Stream Terminated early");
				}
				destination.Write(buffer, 0, currentLength);
				position += (long)currentLength;
				bytesLeft -= (long)currentLength;
				if (progress != null)
				{
					count++;
					if (count == 10)
					{
						progress.Complete = Percentage.FromFraction((float)position / (float)length);
						count = 0;
					}
				}
				chunksize = (int)((bytesLeft < 4096L) ? bytesLeft : 4096L);
			}
			if (progress != null)
			{
				progress.Complete = Percentage.FromFraction(1f);
			}
			destination.Flush();
		}
	}
}
