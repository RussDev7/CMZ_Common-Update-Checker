using System;
using System.IO;

namespace DNA.Security.Cryptography.Utilities.IO
{
	public sealed class Streams
	{
		private Streams()
		{
		}

		public static void Drain(Stream inStr)
		{
			byte[] bs = new byte[512];
			while (inStr.Read(bs, 0, bs.Length) > 0)
			{
			}
		}

		public static byte[] ReadAll(Stream inStr)
		{
			MemoryStream buf = new MemoryStream();
			Streams.PipeAll(inStr, buf);
			return buf.ToArray();
		}

		public static int ReadFully(Stream inStr, byte[] buf)
		{
			return Streams.ReadFully(inStr, buf, 0, buf.Length);
		}

		public static int ReadFully(Stream inStr, byte[] buf, int off, int len)
		{
			int totalRead;
			int numRead;
			for (totalRead = 0; totalRead < len; totalRead += numRead)
			{
				numRead = inStr.Read(buf, off + totalRead, len - totalRead);
				if (numRead < 1)
				{
					break;
				}
			}
			return totalRead;
		}

		public static void PipeAll(Stream inStr, Stream outStr)
		{
			byte[] bs = new byte[512];
			int numRead;
			while ((numRead = inStr.Read(bs, 0, bs.Length)) > 0)
			{
				outStr.Write(bs, 0, numRead);
			}
		}

		private const int BufferSize = 512;
	}
}
