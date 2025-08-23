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
			float freq = this.Frequency.Hertz;
			float dt = 1f / (float)data.SampleRate;
			for (int channel = 0; channel < data.Channels; channel++)
			{
				float[] buffer = data.GetData(channel);
				for (int i = 0; i < buffer.Length; i++)
				{
					this.t += dt;
					buffer[i] = (float)Math.Sin((double)(this.t * freq) * 3.141592653589793 * 2.0);
				}
			}
			return true;
		}

		private float t;

		public Frequency Frequency = Frequency.FromHertz(440f);
	}
}
