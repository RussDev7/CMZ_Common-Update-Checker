using System;

namespace DNA.Drawing.Imaging.Photoshop
{
	public static class LayerInfoFactory
	{
		public static LayerInfo CreateLayerInfo(PsdBinaryReader reader)
		{
			string signature = new string(reader.ReadChars(4));
			if (signature != "8BIM")
			{
				throw new PsdInvalidException("Could not read LayerInfo due to signature mismatch.");
			}
			string key = new string(reader.ReadChars(4));
			int length = reader.ReadInt32();
			long startPosition = reader.BaseStream.Position;
			string text;
			LayerInfo result;
			if ((text = key) != null)
			{
				if (text == "lsct")
				{
					result = new LayerSectionInfo(reader, length);
					goto IL_0088;
				}
				if (text == "luni")
				{
					result = new LayerUnicodeName(reader);
					goto IL_0088;
				}
			}
			result = new RawLayerInfo(reader, key, length);
			IL_0088:
			long endPosition = startPosition + (long)length;
			if (reader.BaseStream.Position < endPosition)
			{
				reader.BaseStream.Position = endPosition;
			}
			return result;
		}
	}
}
