using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DNA.Drawing.Imaging.Photoshop
{
	public class PsdFile
	{
		public Layer BaseLayer { get; set; }

		public ImageCompression ImageCompression { get; set; }

		public PsdFile()
		{
			this.Version = 1;
			this.BaseLayer = new Layer(this);
			this.BaseLayer.Rect = new Rectangle(0, 0, 0, 0);
			this.ImageResources = new List<ImageResource>();
			this.Layers = new List<Layer>();
		}

		public void Load(string fileName)
		{
			using (FileStream stream = new FileStream(fileName, FileMode.Open))
			{
				this.Load(stream);
			}
		}

		public void Load(Stream stream)
		{
			PsdBinaryReader reader = new PsdBinaryReader(stream);
			this.LoadHeader(reader);
			this.LoadColorModeData(reader);
			this.LoadImageResources(reader);
			this.LoadLayerAndMaskInfo(reader);
			this.LoadImage(reader);
			this.DecompressImages();
		}

		public void Save(string fileName)
		{
			using (FileStream stream = new FileStream(fileName, FileMode.Create))
			{
				this.Save(stream);
			}
		}

		public void Save(Stream stream)
		{
			if (this.BitDepth != 8)
			{
				throw new NotImplementedException("Only 8-bit color has been implemented for saving.");
			}
			PsdBinaryWriter writer = new PsdBinaryWriter(stream);
			writer.AutoFlush = true;
			this.PrepareSave();
			this.SaveHeader(writer);
			this.SaveColorModeData(writer);
			this.SaveImageResources(writer);
			this.SaveLayerAndMaskInfo(writer);
			this.SaveImage(writer);
		}

		public short Version { get; private set; }

		public short ChannelCount
		{
			get
			{
				return this.channelCount;
			}
			set
			{
				if (value < 1 || value > 56)
				{
					throw new ArgumentException("Number of channels must be from 1 to 56.");
				}
				this.channelCount = value;
			}
		}

		public int RowCount
		{
			get
			{
				return this.BaseLayer.Rect.Height;
			}
			set
			{
				if (value < 0 || value > 30000)
				{
					throw new ArgumentException("Number of rows must be from 1 to 30000.");
				}
				this.BaseLayer.Rect = new Rectangle(0, 0, this.BaseLayer.Rect.Width, value);
			}
		}

		public int ColumnCount
		{
			get
			{
				return this.BaseLayer.Rect.Width;
			}
			set
			{
				if (value < 0 || value > 30000)
				{
					throw new ArgumentException("Number of columns must be from 1 to 30000.");
				}
				this.BaseLayer.Rect = new Rectangle(0, 0, value, this.BaseLayer.Rect.Height);
			}
		}

		public int BitDepth
		{
			get
			{
				return this.bitDepth;
			}
			set
			{
				if (value <= 8)
				{
					if (value != 1 && value != 8)
					{
						goto IL_0022;
					}
				}
				else if (value != 16 && value != 32)
				{
					goto IL_0022;
				}
				this.bitDepth = value;
				return;
				IL_0022:
				throw new NotImplementedException("Invalid bit depth.");
			}
		}

		public PsdColorMode ColorMode { get; set; }

		private void LoadHeader(PsdBinaryReader reader)
		{
			string signature = new string(reader.ReadChars(4));
			if (signature != "8BPS")
			{
				throw new PsdInvalidException("The given stream is not a valid PSD file");
			}
			this.Version = reader.ReadInt16();
			if (this.Version != 1)
			{
				throw new PsdInvalidException("The PSD file has an unknown version");
			}
			reader.BaseStream.Position += 6L;
			this.ChannelCount = reader.ReadInt16();
			this.RowCount = reader.ReadInt32();
			this.ColumnCount = reader.ReadInt32();
			this.BitDepth = (int)reader.ReadInt16();
			this.ColorMode = (PsdColorMode)reader.ReadInt16();
		}

		private void SaveHeader(PsdBinaryWriter writer)
		{
			string signature = "8BPS";
			writer.Write(signature.ToCharArray());
			writer.Write(this.Version);
			byte[] array = new byte[6];
			writer.Write(array);
			writer.Write(this.ChannelCount);
			writer.Write(this.RowCount);
			writer.Write(this.ColumnCount);
			writer.Write((short)this.BitDepth);
			writer.Write((short)this.ColorMode);
		}

		private void LoadColorModeData(PsdBinaryReader reader)
		{
			uint paletteLength = reader.ReadUInt32();
			if (paletteLength > 0U)
			{
				this.ColorModeData = reader.ReadBytes((int)paletteLength);
			}
		}

		private void SaveColorModeData(PsdBinaryWriter writer)
		{
			writer.Write((uint)this.ColorModeData.Length);
			writer.Write(this.ColorModeData);
		}

		public List<ImageResource> ImageResources { get; set; }

		public ResolutionInfo Resolution
		{
			get
			{
				return (ResolutionInfo)this.ImageResources.Find((ImageResource x) => x.ID == ResourceID.ResolutionInfo);
			}
			set
			{
				this.ImageResources.RemoveAll((ImageResource x) => x.ID == ResourceID.ResolutionInfo);
				this.ImageResources.Add(value);
			}
		}

		private void LoadImageResources(PsdBinaryReader reader)
		{
			this.ImageResources.Clear();
			uint imageResourcesLength = reader.ReadUInt32();
			if (imageResourcesLength <= 0U)
			{
				return;
			}
			long startPosition = reader.BaseStream.Position;
			long endPosition = startPosition + (long)((ulong)imageResourcesLength);
			while (reader.BaseStream.Position < endPosition)
			{
				ImageResource imageResource = ImageResourceFactory.CreateImageResource(reader);
				this.ImageResources.Add(imageResource);
			}
			reader.BaseStream.Position = startPosition + (long)((ulong)imageResourcesLength);
		}

		private void SaveImageResources(PsdBinaryWriter writer)
		{
			using (new PsdBlockLengthWriter(writer))
			{
				foreach (ImageResource imgRes in this.ImageResources)
				{
					imgRes.Save(writer);
				}
			}
		}

		public List<Layer> Layers { get; private set; }

		public bool AbsoluteAlpha { get; set; }

		private void LoadLayerAndMaskInfo(PsdBinaryReader reader)
		{
			uint layersAndMaskLength = reader.ReadUInt32();
			if (layersAndMaskLength <= 0U)
			{
				return;
			}
			long startPosition = reader.BaseStream.Position;
			this.LoadLayers(reader);
			this.LoadGlobalLayerMask(reader);
			if (this.BitDepth > 8 && reader.BaseStream.Position < startPosition + (long)((ulong)layersAndMaskLength))
			{
				string signature = new string(reader.ReadChars(8));
				if (signature == "8BIMLr16" || signature == "8BIMLr32")
				{
					this.LoadLayers(reader);
					this.LoadGlobalLayerMask(reader);
				}
			}
			reader.BaseStream.Position = startPosition + (long)((ulong)layersAndMaskLength);
		}

		private void SaveLayerAndMaskInfo(PsdBinaryWriter writer)
		{
			using (new PsdBlockLengthWriter(writer))
			{
				this.SaveLayers(writer);
				this.SaveGlobalLayerMask(writer);
			}
		}

		private void LoadLayers(PsdBinaryReader reader)
		{
			uint layersInfoSectionLength = reader.ReadUInt32();
			if (layersInfoSectionLength <= 0U)
			{
				return;
			}
			long startPosition = reader.BaseStream.Position;
			short numLayers = reader.ReadInt16();
			if (numLayers < 0)
			{
				this.AbsoluteAlpha = true;
				numLayers = Math.Abs(numLayers);
			}
			this.Layers.Clear();
			if (numLayers == 0)
			{
				return;
			}
			for (int i = 0; i < (int)numLayers; i++)
			{
				this.Layers.Add(new Layer(reader, this));
			}
			foreach (Layer layer in this.Layers)
			{
				foreach (Channel channel in layer.Channels)
				{
					Rectangle rect = ((channel.ID == -2) ? layer.MaskData.Rect : layer.Rect);
					channel.LoadPixelData(reader, rect);
				}
			}
			if (reader.BaseStream.Position % 2L == 1L)
			{
				reader.ReadByte();
			}
			reader.BaseStream.Position = startPosition + (long)((ulong)layersInfoSectionLength);
		}

		private void DecompressImages()
		{
			IEnumerable<Layer> imageLayers = this.Layers.Concat(new List<Layer> { this.BaseLayer });
			foreach (Layer layer in imageLayers)
			{
				foreach (Channel channel in layer.Channels)
				{
					Rectangle rect = ((channel.ID == -2) ? layer.MaskData.Rect : layer.Rect);
					PsdFile.DecompressChannelContext dcc = new PsdFile.DecompressChannelContext(channel, rect);
					dcc.DecompressChannel(null);
				}
			}
			foreach (Layer layer2 in this.Layers)
			{
				if (layer2.Channels.ContainsId(-2))
				{
					layer2.MaskData.ImageData = layer2.Channels.GetId(-2).ImageData;
				}
			}
		}

		public void PrepareSave()
		{
			List<Layer> imageLayers = this.Layers.Concat(new List<Layer> { this.BaseLayer }).ToList<Layer>();
			foreach (Layer layer in imageLayers)
			{
				layer.PrepareSave();
			}
			this.SetVersionInfo();
			this.VerifyLayerSections();
		}

		internal void VerifyLayerSections()
		{
			int depth = 0;
			foreach (Layer layer in this.Layers.Reverse<Layer>())
			{
				IEnumerable<LayerInfo> sectionInfos = layer.AdditionalInfo.Where((LayerInfo info) => info.Key == "lsct");
				int sectionInfoCount = sectionInfos.Count<LayerInfo>();
				if (sectionInfoCount > 1)
				{
					throw new PsdInvalidException("Layer has more than one section info block.");
				}
				if (sectionInfoCount != 0)
				{
					LayerSectionInfo sectionInfo = (LayerSectionInfo)sectionInfos.Single<LayerInfo>();
					switch (sectionInfo.SectionType)
					{
					case LayerSectionType.OpenFolder:
					case LayerSectionType.ClosedFolder:
						depth++;
						break;
					case LayerSectionType.SectionDivider:
						depth--;
						if (depth < 0)
						{
							throw new PsdInvalidException("Layer section ended without matching start marker.");
						}
						break;
					}
				}
			}
			if (depth != 0)
			{
				throw new PsdInvalidException("Layer section not closed by end marker.");
			}
		}

		public void SetVersionInfo()
		{
			IEnumerable<ImageResource> versionInfos = this.ImageResources.Where((ImageResource x) => x.ID == ResourceID.VersionInfo);
			if (versionInfos.Count<ImageResource>() > 1)
			{
				throw new PsdInvalidException("Image has more than one VersionInfo resource.");
			}
			VersionInfo versionInfo = (VersionInfo)versionInfos.SingleOrDefault<ImageResource>();
			if (versionInfo == null)
			{
				versionInfo = new VersionInfo();
				this.ImageResources.Add(versionInfo);
			}
			Assembly assembly = Assembly.GetExecutingAssembly();
			Version version = assembly.GetName().Version;
			string versionString = string.Concat(new object[] { version.Major, ".", version.Minor, ".", version.Build });
			versionInfo.Version = 1U;
			versionInfo.HasRealMergedData = true;
			versionInfo.ReaderName = "Paint.NET PSD Plugin";
			versionInfo.WriterName = "Paint.NET PSD Plugin " + versionString;
			versionInfo.FileVersion = 1U;
		}

		private void SaveLayers(PsdBinaryWriter writer)
		{
			using (new PsdBlockLengthWriter(writer))
			{
				short numberOfLayers = (short)this.Layers.Count;
				if (this.AbsoluteAlpha)
				{
					numberOfLayers = -numberOfLayers;
				}
				writer.Write(numberOfLayers);
				foreach (Layer layer in this.Layers)
				{
					layer.Save(writer);
				}
				foreach (Layer layer2 in this.Layers)
				{
					foreach (Channel channel in layer2.Channels)
					{
						channel.SavePixelData(writer);
					}
				}
				if (writer.BaseStream.Position % 2L == 1L)
				{
					writer.Write(0);
				}
			}
		}

		private void LoadGlobalLayerMask(PsdBinaryReader reader)
		{
			uint maskLength = reader.ReadUInt32();
			if (maskLength <= 0U)
			{
				return;
			}
			this.GlobalLayerMaskData = reader.ReadBytes((int)maskLength);
		}

		private void SaveGlobalLayerMask(PsdBinaryWriter writer)
		{
			writer.Write((uint)this.GlobalLayerMaskData.Length);
			writer.Write(this.GlobalLayerMaskData);
		}

		private void LoadImage(PsdBinaryReader reader)
		{
			this.BaseLayer.Rect = new Rectangle(0, 0, this.ColumnCount, this.RowCount);
			this.ImageCompression = (ImageCompression)reader.ReadInt16();
			switch (this.ImageCompression)
			{
			case ImageCompression.Raw:
			{
				int length = this.RowCount * Util.BytesPerRow(this.BaseLayer.Rect, this.BitDepth);
				for (short i = 0; i < this.ChannelCount; i += 1)
				{
					Channel channel = new Channel(i, this.BaseLayer);
					channel.ImageCompression = this.ImageCompression;
					channel.Length = length;
					channel.ImageData = reader.ReadBytes(length);
					this.BaseLayer.Channels.Add(channel);
				}
				break;
			}
			case ImageCompression.Rle:
			{
				for (short j = 0; j < this.ChannelCount; j += 1)
				{
					int totalRleLength = 0;
					for (int k = 0; k < this.RowCount; k++)
					{
						totalRleLength += (int)reader.ReadUInt16();
					}
					Channel channel2 = new Channel(j, this.BaseLayer);
					channel2.ImageCompression = this.ImageCompression;
					channel2.Length = totalRleLength;
					this.BaseLayer.Channels.Add(channel2);
				}
				foreach (Channel channel3 in this.BaseLayer.Channels)
				{
					channel3.Data = reader.ReadBytes(channel3.Length);
				}
				break;
			}
			}
			if (this.ChannelCount == this.ColorMode.ChannelCount() + 1)
			{
				Channel alphaChannel = this.BaseLayer.Channels.Last<Channel>();
				alphaChannel.ID = -1;
			}
		}

		private void SaveImage(PsdBinaryWriter writer)
		{
			writer.Write((short)this.ImageCompression);
			if (this.ImageCompression == ImageCompression.Rle)
			{
				foreach (Channel channel in this.BaseLayer.Channels)
				{
					writer.Write(channel.RleHeader);
				}
			}
			foreach (Channel channel2 in this.BaseLayer.Channels)
			{
				writer.Write(channel2.Data);
			}
		}

		private short channelCount;

		private int bitDepth;

		public byte[] ColorModeData = new byte[0];

		private byte[] GlobalLayerMaskData = new byte[0];

		private class DecompressChannelContext
		{
			public DecompressChannelContext(Channel ch, Rectangle rect)
			{
				this.ch = ch;
				this.rect = rect;
			}

			public void DecompressChannel(object context)
			{
				this.ch.DecompressImageData(this.rect);
			}

			private Channel ch;

			private Rectangle rect;
		}
	}
}
