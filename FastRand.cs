using System;

namespace DNA
{
	public class FastRand
	{
		public int Seed
		{
			set
			{
				this._idnum = value;
			}
		}

		public FastRand()
		{
			this._idnum = (int)DateTime.Now.Ticks;
		}

		public FastRand(int seed)
		{
			this._idnum = seed;
			this.GetNextValue();
		}

		public float GetNextValue()
		{
			float num2;
			do
			{
				this._idnum ^= 123459876;
				int num = this._idnum / 127773;
				this._idnum = 16807 * (this._idnum - num * 127773) - 2836 * num;
				if (this._idnum < 0)
				{
					this._idnum += int.MaxValue;
				}
				num2 = 4.656613E-10f * (float)this._idnum;
				this._idnum ^= 123459876;
				num2 = 1f - num2;
			}
			while (num2 >= 1f);
			return num2;
		}

		public int GetNextValue(int min, int max)
		{
			return (int)((float)(max - min) * this.GetNextValue()) + min;
		}

		public float GetNextValue(float min, float max)
		{
			float num = this.GetNextValue() * (max - min);
			return num + min;
		}

		private const int IA = 16807;

		private const int IM = 2147483647;

		private const float AM = 4.656613E-10f;

		private const int IQ = 127773;

		private const int IR = 2836;

		private const int MASK = 123459876;

		private int _idnum;
	}
}
