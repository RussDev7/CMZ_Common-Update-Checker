using System;

namespace DNA.SignalProcessing
{
	public class SafeFourierTransform : ISpectralTransform
	{
		public virtual void Transform(float[] data)
		{
			SafeFourierTransform.RealFT(data, 1);
			float norm = 1f / (float)data.Length;
			for (int i = 0; i < data.Length; i++)
			{
				data[i] *= norm;
			}
		}

		public virtual void InverseTransform(float[] data)
		{
			SafeFourierTransform.RealFT(data, -1);
			for (int i = 0; i < data.Length; i++)
			{
				data[i] *= 2f;
			}
		}

		private static void RealFT(float[] data, int isign)
		{
			uint i = (uint)data.Length;
			float c = 0.5f;
			double theta = 3.141592653589793 / (i >> 1);
			float c2;
			if (isign == 1)
			{
				c2 = -0.5f;
				SafeFourierTransform.Four1(data, 1);
			}
			else
			{
				c2 = 0.5f;
				theta = -theta;
			}
			double wtemp = Math.Sin(0.5 * theta);
			double wpr = -2.0 * wtemp * wtemp;
			double wpi = Math.Sin(theta);
			double wr = 1.0 + wpr;
			double wi = wpi;
			uint np3 = i + 3U;
			float h1r;
			for (uint j = 2U; j <= i >> 2; j += 1U)
			{
				uint i2 = j + j - 1U;
				uint i3 = 1U + i2;
				uint i4 = np3 - i3;
				uint i5 = 1U + i4;
				i2 -= 1U;
				i3 -= 1U;
				i4 -= 1U;
				i5 -= 1U;
				h1r = c * (data[(int)((UIntPtr)i2)] + data[(int)((UIntPtr)i4)]);
				float h1i = c * (data[(int)((UIntPtr)i3)] - data[(int)((UIntPtr)i5)]);
				float h2r = -c2 * (data[(int)((UIntPtr)i3)] + data[(int)((UIntPtr)i5)]);
				float h2i = c2 * (data[(int)((UIntPtr)i2)] - data[(int)((UIntPtr)i4)]);
				data[(int)((UIntPtr)i2)] = (float)((double)h1r + wr * (double)h2r - wi * (double)h2i);
				data[(int)((UIntPtr)i3)] = (float)((double)h1i + wr * (double)h2i + wi * (double)h2r);
				data[(int)((UIntPtr)i4)] = (float)((double)h1r - wr * (double)h2r + wi * (double)h2i);
				data[(int)((UIntPtr)i5)] = (float)((double)(-(double)h1i) + wr * (double)h2i + wi * (double)h2r);
				wr = (wtemp = wr) * wpr - wi * wpi + wr;
				wi = wi * wpr + wtemp * wpi + wi;
			}
			h1r = data[0];
			data[0] = h1r + data[1];
			data[1] = h1r - data[1];
			if (isign == -1)
			{
				data[0] *= c;
				data[1] *= c;
				SafeFourierTransform.Four1(data, -1);
			}
		}

		private static void Four1(float[] data, int isign)
		{
			if (!data.Length.IsPowerOf2())
			{
				throw new ArgumentException("Array Lenght must be power of 2");
			}
			uint nn = (uint)data.Length >> 1;
			uint i = nn << 1;
			uint j = 1U;
			for (uint k = 1U; k < i; k += 2U)
			{
				if (j > k)
				{
					float tempv = data[(int)((UIntPtr)(j - 1U))];
					data[(int)((UIntPtr)(j - 1U))] = data[(int)((UIntPtr)(k - 1U))];
					data[(int)((UIntPtr)(k - 1U))] = tempv;
					tempv = data[(int)((UIntPtr)j)];
					data[(int)((UIntPtr)j)] = data[(int)((UIntPtr)k)];
					data[(int)((UIntPtr)k)] = tempv;
				}
				uint l = nn;
				while (l >= 2U && j > l)
				{
					j -= l;
					l >>= 1;
				}
				j += l;
			}
			uint mmax = 2U;
			while (i > mmax)
			{
				uint istep = mmax << 1;
				double theta = (double)isign * (12.566370614359172 / mmax);
				double wtemp = Math.Sin(0.5 * theta);
				double wpr = -2.0 * wtemp * wtemp;
				double wpi = Math.Sin(theta);
				double wr = 1.0;
				double wi = 0.0;
				for (uint l = 1U; l < mmax; l += 2U)
				{
					for (uint k = l; k <= i; k += istep)
					{
						j = k + mmax;
						float tempr = (float)(wr * (double)data[(int)((UIntPtr)(j - 1U))] - wi * (double)data[(int)((UIntPtr)j)]);
						float tempi = (float)(wr * (double)data[(int)((UIntPtr)j)] + wi * (double)data[(int)((UIntPtr)(j - 1U))]);
						data[(int)((UIntPtr)(j - 1U))] = data[(int)((UIntPtr)(k - 1U))] - tempr;
						data[(int)((UIntPtr)j)] = data[(int)((UIntPtr)k)] - tempi;
						data[(int)((UIntPtr)(k - 1U))] += tempr;
						data[(int)((UIntPtr)k)] += tempi;
					}
					wr = (wtemp = wr) * wpr - wi * wpi + wr;
					wi = wi * wpr + wtemp * wpi + wi;
				}
				mmax = istep;
			}
		}

		public void Transform(float[,] data)
		{
			throw new NotSupportedException();
		}

		public void InverseTransform(float[,] data)
		{
			throw new NotSupportedException();
		}

		public void Transform(float[,,] data)
		{
			throw new NotSupportedException();
		}

		public void InverseTransform(float[,,] data)
		{
			throw new NotSupportedException();
		}
	}
}
