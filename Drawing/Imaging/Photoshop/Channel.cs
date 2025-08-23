using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;

namespace DNA.Drawing.Imaging.Photoshop
{
	public class Channel
	{
		public Layer Layer { get; private set; }

		public short ID { get; set; }

		public int Length { get; set; }

		public byte[] Data
		{
			get
			{
				return this.data;
			}
			set
			{
				this.data = value;
				this.dataDecompressed = false;
			}
		}

		public byte[] ImageData
		{
			get
			{
				return this.imageData;
			}
			set
			{
				this.imageData = value;
				this.imageDataCompressed = false;
			}
		}

		public ImageCompression ImageCompression { get; set; }

		public byte[] RleHeader { get; set; }

		internal Channel(short id, Layer layer)
		{
			this.ID = id;
			this.Layer = layer;
		}

		internal Channel(PsdBinaryReader reader, Layer layer)
		{
			this.ID = reader.ReadInt16();
			this.Length = reader.ReadInt32();
			this.Layer = layer;
		}

		internal void Save(PsdBinaryWriter writer)
		{
			writer.Write(this.ID);
			writer.Write(this.Length);
		}

		internal void LoadPixelData(PsdBinaryReader reader, Rectangle rect)
		{
			long position = reader.BaseStream.Position;
			int length = this.Length;
			this.ImageCompression = (ImageCompression)reader.ReadInt16();
			int dataLength = this.Length - 2;
			switch (this.ImageCompression)
			{
			case ImageCompression.Raw:
				this.ImageData = reader.ReadBytes(dataLength);
				return;
			case ImageCompression.Rle:
			{
				reader.ReadBytes(2 * rect.Height);
				int rleDataLength = dataLength - 2 * rect.Height;
				this.Data = reader.ReadBytes(rleDataLength);
				return;
			}
			case ImageCompression.Zip:
			case ImageCompression.ZipPrediction:
				this.Data = reader.ReadBytes(dataLength);
				return;
			default:
				return;
			}
		}

		public void DecompressImageData(Rectangle rect)
		{
			if (this.dataDecompressed)
			{
				return;
			}
			int bytesPerRow = Util.BytesPerRow(rect, this.Layer.PsdFile.BitDepth);
			int bytesTotal = rect.Height * bytesPerRow;
			if (this.ImageCompression != ImageCompression.Raw)
			{
				this.imageData = new byte[bytesTotal];
				MemoryStream stream = new MemoryStream(this.Data);
				switch (this.ImageCompression)
				{
				case ImageCompression.Rle:
				{
					RleReader rleReader = new RleReader(stream);
					for (int i = 0; i < rect.Height; i++)
					{
						int rowIndex = i * bytesPerRow;
						rleReader.Read(this.imageData, rowIndex, bytesPerRow);
					}
					break;
				}
				case ImageCompression.Zip:
				case ImageCompression.ZipPrediction:
				{
					stream.ReadByte();
					stream.ReadByte();
					DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress);
					deflateStream.Read(this.imageData, 0, bytesTotal);
					break;
				}
				}
			}
			bool fReverseEndianness = this.Layer.PsdFile.BitDepth == 16 || (this.Layer.PsdFile.BitDepth == 32 && this.ImageCompression != ImageCompression.ZipPrediction);
			if (fReverseEndianness)
			{
				this.ReverseEndianness(this.imageData, rect);
			}
			if (this.ImageCompression == ImageCompression.ZipPrediction)
			{
				this.UnpredictImageData(rect);
			}
			this.dataDecompressed = true;
		}

		private void ReverseEndianness(byte[] buffer, Rectangle rect)
		{
			int byteDepth = Util.BytesFromBitDepth(this.Layer.PsdFile.BitDepth);
			int pixelsTotal = rect.Width * rect.Height;
			if (pixelsTotal == 0)
			{
				return;
			}
			if (byteDepth == 2)
			{
				Util.SwapByteArray2(buffer, 0, pixelsTotal);
				return;
			}
			if (byteDepth == 4)
			{
				Util.SwapByteArray4(buffer, 0, pixelsTotal);
				return;
			}
			if (byteDepth > 1)
			{
				throw new NotImplementedException("Byte-swapping implemented only for 16-bit and 32-bit depths.");
			}
		}

