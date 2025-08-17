using System;
using System.IO;
using DNA.Threading;

namespace DNA.IO
{
	public static class StreamTools
	{
		public static void ReadFile(this Stream destination, string sourcePath)
		{
			using (FileStream fileStream = File.Open(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				destination.CopyStream(fileStream);
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
			byte[] array = new byte[4096];
			int i = (int)(stream.Length - stream.Position);
			if (shiftBytes > 0)
			{
				long num = stream.Length;
				stream.SetLength(stream.Length + (long)shiftBytes);
				while (i > 4096)
				{
					stream.Position = num - 4096L;
					stream.Read(array, 0, 4096);
					stream.Position = num + (long)shiftBytes;
					stream.Write(array, 0, 4096);
					num -= 4096L;
					i -= i;
				}
				stream.Position = num - (long)i;
				stream.Read(array, 0, i);
				stream.Position = num + (long)shiftBytes;
				stream.Write(array, 0, i);
				return;
			}
			long num2 = stream.Position;
			while (i > 4096)
			{
				stream.Position = num2 - 4096L;
				stream.Read(array, 0, 4096);
				stream.Position = num2 + (long)shiftBytes;
				stream.Write(array, 0, 4096);
				num2 += 4096L;
				i -= i;
			}
			stream.Position = num2 - (long)i;
			stream.Read(array, 0, i);
			stream.Position = num2 + (long)shiftBytes;
			stream.Write(array, 0, i);
			stream.SetLength(stream.Length + (long)shiftBytes);
		}

		public static void CopyStream(this Stream destination, Stream source, long startPosition, long length, IProgressMonitor progress)
		{
			if (progress != null)
			{
				progress.StatusText = "Copying Streams";
			}
			byte[] array = new byte[4096];
			long num = length;
			long num2 = 0L;
			int num3 = 0;
			int num4 = (int)((num < 4096L) ? num : 4096L);
			source.Position = startPosition;
			while (num > 0L)
			{
				int num5 = source.Read(array, 0, num4);
				if (num5 == 0)
				{
					throw new Exception("Stream Terminated early");
				}
				destination.Write(array, 0, num5);
				num2 += (long)num5;
				num -= (long)num5;
				if (progress != null)
				{
					num3++;
					if (num3 == 10)
					{
						progress.Complete = Percentage.FromFraction((float)num2 / (float)length);
						num3 = 0;
					}
				}
				num4 = (int)((num < 4096L) ? num : 4096L);
			}
			if (progress != null)
			{
				progress.Complete = Percentage.FromFraction(1f);
			}
			destination.Flush();
		}
	}
}
