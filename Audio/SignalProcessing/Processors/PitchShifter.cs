using System;

namespace DNA.Audio.SignalProcessing.Processors
{
	public class PitchShifter : SignalProcessor<SpectralData>
	{
		public override bool ProcessBlock(SpectralData data)
		{
			data.CopyTo(this._buffer);
			data.SetZero();
			for (int channel = 0; channel < data.Channels; channel++)
			{
				FrequencyPair[] freqDest = data.GetData(channel);
				FrequencyPair[] freqSource = this._buffer.GetData(channel);
				for (int i = 0; i < freqDest.Length; i++)
				{
					int index = (int)((float)i * this.Pitch);
					if (index < freqDest.Length)
					{
						FrequencyPair[] array = freqDest;
						int num = index;
						array[num].Magnitude = array[num].Magnitude + freqSource[i].Magnitude;
						freqDest[index].Value.Hertz = freqSource[i].Value.Hertz * this.Pitch;
					}
				}
			}
			return true;
		}

		public float Pitch = 1f;

		private SpectralData _buffer = new SpectralData(1, 0);
	}
}
