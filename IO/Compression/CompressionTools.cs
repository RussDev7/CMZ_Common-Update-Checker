using System;
using System.IO;
using DNA.IO.Compression.Zip.Compression;
using DNA.IO.Compression.Zip.Compression.Streams;

namespace DNA.IO.Compression
{
	public class CompressionTools
	{
		public CompressionTools()
		{
			this.deflater = new Deflater(Deflater.DefaultCompression, !this.UseHeaders);
			this.inflater = new Inflater(!this.UseHeaders);
		}

		public CompressionTools(bool useHeaders)
		{
			this.UseHeaders = useHeaders;
		}

		public byte[] Compress(byte[] data)
		{
			byte[] array;
			lock (this.deflater)
			{
				this.deflater.Reset();
				this.outStream.Position = 0L;
				this.outStream.SetLength(0L);
				DeflaterOutputStream deflaterOutputStream = new DeflaterOutputStream(this.outStream, this.deflater);
				BinaryWriter binaryWriter = new BinaryWriter(deflaterOutputStream);
				binaryWriter.Write(data.Length);
				binaryWriter.Write(data, 0, data.Length);
				binaryWriter.Flush();
				deflaterOutputStream.Finish();
				array = this.outStream.ToArray();
			}
			return array;
		}

		public byte[] Decompress(byte[] data)
		{
			byte[] array;
			lock (this.inflater)
			{
				MemoryStream memoryStream = new MemoryStream(data);
				this.inflater.Reset();
				InflaterInputStream inflaterInputStream = new InflaterInputStream(memoryStream, this.inflater);
				BinaryReader binaryReader = new BinaryReader(inflaterInputStream);
				int num = binaryReader.ReadInt32();
				array = binaryReader.ReadBytes(num);
			}
			return array;
		}

		private bool UseHeaders;

		private Inflater inflater;

		private Deflater deflater;

		private MemoryStream outStream = new MemoryStream();
	}
}
