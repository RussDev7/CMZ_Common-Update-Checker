using System;

namespace DNA.Drawing.Imaging.Photoshop
{
	public static class LayerInfoFactory
	{
		public static LayerInfo CreateLayerInfo(PsdBinaryReader reader)
		{
			string text = new string(reader.ReadChars(4));
			if (text != "8BIM")
			{
				throw new PsdInvalidException("Could not read LayerInfo due to signature mismatch.");
			}
			string text2 = new string(reader.ReadChars(4));
			int num = reader.ReadInt32();
			long position = reader.BaseStream.Position;
			string text3;
			LayerInfo layerInfo;
			if ((text3 = text2) != null)
			{
				if (text3 == "lsct")
				{
					layerInfo = new LayerSectionInfo(reader, num);
					goto IL_0088;
				}
				if (text3 == "luni")
				{
					layerInfo = new LayerUnicodeName(reader);
					goto IL_0088;
				}
			}
			layerInfo = new RawLayerInfo(reader, text2, num);
			IL_0088:
			long num2 = position + (long)num;
			if (reader.BaseStream.Position < num2)
			{
				reader.BaseStream.Position = num2;
			}
			return layerInfo;
		}
	}
}
