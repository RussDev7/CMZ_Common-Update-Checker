using System;

namespace DNA.Audio.SignalProcessing.Processors
{
	public class VocoderProcessor : SignalProcessor<RealPCMData>
	{
		public VocoderProcessor(RealPCMData carrier)
		{
			this._fft1 = new VocoderProcessor.FFT(this._windowLength);
			this._fft2 = new VocoderProcessor.FFT(this._windowLength / 2);
			this.ProcessCarrier(carrier);
		}

		private void ProcessCarrier(RealPCMData carrier)
		{
			this.CarrierBuffer = new float[this._windowLength];
			this.CarrierBuffer = carrier.GetData(0);
			float[] carrierSampleBuffer = new float[this._windowLength];
			this._loopedCarrierBuffer = new float[4096, 2];
			VocoderProcessor.ReadLooped(this.CarrierBuffer, carrierSampleBuffer, 0, this._windowLength);
			VocoderProcessor.ToComplexArray(carrierSampleBuffer, this._loopedCarrierBuffer, this._windowLength);
			this._fft1.DoFFT(this._loopedCarrierBuffer, this._windowLength, 1);
			this.NormalizeFFT(this._loopedCarrierBuffer, this._windowLength);
		}

		private static int ReadBuffer(float[] buffer, int sourcePos, float[] dest, int destPos, int length)
		{
			int remaining = buffer.Length - sourcePos;
			if (remaining > length)
			{
				remaining = length;
			}
			int pad = length - remaining;
			for (int i = 0; i < remaining; i++)
			{
				dest[i + destPos] = buffer[i + sourcePos];
			}
			for (int j = 0; j < pad; j++)
			{
				dest[j + destPos] = 0f;
			}
			return remaining;
		}

		private static void ReadLooped(float[] buffer, float[] dest, int dpos, int length)
		{
			int spos = 0;
			while (length > 0)
			{
				if (spos >= buffer.Length)
				{
					spos = 0;
				}
				dest[dpos] = buffer[spos];
				dpos++;
				spos++;
				length--;
			}
		}

		private static void CopyBuffer(float[] source, float[] dest, int length)
		{
			for (int i = 0; i < length; i++)
			{
				dest[i] = source[i];
			}
		}

		private static void ToComplexArray(float[] source, float[,] dest, int length)
		{
			for (int i = 0; i < length; i++)
			{
				dest[i, 0] = source[i];
				dest[i, 1] = 0f;
			}
		}

		private static void ComplexToSample(float[,] complex_array, float[] sample_array, int length)
		{
			for (int i = 0; i < length; i++)
			{
				sample_array[i] = complex_array[i, 0];
			}
		}

		private static int WriteBuffer(float[] output, int outputPos, float[] source, int sourcePos, int length)
		{
			for (int i = 0; i < length; i++)
			{
				output[outputPos + i] = source[sourcePos + i];
			}
			return length;
		}

		private void DoVocode(float[] modulatorBuffer)
		{
			int modulator_length = modulatorBuffer.Length;
			int num = (modulator_length - this._windowOverlap) / (this._windowLength - this._windowOverlap);
			int frame_no = 0;
			int modulatorReadBufferPos = 0;
			int modulatorWriteBufferPos = 0;
			do
			{
				int bytesRead = VocoderProcessor.ReadBuffer(modulatorBuffer, modulatorReadBufferPos, this._modulatorBuffer, this._windowOverlap, this._windowLength - this._windowOverlap);
				modulatorReadBufferPos += bytesRead;
				this.VocodeWindow(this._modulatorBuffer, this._loopedCarrierBuffer, this._outputBuffer);
				VocoderProcessor.ComplexToSample(this._outputBuffer, this._outputNew, this._windowLength);
				for (int i = 0; i < this._windowOverlap; i++)
				{
					this._outputNew[i] = this._outputNew[i] * ((float)i / (float)this._windowOverlap) + this._outputOld[this._windowLength - this._windowOverlap + i] * ((float)(this._windowOverlap - i) / (float)this._windowOverlap);
				}
				int bytesWritten = VocoderProcessor.WriteBuffer(modulatorBuffer, modulatorWriteBufferPos, this._outputNew, 0, this._windowLength - this._windowOverlap);
				modulatorWriteBufferPos += bytesWritten;
				for (int i = 0; i < this._windowOverlap; i++)
				{
					this._modulatorBuffer[i] = this._modulatorBuffer[this._windowLength - this._windowOverlap + i];
				}
				float[] output_temp = this._outputOld;
				this._outputOld = this._outputNew;
				this._outputNew = output_temp;
				frame_no++;
			}
			while (modulatorReadBufferPos < modulatorBuffer.Length);
		}

