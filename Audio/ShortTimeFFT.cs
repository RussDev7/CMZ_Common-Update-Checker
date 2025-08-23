using System;

namespace DNA.Audio
{
	public class ShortTimeFFT
	{
		public ShortTimeFFT(int frameSize)
		{
			this.FrameSize = frameSize;
			this._revTable = new int[frameSize * 2];
			for (int i = 2; i < 2 * this.FrameSize - 2; i += 2)
			{
				int bitm = 2;
				int j = 0;
				while (bitm < frameSize * 2)
				{
					if ((i & bitm) != 0)
					{
						j++;
					}
					j <<= 1;
					bitm <<= 1;
				}
				if (i < j)
				{
					this._revTable[i] = j;
					this._revTable[j] = i;
					this._revTable[i + 1] = j + 1;
					this._revTable[j + 1] = i + 1;
				}
			}
		}

		public void Transform(float[] fftBuffer, int sign)
		{
			int fftFrameSize = this.FrameSize;
			int fftFrameSize2 = fftFrameSize << 1;
			for (int i = 2; i < fftFrameSize2 - 2; i += 2)
			{
				int j = this._revTable[i];
				if (i < j)
				{
					float temp = fftBuffer[i];
					fftBuffer[i] = fftBuffer[j];
					fftBuffer[j] = temp;
					temp = fftBuffer[i + 1];
					fftBuffer[i + 1] = fftBuffer[j + 1];
					fftBuffer[j + 1] = temp;
				}
			}
			int max = (int)(Math.Log((double)fftFrameSize) / Math.Log(2.0) + 0.5);
			int k = 0;
			int le = 2;
			while (k < max)
			{
				le <<= 1;
				int le2 = le >> 1;
				float ur = 1f;
				float ui = 0f;
				float arg = (float)(3.141592653589793 / (double)(le2 >> 1));
				float wr = (float)Math.Cos((double)arg);
				float wi = (float)sign * (float)Math.Sin((double)arg);
				for (int l = 0; l < le2; l += 2)
				{
					float tr;
					for (int m = l; m < fftFrameSize2; m += le)
					{
						int index = m + le2;
						tr = fftBuffer[index] * ur - fftBuffer[index + 1] * ui;
						float ti = fftBuffer[index] * ui + fftBuffer[index + 1] * ur;
						fftBuffer[index] = fftBuffer[m] - tr;
						fftBuffer[index + 1] = fftBuffer[m + 1] - ti;
						fftBuffer[m] += tr;
						fftBuffer[m + 1] += ti;
					}
					tr = ur * wr - ui * wi;
					ui = ur * wi + ui * wr;
					ur = tr;
				}
				k++;
			}
		}

		private int FrameSize;

		private int[] _revTable;
	}
}
