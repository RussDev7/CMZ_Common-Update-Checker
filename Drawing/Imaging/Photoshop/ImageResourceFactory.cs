using System;

namespace DNA.Drawing.Imaging.Photoshop
{
	public static class ImageResourceFactory
	{
		public static ImageResource CreateImageResource(PsdBinaryReader reader)
		{
			new string(reader.ReadChars(4));
			ushort resourceIdInt = reader.ReadUInt16();
			string name = reader.ReadPascalString();
			int resourceDataLength = (int)reader.ReadUInt32();
			long endPosition = reader.BaseStream.Position + (long)resourceDataLength;
			ResourceID resourceId = (ResourceID)resourceIdInt;
			ResourceID resourceID = resourceId;
			ImageResource resource;
			if (resourceID <= ResourceID.ThumbnailBgr)
			{
				switch (resourceID)
				{
				case ResourceID.ResolutionInfo:
					resource = new ResolutionInfo(reader, name);
					goto IL_00B4;
				case ResourceID.AlphaChannelNames:
					resource = new AlphaChannelNames(reader, name, resourceDataLength);
					goto IL_00B4;
				default:
					if (resourceID != ResourceID.ThumbnailBgr)
					{
						goto IL_00A8;
					}
					break;
				}
			}
			else if (resourceID != ResourceID.ThumbnailRgb)
			{
				if (resourceID != ResourceID.VersionInfo)
				{
					goto IL_00A8;
				}
				resource = new VersionInfo(reader, name);
				goto IL_00B4;
			}
			resource = new Thumbnail(reader, resourceId, name, resourceDataLength);
			goto IL_00B4;
			IL_00A8:
			resource = new RawImageResource(reader, name, resourceId, resourceDataLength);
			IL_00B4:
			if (reader.BaseStream.Position % 2L == 1L)
			{
				reader.ReadByte();
			}
			if (reader.BaseStream.Position < endPosition)
			{
				reader.BaseStream.Position = endPosition;
			}
			return resource;
		}
	}
}
