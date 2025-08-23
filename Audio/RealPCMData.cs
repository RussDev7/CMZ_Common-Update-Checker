using System;
using System.IO;
using DNA.Collections;

namespace DNA.Audio
{
	public class RealPCMData
	{
		public int SampleRate { get; private set; }

		public int Channels
		{
			get
			{
				return this._channelData.Length;
			}
		}

		public int Samples
		{
			get
			{
				return this._channelData[0].Length;
			}
		}

		public TimeSpan Time
		{
			get
			{
				return TimeSpan.FromSeconds((double)this.Samples / (double)this.SampleRate);
			}
		}

		public float[] GetData(int channel)
		{
			return this._channelData[channel];
		}

		public void Convert(RawPCMData data)
		{
			if (data.Samples != this.Samples || this.Channels != data.Channels)
			{
				this.Alloc(this.Channels, data.Samples);
			}
			this.SampleRate = data.SampleRate;
			int bitsPerSample = data.BitsPerSample;
			if (bitsPerSample == 16)
			{
				int sampleSize = 2 * this.Channels;
				byte[] inputData = data.ChannelData;
				for (int channnel = 0; channnel < data.Channels; channnel++)
				{
					float[] internalData = this._channelData[channnel];
					int writePos = 0;
					for (int readPos = channnel * 2; readPos < data.ChannelData.Length; readPos += sampleSize)
					{
						short low = (short)inputData[readPos];
						short high = (short)(inputData[readPos + 1] << 8);
						short sample = low | high;
						float fval = (float)sample * 3.0517578E-05f;
						internalData[writePos++] = fval;
					}
				}
				return;
			}
			throw new NotImplementedException();
		}

		private RealPCMData()
		{
		}

		public void Alloc(int channels, int samples)
		{
			this._channelData = ArrayTools.AllocSquareJaggedArray<float>(channels, samples);
		}

		public RealPCMData(int channels, int samples, int sampleRate)
		{
			this.SampleRate = sampleRate;
			this.Alloc(channels, samples);
		}

		public void CombineChannels()
		{
			if (this.Channels == 1)
			{
				return;
			}
			float[][] newData = ArrayTools.AllocSquareJaggedArray<float>(1, this.Samples);
			int samples = this.Samples;
			int channels = this.Channels;
			for (int i = 0; i < samples; i++)
			{
				newData[0][i] = 0f;
				for (int j = 0; j < this.Channels; j++)
				{
					newData[0][i] += this._channelData[j][i];
				}
				newData[0][i] /= (float)channels;
			}
			this._channelData = newData;
		}

		public void AdjustVolume(float modifier)
		{
			int samples = this.Samples;
			for (int i = 0; i < this.Channels; i++)
			{
				for (int j = 0; j < samples; j++)
				{
					this._channelData[i][j] *= modifier;
				}
			}
		}

		public void TrimEndSilence(float threshold)
		{
			int lastSample = 0;
			int samples = this._channelData.GetLength(1);
			for (int i = 0; i < this.Channels; i++)
			{
				for (int j = samples - 1; j > lastSample; j--)
				{
					if (Math.Abs(this._channelData[i][j]) > threshold)
					{
						lastSample = j;
						break;
					}
				}
			}
			if (lastSample == 0)
			{
				return;
			}
			float[][] newData = ArrayTools.AllocSquareJaggedArray<float>(this.Channels, lastSample);
			for (int k = 0; k < this.Channels; k++)
			{
				for (int l = 0; l < lastSample; l++)
				{
					newData[k][l] = this._channelData[k][l];
				}
			}
			this._channelData = newData;
		}

		public void TrimBeginSilence(float threshold)
		{
			int firstSample = this.Samples;
			this._channelData.GetLength(1);
			for (int i = 0; i < this.Channels; i++)
			{
				for (int j = 0; j < firstSample; j++)
				{
					if (Math.Abs(this._channelData[i][j]) > threshold)
					{
						firstSample = j;
						break;
					}
				}
			}
			int newSamples = this.Samples - firstSample;
			float[][] newData = ArrayTools.AllocSquareJaggedArray<float>(this.Channels, newSamples);
			for (int k = 0; k < this.Channels; k++)
			{
				for (int l = 0; l < newSamples; l++)
				{
					newData[k][l] = this._channelData[k][l + firstSample];
				}
			}
			this._channelData = newData;
		}

		public void AdjustSpeed(float modifier)
		{
			this.SampleRate = (int)Math.Ceiling((double)((float)this.SampleRate * modifier));
		}

		public void Resample(int newSampleRate)
		{
			float num = 1f / (float)newSampleRate;
			float num2 = 1f / (float)this.SampleRate;
			int oldSamples = this.Samples;
			int newSamples = (int)((long)this.Samples * (long)newSampleRate / (long)this.SampleRate);
			float[][] newData = ArrayTools.AllocSquareJaggedArray<float>(this.Channels, newSamples);
			for (int chan = 0; chan < this.Channels; chan++)
			{
				for (int i = 0; i < newSamples; i++)
				{
					int sindx = (int)Math.Floor((double)((float)i * (float)(oldSamples - 1) / (float)newSamples));
					newData[chan][i] = this._channelData[chan][sindx];
				}
			}
			this._channelData = newData;
			this.SampleRate = newSampleRate;
		}

		public float GetAverageAmplitude(int sampleStart, int sampleEnd)
		{
			float acc = 0f;
			for (int i = 0; i < this.Channels; i++)
			{
				for (int j = sampleStart; j <= sampleEnd; j++)
				{
					acc += Math.Abs(this._channelData[i][j]);
				}
			}
			return acc / (float)(sampleEnd - sampleStart + 1);
		}

		public float GetMaxAmplitude(int sampleStart, int sampleEnd)
		{
			float max = float.MinValue;
			for (int i = 0; i < this.Channels; i++)
			{
				for (int j = sampleStart; j <= sampleEnd; j++)
				{
					float val = Math.Abs(this._channelData[i][j]);
					max = Math.Max(max, val);
				}
			}
			return max;
		}

		public float GetMaxAmplitude()
		{
			return this.GetMaxAmplitude(0, this.Samples - 1);
		}

		public void Normalize()
		{
			float max = this.GetMaxAmplitude();
			if (max == 0f)
			{
				max = 1f;
			}
			this.AdjustVolume(1f / max);
		}

		private void LoadWavInternal(Stream stream)
		{
			RawPCMData raw = RawPCMData.LoadWav(stream);
			this.Convert(raw);
		}

		public static RealPCMData LoadWav(string path)
		{
			RealPCMData realPCMData;
			using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				realPCMData = RealPCMData.LoadWav(stream);
			}
			return realPCMData;
		}

		public static RealPCMData LoadWav(Stream stream)
		{
			RealPCMData data = new RealPCMData();
			data.LoadWavInternal(stream);
			return data;
		}

		public void SaveWav(string path, int BitsPerSample)
		{
			using (FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				this.SaveWav(stream, BitsPerSample);
			}
		}

		public void SaveWav(Stream stream, int BitsPerSample)
		{
			new BinaryWriter(stream);
			RawPCMData raw = new RawPCMData();
			raw.Convert(this, BitsPerSample);
			raw.SaveWav(stream);
		}

		private float[][] _channelData = new float[][] { new float[0] };
	}
}