		private void VocodeWindow(float[] modulator, float[,] carrier, float[,] output)
		{
			int band_length = this._windowLength / (this._bandCount * 2);
			int extra_band_length = this._windowLength / 2 - band_length * (this._bandCount - 1);
			this.RealFFTMag(modulator, this._windowLength);
			for (int band_no = 0; band_no < this._bandCount; band_no++)
			{
				int i = ((band_no == this._bandCount - 1) ? extra_band_length : band_length);
				float j = 0f;
				float c = 0f;
				int k = 0;
				int l = band_no * band_length;
				int m = this._windowLength - l - 1;
				while (k < i)
				{
					if (this._Normalize)
					{
						float c2 = carrier[l, 0] * carrier[l, 0] + carrier[l, 1] * carrier[l, 1];
						float c3 = carrier[m, 0] * carrier[m, 0] + carrier[m, 1] * carrier[m, 1];
						c += (float)(Math.Sqrt((double)c2) + Math.Sqrt((double)c3));
					}
					j += modulator[l];
					k++;
					l++;
					m--;
				}
				if (!this._Normalize)
				{
					c = 1f;
				}
				if (c == 0f)
				{
					c = 0.0001f;
				}
				float mc = j / c;
				k = 0;
				l = band_no * band_length;
				m = this._windowLength - l - 1;
				while (k < i)
				{
					output[l, 0] = carrier[l, 0] * mc;
					output[l, 1] = carrier[l, 1] * mc;
					output[m, 0] = carrier[m, 0] * mc;
					output[m, 1] = carrier[m, 1] * mc;
					k++;
					l++;
					m--;
				}
			}
			this._fft1.DoFFT(output, this._windowLength, -1);
		}

		private void RealFFTMag(float[] data, int windowlength)
		{
			float[,] x = new float[windowlength / 2, 2];
			for (int i = 0; i < windowlength; i += 2)
			{
				x[i >> 1, 0] = data[i];
				x[i >> 1, 1] = data[i + 1];
			}
			this._fft2.DoFFT(x, windowlength / 2, 1);
			data[0] = (x[0, 0] + x[0, 1]) / (float)windowlength;
			for (int i = 1; i < windowlength / 2; i++)
			{
				double arg = 6.283185307179586 * (double)i / (double)windowlength;
				double c = Math.Cos(arg);
				double s = Math.Sin(arg);
				double ti = (double)((x[i, 1] + x[windowlength / 2 - i, 1]) / 2f);
				double tr = (double)((x[i, 0] - x[windowlength / 2 - i, 0]) / 2f);
				double xr = (double)((x[i, 0] + x[windowlength / 2 - i, 0]) / 2f) + c * ti - s * tr;
				double xi = (double)((x[i, 1] - x[windowlength / 2 - i, 1]) / 2f) - s * ti - c * tr;
				xr /= (double)(windowlength / 2);
				xi /= (double)(windowlength / 2);
				data[i] = (float)Math.Sqrt(xr * xr + xi * xi);
			}
			data[windowlength / 2] = (x[0, 0] - x[0, 1]) / (float)windowlength;
		}

		private void NormalizeFFT(float[,] x, int n)
		{
			for (int i = 0; i < n; i++)
			{
				x[i, 0] /= (float)n;
				x[i, 1] /= (float)n;
			}
		}