		private unsafe void UnpredictImageData(Rectangle rect)
		{
			if (this.Layer.PsdFile.BitDepth == 16)
			{
				fixed (byte* ptrData = &this.imageData[0])
				{
					for (int iRow = 0; iRow < rect.Height; iRow++)
					{
						ushort* ptr = (ushort*)ptrData + iRow * rect.Width;
						ushort* ptrEnd = (ushort*)ptrData + (iRow + 1) * rect.Width;
						for (ptr++; ptr < ptrEnd; ptr++)
						{
							*ptr += *(ptr - 1);
						}
					}
				}
				return;
			}
			if (this.Layer.PsdFile.BitDepth == 32)
			{
				byte[] reorderedData = new byte[this.imageData.Length];
				fixed (byte* ptrData2 = &this.imageData[0])
				{
					for (int iRow2 = 0; iRow2 < rect.Height; iRow2++)
					{
						byte* ptr2 = ptrData2 + (IntPtr)(iRow2 * rect.Width) * 4;
						byte* ptrEnd2 = ptrData2 + (IntPtr)((iRow2 + 1) * rect.Width) * 4;
						for (ptr2++; ptr2 < ptrEnd2; ptr2++)
						{
							*ptr2 += *(ptr2 - 1);
						}
					}
					int offset = rect.Width;
					int offset2 = 2 * offset;
					int offset3 = 3 * offset;
					fixed (byte* dstPtrData = &reorderedData[0])
					{
						for (int iRow3 = 0; iRow3 < rect.Height; iRow3++)
						{
							byte* dstPtr = dstPtrData + (IntPtr)(iRow3 * rect.Width) * 4;
							byte* dstPtrEnd = dstPtrData + (IntPtr)((iRow3 + 1) * rect.Width) * 4;
							byte* srcPtr = ptrData2 + (IntPtr)(iRow3 * rect.Width) * 4;
							while (dstPtr < dstPtrEnd)
							{
								*(dstPtr++) = srcPtr[offset3];
								*(dstPtr++) = srcPtr[offset2];
								*(dstPtr++) = srcPtr[offset];
								*(dstPtr++) = *srcPtr;
								srcPtr++;
							}
						}
					}
				}
				this.imageData = reorderedData;
				return;
			}
			throw new PsdInvalidException("ZIP with prediction is only available for 16 and 32 bit depths.");
		}

		public void CompressImageData()
		{
			if (this.imageDataCompressed)
			{
				return;
			}
			if (this.ImageCompression == ImageCompression.Rle)
			{
				MemoryStream dataStream = new MemoryStream();
				MemoryStream headerStream = new MemoryStream();
				RleWriter rleWriter = new RleWriter(dataStream);
				PsdBinaryWriter headerWriter = new PsdBinaryWriter(headerStream);
				ushort[] rleRowLengths = new ushort[this.Layer.Rect.Height];
				int bytesPerRow = Util.BytesPerRow(this.Layer.Rect, this.Layer.PsdFile.BitDepth);
				for (int row = 0; row < this.Layer.Rect.Height; row++)
				{
					int rowIndex = row * this.Layer.Rect.Width;
					rleRowLengths[row] = (ushort)rleWriter.Write(this.ImageData, rowIndex, bytesPerRow);
				}
				for (int i = 0; i < rleRowLengths.Length; i++)
				{
					headerWriter.Write(rleRowLengths[i]);
				}
				headerStream.Flush();
				this.RleHeader = headerStream.ToArray();
				headerStream.Close();
				dataStream.Flush();
				this.data = dataStream.ToArray();
				dataStream.Close();
				this.Length = 2 + this.RleHeader.Length + this.Data.Length;
			}
			else
			{
				this.data = this.ImageData;
				this.Length = 2 + this.Data.Length;
			}
			this.imageDataCompressed = true;
		}

		internal void SavePixelData(PsdBinaryWriter writer)
		{
			writer.Write((short)this.ImageCompression);
			if (this.ImageCompression == ImageCompression.Rle)
			{
				writer.Write(this.RleHeader);
			}
			writer.Write(this.Data);
		}

		private byte[] data;

		private bool dataDecompressed;

		private byte[] imageData;

		private bool imageDataCompressed;
	}
}
