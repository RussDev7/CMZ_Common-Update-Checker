using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace DNA.Drawing.Imaging.Photoshop
{
	public class Thumbnail : ImageResource
	{
		public override ResourceID ID
		{
			get
			{
				return ResourceID.ThumbnailRgb;
			}
		}

		public Bitmap Image { get; set; }

		public Thumbnail(string name)
			: base(name)
		{
		}

		public Thumbnail(PsdBinaryReader reader, ResourceID id, string name, int numBytes)
			: base(name)
		{
			uint format = reader.ReadUInt32();
			uint width = reader.ReadUInt32();
			uint height = reader.ReadUInt32();
			reader.ReadUInt32();
			reader.ReadUInt32();
			reader.ReadUInt32();
			reader.ReadUInt16();
			reader.ReadUInt16();
			if (format == 0U)
			{
				this.Image = new Bitmap((int)width, (int)height, PixelFormat.Format24bppRgb);
				return;
			}
			if (format != 1U)
			{
				throw new PsdInvalidException("Unknown thumbnail format.");
			}
			byte[] imgData = reader.ReadBytes(numBytes - 28);
			using (MemoryStream stream = new MemoryStream(imgData))
			{
				Bitmap bitmap = new Bitmap(stream);
				this.Image = (Bitmap)bitmap.Clone();
			}
			if (id == ResourceID.ThumbnailBgr)
			{
				return;
			}
		}

		protected override void WriteData(PsdBinaryWriter writer)
		{
			throw new NotImplementedException();
		}
	}
}
