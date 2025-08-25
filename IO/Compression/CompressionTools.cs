using System;
using System.IO;
using System.IO.Compression;

namespace DNA.IO.Compression
{
	public class CompressionTools
	{
		public byte[] Compress(byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			byte[] array;
			using (MemoryStream outMs = new MemoryStream())
			{
				using (DeflateStream ds = new DeflateStream(outMs, CompressionMode.Compress, true))
				{
					using (BinaryWriter bw = new BinaryWriter(ds))
					{
						bw.Write(data.Length);
						bw.Write(data, 0, data.Length);
						bw.Flush();
					}
				}
				array = outMs.ToArray();
			}
			return array;
		}

		public byte[] Decompress(byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			byte[] array;
			using (MemoryStream inMs = new MemoryStream(data))
			{
				using (DeflateStream ds = new DeflateStream(inMs, CompressionMode.Decompress, true))
				{
					using (BinaryReader br = new BinaryReader(ds))
					{
						int len = br.ReadInt32();
						if (len < 0)
						{
							throw new InvalidDataException("Negative length in compressed stream.");
						}
						byte[] payload = br.ReadBytes(len);
						if (payload.Length != len)
						{
							throw new EndOfStreamException("Truncated compressed payload.");
						}
						array = payload;
					}
				}
			}
			return array;
		}
	}
}
