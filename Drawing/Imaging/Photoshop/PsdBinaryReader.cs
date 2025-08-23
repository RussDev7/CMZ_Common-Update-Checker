using System;
using System.IO;
using System.Text;

namespace DNA.Drawing.Imaging.Photoshop
{
	public class PsdBinaryReader : IDisposable
	{
		public Stream BaseStream
		{
			get
			{
				return this.reader.BaseStream;
			}
		}

		public PsdBinaryReader(Stream stream)
		{
			this.reader = new BinaryReader(stream, Encoding.Default);
		}

		public byte ReadByte()
		{
			return this.reader.ReadByte();
		}

		public byte[] ReadBytes(int count)
		{
			return this.reader.ReadBytes(count);
		}

		public char[] ReadChars(int count)
		{
			return this.reader.ReadChars(count);
		}

		public bool ReadBoolean()
		{
			return this.reader.ReadBoolean();
		}

		public unsafe short ReadInt16()
		{
			short val = this.reader.ReadInt16();
			Util.SwapBytes((byte*)(&val), 2);
			return val;
		}

		public unsafe int ReadInt32()
		{
			int val = this.reader.ReadInt32();
			Util.SwapBytes((byte*)(&val), 4);
			return val;
		}

		public unsafe long ReadInt64()
		{
			long val = this.reader.ReadInt64();
			Util.SwapBytes((byte*)(&val), 8);
			return val;
		}

		public unsafe ushort ReadUInt16()
		{
			ushort val = this.reader.ReadUInt16();
			Util.SwapBytes((byte*)(&val), 2);
			return val;
		}

		public unsafe uint ReadUInt32()
		{
			uint val = this.reader.ReadUInt32();
			Util.SwapBytes((byte*)(&val), 4);
			return val;
		}

		public unsafe ulong ReadUInt64()
		{
			ulong val = this.reader.ReadUInt64();
			Util.SwapBytes((byte*)(&val), 8);
			return val;
		}

		public string ReadPascalString()
		{
			byte stringLength = this.ReadByte();
			char[] c = this.ReadChars((int)stringLength);
			if (stringLength % 2 == 0)
			{
				this.ReadByte();
			}
			return new string(c);
		}

		public string ReadUnicodeString()
		{
			int numChars = this.ReadInt32();
			int length = 2 * numChars;
			byte[] data = this.ReadBytes(length);
			return Encoding.BigEndianUnicode.GetString(data, 0, length);
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
			if (disposing && this.reader != null)
			{
				this.reader.Close();
				this.reader = null;
			}
			this.disposed = true;
		}

		private BinaryReader reader;

		private bool disposed;
	}
}
