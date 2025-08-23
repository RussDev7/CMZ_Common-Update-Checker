using System;
using Microsoft.Xna.Framework.Media;

namespace DNA.Audio
{
	public class BeatFinder
	{
		public bool BeatFound
		{
			get
			{
				return this.beatFound;
			}
		}

		public float AverageBMP
		{
			get
			{
				return this.averageBMP;
			}
		}

		public void Update(VisualizationData visData, TimeSpan elaspedTime)
		{
			this.timeSinceLastPeek += elaspedTime;
			float averageValue = 0f;
			for (int i = 0; i < visData.Samples.Count; i++)
			{
				float value = visData.Samples[i];
				value *= value;
				averageValue += value;
			}
			averageValue /= (float)visData.Samples.Count;
			if (averageValue != this.historicalAverages[this.historicalAverages.Length - 1])
			{
				float avgLevel = 0f;
				for (int j = 0; j < this.historicalAverages.Length; j++)
				{
					avgLevel += this.historicalAverages[j];
				}
				avgLevel /= (float)this.historicalAverages.Length;
				float varaince = 0f;
				for (int k = 0; k < this.historicalAverages.Length; k++)
				{
					varaince += Math.Abs(this.historicalAverages[k] - avgLevel);
				}
				varaince /= (float)this.historicalAverages.Length;
				float diff = averageValue - avgLevel;
				if (diff >= varaince && this.timeSinceLastPeek.TotalSeconds > 0.20000000298023224)
				{
					this.beatFound = true;
					TimeSpan avgInterval = TimeSpan.Zero;
					for (int l = 0; l < this.beats.Length - 1; l++)
					{
						avgInterval += this.beats[l];
						this.beats[l] = this.beats[l + 1];
					}
					this.averageBMP = 60f / (float)TimeSpan.FromSeconds(avgInterval.TotalSeconds / (double)(this.beats.Length - 1)).TotalSeconds;
					this.beats[this.beats.Length - 1] = this.timeSinceLastPeek;
					this.timeSinceLastPeek = TimeSpan.Zero;
				}
				else
				{
					this.beatFound = false;
				}
				for (int m = 0; m < this.historicalAverages.Length - 1; m++)
				{
					this.historicalAverages[m] = this.historicalAverages[m + 1];
				}
				this.historicalAverages[this.historicalAverages.Length - 1] = averageValue;
			}
		}

		private TimeSpan[] beats = new TimeSpan[30];

		private TimeSpan timeSinceLastPeek = TimeSpan.Zero;

		private float averageBMP;

		private float[] historicalAverages = new float[50];

		private bool beatFound;
	}
}
