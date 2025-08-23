using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;

namespace DNA.Drawing.Imaging.Photoshop
{
	public class Layer
	{
		internal PsdFile PsdFile { get; private set; }

		public Rectangle Rect { get; set; }

		public ChannelList Channels { get; private set; }

		public Channel AlphaChannel
		{
			get
			{
				if (this.Channels.ContainsId(-1))
				{
					return this.Channels.GetId(-1);
				}
				return null;
			}
		}

		public string BlendModeKey
		{
			get
			{
				return this.blendModeKey;
			}
			set
			{
				if (value.Length != 4)
				{
					throw new ArgumentException("Key length must be 4");
				}
				this.blendModeKey = value;
			}
		}

		public byte Opacity { get; set; }

		public bool Clipping { get; set; }

		public bool Visible
		{
			get
			{
				return !this.flags[Layer.visibleBit];
			}
			set
			{
				this.flags[Layer.visibleBit] = !value;
			}
		}

		public bool ProtectTrans
		{
			get
			{
				return this.flags[Layer.protectTransBit];
			}
			set
			{
				this.flags[Layer.protectTransBit] = value;
			}
		}

		public string Name { get; set; }

		public BlendingRanges BlendingRangesData { get; set; }

		public Mask MaskData { get; set; }

		public List<LayerInfo> AdditionalInfo { get; set; }

		public Layer(PsdFile psdFile)
		{
			this.PsdFile = psdFile;
			this.Rect = Rectangle.Empty;
			this.Channels = new ChannelList();
			this.BlendModeKey = "norm";
			this.AdditionalInfo = new List<LayerInfo>();
		}

		public Layer(PsdBinaryReader reader, PsdFile psdFile)
		{
			this.PsdFile = psdFile;
			Rectangle rect = default(Rectangle);
			rect.Y = reader.ReadInt32();
			rect.X = reader.ReadInt32();
			rect.Height = reader.ReadInt32() - rect.Y;
			rect.Width = reader.ReadInt32() - rect.X;
			this.Rect = rect;
			int numberOfChannels = (int)reader.ReadUInt16();
			this.Channels = new ChannelList();
			for (int channel = 0; channel < numberOfChannels; channel++)
			{
				Channel ch = new Channel(reader, this);
				this.Channels.Add(ch);
			}
			string signature = new string(reader.ReadChars(4));
			if (signature != "8BIM")
			{
				throw new PsdInvalidException("Invalid signature in channel header.");
			}
			this.BlendModeKey = new string(reader.ReadChars(4));
			this.Opacity = reader.ReadByte();
			this.Clipping = reader.ReadBoolean();
			byte flagsByte = reader.ReadByte();
			this.flags = new BitVector32((int)flagsByte);
			reader.ReadByte();
			uint extraDataSize = reader.ReadUInt32();
			long extraDataStartPosition = reader.BaseStream.Position;
			this.MaskData = new Mask(reader, this);
			this.BlendingRangesData = new BlendingRanges(reader, this);
			long namePosition = reader.BaseStream.Position;
			this.Name = reader.ReadPascalString();
			int paddingBytes = (int)((reader.BaseStream.Position - namePosition) % 4L);
			reader.ReadBytes(paddingBytes);
			long adjustmentLayerEndPos = extraDataStartPosition + (long)((ulong)extraDataSize);
			this.AdditionalInfo = new List<LayerInfo>();
			try
			{
				while (reader.BaseStream.Position < adjustmentLayerEndPos)
				{
					this.AdditionalInfo.Add(LayerInfoFactory.CreateLayerInfo(reader));
				}
			}
			catch
			{
				reader.BaseStream.Position = adjustmentLayerEndPos;
			}
			foreach (LayerInfo adjustmentInfo in this.AdditionalInfo)
			{
				string key;
				if ((key = adjustmentInfo.Key) != null && key == "luni")
				{
					this.Name = ((LayerUnicodeName)adjustmentInfo).Name;
				}
			}
		}

		public unsafe void CreateMissingChannels()
		{
			short channelCount = this.PsdFile.ColorMode.ChannelCount();
			for (short id = 0; id < channelCount; id += 1)
			{
				if (!this.Channels.ContainsId((int)id))
				{
					int size = this.Rect.Height * this.Rect.Width;
					Channel ch = new Channel(id, this);
					ch.ImageData = new byte[size];
					fixed (byte* ptr = &ch.ImageData[0])
					{
						Util.Fill(ptr, byte.MaxValue, size);
					}
					this.Channels.Add(ch);
				}
			}
		}

		public void PrepareSave()
		{
			foreach (Channel ch in this.Channels)
			{
				ch.CompressImageData();
			}
			IEnumerable<LayerInfo> layerUnicodeNames = this.AdditionalInfo.Where((LayerInfo x) => x is LayerUnicodeName);
			if (layerUnicodeNames.Count<LayerInfo>() > 1)
			{
				throw new PsdInvalidException("Layer has more than one LayerUnicodeName.");
			}
			LayerUnicodeName layerUnicodeName = (LayerUnicodeName)layerUnicodeNames.FirstOrDefault<LayerInfo>();
			if (layerUnicodeName == null)
			{
				layerUnicodeName = new LayerUnicodeName(this.Name);
				this.AdditionalInfo.Add(layerUnicodeName);
				return;
			}
			if (layerUnicodeName.Name != this.Name)
			{
				layerUnicodeName.Name = this.Name;
			}
		}

		public void Save(PsdBinaryWriter writer)
		{
			writer.Write(this.Rect.Top);
			writer.Write(this.Rect.Left);
			writer.Write(this.Rect.Bottom);
			writer.Write(this.Rect.Right);
			writer.Write((short)this.Channels.Count);
			foreach (Channel ch in this.Channels)
			{
				ch.Save(writer);
			}
			writer.Write(Util.SIGNATURE_8BIM);
			writer.Write(this.BlendModeKey.ToCharArray());
			writer.Write(this.Opacity);
			writer.Write(this.Clipping);
			writer.Write((byte)this.flags.Data);
			writer.Write(0);
			using (new PsdBlockLengthWriter(writer))
			{
				this.MaskData.Save(writer);
				this.BlendingRangesData.Save(writer);
				long namePosition = writer.BaseStream.Position;
				writer.WritePascalString(this.Name);
				int paddingBytes = (int)((writer.BaseStream.Position - namePosition) % 4L);
				for (int i = 0; i < paddingBytes; i++)
				{
					writer.Write(0);
				}
				foreach (LayerInfo info in this.AdditionalInfo)
				{
					info.Save(writer);
				}
			}
		}

		private string blendModeKey;

		private static int protectTransBit = BitVector32.CreateMask();

		private static int visibleBit = BitVector32.CreateMask(Layer.protectTransBit);

		private BitVector32 flags = default(BitVector32);
	}
}
