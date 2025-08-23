using System;
using System.IO;
using System.Text;

namespace DNA.Drawing.Imaging.Photoshop
{
	public class PsdBinaryWriter : IDisposable
	{
		public Stream BaseStream
		{
			get
			{
				return this.writer.BaseStream;
			}
		}

		public bool AutoFlush { get; set; }

		public PsdBinaryWriter(Stream stream)
		{
			this.writer = new BinaryWriter(stream);
		}

		public void Flush()
		{
			this.writer.Flush();
		}

		public void WritePascalString(string s)
		{
			string str = ((s.Length > 255) ? s.Substring(0, 255) : s);
			byte[] bytesArray = Encoding.Default.GetBytes(str);
			this.Write((byte)bytesArray.Length);
			this.Write(bytesArray);
			if (bytesArray.Length % 2 == 0)
			{
				this.Write(0);
			}
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}

		public void WriteUnicodeString(string s)
		{
			this.Write(s.Length);
			byte[] data = Encoding.BigEndianUnicode.GetBytes(s);
			this.Write(data);
		}

		public void Write(bool value)
		{
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}

		public void Write(char[] value)
		{
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}

		public void Write(byte[] value)
		{
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}

		public void Write(byte value)
		{
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}

		public unsafe void Write(short value)
		{
			Util.SwapBytes2((byte*)(&value));
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}

		public unsafe void Write(int value)
		{
			Util.SwapBytes4((byte*)(&value));
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}

		public unsafe void Write(long value)
		{
			Util.SwapBytes((byte*)(&value), 8);
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}

		public unsafe void Write(ushort value)
		{
			Util.SwapBytes2((byte*)(&value));
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}

		public unsafe void Write(uint value)
		{
			Util.SwapBytes4((byte*)(&value));
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}

		public unsafe void Write(ulong value)
		{
			Util.SwapBytes((byte*)(&value), 8);
			this.writer.Write(value);
			if (this.AutoFlush)
			{
				this.Flush();
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this.disposed)
			{
				return;
			}
			if (disposing && this.writer != null)
			{
				this.writer.Close();
				this.writer = null;
			}
			this.disposed = true;
		}

		private BinaryWriter writer;

		private bool disposed;
	}
}
