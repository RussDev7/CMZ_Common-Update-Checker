using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using DNA.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace DNA.Drawing.Imaging
{
	public static class TextureLoader
	{
		private static bool IsPowerOfTwo(int x)
		{
			int count = 0;
			while (x != 0)
			{
				count += x & 1;
				x >>= 1;
			}
			return count == 1;
		}

		private static Byte4[] MakeMipmap(Byte4[] source, ref int width, ref int height, bool treatDataAsNormals)
		{
			if (width == 1 || height == 1)
			{
				return null;
			}
			int tindex = 0;
			TextureLoader._mipmapOffsets[2] = width;
			TextureLoader._mipmapOffsets[3] = width + 1;
			Byte4[] result = new Byte4[width * height / 4];
			for (int y = 0; y < height; y += 2)
			{
				int sindex = y * width;
				for (int x = 0; x < width; x += 2)
				{
					Vector4 sum = Vector4.Zero;
					int count = 0;
					if (treatDataAsNormals)
					{
						for (int i = 0; i < 4; i++)
						{
							sum += source[sindex + TextureLoader._mipmapOffsets[i]].ToVector4();
						}
					}
					else
					{
						for (int j = 0; j < 4; j++)
						{
							Vector4 p = source[sindex + TextureLoader._mipmapOffsets[j]].ToVector4();
							if (p.W > 5f)
							{
								sum += p;
								count++;
							}
						}
					}
					if (treatDataAsNormals)
					{
						float w = sum.W / 4f;
						sum.W = 0f;
						sum.Normalize();
						sum *= 255f;
						sum.W = MathHelper.Clamp((float)Math.Floor((double)w), 0f, 255f);
					}
					else
					{
						if (count > 0)
						{
							sum *= 1f / (float)count;
						}
						if (sum.W >= 128f)
						{
							sum.W = 255f;
						}
						else
						{
							sum.W = 0f;
						}
					}
					sum.X = MathHelper.Clamp((float)Math.Floor((double)sum.X), 0f, 255f);
					sum.Y = MathHelper.Clamp((float)Math.Floor((double)sum.Y), 0f, 255f);
					sum.Z = MathHelper.Clamp((float)Math.Floor((double)sum.Z), 0f, 255f);
					result[tindex++] = new Byte4(sum);
					sindex += 2;
				}
			}
			width >>= 1;
			height >>= 1;
			return result;
		}

		private static void SwapChannels(byte[] data)
		{
			for (int i = 0; i < data.Length; i += 4)
			{
				byte t = data[i];
				data[i] = data[i + 2];
				data[i + 2] = t;
			}
		}

		private static Texture2D LoadThroughBitmap(GraphicsDevice gd, string filename, bool makeMipmaps, bool normalizeMipmaps)
		{
			Texture2D result = null;
			Bitmap bitmap = Image.FromFile(filename) as Bitmap;
			PixelFormat pxf = bitmap.PixelFormat;
			if (pxf != PixelFormat.Format32bppArgb && pxf != PixelFormat.Format32bppRgb)
			{
				throw new TextureLoader.FormatException(string.Concat(new string[]
				{
					"Map ",
					filename,
					" has wrong format (",
					bitmap.PixelFormat.ToString(),
					") must be 32 bit ARGB"
				}));
			}
			int w = bitmap.Width;
			int h = bitmap.Height;
			BitmapData data = bitmap.LockBits(new global::System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			int length = data.Stride * data.Height;
			int stride = data.Stride;
			byte[] tmp = new byte[length];
			Marshal.Copy(data.Scan0, tmp, 0, length);
			bitmap.UnlockBits(data);
			bitmap.Dispose();
			if (pxf == PixelFormat.Format32bppRgb)
			{
				for (int i = 3; i < tmp.Length; i += 4)
				{
					tmp[i] = byte.MaxValue;
				}
			}
			TextureLoader.SwapChannels(tmp);
			bool doMipMaps = makeMipmaps && TextureLoader.IsPowerOfTwo(w) && TextureLoader.IsPowerOfTwo(h);
			result = new Texture2D(gd, w, h, doMipMaps, SurfaceFormat.Color);
			if (makeMipmaps && TextureLoader.IsPowerOfTwo(w) && TextureLoader.IsPowerOfTwo(h))
			{
				Byte4[] d = new Byte4[w * h];
				d.AsUintArray(delegate(uint[] value)
				{
					Buffer.BlockCopy(tmp, 0, value, 0, tmp.Length);
				});
				int width = w;
				int height = h;
				int mip = 0;
				while (d != null)
				{
					result.SetData<Byte4>(mip++, null, d, 0, d.Length);
					d = TextureLoader.MakeMipmap(d, ref width, ref height, normalizeMipmaps);
				}
			}
			else
			{
				tmp.AsByte4Array(delegate(Byte4[] value)
				{
					result.SetData<Byte4>(value);
				});
			}
			return result;
		}

		private static Texture2D LoadFromDDS(GraphicsDevice gd, string filename)
		{
			Texture2D result = null;
			using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				BinaryReader reader = new BinaryReader(stream);
				TextureLoader.DDSFile ddsFile = default(TextureLoader.DDSFile);
				ddsFile.Read(filename, reader);
				bool dxtFormat = false;
				bool sixteenBit = false;
				SurfaceFormat format;
				if ((ddsFile.Header.PixelFormat.Flags & 64U) != 0U)
				{
					if (ddsFile.Header.PixelFormat.RGBBitCount == 32U)
					{
						format = SurfaceFormat.Color;
					}
					else
					{
						if (ddsFile.Header.PixelFormat.RGBBitCount != 16U)
						{
							throw new TextureLoader.FormatException("Uncompressed DDS Surface format must be 32bit RGBA or ABGR, or  16 bit BGR565, BGRA4444, or BGRA5551: " + filename);
						}
						sixteenBit = true;
						uint rgbbitCount = ddsFile.Header.PixelFormat.RGBBitCount;
						if (rgbbitCount != 1984U)
						{
							if (rgbbitCount != 2016U)
							{
								if (rgbbitCount != 3840U)
								{
									throw new TextureLoader.FormatException("Uncompressed DDS Surface format must be 32bit RGBA or ABGR, or  16 bit BGR565, BGRA4444, or BGRA5551: " + filename);
								}
								format = SurfaceFormat.Bgra4444;
							}
							else
							{
								format = SurfaceFormat.Bgr565;
							}
						}
						else
						{
							format = SurfaceFormat.Bgra5551;
						}
					}
				}
				else
				{
					dxtFormat = true;
					uint fourCC = ddsFile.Header.PixelFormat.FourCC;
					if (fourCC != 827611204U)
					{
						if (fourCC != 861165636U)
						{
							if (fourCC != 894720068U)
							{
								throw new TextureLoader.FormatException("Compressed DDS Surface format must be DXT1, DXT3, DXT5: " + filename);
							}
							format = SurfaceFormat.Dxt5;
						}
						else
						{
							format = SurfaceFormat.Dxt3;
						}
					}
					else
					{
						format = SurfaceFormat.Dxt1;
					}
				}
				result = new Texture2D(gd, (int)ddsFile.Header.Width, (int)ddsFile.Header.Height, ddsFile.Header.MipMapCount > 0U, format);
				int width = (int)ddsFile.Header.Width;
				int height = (int)ddsFile.Header.Height;
				for (int i = 0; i < (int)(1U + ddsFile.Header.MipMapCount); i++)
				{
					if (dxtFormat)
					{
						int size = Math.Max(1, (width + 3) / 4) * Math.Max(1, (height + 3) / 4) * ((format == SurfaceFormat.Dxt1) ? 8 : 16);
						byte[] byteData = reader.ReadBytes(size);
						if (byteData.Length != 0)
						{
							result.SetData<byte>(i, null, byteData, 0, byteData.Length);
						}
					}
					else if (sixteenBit)
					{
						int size = width * height * 2;
						short[] sdata = new short[size / 2];
						byte[] byteData = reader.ReadBytes(size);
						if (byteData.Length != 0)
						{
							Buffer.BlockCopy(byteData, 0, sdata, 0, byteData.Length);
							result.SetData<short>(i, null, sdata, 0, sdata.Length);
						}
					}
					else
					{
						int size = width * height * 4;
						int[] idata = new int[size / 4];
						byte[] byteData = reader.ReadBytes(size);
						if (byteData.Length != 0)
						{
							Buffer.BlockCopy(byteData, 0, idata, 0, byteData.Length);
							result.SetData<int>(i, null, idata, 0, idata.Length);
						}
					}
					width /= 2;
					height /= 2;
				}
			}
			return result;
		}

		public static Texture2D LoadFromFile(GraphicsDevice gd, string filename, bool makeMipmaps, bool normalizeMipmaps)
		{
			string ext = Path.GetExtension(filename).ToLower();
			Texture2D result;
			if (ext == ".dds")
			{
				result = TextureLoader.LoadFromDDS(gd, filename);
			}
			else
			{
				result = TextureLoader.LoadThroughBitmap(gd, filename, makeMipmaps, normalizeMipmaps);
			}
			return result;
		}

		private static int[] _mipmapOffsets = new int[] { 0, 1, 0, 1 };

		[Serializable]
		public class FormatException : Exception
		{
			public FormatException()
			{
			}

			public FormatException(string message)
				: base(message)
			{
			}

			public FormatException(string message, Exception inner)
				: base(message, inner)
			{
			}

			protected FormatException(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
		}

		private struct DDS_PIXELFORMAT
		{
			public void Read(string filename, BinaryReader reader)
			{
				this.Size = reader.ReadUInt32();
				this.Flags = reader.ReadUInt32();
				this.FourCC = reader.ReadUInt32();
				this.RGBBitCount = reader.ReadUInt32();
				this.RBitMask = reader.ReadUInt32();
				this.GBitMask = reader.ReadUInt32();
				this.BBitMask = reader.ReadUInt32();
				this.ABitMask = reader.ReadUInt32();
				if ((this.Flags & 68U) == 0U)
				{
					throw new TextureLoader.FormatException("Pixel format must be either Uncompressed ARGB or DXT1, 3, 5 in DDS file" + filename);
				}
				if (this.Size != 32U)
				{
					throw new TextureLoader.FormatException("Wrong header size in DDS file " + filename);
				}
				if ((this.Flags & 4U) != 0U && this.FourCC == 808540228U)
				{
					throw new TextureLoader.FormatException("Cannot load DX10 DDS file " + filename);
				}
			}

			public uint Size;

			public uint Flags;

			public uint FourCC;

			public uint RGBBitCount;

			public uint RBitMask;

			public uint GBitMask;

			public uint BBitMask;

			public uint ABitMask;
		}

		private struct DDS_HEADER
		{
			public void Read(string filename, BinaryReader reader)
			{
				this.Size = reader.ReadUInt32();
				if (this.Size != 124U)
				{
					throw new TextureLoader.FormatException("Wrong header size in DDS file " + filename);
				}
				this.Flags = reader.ReadUInt32();
				this.Height = reader.ReadUInt32();
				this.Width = reader.ReadUInt32();
				this.PitchOrLinearSize = reader.ReadUInt32();
				this.Depth = reader.ReadUInt32();
				this.MipMapCount = reader.ReadUInt32();
				this.Reserved1 = reader.ReadUInt32();
				this.Reserved2 = reader.ReadUInt32();
				this.Reserved3 = reader.ReadUInt32();
				this.Reserved4 = reader.ReadUInt32();
				this.Reserved5 = reader.ReadUInt32();
				this.Reserved6 = reader.ReadUInt32();
				this.Reserved7 = reader.ReadUInt32();
				this.Reserved8 = reader.ReadUInt32();
				this.Reserved9 = reader.ReadUInt32();
				this.Reserved10 = reader.ReadUInt32();
				this.Reserved11 = reader.ReadUInt32();
				this.PixelFormat.Read(filename, reader);
				this.Caps = reader.ReadUInt32();
				this.Caps2 = reader.ReadUInt32();
				this.Caps3 = reader.ReadUInt32();
				this.Caps4 = reader.ReadUInt32();
				this.Reserved12 = reader.ReadUInt32();
				if ((this.Caps & 4096U) == 0U)
				{
					throw new TextureLoader.FormatException("Texture flag not present in DDS file" + filename);
				}
			}

			public uint Size;

			public uint Flags;

			public uint Height;

			public uint Width;

			public uint PitchOrLinearSize;

			public uint Depth;

			public uint MipMapCount;

			public uint Reserved1;

			public uint Reserved2;

			public uint Reserved3;

			public uint Reserved4;

			public uint Reserved5;

			public uint Reserved6;

			public uint Reserved7;

			public uint Reserved8;

			public uint Reserved9;

			public uint Reserved10;

			public uint Reserved11;

			public TextureLoader.DDS_PIXELFORMAT PixelFormat;

			public uint Caps;

			public uint Caps2;

			public uint Caps3;

			public uint Caps4;

			public uint Reserved12;
		}

		private struct DDSFile
		{
			public void Read(string filename, BinaryReader reader)
			{
				this.Magic = reader.ReadUInt32();
				if (this.Magic != 542327876U)
				{
					throw new TextureLoader.FormatException("Wrong magic number in DDS file " + filename);
				}
				this.Header.Read(filename, reader);
			}

			public uint Magic;

			public TextureLoader.DDS_HEADER Header;
		}
	}
}
