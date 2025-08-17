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
			FrequencyPair[] data2 = data.GetData(0);
			float num = float.MinValue;
			for (int i = 0; i < data2.Length; i++)
			{
				if (data2[i].Magnitude > num)
				{
					num = data2[i].Magnitude;
					this._primary = data2[i];
				}
			}
			this._primary.Value.Hertz = Math.Abs(this._primary.Value.Hertz);
			return true;
		}

		private FrequencyPair _primary;
	}
}
