using System;

namespace DNA.Audio.SignalProcessing.Processors
{
	public class RealLevelsProcessor : SignalProcessor<RealPCMData>
	{
		public override bool ProcessBlock(RealPCMData data)
		{
			float[] cdata = data.GetData(0);
			this.PeakLevel = 0f;
			this.averageLevel = 0f;
			for (int i = 0; i < cdata.Length; i++)
			{
				float val = Math.Abs(cdata[i]);
				this.PeakLevel = ((val > this.PeakLevel) ? val : this.PeakLevel);
				this.averageLevel += val;
			}
			this.averageLevel /= (float)cdata.Length;
			return true;
		}

		public float PeakLevel;

		public float averageLevel;

		public float Volume = 1f;
	}
}
