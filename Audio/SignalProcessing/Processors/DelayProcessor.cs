using System;

namespace DNA.Audio.SignalProcessing.Processors
{
	public class DelayProcessor : SignalProcessor<RealPCMData>
	{
		public override bool ProcessBlock(RealPCMData data)
		{
			int delayLen = (int)((double)data.SampleRate * this.Delay.TotalSeconds);
			int bufferNeeded = delayLen * 2;
			if (data.Channels != this._delayBuffer.Channels || bufferNeeded != this._delayBuffer.Samples)
			{
				this._delayBuffer = new RealPCMData(data.Channels, bufferNeeded, data.SampleRate);
			}
			for (int channel = 0; channel < data.Channels; channel++)
			{
				float[] buffer = data.GetData(channel);
				float[] delayBuffer = this._delayBuffer.GetData(channel);
				for (int i = 0; i < buffer.Length; i++)
				{
					float currentValue = buffer[i];
					float oldValue = delayBuffer[this._delayPos] * this.Decay;
					currentValue += oldValue;
					buffer[i] = currentValue;
					int savePos = (this._delayPos + delayLen) % delayBuffer.Length;
					delayBuffer[savePos] = currentValue;
					this._delayPos++;
					if (this._delayPos >= delayBuffer.Length)
					{
						this._delayPos = 0;
					}
				}
			}
			return true;
		}

		public TimeSpan Delay = TimeSpan.FromMilliseconds(300.0);

		public float Decay = 0.5f;

		private RealPCMData _delayBuffer = new RealPCMData(1, 0, 1);

		private int _delayPos;
	}
}
