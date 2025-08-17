using System;
using DNA.Data.Units;
using DNA.Multimedia.Audio;

namespace DNA.Audio.SignalProcessing.Processors
{
	public class AutoTuner : SignalProcessor<SpectralData>
	{
		public AnaylzeProcessor Anaylizer
		{
			get
			{
				return this._anaylizer;
			}
		}

		public override bool ProcessBlock(SpectralData data)
		{
			this._anaylizer.ProcessBlock(data);
			Frequency value = this._anaylizer.PrimaryFrequency.Value;
			value.Hertz = Math.Abs(value.Hertz);
			Tone tone = Tone.FromFrequency(value);
			int num = tone.KeyValue;
			if (tone.Detune > 0.5f)
			{
				num++;
			}
			if (tone.Detune < -0.5f)
			{
				num--;
			}
			Tone tone2 = Tone.FromKeyIndex(num);
			if (value.Hertz > 0f)
			{
				float num2 = tone2.Frequency.Hertz / value.Hertz;
				this._pitchShifter.Pitch = num2;
			}
			else
			{
				this._pitchShifter.Pitch = 1f;
			}
			this._pitchShifter.ProcessBlock(data);
			return true;
		}

		private PitchShifter _pitchShifter = new PitchShifter();

		private AnaylzeProcessor _anaylizer = new AnaylzeProcessor();
	}
}
