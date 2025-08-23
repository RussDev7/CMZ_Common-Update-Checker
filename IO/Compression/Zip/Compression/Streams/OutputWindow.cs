using System;

namespace DNA.IO.Compression.Zip.Compression.Streams
{
	public class OutputWindow
	{
		public void Write(int abyte)
		{
			if (this.windowFilled++ == OutputWindow.WINDOW_SIZE)
			{
				throw new InvalidOperationException("Window full");
			}
			this.window[this.windowEnd++] = (byte)abyte;
			this.windowEnd &= OutputWindow.WINDOW_MASK;
		}

		private void SlowRepeat(int repStart, int len, int dist)
		{
			while (len-- > 0)
			{
				this.window[this.windowEnd++] = this.window[repStart++];
				this.windowEnd &= OutputWindow.WINDOW_MASK;
				repStart &= OutputWindow.WINDOW_MASK;
			}
		}

		public void Repeat(int len, int dist)
		{
			if ((this.windowFilled += len) > OutputWindow.WINDOW_SIZE)
			{
				throw new InvalidOperationException("Window full");
			}
			int rep_start = (this.windowEnd - dist) & OutputWindow.WINDOW_MASK;
			int border = OutputWindow.WINDOW_SIZE - len;
			if (rep_start > border || this.windowEnd >= border)
			{
				this.SlowRepeat(rep_start, len, dist);
				return;
			}
			if (len <= dist)
			{
				Array.Copy(this.window, rep_start, this.window, this.windowEnd, len);
				this.windowEnd += len;
				return;
			}
			while (len-- > 0)
			{
				this.window[this.windowEnd++] = this.window[rep_start++];
			}
		}

		public int CopyStored(StreamManipulator input, int len)
		{
			len = Math.Min(Math.Min(len, OutputWindow.WINDOW_SIZE - this.windowFilled), input.AvailableBytes);
			int tailLen = OutputWindow.WINDOW_SIZE - this.windowEnd;
			int copied;
			if (len > tailLen)
			{
				copied = input.CopyBytes(this.window, this.windowEnd, tailLen);
				if (copied == tailLen)
				{
					copied += input.CopyBytes(this.window, 0, len - tailLen);
				}
			}
			else
			{
				copied = input.CopyBytes(this.window, this.windowEnd, len);
			}
			this.windowEnd = (this.windowEnd + copied) & OutputWindow.WINDOW_MASK;
			this.windowFilled += copied;
			return copied;
		}

		public void CopyDict(byte[] dict, int offset, int len)
		{
			if (this.windowFilled > 0)
			{
				throw new InvalidOperationException();
			}
			if (len > OutputWindow.WINDOW_SIZE)
			{
				offset += len - OutputWindow.WINDOW_SIZE;
				len = OutputWindow.WINDOW_SIZE;
			}
			Array.Copy(dict, offset, this.window, 0, len);
			this.windowEnd = len & OutputWindow.WINDOW_MASK;
		}

		public int GetFreeSpace()
		{
			return OutputWindow.WINDOW_SIZE - this.windowFilled;
		}

		public int GetAvailable()
		{
			return this.windowFilled;
		}

		public int CopyOutput(byte[] output, int offset, int len)
		{
			int copy_end = this.windowEnd;
			if (len > this.windowFilled)
			{
				len = this.windowFilled;
			}
			else
			{
				copy_end = (this.windowEnd - this.windowFilled + len) & OutputWindow.WINDOW_MASK;
			}
			int copied = len;
			int tailLen = len - copy_end;
			if (tailLen > 0)
			{
				Array.Copy(this.window, OutputWindow.WINDOW_SIZE - tailLen, output, offset, tailLen);
				offset += tailLen;
				len = copy_end;
			}
			Array.Copy(this.window, copy_end - len, output, offset, len);
			this.windowFilled -= copied;
			if (this.windowFilled < 0)
			{
				throw new InvalidOperationException();
			}
			return copied;
		}

		public void Reset()
		{
			this.windowFilled = (this.windowEnd = 0);
		}

		private static int WINDOW_SIZE = 32768;

		private static int WINDOW_MASK = OutputWindow.WINDOW_SIZE - 1;

		private byte[] window = new byte[OutputWindow.WINDOW_SIZE];

		private int windowEnd;

		private int windowFilled;
	}
}
