using System;

namespace DNA.Drawing.Imaging.Photoshop
{
	public static class ImageResourceFactory
	{
		public static ImageResource CreateImageResource(PsdBinaryReader reader)
		{
			new string(reader.ReadChars(4));
			ushort num = reader.ReadUInt16();
			string text = reader.ReadPascalString();
			int num2 = (int)reader.ReadUInt32();
			long num3 = reader.BaseStream.Position + (long)num2;
			ResourceID resourceID = (ResourceID)num;
			ResourceID resourceID2 = resourceID;
			ImageResource imageResource;
			if (resourceID2 <= ResourceID.ThumbnailBgr)
			{
				switch (resourceID2)
				{
				case ResourceID.ResolutionInfo:
					imageResource = new ResolutionInfo(reader, text);
					goto IL_00B4;
				case ResourceID.AlphaChannelNames:
					imageResource = new AlphaChannelNames(reader, text, num2);
					goto IL_00B4;
				default:
					if (resourceID2 != ResourceID.ThumbnailBgr)
					{
						goto IL_00A8;
					}
					break;
				}
			}
			else if (resourceID2 != ResourceID.ThumbnailRgb)
			{
				if (resourceID2 != ResourceID.VersionInfo)
				{
					goto IL_00A8;
				}
				imageResource = new VersionInfo(reader, text);
				goto IL_00B4;
			}
			imageResource = new Thumbnail(reader, resourceID, text, num2);
			goto IL_00B4;
			IL_00A8:
			imageResource = new RawImageResource(reader, text, resourceID, num2);
			IL_00B4:
			if (reader.BaseStream.Position % 2L == 1L)
			{
				reader.ReadByte();
			}
			if (reader.BaseStream.Position < num3)
			{
				reader.BaseStream.Position = num3;
			}
			return imageResource;
		}
	}
}
