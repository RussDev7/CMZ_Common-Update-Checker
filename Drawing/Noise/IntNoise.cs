using System;

namespace DNA.Drawing.Noise
{
	public class IntNoise
	{
		public IntNoise()
		{
			this.Initalize(new Random());
		}

		public IntNoise(Random r)
		{
			this.Initalize(r);
		}

		private void Initalize(Random r)
		{
			for (int i = 0; i < 256; i++)
			{
				IntNoise._permute[256 + i] = (IntNoise._permute[i] = r.Next(256));
			}
			for (int j = 0; j < 512; j++)
			{
				IntNoise._permute[512 + j] = IntNoise._permute[j];
			}
		}

		public int ComputeNoise(IntVector3 v)
		{
			return this.ComputeNoise(v.X, v.Y, v.Z);
		}

		public int ComputeNoise(int x, int y, int z)
		{
			int num = x & 255;
			int num2 = y & 255;
			int num3 = z & 255;
			int num4 = IntNoise._permute[num] + num2;
			int num5 = IntNoise._permute[num4] + num3;
			return IntNoise._permute[num5];
		}

		public int ComputeNoise(int x, int y)
		{
			int num = x & 255;
			int num2 = y & 255;
			int num3 = IntNoise._permute[num] + num2;
			return IntNoise._permute[num3];
		}

		private static int[] _permute = new int[1024];
	}
}
