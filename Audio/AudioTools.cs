using System;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework.Audio;

namespace DNA.Audio
{
	public static class AudioTools
	{
		public static void ShortTimeFourierTransform(float[] fftBuffer, int fftFrameSize, int sign)
		{
			int fftFrameSize2 = fftFrameSize << 1;
			for (int i = 2; i < 2 * fftFrameSize - 2; i += 2)
			{
				int bitm = 2;
				int j = 0;
				while (bitm < fftFrameSize2)
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
				for (int j = 0; j < le2; j += 2)
				{
					float tr;
					for (int i = j; i < fftFrameSize2; i += le)
					{
						int index = i + le2;
						tr = fftBuffer[index] * ur - fftBuffer[index + 1] * ui;
						float ti = fftBuffer[index] * ui + fftBuffer[index + 1] * ur;
						fftBuffer[index] = fftBuffer[i] - tr;
						fftBuffer[index + 1] = fftBuffer[i + 1] - ti;
						fftBuffer[i] += tr;
						fftBuffer[i + 1] += ti;
					}
					tr = ur * wr - ui * wi;
					ui = ur * wi + ui * wr;
					ur = tr;
				}
				k++;
			}
		}

		public static Microphone GetMic(SignedInGamer gamer)
		{
			foreach (Microphone mic in Microphone.All)
			{
				if (gamer.IsHeadset(mic))
				{
					try
					{
						MicrophoneState state = mic.State;
					}
					catch (NoMicrophoneConnectedException)
					{
						return null;
					}
					return mic;
				}
			}
			return null;
		}
	}
}
