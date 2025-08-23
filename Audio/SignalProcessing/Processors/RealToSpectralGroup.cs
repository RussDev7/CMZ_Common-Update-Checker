using System;

namespace DNA.Audio.SignalProcessing.Processors
{
	public class RealToSpectralGroup : SignalProcessorGroup<RealPCMData, SpectralData>
	{
		private void Alloc()
		{
			this._specbuffer = new SpectralData(1, this.FFTSize / 2 + 1);
			this._fft = new ShortTimeFFT(this.FFTSize);
			this._windowVals = new float[this.FFTSize];
			for (int i = 0; i < this.FFTSize; i++)
			{
				this._windowVals[i] = -0.5f * (float)Math.Cos(6.283185307179586 * (double)((float)i) / (double)((float)this.FFTSize)) + 0.5f;
			}
		}

		public override bool ProcessBlock(RealPCMData data)
		{
			if (this._windowVals == null)
			{
				this.Alloc();
			}
			int numSampsToProcess = data.Samples;
			int sampleRate = data.SampleRate;
			float[] indata = data.GetData(0);
			int fftFrameSize = this.FFTSize;
			int osamp = this.OverSampling;
			float pi = 3.1415927f;
			float pi2 = 6.2831855f;
			float[] outdata = indata;
			int fftFrameSize2 = fftFrameSize / 2;
			int stepSize = fftFrameSize / osamp;
			float freqPerBin = (float)sampleRate / (float)fftFrameSize;
			float expct = (float)(6.283185307179586 * (double)((float)stepSize) / (double)((float)fftFrameSize));
			int inFifoLatency = fftFrameSize - stepSize;
			if (this.gRover == 0)
			{
				this.gRover = inFifoLatency;
			}
			int inputPos = 0;
			int outputPos = 0;
			FrequencyPair[] freqs = this._specbuffer.GetData(0);
			while (inputPos < numSampsToProcess)
			{
				int dataLeft = numSampsToProcess - inputPos;
				int blockSize = fftFrameSize - this.gRover;
				if (blockSize > dataLeft)
				{
					blockSize = dataLeft;
				}
				Buffer.BlockCopy(indata, inputPos * 4, this.gInFIFO, this.gRover * 4, blockSize * 4);
				Buffer.BlockCopy(this.gOutFIFO, (this.gRover - inFifoLatency) * 4, outdata, outputPos * 4, blockSize * 4);
				inputPos += blockSize;
				outputPos += blockSize;
				this.gRover += blockSize;
				if (this.gRover >= fftFrameSize)
				{
					this.gRover = inFifoLatency;
					for (int i = 0; i < fftFrameSize; i++)
					{
						this.gFFTworksp[2 * i] = this.gInFIFO[i] * this._windowVals[i];
						this.gFFTworksp[2 * i + 1] = 0f;
					}
					this._fft.Transform(this.gFFTworksp, -1);
					for (int i = 0; i <= fftFrameSize2; i++)
					{
						float real = this.gFFTworksp[2 * i];
						float imag = this.gFFTworksp[2 * i + 1];
						float magn = 2f * (float)Math.Sqrt((double)(real * real + imag * imag));
						float phase = (float)Math.Atan2((double)imag, (double)real);
						float tmp = phase - this.gLastPhase[i];
						this.gLastPhase[i] = phase;
						tmp -= (float)i * expct;
						int qpd = (int)(tmp / pi);
						if (qpd >= 0)
						{
							qpd += qpd & 1;
						}
						else
						{
							qpd -= qpd & 1;
						}
						tmp -= pi * (float)qpd;
						tmp = (float)osamp * tmp / pi2;
						tmp = (float)i * freqPerBin + tmp * freqPerBin;
						freqs[i].Magnitude = magn;
						freqs[i].Value.Hertz = tmp;
					}
					for (int j = 0; j < base.Count; j++)
					{
						SignalProcessor<SpectralData> proc = base[j];
						if (proc.Active)
						{
							proc.ProcessBlock(this._specbuffer);
						}
					}
					for (int i = 0; i <= fftFrameSize2; i++)
					{
						float magn = freqs[i].Magnitude;
						float tmp = freqs[i].Value.Hertz;
						tmp -= (float)i * freqPerBin;
						tmp /= freqPerBin;
						tmp = pi2 * tmp / (float)osamp;
						tmp += (float)i * expct;
						this.gSumPhase[i] += tmp;
						float phase = this.gSumPhase[i];
						this.gFFTworksp[2 * i] = magn * (float)Math.Cos((double)phase);
						this.gFFTworksp[2 * i + 1] = magn * (float)Math.Sin((double)phase);
					}
					for (int i = fftFrameSize + 2; i < 2 * fftFrameSize; i++)
					{
						this.gFFTworksp[i] = 0f;
					}
					this._fft.Transform(this.gFFTworksp, 1);
					float mval = 2f / (float)(fftFrameSize2 * osamp);
					for (int i = 0; i < fftFrameSize; i++)
					{
						this.gOutputAccum[i] += mval * this._windowVals[i] * this.gFFTworksp[2 * i];
					}
					Buffer.BlockCopy(this.gOutputAccum, 0, this.gOutFIFO, 0, stepSize * 4);
					Buffer.BlockCopy(this.gOutputAccum, stepSize * 4, this.gOutputAccum, 0, fftFrameSize * 4);
					Buffer.BlockCopy(this.gInFIFO, stepSize * 4, this.gInFIFO, 0, inFifoLatency * 4);
				}
			}
			return true;
		}

		private const int MAX_FRAME_LENGTH = 4096;

		private float[] gInFIFO = new float[4096];

		private float[] gOutFIFO = new float[4096];

		private float[] gFFTworksp = new float[8192];

		private float[] gLastPhase = new float[2049];

		private float[] gSumPhase = new float[2049];

		private float[] gOutputAccum = new float[8192];

		private int gRover;

		public int OverSampling = 2;

		public int FFTSize = 256;

		private float[] _windowVals;

		private ShortTimeFFT _fft = new ShortTimeFFT(1024);

		private SpectralData _specbuffer = new SpectralData(1, 1024);
	}
}
