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
				DeflaterOutputStream stream = new DeflaterOutputStream(this.outStream, this.deflater);
				BinaryWriter writer = new BinaryWriter(stream);
				writer.Write(data.Length);
				writer.Write(data, 0, data.Length);
				writer.Flush();
				stream.Finish();
				array = this.outStream.ToArray();
			}
			return array;
		}

		public byte[] Decompress(byte[] data)
		{
			byte[] array;
			lock (this.inflater)
			{
				MemoryStream inStream = new MemoryStream(data);
				this.inflater.Reset();
				InflaterInputStream inflateStream = new InflaterInputStream(inStream, this.inflater);
				BinaryReader reader = new BinaryReader(inflateStream);
				int len = reader.ReadInt32();
				array = reader.ReadBytes(len);
			}
			return array;
		}

		private bool UseHeaders;

		private Inflater inflater;

		private Deflater deflater;

		private MemoryStream outStream = new MemoryStream();
	}
}
