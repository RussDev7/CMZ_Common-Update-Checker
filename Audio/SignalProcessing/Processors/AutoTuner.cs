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
			Frequency primaryFreq = this._anaylizer.PrimaryFrequency.Value;
			primaryFreq.Hertz = Math.Abs(primaryFreq.Hertz);
			Tone note = Tone.FromFrequency(primaryFreq);
			int key = note.KeyValue;
			if (note.Detune > 0.5f)
			{
				key++;
			}
			if (note.Detune < -0.5f)
			{
				key--;
			}
			Tone baseTone = Tone.FromKeyIndex(key);
			if (primaryFreq.Hertz > 0f)
			{
				float detune = baseTone.Frequency.Hertz / primaryFreq.Hertz;
				this._pitchShifter.Pitch = detune;
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
