using System;
using DNA.Data.Units;

namespace DNA.Audio.SignalProcessing.Processors
{
	public class ToneGenerator : SignalProcessor<RealPCMData>
	{
		public void GenerateBlock()
		{
		}

		public override bool ProcessBlock(RealPCMData data)
		{
			float num = this.Frequency.Hertz;
			float num2 = 1f / (float)data.SampleRate;
			for (int i = 0; i < data.Channels; i++)
			{
				float[] data2 = data.GetData(i);
				for (int j = 0; j < data2.Length; j++)
				{
					this.t += num2;
					data2[j] = (float)Math.Sin((double)(this.t * num) * 3.141592653589793 * 2.0);
				}
			}
			return true;
		}

		private float t;

		public Frequency Frequency = Frequency.FromHertz(440f);
	}
}
