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
			int X = x & 255;
			int Y = y & 255;
			int Z = z & 255;
			int A = IntNoise._permute[X] + Y;
			int AA = IntNoise._permute[A] + Z;
			return IntNoise._permute[AA];
		}

		public int ComputeNoise(int x, int y)
		{
			int X = x & 255;
			int Y = y & 255;
			int A = IntNoise._permute[X] + Y;
			return IntNoise._permute[A];
		}

		private static int[] _permute = new int[1024];
	}
}
