using System;
using System.Collections.Generic;
using System.IO;

namespace DNA.Audio
{
	public class RawPCMData
	{
		public byte[] ChannelData
		{
			get
			{
				return this._channelData;
			}
		}

		public int Channels { get; private set; }

		public int SampleRate { get; private set; }

		public int BitsPerSample { get; private set; }

		public int Samples
		{
			get
			{
				return this._channelData.Length / (this.Channels * (this.BitsPerSample >> 3));
			}
		}

		public TimeSpan Time
		{
			get
			{
				return TimeSpan.FromSeconds((double)this.Samples / (double)this.SampleRate);
			}
		}

		public RawPCMData()
		{
			this.Channels = 1;
		}

		public RawPCMData(int channels, int samples, int sampleRate, int bitsPerSample)
		{
			this.Channels = channels;
			this.SampleRate = sampleRate;
			this.BitsPerSample = bitsPerSample;
			this._channelData = new byte[channels * samples * (this.BitsPerSample >> 3)];
		}

		public RawPCMData(int channels, int sampleRate, int bitsPerSample, byte[] channelData)
		{
			this.Channels = channels;
			this.SampleRate = sampleRate;
			this.BitsPerSample = bitsPerSample;
			this._channelData = channelData;
		}

		public RawPCMData(RealPCMData data, int bitsPerSample)
		{
			this.Convert(data, bitsPerSample);
		}

		public void Alloc(int channels, int samples)
		{
			this._channelData = new byte[channels * samples * (this.BitsPerSample >> 3)];
		}

		public void Convert(RealPCMData data, int bitsPerSample)
		{
			if (data.Channels != this.Channels || data.Samples != data.Samples || this.BitsPerSample != bitsPerSample)
			{
				this.BitsPerSample = bitsPerSample;
				this.Alloc(data.Channels, data.Samples);
			}
			int bitsPerSample2 = this.BitsPerSample;
			if (bitsPerSample2 == 16)
			{
				for (int channel = 0; channel < this.Channels; channel++)
				{
					int sampleBytes = 2;
					int sampleSize = this.Channels * sampleBytes;
					float[] internalData = data.GetData(channel);
					int byteIndex = sampleBytes * channel;
					foreach (float fval in internalData)
					{
						short samp = (short)(fval * 32768f);
						this._channelData[byteIndex] = (byte)(samp & 255);
						this._channelData[byteIndex + 1] = (byte)(samp >> 8);
						byteIndex += sampleSize;
					}
				}
				return;
			}
			throw new NotImplementedException();
		}

		private void LoadWavInternal(Stream stream)
		{
			this.LoadWav(new BinaryReader(stream));
		}

		public static RawPCMData LoadWav(string path)
		{
			RawPCMData rawPCMData;
			using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				rawPCMData = RawPCMData.LoadWav(stream);
			}
			return rawPCMData;
		}

		public static RawPCMData LoadWav(Stream stream)
		{
			RawPCMData data = new RawPCMData();
			data.LoadWavInternal(stream);
			return data;
		}

		private int LoadWav(BinaryReader reader)
		{
			this._channelData = null;
			if (reader.ReadUInt32() != 1179011410U)
			{
				throw new Exception("Not a valid Wav File");
			}
			int size = reader.ReadInt32();
			long posstart = reader.BaseStream.Position;
			if (reader.ReadUInt32() != 1163280727U)
			{
				throw new Exception("Not a valid Wav File");
			}
			long end = posstart + (long)size;
			new List<List<float>>();
			while (reader.BaseStream.Position < end)
			{
				uint chunkId = reader.ReadUInt32();
				int chunkSize = reader.ReadInt32();
				long startPos = reader.BaseStream.Position;
				long endPos = startPos + (long)chunkSize;
				uint num = chunkId;
				if (num != 544501094U)
				{
					if (num != 1635017060U)
					{
						if (num == 1953393779U)
						{
							throw new Exception("Silence Chunks not Supported");
						}
					}
					else
					{
						if (this._channelData != null)
						{
							throw new Exception("Mutiple Wav Chunks Not Supported");
						}
						this._channelData = reader.ReadBytes(chunkSize);
						int bitsPerSample = this.BitsPerSample;
					}
				}
				else
				{
					RawPCMData.WavCompression compression = (RawPCMData.WavCompression)reader.ReadUInt16();
					reader.ReadUInt16();
					this.SampleRate = reader.ReadInt32();
					reader.ReadInt32();
					reader.ReadUInt16();
					this.BitsPerSample = (int)reader.ReadUInt16();
					if (compression != RawPCMData.WavCompression.PCM)
					{
						throw new Exception("Unsupported Wav Compression " + compression.ToString());
					}
				}
				if (reader.BaseStream.Position < endPos)
				{
					reader.BaseStream.Position = endPos;
				}
			}
			return this.BitsPerSample;
		}

		public void SaveWav(string path)
		{
			using (FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				this.SaveWav(stream);
			}
		}

		public void SaveWav(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			this.SaveWav(writer);
		}

		public void SaveWav(BinaryWriter writer)
		{
			writer.Write(1179011410U);
			int blockAlign = this.BitsPerSample * this.Channels >> 3;
			int dataSize = blockAlign * this.Samples;
			writer.Write(36 + dataSize);
			writer.Write(1163280727U);
			writer.Write(544501094U);
			writer.Write(16);
			writer.Write(1);
			writer.Write((short)this.Channels);
			writer.Write(this.SampleRate);
			writer.Write(this.SampleRate * blockAlign);
			writer.Write((short)blockAlign);
			writer.Write((short)this.BitsPerSample);
			writer.Write(1635017060U);
			writer.Write(dataSize);
			int samples = this.Samples;
			int channels = this.Channels;
			byte[] swapBuffer;
			if (this.BitsPerSample == 16)
			{
				swapBuffer = new byte[this._channelData.Length];
			}
			else
			{
				swapBuffer = this._channelData;
			}
			writer.Write(swapBuffer);
		}

		private byte[] _channelData = new byte[0];

		private enum WavCompression
		{
			Unknown,
			PCM,
			MicrosoftADPCM,
			ITUG711alaw = 6,
			ITUG711Alaw,
			IMAADPCM = 17,
			ITUG723ADPCM = 20,
			GSM610 = 49,
			ITUG721ADPCM = 64,
			MPEG = 80
		}
	}
}
