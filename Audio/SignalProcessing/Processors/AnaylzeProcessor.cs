using System;

namespace DNA.Audio.SignalProcessing.Processors
{
	public class AnaylzeProcessor : SignalProcessor<SpectralData>
	{
		public FrequencyPair PrimaryFrequency
		{
			get
			{
				return this._primary;
			}
		}

		public override bool ProcessBlock(SpectralData data)
		{
			FrequencyPair[] freqs = data.GetData(0);
			float max = float.MinValue;
			for (int i = 0; i < freqs.Length; i++)
			{
				if (freqs[i].Magnitude > max)
				{
					max = freqs[i].Magnitude;
					this._primary = freqs[i];
				}
			}
			this._primary.Value.Hertz = Math.Abs(this._primary.Value.Hertz);
			return true;
		}

		private FrequencyPair _primary;
	}
}
