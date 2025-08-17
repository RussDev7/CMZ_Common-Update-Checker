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
			short num = this.reader.ReadInt16();
			Util.SwapBytes((byte*)(&num), 2);
			return num;
		}

		public unsafe int ReadInt32()
		{
			int num = this.reader.ReadInt32();
			Util.SwapBytes((byte*)(&num), 4);
			return num;
		}

		public unsafe long ReadInt64()
		{
			long num = this.reader.ReadInt64();
			Util.SwapBytes((byte*)(&num), 8);
			return num;
		}

		public unsafe ushort ReadUInt16()
		{
			ushort num = this.reader.ReadUInt16();
			Util.SwapBytes((byte*)(&num), 2);
			return num;
		}

		public unsafe uint ReadUInt32()
		{
			uint num = this.reader.ReadUInt32();
			Util.SwapBytes((byte*)(&num), 4);
			return num;
		}

		public unsafe ulong ReadUInt64()
		{
			ulong num = this.reader.ReadUInt64();
			Util.SwapBytes((byte*)(&num), 8);
			return num;
		}

		public string ReadPascalString()
		{
			byte b = this.ReadByte();
			char[] array = this.ReadChars((int)b);
			if (b % 2 == 0)
			{
				this.ReadByte();
			}
			return new string(array);
		}

		public string ReadUnicodeString()
		{
			int num = this.ReadInt32();
			int num2 = 2 * num;
			byte[] array = this.ReadBytes(num2);
			return Encoding.BigEndianUnicode.GetString(array, 0, num2);
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
