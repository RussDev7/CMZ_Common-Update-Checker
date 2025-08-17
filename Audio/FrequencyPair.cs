using System;
using DNA.Data.Units;

namespace DNA.Audio
{
	public struct FrequencyPair
	{
		public override string ToString()
		{
			return this.Value.ToString() + " " + this.Magnitude.ToString();
		}

		public Frequency Value;

		public float Magnitude;
	}
}
