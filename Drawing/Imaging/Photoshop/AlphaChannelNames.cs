using System;
using System.Collections.Generic;

namespace DNA.Drawing.Imaging.Photoshop
{
	public class AlphaChannelNames : ImageResource
	{
		public override ResourceID ID
		{
			get
			{
				return ResourceID.AlphaChannelNames;
			}
		}

		public List<string> ChannelNames
		{
			get
			{
				return this.channelNames;
			}
		}

		public AlphaChannelNames()
			: base(string.Empty)
		{
		}

		public AlphaChannelNames(PsdBinaryReader reader, string name, int resourceDataLength)
			: base(name)
		{
			long endPosition = reader.BaseStream.Position + (long)resourceDataLength;
			while (reader.BaseStream.Position < endPosition)
			{
				byte stringLength = reader.ReadByte();
				string channelName = new string(reader.ReadChars((int)stringLength));
				if (channelName.Length > 0)
				{
					this.channelNames.Add(channelName);
				}
			}
		}

		protected override void WriteData(PsdBinaryWriter writer)
		{
			foreach (string channelName in this.channelNames)
			{
				writer.Write((byte)channelName.Length);
				writer.Write(channelName.ToCharArray());
			}
		}

		private List<string> channelNames = new List<string>();
	}
}