		private void Initalize(int size)
		{
			this._modulatorBuffer = new float[size];
			this._outputBuffer = new float[size, 2];
			this._outputOld = new float[size];
			this._outputNew = new float[size];
		}

		public override bool ProcessBlock(RealPCMData data)
		{
			return true;
		}

		private const int MaxWindowSize = 4096;

		private int _windowLength = 512;

		private int _windowOverlap = 256;

		private int _bandCount = 16;

		private bool _Normalize = true;

		private float[] CarrierBuffer;

		private float[] _modulatorBuffer = new float[4096];

		private float[,] _loopedCarrierBuffer;

		private float[,] _outputBuffer = new float[4096, 2];

		private float[] _outputOld = new float[4096];

		private float[] _outputNew = new float[4096];

		private VocoderProcessor.FFT _fft1;

		private VocoderProcessor.FFT _fft2;

		private class FFT
		{
			private static int ilog2(int n)
			{
				int i = -1;
				while (n != 0)
				{
					i++;
					n >>= 1;
				}
				return i;
			}

			private static int BitReverse(int k, int nu)
			{
				int outv = 0;
				for (int i = 0; i < nu; i++)
				{
					outv <<= 1;
					outv |= k & 1;
					k >>= 1;
				}
				return outv;
			}

			private void CreateArrays(int n)
			{
				int nu = VocoderProcessor.FFT.ilog2(n);
				this._sinTable = new float[n];
				this._cosTable = new float[n];
				this._revTable = new int[n];
				for (int i = 0; i < n; i++)
				{
					double arg = 6.283185307179586 * (double)i / (double)n;
					this._cosTable[i] = (float)Math.Cos(arg);
					this._sinTable[i] = (float)Math.Sin(arg);
					this._revTable[i] = VocoderProcessor.FFT.BitReverse(i, nu);
				}
			}

			public FFT(int windowSize)
			{
				int nu = VocoderProcessor.FFT.ilog2(windowSize);
				this._sinTable = new float[windowSize];
				this._cosTable = new float[windowSize];
				this._revTable = new int[windowSize];
				for (int i = 0; i < windowSize; i++)
				{
					double arg = 6.283185307179586 * (double)i / (double)windowSize;
					this._cosTable[i] = (float)Math.Cos(arg);
					this._sinTable[i] = (float)Math.Sin(arg);
					this._revTable[i] = VocoderProcessor.FFT.BitReverse(i, nu);
				}
			}

			public void DoFFT(float[,] data, int windowSize, int sign)
			{
				int nu = VocoderProcessor.FFT.ilog2(windowSize);
				int dual_space = windowSize;
				int nu2 = nu;
				for (int i = 0; i < nu; i++)
				{
					dual_space /= 2;
					nu2--;
					for (int j = 0; j < windowSize; j += dual_space)
					{
						int k = 0;
						while (k < dual_space)
						{
							int p = this._revTable[j >> nu2];
							float treal = data[j + dual_space, 0] * this._cosTable[p] + data[j + dual_space, 1] * this._sinTable[p] * (float)sign;
							float timag = data[j + dual_space, 1] * this._cosTable[p] - data[j + dual_space, 0] * this._sinTable[p] * (float)sign;
							data[j + dual_space, 0] = data[j, 0] - treal;
							data[j + dual_space, 1] = data[j, 1] - timag;
							data[j, 0] += treal;
							data[j, 1] += timag;
							k++;
							j++;
						}
					}
				}
				for (int j = 0; j < windowSize; j++)
				{
					int k = this._revTable[j];
					if (k > j)
					{
						float treal2 = data[j, 0];
						float timag2 = data[j, 1];
						data[j, 0] = data[k, 0];
						data[j, 1] = data[k, 1];
						data[k, 0] = treal2;
						data[k, 1] = timag2;
					}
				}
			}

			private float[] _sinTable;

			private float[] _cosTable;

			private int[] _revTable;
		}
	}
}
