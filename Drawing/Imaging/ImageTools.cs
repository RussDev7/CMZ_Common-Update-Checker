using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace DNA.Drawing.Imaging
{
	public static class ImageTools
	{
		private static ImageCodecInfo GetEncoderFormat(string formatName)
		{
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
			foreach (ImageCodecInfo codec in codecs)
			{
				if (string.Compare(codec.FormatDescription, formatName, true) == 0)
				{
					return codec;
				}
			}
			throw new ArgumentException("Cannot Find Codec " + formatName);
		}

		public static void SaveJpg(Bitmap bitmap, Stream outStream, Percentage quality)
		{
			ImageCodecInfo encoder = ImageTools.GetEncoderFormat("JPEG");
			EncoderParameters encoderParams = new EncoderParameters(1);
			encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality.Percent);
			bitmap.Save(outStream, encoder, encoderParams);
		}

		public static string GetFileExtenstion(ImageFormat format)
		{
			if (format.Guid == ImageFormat.Bmp.Guid)
			{
				return ".bmp";
			}
			if (format.Guid == ImageFormat.Emf.Guid)
			{
				return ".emf";
			}
			if (format.Guid == ImageFormat.Exif.Guid)
			{
				return ".emf";
			}
			if (format.Guid == ImageFormat.Gif.Guid)
			{
				return ".gif";
			}
			if (format.Guid == ImageFormat.Icon.Guid)
			{
				return ".ico";
			}
			if (format.Guid == ImageFormat.Jpeg.Guid)
			{
				return ".jpg";
			}
			if (format.Guid == ImageFormat.Png.Guid)
			{
				return ".png";
			}
			return null;
		}

		public static Image ReshapeImageBuffered(Image orginalImage, Size newSize, Color bufferColor, PixelFormat destFormat)
		{
			Bitmap newBmp = new Bitmap(newSize.Width, newSize.Height, destFormat);
			using (Graphics g = Graphics.FromImage(newBmp))
			{
				g.Clear(bufferColor);
				float sourceAspect = (float)orginalImage.Width / (float)orginalImage.Height;
				float destAspect = (float)newSize.Width / (float)newSize.Height;
				if (sourceAspect > destAspect)
				{
					float factor = (float)newSize.Width / (float)orginalImage.Width;
					int newHeight = (int)(factor * (float)orginalImage.Height);
					int diff = newSize.Height - newHeight;
					Rectangle destRect = new Rectangle(0, diff / 2, newSize.Width, newHeight);
					g.DrawImage(orginalImage, destRect, 0, 0, orginalImage.Width, orginalImage.Height, GraphicsUnit.Pixel);
				}
				else
				{
					float factor2 = (float)newSize.Height / (float)orginalImage.Height;
					int newWidth = (int)(factor2 * (float)orginalImage.Width);
					int diff2 = newSize.Width - newWidth;
					Rectangle destRect2 = new Rectangle(diff2 / 2, 0, newWidth, newSize.Height);
					g.DrawImage(orginalImage, destRect2, 0, 0, orginalImage.Width, orginalImage.Height, GraphicsUnit.Pixel);
				}
			}
			return newBmp;
		}

		public static Image ReshapeImageCropped(Image sourceImage, Size newSize, ImageTools.CropOptions cropOptions)
		{
			float newAspect = (float)newSize.Width / (float)newSize.Height;
			Image bitmap = ImageTools.CropToAspectRatio(sourceImage, newAspect, cropOptions);
			return ImageTools.Resample(bitmap, Percentage.FromFraction((float)newSize.Width / (float)bitmap.Width));
		}

		public static string GetFileExtenstion(Image image)
		{
			return ImageTools.GetFileExtenstion(image.RawFormat);
		}

		public static Bitmap ChangeFormat(Image source, PixelFormat format)
		{
			Bitmap dest = new Bitmap(source.Width, source.Height, format);
			using (Graphics g = Graphics.FromImage(dest))
			{
				g.DrawImageUnscaled(source, 0, 0);
			}
			return dest;
		}

		public static Image Resample(Image source, Percentage percentage)
		{
			int newWidth = (int)Math.Round((double)(percentage.Fraction * (float)source.Width));
			int newHeight = (int)Math.Round((double)(percentage.Fraction * (float)source.Height));
			return ImageTools.Resample(source, new Size(newWidth, newHeight));
		}

		public static Image Resample(Image source, Size newSize)
		{
			Bitmap newImage = new Bitmap(source, newSize);
			using (Graphics g = Graphics.FromImage(newImage))
			{
				Rectangle srect = new Rectangle(0, 0, source.Width, source.Height);
				Rectangle drect = new Rectangle(0, 0, newImage.Width, newImage.Height);
				g.DrawImage(source, drect, srect, GraphicsUnit.Pixel);
			}
			return newImage;
		}

		public static Image CropToAspectRatio(Image source, float cropAspectRatio, ImageTools.CropOptions options)
		{
			float sourceAspect = (float)source.Width / (float)source.Height;
			int newY;
			int newX;
			int yoff;
			int xoff;
			if (cropAspectRatio < sourceAspect)
			{
				newY = source.Height;
				newX = (int)((float)source.Height * cropAspectRatio);
				yoff = 0;
				switch (options)
				{
				case ImageTools.CropOptions.Start:
					xoff = 0;
					break;
				case ImageTools.CropOptions.Middle:
					xoff = (source.Width - newX) / 2;
					break;
				case ImageTools.CropOptions.End:
					xoff = newX - source.Width;
					break;
				default:
					throw new ArgumentException("Not a valid Crop option");
				}
			}
			else
			{
				newX = source.Width;
				newY = (int)((float)source.Width / cropAspectRatio);
				xoff = 0;
				switch (options)
				{
				case ImageTools.CropOptions.Start:
					yoff = 0;
					break;
				case ImageTools.CropOptions.Middle:
					yoff = (source.Height - newY) / 2;
					break;
				case ImageTools.CropOptions.End:
					yoff = newY - source.Height;
					break;
				default:
					throw new ArgumentException("Not a valid Crop option");
				}
			}
			Bitmap dest = new Bitmap(source, newX, newY);
			using (Graphics g = Graphics.FromImage(dest))
			{
				Rectangle srect = new Rectangle(xoff, yoff, newX, newY);
				Rectangle drect = new Rectangle(0, 0, dest.Width, dest.Height);
				g.DrawImage(source, drect, srect, GraphicsUnit.Pixel);
			}
			return dest;
		}

		public static Bitmap ExtractSubImage(Bitmap sourceBmp, Rectangle sourceRect)
		{
			Bitmap destBmp = new Bitmap(sourceRect.Width, sourceRect.Height, sourceBmp.PixelFormat);
			Graphics g = Graphics.FromImage(destBmp);
			Rectangle destRect = new Rectangle(new Point(0, 0), sourceRect.Size);
			g.DrawImage(sourceBmp, destRect, sourceRect, GraphicsUnit.Pixel);
			return destBmp;
		}

		public static bool IsEditable(PixelFormat format)
		{
			return format != PixelFormat.Format1bppIndexed && format != PixelFormat.Format4bppIndexed && format != PixelFormat.Format8bppIndexed && format != PixelFormat.Undefined && format != PixelFormat.Undefined && format != PixelFormat.Format16bppArgb1555 && format != PixelFormat.Format16bppGrayScale;
		}

		public static Bitmap WaterMark(Image orginal, Image watermark, Rectangle markLocation)
		{
			Bitmap resultImage;
			if (ImageTools.IsEditable(orginal.PixelFormat))
			{
				resultImage = ImageTools.ChangeFormat(orginal, orginal.PixelFormat);
			}
			else
			{
				resultImage = ImageTools.ChangeFormat(orginal, PixelFormat.Format32bppArgb);
			}
			using (Graphics g = Graphics.FromImage(resultImage))
			{
				float[][] array = new float[5][];
				float[][] array2 = array;
				int num = 0;
				float[] array3 = new float[5];
				array3[0] = 1f;
				array3[3] = 1f;
				array2[num] = array3;
				float[][] array4 = array;
				int num2 = 1;
				float[] array5 = new float[5];
				array5[1] = 1f;
				array5[3] = 1f;
				array4[num2] = array5;
				float[][] array6 = array;
				int num3 = 2;
				float[] array7 = new float[5];
				array7[2] = 1f;
				array7[3] = 1f;
				array6[num3] = array7;
				float[][] array8 = array;
				int num4 = 3;
				float[] array9 = new float[5];
				array8[num4] = array9;
				array[4] = new float[] { 0f, 0f, 0f, 0f, 1f };
				float[][] matrixItems = array;
				ColorMatrix colorMatrix = new ColorMatrix(matrixItems);
				ImageAttributes imageAtt = new ImageAttributes();
				imageAtt.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
				g.DrawImage(watermark, markLocation, 0, 0, watermark.Width, watermark.Height, GraphicsUnit.Pixel, imageAtt);
			}
			return resultImage;
		}

		public enum CropOptions
		{
			Start,
			Middle,
			End
		}
	}
}
