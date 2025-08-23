using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DNA.Security.Cryptography.Math
{
	[Serializable]
	public class BigInteger
	{
		static BigInteger()
		{
			for (int i = 0; i < BigInteger.primeLists.Length; i++)
			{
				int[] primeList = BigInteger.primeLists[i];
				int product = 1;
				for (int j = 0; j < primeList.Length; j++)
				{
					product *= primeList[j];
				}
				BigInteger.primeProducts[i] = product;
			}
		}

		private static int GetByteLength(int nBits)
		{
			return (nBits + 8 - 1) / 8;
		}

		private BigInteger()
		{
		}

		private BigInteger(int signum, int[] mag, bool checkMag)
		{
			if (!checkMag)
			{
				this.sign = signum;
				this.magnitude = mag;
				return;
			}
			int i = 0;
			while (i < mag.Length && mag[i] == 0)
			{
				i++;
			}
			if (i == mag.Length)
			{
				this.magnitude = BigInteger.ZeroMagnitude;
				return;
			}
			this.sign = signum;
			if (i == 0)
			{
				this.magnitude = mag;
				return;
			}
			this.magnitude = new int[mag.Length - i];
			Array.Copy(mag, i, this.magnitude, 0, this.magnitude.Length);
		}

		public BigInteger(string value)
			: this(value, 10)
		{
		}

		public BigInteger(string str, int radix)
		{
			if (str.Length == 0)
			{
				throw new FormatException("Zero length BigInteger");
			}
			NumberStyles style;
			int chunk;
			BigInteger r;
			BigInteger rE;
			if (radix != 2)
			{
				if (radix != 10)
				{
					if (radix != 16)
					{
						throw new FormatException("Only bases 2, 10, or 16 allowed");
					}
					style = NumberStyles.AllowHexSpecifier;
					chunk = BigInteger.chunk16;
					r = BigInteger.radix16;
					rE = BigInteger.radix16E;
				}
				else
				{
					style = NumberStyles.Integer;
					chunk = BigInteger.chunk10;
					r = BigInteger.radix10;
					rE = BigInteger.radix10E;
				}
			}
			else
			{
				style = NumberStyles.Integer;
				chunk = BigInteger.chunk2;
				r = BigInteger.radix2;
				rE = BigInteger.radix2E;
			}
			int index = 0;
			this.sign = 1;
			if (str[0] == '-')
			{
				if (str.Length == 1)
				{
					throw new FormatException("Zero length BigInteger");
				}
				this.sign = -1;
				index = 1;
			}
			while (index < str.Length && int.Parse(str[index].ToString(), style) == 0)
			{
				index++;
			}
			if (index >= str.Length)
			{
				this.sign = 0;
				this.magnitude = BigInteger.ZeroMagnitude;
				return;
			}
			BigInteger b = BigInteger.Zero;
			int next = index + chunk;
			if (next <= str.Length)
			{
				string s;
				for (;;)
				{
					s = str.Substring(index, chunk);
					ulong i = ulong.Parse(s, style);
					BigInteger bi = BigInteger.createUValueOf(i);
					if (radix != 2)
					{
						if (radix != 16)
						{
							b = b.Multiply(rE);
						}
						else
						{
							b = b.ShiftLeft(64);
						}
					}
					else
					{
						if (i > 1UL)
						{
							break;
						}
						b = b.ShiftLeft(1);
					}
					b = b.Add(bi);
					index = next;
					next += chunk;
					if (next > str.Length)
					{
						goto IL_01B6;
					}
				}
				throw new FormatException("Bad character in radix 2 string: " + s);
			}
			IL_01B6:
			if (index < str.Length)
			{
				string s2 = str.Substring(index);
				ulong j = ulong.Parse(s2, style);
				BigInteger bi2 = BigInteger.createUValueOf(j);
				if (b.sign > 0)
				{
					if (radix != 2)
					{
						if (radix == 16)
						{
							b = b.ShiftLeft(s2.Length << 2);
						}
						else
						{
							b = b.Multiply(r.Pow(s2.Length));
						}
					}
					b = b.Add(bi2);
				}
				else
				{
					b = bi2;
				}
			}
			this.magnitude = b.magnitude;
		}

		public BigInteger(byte[] bytes)
			: this(bytes, 0, bytes.Length)
		{
		}

		public BigInteger(byte[] bytes, int offset, int length)
		{
			if (length == 0)
			{
				throw new FormatException("Zero length BigInteger");
			}
			if ((sbyte)bytes[offset] >= 0)
			{
				this.magnitude = BigInteger.MakeMagnitude(bytes, offset, length);
				this.sign = ((this.magnitude.Length > 0) ? 1 : 0);
				return;
			}
			this.sign = -1;
			int end = offset + length;
			int iBval = offset;
			while (iBval < end && (sbyte)bytes[iBval] == -1)
			{
				iBval++;
			}
			if (iBval >= end)
			{
				this.magnitude = BigInteger.One.magnitude;
				return;
			}
			int numBytes = end - iBval;
			byte[] inverse = new byte[numBytes];
			int index = 0;
			while (index < numBytes)
			{
				inverse[index++] = ~bytes[iBval++];
			}
			while (inverse[--index] == 255)
			{
				inverse[index] = 0;
			}
			byte[] array = inverse;
			int num = index;
			array[num] += 1;
			this.magnitude = BigInteger.MakeMagnitude(inverse, 0, inverse.Length);
		}

		private static int[] MakeMagnitude(byte[] bytes, int offset, int length)
		{
			int end = offset + length;
			int firstSignificant = offset;
			while (firstSignificant < end && bytes[firstSignificant] == 0)
			{
				firstSignificant++;
			}
			if (firstSignificant >= end)
			{
				return BigInteger.ZeroMagnitude;
			}
			int nInts = (end - firstSignificant + 3) / 4;
			int bCount = (end - firstSignificant) % 4;
			if (bCount == 0)
			{
				bCount = 4;
			}
			if (nInts < 1)
			{
				return BigInteger.ZeroMagnitude;
			}
			int[] mag = new int[nInts];
			int v = 0;
			int magnitudeIndex = 0;
			for (int i = firstSignificant; i < end; i++)
			{
				v <<= 8;
				v |= (int)(bytes[i] & byte.MaxValue);
				bCount--;
				if (bCount <= 0)
				{
					mag[magnitudeIndex] = v;
					magnitudeIndex++;
					bCount = 4;
					v = 0;
				}
			}
			if (magnitudeIndex < mag.Length)
			{
				mag[magnitudeIndex] = v;
			}
			return mag;
		}

		public BigInteger(int sign, byte[] bytes)
			: this(sign, bytes, 0, bytes.Length)
		{
		}

		public BigInteger(int sign, byte[] bytes, int offset, int length)
		{
			if (sign < -1 || sign > 1)
			{
				throw new FormatException("Invalid sign value");
			}
			if (sign == 0)
			{
				this.magnitude = BigInteger.ZeroMagnitude;
				return;
			}
			this.magnitude = BigInteger.MakeMagnitude(bytes, offset, length);
			this.sign = ((this.magnitude.Length < 1) ? 0 : sign);
		}

		public BigInteger(int sizeInBits, Random random)
		{
			if (sizeInBits < 0)
			{
				throw new ArgumentException("sizeInBits must be non-negative");
			}
			this.nBits = -1;
			this.nBitLength = -1;
			if (sizeInBits == 0)
			{
				this.magnitude = BigInteger.ZeroMagnitude;
				return;
			}
			int nBytes = BigInteger.GetByteLength(sizeInBits);
			byte[] b = new byte[nBytes];
			random.NextBytes(b);
			byte[] array = b;
			int num = 0;
			array[num] &= BigInteger.rndMask[8 * nBytes - sizeInBits];
			this.magnitude = BigInteger.MakeMagnitude(b, 0, b.Length);
			this.sign = ((this.magnitude.Length < 1) ? 0 : 1);
		}

		public BigInteger(int bitLength, int certainty, Random random)
		{
			if (bitLength < 2)
			{
				throw new ArithmeticException("bitLength < 2");
			}
			this.sign = 1;
			this.nBitLength = bitLength;
			if (bitLength == 2)
			{
				this.magnitude = ((random.Next(2) == 0) ? BigInteger.Two.magnitude : BigInteger.Three.magnitude);
				return;
			}
			int nBytes = BigInteger.GetByteLength(bitLength);
			byte[] b = new byte[nBytes];
			int xBits = 8 * nBytes - bitLength;
			byte mask = BigInteger.rndMask[xBits];
			for (;;)
			{
				random.NextBytes(b);
				byte[] array = b;
				int num = 0;
				array[num] &= mask;
				byte[] array2 = b;
				int num2 = 0;
				array2[num2] |= (byte)(1 << 7 - xBits);
				byte[] array3 = b;
				int num3 = nBytes - 1;
				array3[num3] |= 1;
				this.magnitude = BigInteger.MakeMagnitude(b, 0, b.Length);
				this.nBits = -1;
				this.mQuote = -1L;
				if (certainty < 1)
				{
					break;
				}
				if (this.CheckProbablePrime(certainty, random))
				{
					return;
				}
				if (bitLength > 32)
				{
					for (int rep = 0; rep < 10000; rep++)
					{
						int i = 33 + random.Next(bitLength - 2);
						this.magnitude[this.magnitude.Length - (i >> 5)] ^= 1 << i;
						this.magnitude[this.magnitude.Length - 1] ^= random.Next() + 1 << 1;
						this.mQuote = -1L;
						if (this.CheckProbablePrime(certainty, random))
						{
							return;
						}
					}
				}
			}
		}

		public BigInteger Abs()
		{
			if (this.sign < 0)
			{
				return this.Negate();
			}
			return this;
		}

		private static int[] AddMagnitudes(int[] a, int[] b)
		{
			int tI = a.Length - 1;
			int vI = b.Length - 1;
			long i = 0L;
			while (vI >= 0)
			{
				i += (long)((ulong)a[tI] + (ulong)b[vI--]);
				a[tI--] = (int)i;
				i = (long)((ulong)i >> 32);
			}
			if (i != 0L)
			{
				while (tI >= 0 && ++a[tI--] == 0)
				{
				}
			}
			return a;
		}

		public BigInteger Add(BigInteger value)
		{
			if (this.sign == 0)
			{
				return value;
			}
			if (this.sign == value.sign)
			{
				return this.AddToMagnitude(value.magnitude);
			}
			if (value.sign == 0)
			{
				return this;
			}
			if (value.sign < 0)
			{
				return this.Subtract(value.Negate());
			}
			return value.Subtract(this.Negate());
		}

		private BigInteger AddToMagnitude(int[] magToAdd)
		{
			int[] big;
			int[] small;
			if (this.magnitude.Length < magToAdd.Length)
			{
				big = magToAdd;
				small = this.magnitude;
			}
			else
			{
				big = this.magnitude;
				small = magToAdd;
			}
			uint limit = uint.MaxValue;
			if (big.Length == small.Length)
			{
				limit -= (uint)small[0];
			}
			bool possibleOverflow = big[0] >= (int)limit;
			int[] bigCopy;
			if (possibleOverflow)
			{
				bigCopy = new int[big.Length + 1];
				big.CopyTo(bigCopy, 1);
			}
			else
			{
				bigCopy = (int[])big.Clone();
			}
			bigCopy = BigInteger.AddMagnitudes(bigCopy, small);
			return new BigInteger(this.sign, bigCopy, possibleOverflow);
		}

		public BigInteger And(BigInteger value)
		{
			if (this.sign == 0 || value.sign == 0)
			{
				return BigInteger.Zero;
			}
			int[] aMag = ((this.sign > 0) ? this.magnitude : this.Add(BigInteger.One).magnitude);
			int[] bMag = ((value.sign > 0) ? value.magnitude : value.Add(BigInteger.One).magnitude);
			bool resultNeg = this.sign < 0 && value.sign < 0;
			int resultLength = Math.Max(aMag.Length, bMag.Length);
			int[] resultMag = new int[resultLength];
			int aStart = resultMag.Length - aMag.Length;
			int bStart = resultMag.Length - bMag.Length;
			for (int i = 0; i < resultMag.Length; i++)
			{
				int aWord = ((i >= aStart) ? aMag[i - aStart] : 0);
				int bWord = ((i >= bStart) ? bMag[i - bStart] : 0);
				if (this.sign < 0)
				{
					aWord = ~aWord;
				}
				if (value.sign < 0)
				{
					bWord = ~bWord;
				}
				resultMag[i] = aWord & bWord;
				if (resultNeg)
				{
					resultMag[i] = ~resultMag[i];
				}
			}
			BigInteger result = new BigInteger(1, resultMag, true);
			if (resultNeg)
			{
				result = result.Not();
			}
			return result;
		}

		public BigInteger AndNot(BigInteger val)
		{
			return this.And(val.Not());
		}

		public int BitCount
		{
			get
			{
				if (this.nBits == -1)
				{
					if (this.sign < 0)
					{
						this.nBits = this.Not().BitCount;
					}
					else
					{
						int sum = 0;
						for (int i = 0; i < this.magnitude.Length; i++)
						{
							sum += (int)BigInteger.bitCounts[(int)((byte)this.magnitude[i])];
							sum += (int)BigInteger.bitCounts[(int)((byte)(this.magnitude[i] >> 8))];
							sum += (int)BigInteger.bitCounts[(int)((byte)(this.magnitude[i] >> 16))];
							sum += (int)BigInteger.bitCounts[(int)((byte)(this.magnitude[i] >> 24))];
						}
						this.nBits = sum;
					}
				}
				return this.nBits;
			}
		}

		private int calcBitLength(int indx, int[] mag)
		{
			while (indx < mag.Length)
			{
				if (mag[indx] != 0)
				{
					int bitLength = 32 * (mag.Length - indx - 1);
					int firstMag = mag[indx];
					bitLength += BigInteger.BitLen(firstMag);
					if (this.sign < 0 && (firstMag & -firstMag) == firstMag)
					{
						while (++indx < mag.Length)
						{
							if (mag[indx] != 0)
							{
								return bitLength;
							}
						}
						bitLength--;
					}
					return bitLength;
				}
				indx++;
			}
			return 0;
		}

		public int BitLength
		{
			get
			{
				if (this.nBitLength == -1)
				{
					this.nBitLength = ((this.sign == 0) ? 0 : this.calcBitLength(0, this.magnitude));
				}
				return this.nBitLength;
			}
		}

		private static int BitLen(int w)
		{
			if (w >= 32768)
			{
				if (w >= 8388608)
				{
					if (w >= 134217728)
					{
						if (w >= 536870912)
						{
							if (w >= 1073741824)
							{
								return 31;
							}
							return 30;
						}
						else
						{
							if (w >= 268435456)
							{
								return 29;
							}
							return 28;
						}
					}
					else if (w >= 33554432)
					{
						if (w >= 67108864)
						{
							return 27;
						}
						return 26;
					}
					else
					{
						if (w >= 16777216)
						{
							return 25;
						}
						return 24;
					}
				}
				else if (w >= 524288)
				{
					if (w >= 2097152)
					{
						if (w >= 4194304)
						{
							return 23;
						}
						return 22;
					}
					else
					{
						if (w >= 1048576)
						{
							return 21;
						}
						return 20;
					}
				}
				else if (w >= 131072)
				{
					if (w >= 262144)
					{
						return 19;
					}
					return 18;
				}
				else
				{
					if (w >= 65536)
					{
						return 17;
					}
					return 16;
				}
			}
			else if (w >= 128)
			{
				if (w >= 2048)
				{
					if (w >= 8192)
					{
						if (w >= 16384)
						{
							return 15;
						}
						return 14;
					}
					else
					{
						if (w >= 4096)
						{
							return 13;
						}
						return 12;
					}
				}
				else if (w >= 512)
				{
					if (w >= 1024)
					{
						return 11;
					}
					return 10;
				}
				else
				{
					if (w >= 256)
					{
						return 9;
					}
					return 8;
				}
			}
			else if (w >= 8)
			{
				if (w >= 32)
				{
					if (w >= 64)
					{
						return 7;
					}
					return 6;
				}
				else
				{
					if (w >= 16)
					{
						return 5;
					}
					return 4;
				}
			}
			else if (w >= 2)
			{
				if (w >= 4)
				{
					return 3;
				}
				return 2;
			}
			else
			{
				if (w >= 1)
				{
					return 1;
				}
				if (w >= 0)
				{
					return 0;
				}
				return 32;
			}
		}

		private bool QuickPow2Check()
		{
			return this.sign > 0 && this.nBits == 1;
		}

		public int CompareTo(object obj)
		{
			return this.CompareTo((BigInteger)obj);
		}

		private static int CompareTo(int xIndx, int[] x, int yIndx, int[] y)
		{
			while (xIndx != x.Length)
			{
				if (x[xIndx] != 0)
				{
					break;
				}
				xIndx++;
			}
			while (yIndx != y.Length && y[yIndx] == 0)
			{
				yIndx++;
			}
			return BigInteger.CompareNoLeadingZeroes(xIndx, x, yIndx, y);
		}

		private static int CompareNoLeadingZeroes(int xIndx, int[] x, int yIndx, int[] y)
		{
			int diff = x.Length - y.Length - (xIndx - yIndx);
			if (diff == 0)
			{
				while (xIndx < x.Length)
				{
					uint v = (uint)x[xIndx++];
					uint v2 = (uint)y[yIndx++];
					if (v != v2)
					{
						if (v >= v2)
						{
							return 1;
						}
						return -1;
					}
				}
				return 0;
			}
			if (diff >= 0)
			{
				return 1;
			}
			return -1;
		}

		public int CompareTo(BigInteger value)
		{
			if (this.sign < value.sign)
			{
				return -1;
			}
			if (this.sign > value.sign)
			{
				return 1;
			}
			if (this.sign != 0)
			{
				return this.sign * BigInteger.CompareNoLeadingZeroes(0, this.magnitude, 0, value.magnitude);
			}
			return 0;
		}

		private int[] Divide(int[] x, int[] y)
		{
			int xStart = 0;
			while (xStart < x.Length && x[xStart] == 0)
			{
				xStart++;
			}
			int yStart = 0;
			while (yStart < y.Length && y[yStart] == 0)
			{
				yStart++;
			}
			int xyCmp = BigInteger.CompareNoLeadingZeroes(xStart, x, yStart, y);
			int[] count;
			if (xyCmp > 0)
			{
				int yBitLength = this.calcBitLength(yStart, y);
				int xBitLength = this.calcBitLength(xStart, x);
				int shift = xBitLength - yBitLength;
				int iCountStart = 0;
				int cStart = 0;
				int cBitLength = yBitLength;
				int[] iCount;
				int[] c;
				if (shift > 0)
				{
					iCount = new int[(shift >> 5) + 1];
					iCount[0] = 1 << shift % 32;
					c = BigInteger.ShiftLeft(y, shift);
					cBitLength += shift;
				}
				else
				{
					iCount = new int[] { 1 };
					int len = y.Length - yStart;
					c = new int[len];
					Array.Copy(y, yStart, c, 0, len);
				}
				count = new int[iCount.Length];
				for (;;)
				{
					if (cBitLength < xBitLength || BigInteger.CompareNoLeadingZeroes(xStart, x, cStart, c) >= 0)
					{
						BigInteger.Subtract(xStart, x, cStart, c);
						BigInteger.AddMagnitudes(count, iCount);
						while (x[xStart] == 0)
						{
							if (++xStart == x.Length)
							{
								return count;
							}
						}
						xBitLength = 32 * (x.Length - xStart - 1) + BigInteger.BitLen(x[xStart]);
						if (xBitLength <= yBitLength)
						{
							if (xBitLength < yBitLength)
							{
								return count;
							}
							xyCmp = BigInteger.CompareNoLeadingZeroes(xStart, x, yStart, y);
							if (xyCmp <= 0)
							{
								goto IL_01CA;
							}
						}
					}
					shift = cBitLength - xBitLength;
					if (shift == 1)
					{
						uint firstC = (uint)c[cStart] >> 1;
						uint firstX = (uint)x[xStart];
						if (firstC > firstX)
						{
							shift++;
						}
					}
					if (shift < 2)
					{
						c = BigInteger.ShiftRightOneInPlace(cStart, c);
						cBitLength--;
						iCount = BigInteger.ShiftRightOneInPlace(iCountStart, iCount);
					}
					else
					{
						c = BigInteger.ShiftRightInPlace(cStart, c, shift);
						cBitLength -= shift;
						iCount = BigInteger.ShiftRightInPlace(iCountStart, iCount, shift);
					}
					while (c[cStart] == 0)
					{
						cStart++;
					}
					while (iCount[iCountStart] == 0)
					{
						iCountStart++;
					}
				}
				return count;
			}
			count = new int[1];
			IL_01CA:
			if (xyCmp == 0)
			{
				BigInteger.AddMagnitudes(count, BigInteger.One.magnitude);
				Array.Clear(x, xStart, x.Length - xStart);
			}
			return count;
		}

		public BigInteger Divide(BigInteger val)
		{
			if (val.sign == 0)
			{
				throw new ArithmeticException("Division by zero error");
			}
			if (this.sign == 0)
			{
				return BigInteger.Zero;
			}
			if (!val.QuickPow2Check())
			{
				int[] mag = (int[])this.magnitude.Clone();
				return new BigInteger(this.sign * val.sign, this.Divide(mag, val.magnitude), true);
			}
			BigInteger result = this.Abs().ShiftRight(val.Abs().BitLength - 1);
			if (val.sign != this.sign)
			{
				return result.Negate();
			}
			return result;
		}

		public BigInteger[] DivideAndRemainder(BigInteger val)
		{
			if (val.sign == 0)
			{
				throw new ArithmeticException("Division by zero error");
			}
			BigInteger[] biggies = new BigInteger[2];
			if (this.sign == 0)
			{
				biggies[0] = BigInteger.Zero;
				biggies[1] = BigInteger.Zero;
			}
			else if (val.QuickPow2Check())
			{
				int e = val.Abs().BitLength - 1;
				BigInteger quotient = this.Abs().ShiftRight(e);
				int[] remainder = this.LastNBits(e);
				biggies[0] = ((val.sign == this.sign) ? quotient : quotient.Negate());
				biggies[1] = new BigInteger(this.sign, remainder, true);
			}
			else
			{
				int[] remainder2 = (int[])this.magnitude.Clone();
				int[] quotient2 = this.Divide(remainder2, val.magnitude);
				biggies[0] = new BigInteger(this.sign * val.sign, quotient2, true);
				biggies[1] = new BigInteger(this.sign, remainder2, true);
			}
			return biggies;
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}
			BigInteger biggie = obj as BigInteger;
			if (biggie == null)
			{
				return false;
			}
			if (biggie.sign != this.sign || biggie.magnitude.Length != this.magnitude.Length)
			{
				return false;
			}
			for (int i = 0; i < this.magnitude.Length; i++)
			{
				if (biggie.magnitude[i] != this.magnitude[i])
				{
					return false;
				}
			}
			return true;
		}

		public BigInteger Gcd(BigInteger value)
		{
			if (value.sign == 0)
			{
				return this.Abs();
			}
			if (this.sign == 0)
			{
				return value.Abs();
			}
			BigInteger u = this;
			BigInteger v = value;
			while (v.sign != 0)
			{
				BigInteger r = u.Mod(v);
				u = v;
				v = r;
			}
			return u;
		}

		public override int GetHashCode()
		{
			int hc = this.magnitude.Length;
			if (this.magnitude.Length > 0)
			{
				hc ^= this.magnitude[0];
				if (this.magnitude.Length > 1)
				{
					hc ^= this.magnitude[this.magnitude.Length - 1];
				}
			}
			if (this.sign >= 0)
			{
				return hc;
			}
			return ~hc;
		}

		private BigInteger Inc()
		{
			if (this.sign == 0)
			{
				return BigInteger.One;
			}
			if (this.sign < 0)
			{
				return new BigInteger(-1, BigInteger.doSubBigLil(this.magnitude, BigInteger.One.magnitude), true);
			}
			return this.AddToMagnitude(BigInteger.One.magnitude);
		}

		public int IntValue
		{
			get
			{
				if (this.sign == 0)
				{
					return 0;
				}
				if (this.sign <= 0)
				{
					return -this.magnitude[this.magnitude.Length - 1];
				}
				return this.magnitude[this.magnitude.Length - 1];
			}
		}

		public bool IsProbablePrime(int certainty)
		{
			if (certainty <= 0)
			{
				return true;
			}
			BigInteger i = this.Abs();
			if (!i.TestBit(0))
			{
				return i.Equals(BigInteger.Two);
			}
			return !i.Equals(BigInteger.One) && i.CheckProbablePrime(certainty, BigInteger.RandomSource);
		}

		private bool CheckProbablePrime(int certainty, Random random)
		{
			int numLists = Math.Min(this.BitLength - 1, BigInteger.primeLists.Length);
			for (int i = 0; i < numLists; i++)
			{
				int test = this.Remainder(BigInteger.primeProducts[i]);
				foreach (int prime in BigInteger.primeLists[i])
				{
					if (test % prime == 0)
					{
						return this.BitLength < 16 && this.IntValue == prime;
					}
				}
			}
			return this.RabinMillerTest(certainty, random);
		}

		internal bool RabinMillerTest(int certainty, Random random)
		{
			BigInteger nMinusOne = this.Subtract(BigInteger.One);
			int s = nMinusOne.GetLowestSetBit();
			BigInteger r = nMinusOne.ShiftRight(s);
			for (;;)
			{
				BigInteger a = new BigInteger(this.BitLength, random);
				if (a.CompareTo(BigInteger.One) > 0 && a.CompareTo(nMinusOne) < 0)
				{
					BigInteger y = a.ModPow(r, this);
					if (!y.Equals(BigInteger.One))
					{
						int i = 0;
						while (!y.Equals(nMinusOne))
						{
							if (++i == s)
							{
								return false;
							}
							y = y.ModPow(BigInteger.Two, this);
							if (y.Equals(BigInteger.One))
							{
								return false;
							}
						}
					}
					certainty -= 2;
					if (certainty <= 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		public long LongValue
		{
			get
			{
				if (this.sign == 0)
				{
					return 0L;
				}
				long v;
				if (this.magnitude.Length > 1)
				{
					v = ((long)this.magnitude[this.magnitude.Length - 2] << 32) | ((long)this.magnitude[this.magnitude.Length - 1] & (long)((ulong)(-1)));
				}
				else
				{
					v = (long)this.magnitude[this.magnitude.Length - 1] & (long)((ulong)(-1));
				}
				if (this.sign >= 0)
				{
					return v;
				}
				return -v;
			}
		}

		public BigInteger Max(BigInteger value)
		{
			if (this.CompareTo(value) <= 0)
			{
				return value;
			}
			return this;
		}

		public BigInteger Min(BigInteger value)
		{
			if (this.CompareTo(value) >= 0)
			{
				return value;
			}
			return this;
		}

		public BigInteger Mod(BigInteger m)
		{
			if (m.sign < 1)
			{
				throw new ArithmeticException("Modulus must be positive");
			}
			BigInteger biggie = this.Remainder(m);
			if (biggie.sign < 0)
			{
				return biggie.Add(m);
			}
			return biggie;
		}

		public BigInteger ModInverse(BigInteger m)
		{
			if (m.sign < 1)
			{
				throw new ArithmeticException("Modulus must be positive");
			}
			BigInteger x = new BigInteger();
			BigInteger gcd = BigInteger.ExtEuclid(this.Mod(m), m, x, null);
			if (!gcd.Equals(BigInteger.One))
			{
				throw new ArithmeticException("Numbers not relatively prime.");
			}
			if (x.sign < 0)
			{
				x.sign = 1;
				x.magnitude = BigInteger.doSubBigLil(m.magnitude, x.magnitude);
			}
			return x;
		}

		private static BigInteger ExtEuclid(BigInteger a, BigInteger b, BigInteger u1Out, BigInteger u2Out)
		{
			BigInteger u = BigInteger.One;
			BigInteger u2 = a;
			BigInteger v = BigInteger.Zero;
			BigInteger v2 = b;
			while (v2.sign > 0)
			{
				BigInteger[] q = u2.DivideAndRemainder(v2);
				BigInteger tmp = v.Multiply(q[0]);
				BigInteger tn = u.Subtract(tmp);
				u = v;
				v = tn;
				u2 = v2;
				v2 = q[1];
			}
			if (u1Out != null)
			{
				u1Out.sign = u.sign;
				u1Out.magnitude = u.magnitude;
			}
			if (u2Out != null)
			{
				BigInteger tmp2 = u.Multiply(a);
				tmp2 = u2.Subtract(tmp2);
				BigInteger res = tmp2.Divide(b);
				u2Out.sign = res.sign;
				u2Out.magnitude = res.magnitude;
			}
			return u2;
		}

		private static void ZeroOut(int[] x)
		{
			Array.Clear(x, 0, x.Length);
		}

		public BigInteger ModPow(BigInteger exponent, BigInteger m)
		{
			if (m.sign < 1)
			{
				throw new ArithmeticException("Modulus must be positive");
			}
			if (m.Equals(BigInteger.One))
			{
				return BigInteger.Zero;
			}
			if (exponent.sign == 0)
			{
				return BigInteger.One;
			}
			if (this.sign == 0)
			{
				return BigInteger.Zero;
			}
			int[] zVal = null;
			int[] yAccum = null;
			bool useMonty = (m.magnitude[m.magnitude.Length - 1] & 1) == 1;
			long mQ = 0L;
			if (useMonty)
			{
				mQ = m.GetMQuote();
				BigInteger tmp = this.ShiftLeft(32 * m.magnitude.Length).Mod(m);
				zVal = tmp.magnitude;
				useMonty = zVal.Length <= m.magnitude.Length;
				if (useMonty)
				{
					yAccum = new int[m.magnitude.Length + 1];
					if (zVal.Length < m.magnitude.Length)
					{
						int[] longZ = new int[m.magnitude.Length];
						zVal.CopyTo(longZ, longZ.Length - zVal.Length);
						zVal = longZ;
					}
				}
			}
			if (!useMonty)
			{
				if (this.magnitude.Length <= m.magnitude.Length)
				{
					zVal = new int[m.magnitude.Length];
					this.magnitude.CopyTo(zVal, zVal.Length - this.magnitude.Length);
				}
				else
				{
					BigInteger tmp2 = this.Remainder(m);
					zVal = new int[m.magnitude.Length];
					tmp2.magnitude.CopyTo(zVal, zVal.Length - tmp2.magnitude.Length);
				}
				yAccum = new int[m.magnitude.Length * 2];
			}
			int[] yVal = new int[m.magnitude.Length];
			for (int i = 0; i < exponent.magnitude.Length; i++)
			{
				int v = exponent.magnitude[i];
				int bits = 0;
				if (i == 0)
				{
					while (v > 0)
					{
						v <<= 1;
						bits++;
					}
					zVal.CopyTo(yVal, 0);
					v <<= 1;
					bits++;
				}
				while (v != 0)
				{
					if (useMonty)
					{
						BigInteger.MultiplyMonty(yAccum, yVal, yVal, m.magnitude, mQ);
					}
					else
					{
						BigInteger.Square(yAccum, yVal);
						this.Remainder(yAccum, m.magnitude);
						Array.Copy(yAccum, yAccum.Length - yVal.Length, yVal, 0, yVal.Length);
						BigInteger.ZeroOut(yAccum);
					}
					bits++;
					if (v < 0)
					{
						if (useMonty)
						{
							BigInteger.MultiplyMonty(yAccum, yVal, zVal, m.magnitude, mQ);
						}
						else
						{
							BigInteger.Multiply(yAccum, yVal, zVal);
							this.Remainder(yAccum, m.magnitude);
							Array.Copy(yAccum, yAccum.Length - yVal.Length, yVal, 0, yVal.Length);
							BigInteger.ZeroOut(yAccum);
						}
					}
					v <<= 1;
				}
				while (bits < 32)
				{
					if (useMonty)
					{
						BigInteger.MultiplyMonty(yAccum, yVal, yVal, m.magnitude, mQ);
					}
					else
					{
						BigInteger.Square(yAccum, yVal);
						this.Remainder(yAccum, m.magnitude);
						Array.Copy(yAccum, yAccum.Length - yVal.Length, yVal, 0, yVal.Length);
						BigInteger.ZeroOut(yAccum);
					}
					bits++;
				}
			}
			if (useMonty)
			{
				BigInteger.ZeroOut(zVal);
				zVal[zVal.Length - 1] = 1;
				BigInteger.MultiplyMonty(yAccum, yVal, zVal, m.magnitude, mQ);
			}
			BigInteger result = new BigInteger(1, yVal, true);
			if (exponent.sign <= 0)
			{
				return result.ModInverse(m);
			}
			return result;
		}

		private static int[] Square(int[] w, int[] x)
		{
			int wBase = w.Length - 1;
			ulong u;
			ulong u2;
			for (int i = x.Length - 1; i != 0; i--)
			{
				ulong v = (ulong)x[i];
				u = v * v;
				u2 = u >> 32;
				u = (ulong)((uint)u);
				u += (ulong)w[wBase];
				w[wBase] = (int)((uint)u);
				ulong c = u2 + (u >> 32);
				for (int j = i - 1; j >= 0; j--)
				{
					wBase--;
					u = v * (ulong)x[j];
					u2 = u >> 31;
					u = (ulong)((uint)((uint)u << 1));
					u += c + (ulong)w[wBase];
					w[wBase] = (int)((uint)u);
					c = u2 + (u >> 32);
				}
				c += (ulong)w[--wBase];
				w[wBase] = (int)((uint)c);
				if (--wBase >= 0)
				{
					w[wBase] = (int)((uint)(c >> 32));
				}
				wBase += i;
			}
			u = (ulong)x[0];
			u *= u;
			u2 = u >> 32;
			u &= (ulong)(-1);
			u += (ulong)w[wBase];
			w[wBase] = (int)((uint)u);
			if (--wBase >= 0)
			{
				w[wBase] = (int)((uint)(u2 + (u >> 32) + (ulong)w[wBase]));
			}
			return w;
		}

		private static int[] Multiply(int[] x, int[] y, int[] z)
		{
			int i = z.Length;
			if (i < 1)
			{
				return x;
			}
			int xBase = x.Length - y.Length;
			long val;
			for (;;)
			{
				long a = (long)z[--i] & (long)((ulong)(-1));
				val = 0L;
				for (int j = y.Length - 1; j >= 0; j--)
				{
					val += a * ((long)y[j] & (long)((ulong)(-1))) + ((long)x[xBase + j] & (long)((ulong)(-1)));
					x[xBase + j] = (int)val;
					val = (long)((ulong)val >> 32);
				}
				xBase--;
				if (i < 1)
				{
					break;
				}
				x[xBase] = (int)val;
			}
			if (xBase >= 0)
			{
				x[xBase] = (int)val;
			}
			return x;
		}

		private static long FastExtEuclid(long a, long b, long[] uOut)
		{
			long u = 1L;
			long u2 = a;
			long v = 0L;
			long tn;
			for (long v2 = b; v2 > 0L; v2 = tn)
			{
				long q = u2 / v2;
				tn = u - v * q;
				u = v;
				v = tn;
				tn = u2 - v2 * q;
				u2 = v2;
			}
			uOut[0] = u;
			uOut[1] = (u2 - u * a) / b;
			return u2;
		}

		private static long FastModInverse(long v, long m)
		{
			if (m < 1L)
			{
				throw new ArithmeticException("Modulus must be positive");
			}
			long[] x = new long[2];
			long gcd = BigInteger.FastExtEuclid(v, m, x);
			if (gcd != 1L)
			{
				throw new ArithmeticException("Numbers not relatively prime.");
			}
			if (x[0] < 0L)
			{
				x[0] += m;
			}
			return x[0];
		}

		private long GetMQuote()
		{
			if (this.mQuote != -1L)
			{
				return this.mQuote;
			}
			if (this.magnitude.Length == 0 || (this.magnitude[this.magnitude.Length - 1] & 1) == 0)
			{
				return -1L;
			}
			long v = (long)(~this.magnitude[this.magnitude.Length - 1] | 1) & (long)((ulong)(-1));
			this.mQuote = BigInteger.FastModInverse(v, 4294967296L);
			return this.mQuote;
		}

		private static void MultiplyMonty(int[] a, int[] x, int[] y, int[] m, long mQuote)
		{
			if (m.Length == 1)
			{
				x[0] = (int)BigInteger.MultiplyMontyNIsOne((uint)x[0], (uint)y[0], (uint)m[0], (ulong)mQuote);
				return;
			}
			int i = m.Length;
			int nMinus = i - 1;
			long y_0 = (long)y[nMinus] & (long)((ulong)(-1));
			Array.Clear(a, 0, i + 1);
			for (int j = i; j > 0; j--)
			{
				long x_i = (long)x[j - 1] & (long)((ulong)(-1));
				long u = (((((long)a[i] & (long)((ulong)(-1))) + ((x_i * y_0) & (long)((ulong)(-1)))) & (long)((ulong)(-1))) * mQuote) & (long)((ulong)(-1));
				long prod = x_i * y_0;
				long prod2 = u * ((long)m[nMinus] & (long)((ulong)(-1)));
				long tmp = ((long)a[i] & (long)((ulong)(-1))) + (prod & (long)((ulong)(-1))) + (prod2 & (long)((ulong)(-1)));
				long carry = (long)(((ulong)prod >> 32) + ((ulong)prod2 >> 32) + ((ulong)tmp >> 32));
				for (int k = nMinus; k > 0; k--)
				{
					prod = x_i * ((long)y[k - 1] & (long)((ulong)(-1)));
					prod2 = u * ((long)m[k - 1] & (long)((ulong)(-1)));
					tmp = ((long)a[k] & (long)((ulong)(-1))) + (prod & (long)((ulong)(-1))) + (prod2 & (long)((ulong)(-1))) + (carry & (long)((ulong)(-1)));
					carry = (long)(((ulong)carry >> 32) + ((ulong)prod >> 32) + ((ulong)prod2 >> 32) + ((ulong)tmp >> 32));
					a[k + 1] = (int)tmp;
				}
				carry += (long)a[0] & (long)((ulong)(-1));
				a[1] = (int)carry;
				a[0] = (int)((ulong)carry >> 32);
			}
			if (BigInteger.CompareTo(0, a, 0, m) >= 0)
			{
				BigInteger.Subtract(0, a, 0, m);
			}
			Array.Copy(a, 1, x, 0, i);
		}

		private static uint MultiplyMontyNIsOne(uint x, uint y, uint m, ulong mQuote)
		{
			ulong um = (ulong)m;
			ulong prod = (ulong)x * (ulong)y;
			ulong u = (prod * mQuote) & BigInteger.UIMASK;
			ulong prod2 = u * um;
			ulong tmp = (prod & BigInteger.UIMASK) + (prod2 & BigInteger.UIMASK);
			ulong carry = (prod >> 32) + (prod2 >> 32) + (tmp >> 32);
			if (carry > um)
			{
				carry -= um;
			}
			return (uint)(carry & BigInteger.UIMASK);
		}

		public BigInteger Multiply(BigInteger val)
		{
			if (this.sign == 0 || val.sign == 0)
			{
				return BigInteger.Zero;
			}
			if (val.QuickPow2Check())
			{
				BigInteger result = this.ShiftLeft(val.Abs().BitLength - 1);
				if (val.sign <= 0)
				{
					return result.Negate();
				}
				return result;
			}
			else
			{
				if (!this.QuickPow2Check())
				{
					int resLength = (this.BitLength + val.BitLength) / 32 + 1;
					int[] res = new int[resLength];
					if (val == this)
					{
						BigInteger.Square(res, this.magnitude);
					}
					else
					{
						BigInteger.Multiply(res, this.magnitude, val.magnitude);
					}
					return new BigInteger(this.sign * val.sign, res, true);
				}
				BigInteger result2 = val.ShiftLeft(this.Abs().BitLength - 1);
				if (this.sign <= 0)
				{
					return result2.Negate();
				}
				return result2;
			}
		}

		public BigInteger Negate()
		{
			if (this.sign == 0)
			{
				return this;
			}
			return new BigInteger(-this.sign, this.magnitude, false);
		}

		public BigInteger NextProbablePrime()
		{
			if (this.sign < 0)
			{
				throw new ArithmeticException("Cannot be called on value < 0");
			}
			if (this.CompareTo(BigInteger.Two) < 0)
			{
				return BigInteger.Two;
			}
			BigInteger i = this.Inc().SetBit(0);
			while (!i.CheckProbablePrime(100, BigInteger.RandomSource))
			{
				i = i.Add(BigInteger.Two);
			}
			return i;
		}

		public BigInteger Not()
		{
			return this.Inc().Negate();
		}

		public BigInteger Pow(int exp)
		{
			if (exp < 0)
			{
				throw new ArithmeticException("Negative exponent");
			}
			if (exp == 0)
			{
				return BigInteger.One;
			}
			if (this.sign == 0 || this.Equals(BigInteger.One))
			{
				return this;
			}
			BigInteger y = BigInteger.One;
			BigInteger z = this;
			for (;;)
			{
				if ((exp & 1) == 1)
				{
					y = y.Multiply(z);
				}
				exp >>= 1;
				if (exp == 0)
				{
					break;
				}
				z = z.Multiply(z);
			}
			return y;
		}

		public static BigInteger ProbablePrime(int bitLength, Random random)
		{
			return new BigInteger(bitLength, 100, random);
		}

		private int Remainder(int m)
		{
			long acc = 0L;
			for (int pos = 0; pos < this.magnitude.Length; pos++)
			{
				long posVal = (long)((ulong)this.magnitude[pos]);
				acc = ((acc << 32) | posVal) % (long)m;
			}
			return (int)acc;
		}

		private int[] Remainder(int[] x, int[] y)
		{
			int xStart = 0;
			while (xStart < x.Length && x[xStart] == 0)
			{
				xStart++;
			}
			int yStart = 0;
			while (yStart < y.Length && y[yStart] == 0)
			{
				yStart++;
			}
			int xyCmp = BigInteger.CompareNoLeadingZeroes(xStart, x, yStart, y);
			if (xyCmp > 0)
			{
				int yBitLength = this.calcBitLength(yStart, y);
				int xBitLength = this.calcBitLength(xStart, x);
				int shift = xBitLength - yBitLength;
				int cStart = 0;
				int cBitLength = yBitLength;
				int[] c;
				if (shift > 0)
				{
					c = BigInteger.ShiftLeft(y, shift);
					cBitLength += shift;
				}
				else
				{
					int len = y.Length - yStart;
					c = new int[len];
					Array.Copy(y, yStart, c, 0, len);
				}
				for (;;)
				{
					if (cBitLength < xBitLength || BigInteger.CompareNoLeadingZeroes(xStart, x, cStart, c) >= 0)
					{
						BigInteger.Subtract(xStart, x, cStart, c);
						while (x[xStart] == 0)
						{
							if (++xStart == x.Length)
							{
								return x;
							}
						}
						xBitLength = 32 * (x.Length - xStart - 1) + BigInteger.BitLen(x[xStart]);
						if (xBitLength <= yBitLength)
						{
							if (xBitLength < yBitLength)
							{
								return x;
							}
							xyCmp = BigInteger.CompareNoLeadingZeroes(xStart, x, yStart, y);
							if (xyCmp <= 0)
							{
								goto IL_0152;
							}
						}
					}
					shift = cBitLength - xBitLength;
					if (shift == 1)
					{
						uint firstC = (uint)c[cStart] >> 1;
						uint firstX = (uint)x[xStart];
						if (firstC > firstX)
						{
							shift++;
						}
					}
					if (shift < 2)
					{
						c = BigInteger.ShiftRightOneInPlace(cStart, c);
						cBitLength--;
					}
					else
					{
						c = BigInteger.ShiftRightInPlace(cStart, c, shift);
						cBitLength -= shift;
					}
					while (c[cStart] == 0)
					{
						cStart++;
					}
				}
				return x;
			}
			IL_0152:
			if (xyCmp == 0)
			{
				Array.Clear(x, xStart, x.Length - xStart);
			}
			return x;
		}

		public BigInteger Remainder(BigInteger n)
		{
			if (n.sign == 0)
			{
				throw new ArithmeticException("Division by zero error");
			}
			if (this.sign == 0)
			{
				return BigInteger.Zero;
			}
			if (n.magnitude.Length == 1)
			{
				int val = n.magnitude[0];
				if (val > 0)
				{
					if (val == 1)
					{
						return BigInteger.Zero;
					}
					int rem = this.Remainder(val);
					if (rem != 0)
					{
						return new BigInteger(this.sign, new int[] { rem }, false);
					}
					return BigInteger.Zero;
				}
			}
			if (BigInteger.CompareNoLeadingZeroes(0, this.magnitude, 0, n.magnitude) < 0)
			{
				return this;
			}
			int[] result;
			if (n.QuickPow2Check())
			{
				result = this.LastNBits(n.Abs().BitLength - 1);
			}
			else
			{
				result = (int[])this.magnitude.Clone();
				result = this.Remainder(result, n.magnitude);
			}
			return new BigInteger(this.sign, result, true);
		}

		private int[] LastNBits(int n)
		{
			if (n < 1)
			{
				return BigInteger.ZeroMagnitude;
			}
			int numWords = (n + 32 - 1) / 32;
			numWords = Math.Min(numWords, this.magnitude.Length);
			int[] result = new int[numWords];
			Array.Copy(this.magnitude, this.magnitude.Length - numWords, result, 0, numWords);
			int hiBits = n % 32;
			if (hiBits != 0)
			{
				result[0] &= ~(-1 << hiBits);
			}
			return result;
		}

		private static int[] ShiftLeft(int[] mag, int n)
		{
			int nInts = (int)((uint)n >> 5);
			int nBits = n & 31;
			int magLen = mag.Length;
			int[] newMag;
			if (nBits == 0)
			{
				newMag = new int[magLen + nInts];
				mag.CopyTo(newMag, 0);
			}
			else
			{
				int i = 0;
				int nBits2 = 32 - nBits;
				int highBits = (int)((uint)mag[0] >> nBits2);
				if (highBits != 0)
				{
					newMag = new int[magLen + nInts + 1];
					newMag[i++] = highBits;
				}
				else
				{
					newMag = new int[magLen + nInts];
				}
				int j = mag[0];
				for (int k = 0; k < magLen - 1; k++)
				{
					int next = mag[k + 1];
					newMag[i++] = (j << nBits) | (int)((uint)next >> nBits2);
					j = next;
				}
				newMag[i] = mag[magLen - 1] << nBits;
			}
			return newMag;
		}

		public BigInteger ShiftLeft(int n)
		{
			if (this.sign == 0 || this.magnitude.Length == 0)
			{
				return BigInteger.Zero;
			}
			if (n == 0)
			{
				return this;
			}
			if (n < 0)
			{
				return this.ShiftRight(-n);
			}
			BigInteger result = new BigInteger(this.sign, BigInteger.ShiftLeft(this.magnitude, n), true);
			if (this.nBits != -1)
			{
				result.nBits = ((this.sign > 0) ? this.nBits : (this.nBits + n));
			}
			if (this.nBitLength != -1)
			{
				result.nBitLength = this.nBitLength + n;
			}
			return result;
		}

		private static int[] ShiftRightInPlace(int start, int[] mag, int n)
		{
			int nInts = (int)(((uint)n >> 5) + (uint)start);
			int nBits = n & 31;
			int magEnd = mag.Length - 1;
			if (nInts != start)
			{
				int delta = nInts - start;
				for (int i = magEnd; i >= nInts; i--)
				{
					mag[i] = mag[i - delta];
				}
				for (int j = nInts - 1; j >= start; j--)
				{
					mag[j] = 0;
				}
			}
			if (nBits != 0)
			{
				int nBits2 = 32 - nBits;
				int k = mag[magEnd];
				for (int l = magEnd; l > nInts; l--)
				{
					int next = mag[l - 1];
					mag[l] = (int)(((uint)k >> nBits) | (uint)((uint)next << nBits2));
					k = next;
				}
				mag[nInts] = (int)((uint)mag[nInts] >> nBits);
			}
			return mag;
		}

		private static int[] ShiftRightOneInPlace(int start, int[] mag)
		{
			int i = mag.Length;
			int j = mag[i - 1];
			while (--i > start)
			{
				int next = mag[i - 1];
				mag[i] = (int)(((uint)j >> 1) | (uint)((uint)next << 31));
				j = next;
			}
			mag[start] = (int)((uint)mag[start] >> 1);
			return mag;
		}

		public BigInteger ShiftRight(int n)
		{
			if (n == 0)
			{
				return this;
			}
			if (n < 0)
			{
				return this.ShiftLeft(-n);
			}
			if (n < this.BitLength)
			{
				int resultLength = this.BitLength - n + 31 >> 5;
				int[] res = new int[resultLength];
				int numInts = n >> 5;
				int numBits = n & 31;
				if (numBits == 0)
				{
					Array.Copy(this.magnitude, 0, res, 0, res.Length);
				}
				else
				{
					int numBits2 = 32 - numBits;
					int magPos = this.magnitude.Length - 1 - numInts;
					for (int i = resultLength - 1; i >= 0; i--)
					{
						res[i] = (int)((uint)this.magnitude[magPos--] >> (numBits & 31));
						if (magPos >= 0)
						{
							res[i] |= this.magnitude[magPos] << numBits2;
						}
					}
				}
				return new BigInteger(this.sign, res, false);
			}
			if (this.sign >= 0)
			{
				return BigInteger.Zero;
			}
			return BigInteger.One.Negate();
		}

		public int SignValue
		{
			get
			{
				return this.sign;
			}
		}

		private static int[] Subtract(int xStart, int[] x, int yStart, int[] y)
		{
			int iT = x.Length;
			int iV = y.Length;
			int borrow = 0;
			do
			{
				long i = ((long)x[--iT] & (long)((ulong)(-1))) - ((long)y[--iV] & (long)((ulong)(-1))) + (long)borrow;
				x[iT] = (int)i;
				borrow = (int)(i >> 63);
			}
			while (iV > yStart);
			if (borrow != 0)
			{
				while (--x[--iT] == -1)
				{
				}
			}
			return x;
		}

		public BigInteger Subtract(BigInteger n)
		{
			if (n.sign == 0)
			{
				return this;
			}
			if (this.sign == 0)
			{
				return n.Negate();
			}
			if (this.sign != n.sign)
			{
				return this.Add(n.Negate());
			}
			int compare = BigInteger.CompareNoLeadingZeroes(0, this.magnitude, 0, n.magnitude);
			if (compare == 0)
			{
				return BigInteger.Zero;
			}
			BigInteger bigun;
			BigInteger lilun;
			if (compare < 0)
			{
				bigun = n;
				lilun = this;
			}
			else
			{
				bigun = this;
				lilun = n;
			}
			return new BigInteger(this.sign * compare, BigInteger.doSubBigLil(bigun.magnitude, lilun.magnitude), true);
		}

		private static int[] doSubBigLil(int[] bigMag, int[] lilMag)
		{
			int[] res = (int[])bigMag.Clone();
			return BigInteger.Subtract(0, res, 0, lilMag);
		}

		public byte[] ToByteArray()
		{
			return this.ToByteArray(false);
		}

		public byte[] ToByteArrayUnsigned()
		{
			return this.ToByteArray(true);
		}

		private byte[] ToByteArray(bool unsigned)
		{
			if (this.sign != 0)
			{
				int nBits = ((unsigned && this.sign > 0) ? this.BitLength : (this.BitLength + 1));
				int nBytes = BigInteger.GetByteLength(nBits);
				byte[] bytes = new byte[nBytes];
				int magIndex = this.magnitude.Length;
				int bytesIndex = bytes.Length;
				if (this.sign > 0)
				{
					while (magIndex > 1)
					{
						uint mag = (uint)this.magnitude[--magIndex];
						bytes[--bytesIndex] = (byte)mag;
						bytes[--bytesIndex] = (byte)(mag >> 8);
						bytes[--bytesIndex] = (byte)(mag >> 16);
						bytes[--bytesIndex] = (byte)(mag >> 24);
					}
					uint lastMag;
					for (lastMag = (uint)this.magnitude[0]; lastMag > 255U; lastMag >>= 8)
					{
						bytes[--bytesIndex] = (byte)lastMag;
					}
					bytes[bytesIndex - 1] = (byte)lastMag;
				}
				else
				{
					bool carry = true;
					while (magIndex > 1)
					{
						uint mag2 = (uint)(~(uint)this.magnitude[--magIndex]);
						if (carry)
						{
							carry = (mag2 += 1U) == 0U;
						}
						bytes[--bytesIndex] = (byte)mag2;
						bytes[--bytesIndex] = (byte)(mag2 >> 8);
						bytes[--bytesIndex] = (byte)(mag2 >> 16);
						bytes[--bytesIndex] = (byte)(mag2 >> 24);
					}
					uint lastMag2 = (uint)this.magnitude[0];
					if (carry)
					{
						lastMag2 -= 1U;
					}
					while (lastMag2 > 255U)
					{
						bytes[--bytesIndex] = (byte)(~(byte)lastMag2);
						lastMag2 >>= 8;
					}
					bytes[--bytesIndex] = (byte)(~(byte)lastMag2);
					if (bytesIndex > 0)
					{
						bytes[bytesIndex - 1] = byte.MaxValue;
					}
				}
				return bytes;
			}
			if (!unsigned)
			{
				return new byte[1];
			}
			return BigInteger.ZeroEncoding;
		}

		public override string ToString()
		{
			return this.ToString(10);
		}

		public string ToString(int radix)
		{
			if (radix != 2 && radix != 10 && radix != 16)
			{
				throw new FormatException("Only bases 2, 10, 16 are allowed");
			}
			if (this.magnitude == null)
			{
				return "null";
			}
			if (this.sign == 0)
			{
				return "0";
			}
			StringBuilder sb = new StringBuilder();
			if (radix == 16)
			{
				sb.Append(this.magnitude[0].ToString("x"));
				for (int i = 1; i < this.magnitude.Length; i++)
				{
					sb.Append(this.magnitude[i].ToString("x8"));
				}
			}
			else if (radix == 2)
			{
				sb.Append('1');
				for (int j = this.BitLength - 2; j >= 0; j--)
				{
					sb.Append(this.TestBit(j) ? '1' : '0');
				}
			}
			else
			{
				Stack<string> S = new Stack<string>();
				BigInteger bs = BigInteger.ValueOf((long)radix);
				BigInteger u = this.Abs();
				while (u.sign != 0)
				{
					BigInteger b = u.Mod(bs);
					if (b.sign == 0)
					{
						S.Push("0");
					}
					else
					{
						S.Push(b.magnitude[0].ToString("d"));
					}
					u = u.Divide(bs);
				}
				while (S.Count != 0)
				{
					sb.Append(S.Pop());
				}
			}
			string s = sb.ToString();
			if (s[0] == '0')
			{
				int nonZeroPos = 0;
				while (s[++nonZeroPos] == '0')
				{
				}
				s = s.Substring(nonZeroPos);
			}
			if (this.sign == -1)
			{
				s = "-" + s;
			}
			return s;
		}

		private static BigInteger createUValueOf(ulong value)
		{
			int msw = (int)(value >> 32);
			int lsw = (int)value;
			if (msw != 0)
			{
				return new BigInteger(1, new int[] { msw, lsw }, false);
			}
			if (lsw != 0)
			{
				BigInteger i = new BigInteger(1, new int[] { lsw }, false);
				if ((lsw & -lsw) == lsw)
				{
					i.nBits = 1;
				}
				return i;
			}
			return BigInteger.Zero;
		}

		private static BigInteger createValueOf(long value)
		{
			if (value >= 0L)
			{
				return BigInteger.createUValueOf((ulong)value);
			}
			if (value == -9223372036854775808L)
			{
				return BigInteger.createValueOf(~value).Not();
			}
			return BigInteger.createValueOf(-value).Negate();
		}

		public static BigInteger ValueOf(long value)
		{
			if (value <= 3L)
			{
				if (value < 0L)
				{
					goto IL_0049;
				}
				switch ((int)value)
				{
				case 0:
					return BigInteger.Zero;
				case 1:
					return BigInteger.One;
				case 2:
					return BigInteger.Two;
				case 3:
					return BigInteger.Three;
				}
			}
			if (value == 10L)
			{
				return BigInteger.Ten;
			}
			IL_0049:
			return BigInteger.createValueOf(value);
		}

		public int GetLowestSetBit()
		{
			if (this.sign == 0)
			{
				return -1;
			}
			int w = this.magnitude.Length;
			while (--w > 0 && this.magnitude[w] == 0)
			{
			}
			int word = this.magnitude[w];
			int b = (((word & 65535) == 0) ? (((word & 16711680) == 0) ? 7 : 15) : (((word & 255) == 0) ? 23 : 31));
			while (b > 0 && word << b != -2147483648)
			{
				b--;
			}
			return (this.magnitude.Length - w) * 32 - (b + 1);
		}

		public bool TestBit(int n)
		{
			if (n < 0)
			{
				throw new ArithmeticException("Bit position must not be negative");
			}
			if (this.sign < 0)
			{
				return !this.Not().TestBit(n);
			}
			int wordNum = n / 32;
			if (wordNum >= this.magnitude.Length)
			{
				return false;
			}
			int word = this.magnitude[this.magnitude.Length - 1 - wordNum];
			return ((word >> n % 32) & 1) > 0;
		}

		public BigInteger Or(BigInteger value)
		{
			if (this.sign == 0)
			{
				return value;
			}
			if (value.sign == 0)
			{
				return this;
			}
			int[] aMag = ((this.sign > 0) ? this.magnitude : this.Add(BigInteger.One).magnitude);
			int[] bMag = ((value.sign > 0) ? value.magnitude : value.Add(BigInteger.One).magnitude);
			bool resultNeg = this.sign < 0 || value.sign < 0;
			int resultLength = Math.Max(aMag.Length, bMag.Length);
			int[] resultMag = new int[resultLength];
			int aStart = resultMag.Length - aMag.Length;
			int bStart = resultMag.Length - bMag.Length;
			for (int i = 0; i < resultMag.Length; i++)
			{
				int aWord = ((i >= aStart) ? aMag[i - aStart] : 0);
				int bWord = ((i >= bStart) ? bMag[i - bStart] : 0);
				if (this.sign < 0)
				{
					aWord = ~aWord;
				}
				if (value.sign < 0)
				{
					bWord = ~bWord;
				}
				resultMag[i] = aWord | bWord;
				if (resultNeg)
				{
					resultMag[i] = ~resultMag[i];
				}
			}
			BigInteger result = new BigInteger(1, resultMag, true);
			if (resultNeg)
			{
				result = result.Not();
			}
			return result;
		}

		public BigInteger Xor(BigInteger value)
		{
			if (this.sign == 0)
			{
				return value;
			}
			if (value.sign == 0)
			{
				return this;
			}
			int[] aMag = ((this.sign > 0) ? this.magnitude : this.Add(BigInteger.One).magnitude);
			int[] bMag = ((value.sign > 0) ? value.magnitude : value.Add(BigInteger.One).magnitude);
			bool resultNeg = (this.sign < 0 && value.sign >= 0) || (this.sign >= 0 && value.sign < 0);
			int resultLength = Math.Max(aMag.Length, bMag.Length);
			int[] resultMag = new int[resultLength];
			int aStart = resultMag.Length - aMag.Length;
			int bStart = resultMag.Length - bMag.Length;
			for (int i = 0; i < resultMag.Length; i++)
			{
				int aWord = ((i >= aStart) ? aMag[i - aStart] : 0);
				int bWord = ((i >= bStart) ? bMag[i - bStart] : 0);
				if (this.sign < 0)
				{
					aWord = ~aWord;
				}
				if (value.sign < 0)
				{
					bWord = ~bWord;
				}
				resultMag[i] = aWord ^ bWord;
				if (resultNeg)
				{
					resultMag[i] = ~resultMag[i];
				}
			}
			BigInteger result = new BigInteger(1, resultMag, true);
			if (resultNeg)
			{
				result = result.Not();
			}
			return result;
		}

		public BigInteger SetBit(int n)
		{
			if (n < 0)
			{
				throw new ArithmeticException("Bit address less than zero");
			}
			if (this.TestBit(n))
			{
				return this;
			}
			if (this.sign > 0 && n < this.BitLength - 1)
			{
				return this.FlipExistingBit(n);
			}
			return this.Or(BigInteger.One.ShiftLeft(n));
		}

		public BigInteger ClearBit(int n)
		{
			if (n < 0)
			{
				throw new ArithmeticException("Bit address less than zero");
			}
			if (!this.TestBit(n))
			{
				return this;
			}
			if (this.sign > 0 && n < this.BitLength - 1)
			{
				return this.FlipExistingBit(n);
			}
			return this.AndNot(BigInteger.One.ShiftLeft(n));
		}

		public BigInteger FlipBit(int n)
		{
			if (n < 0)
			{
				throw new ArithmeticException("Bit address less than zero");
			}
			if (this.sign > 0 && n < this.BitLength - 1)
			{
				return this.FlipExistingBit(n);
			}
			return this.Xor(BigInteger.One.ShiftLeft(n));
		}

		private BigInteger FlipExistingBit(int n)
		{
			int[] mag = (int[])this.magnitude.Clone();
			mag[mag.Length - 1 - (n >> 5)] ^= 1 << n;
			return new BigInteger(this.sign, mag, false);
		}

		private const long IMASK = 4294967295L;

		private const int BitsPerByte = 8;

		private const int BitsPerInt = 32;

		private const int BytesPerInt = 4;

		private static readonly int[][] primeLists = new int[][]
		{
			new int[] { 3, 5, 7, 11, 13, 17, 19, 23 },
			new int[] { 29, 31, 37, 41, 43 },
			new int[] { 47, 53, 59, 61, 67 },
			new int[] { 71, 73, 79, 83 },
			new int[] { 89, 97, 101, 103 },
			new int[] { 107, 109, 113, 127 },
			new int[] { 131, 137, 139, 149 },
			new int[] { 151, 157, 163, 167 },
			new int[] { 173, 179, 181, 191 },
			new int[] { 193, 197, 199, 211 },
			new int[] { 223, 227, 229 },
			new int[] { 233, 239, 241 },
			new int[] { 251, 257, 263 },
			new int[] { 269, 271, 277 },
			new int[] { 281, 283, 293 },
			new int[] { 307, 311, 313 },
			new int[] { 317, 331, 337 },
			new int[] { 347, 349, 353 },
			new int[] { 359, 367, 373 },
			new int[] { 379, 383, 389 },
			new int[] { 397, 401, 409 },
			new int[] { 419, 421, 431 },
			new int[] { 433, 439, 443 },
			new int[] { 449, 457, 461 },
			new int[] { 463, 467, 479 },
			new int[] { 487, 491, 499 },
			new int[] { 503, 509, 521 },
			new int[] { 523, 541, 547 },
			new int[] { 557, 563, 569 },
			new int[] { 571, 577, 587 },
			new int[] { 593, 599, 601 },
			new int[] { 607, 613, 617 },
			new int[] { 619, 631, 641 },
			new int[] { 643, 647, 653 },
			new int[] { 659, 661, 673 },
			new int[] { 677, 683, 691 },
			new int[] { 701, 709, 719 },
			new int[] { 727, 733, 739 },
			new int[] { 743, 751, 757 },
			new int[] { 761, 769, 773 },
			new int[] { 787, 797, 809 },
			new int[] { 811, 821, 823 },
			new int[] { 827, 829, 839 },
			new int[] { 853, 857, 859 },
			new int[] { 863, 877, 881 },
			new int[] { 883, 887, 907 },
			new int[] { 911, 919, 929 },
			new int[] { 937, 941, 947 },
			new int[] { 953, 967, 971 },
			new int[] { 977, 983, 991 },
			new int[] { 997, 1009, 1013 },
			new int[] { 1019, 1021, 1031 }
		};

		private static readonly int[] primeProducts = new int[BigInteger.primeLists.Length];

		private static readonly ulong UIMASK = (ulong)(-1);

		private static readonly int[] ZeroMagnitude = new int[0];

		private static readonly byte[] ZeroEncoding = new byte[0];

		public static readonly BigInteger Zero = new BigInteger(0, BigInteger.ZeroMagnitude, false);

		public static readonly BigInteger One = BigInteger.createUValueOf(1UL);

		public static readonly BigInteger Two = BigInteger.createUValueOf(2UL);

		public static readonly BigInteger Three = BigInteger.createUValueOf(3UL);

		public static readonly BigInteger Ten = BigInteger.createUValueOf(10UL);

		private static readonly int chunk2 = 1;

		private static readonly BigInteger radix2 = BigInteger.ValueOf(2L);

		private static readonly BigInteger radix2E = BigInteger.radix2.Pow(BigInteger.chunk2);

		private static readonly int chunk10 = 19;

		private static readonly BigInteger radix10 = BigInteger.ValueOf(10L);

		private static readonly BigInteger radix10E = BigInteger.radix10.Pow(BigInteger.chunk10);

		private static readonly int chunk16 = 16;

		private static readonly BigInteger radix16 = BigInteger.ValueOf(16L);

		private static readonly BigInteger radix16E = BigInteger.radix16.Pow(BigInteger.chunk16);

		private static readonly Random RandomSource = new Random();

		private int sign;

		private int[] magnitude;

		private int nBits = -1;

		private int nBitLength = -1;

		private long mQuote = -1L;

		private static readonly byte[] rndMask = new byte[] { byte.MaxValue, 127, 63, 31, 15, 7, 3, 1 };

		private static readonly byte[] bitCounts = new byte[]
		{
			0, 1, 1, 2, 1, 2, 2, 3, 1, 2,
			2, 3, 2, 3, 3, 4, 1, 2, 2, 3,
			2, 3, 3, 4, 2, 3, 3, 4, 3, 4,
			4, 5, 1, 2, 2, 3, 2, 3, 3, 4,
			2, 3, 3, 4, 3, 4, 4, 5, 2, 3,
			3, 4, 3, 4, 4, 5, 3, 4, 4, 5,
			4, 5, 5, 6, 1, 2, 2, 3, 2, 3,
			3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
			2, 3, 3, 4, 3, 4, 4, 5, 3, 4,
			4, 5, 4, 5, 5, 6, 2, 3, 3, 4,
			3, 4, 4, 5, 3, 4, 4, 5, 4, 5,
			5, 6, 3, 4, 4, 5, 4, 5, 5, 6,
			4, 5, 5, 6, 5, 6, 6, 7, 1, 2,
			2, 3, 2, 3, 3, 4, 2, 3, 3, 4,
			3, 4, 4, 5, 2, 3, 3, 4, 3, 4,
			4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
			2, 3, 3, 4, 3, 4, 4, 5, 3, 4,
			4, 5, 4, 5, 5, 6, 3, 4, 4, 5,
			4, 5, 5, 6, 4, 5, 5, 6, 5, 6,
			6, 7, 2, 3, 3, 4, 3, 4, 4, 5,
			3, 4, 4, 5, 4, 5, 5, 6, 3, 4,
			4, 5, 4, 5, 5, 6, 4, 5, 5, 6,
			5, 6, 6, 7, 3, 4, 4, 5, 4, 5,
			5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
			4, 5, 5, 6, 5, 6, 6, 7, 5, 6,
			6, 7, 6, 7, 7, 8
		};
	}
}
