using System;

namespace DNA.Audio.SignalProcessing.Processors
{
	public class CompressorProcessor : RealLevelsProcessor
	{
		public override bool ProcessBlock(RealPCMData data)
		{
			base.ProcessBlock(data);
			this.levelHistory[this.index++] = this.PeakLevel;
			if (this.index >= this.levelHistory.Length - 1)
			{
				this.index = 0;
			}
			float averageLevelHistory = 0f;
			for (int i = 0; i < this.levelHistory.Length; i++)
			{
				averageLevelHistory += this.levelHistory[i];
			}
			averageLevelHistory /= (float)this.levelHistory.Length;
			float LevelMultiplier = this.targetLevel / averageLevelHistory;
			float[] cdata = data.GetData(0);
			if (this.PeakLevel * LevelMultiplier > 0.8f)
			{
				LevelMultiplier = 0.8f / this.PeakLevel;
			}
			if (this.PeakLevel > 0.15f)
			{
				for (int j = 0; j < cdata.Length; j++)
				{
					cdata[j] *= LevelMultiplier;
				}
			}
			return true;
		}

		public float targetLevel = 1f;

		private float[] levelHistory = new float[20];

		private int index;
	}
}
