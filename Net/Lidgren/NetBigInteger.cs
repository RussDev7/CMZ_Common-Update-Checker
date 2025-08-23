using System;
using System.Collections;
using System.Globalization;
using System.Text;

namespace DNA.Net.Lidgren
{
	internal class NetBigInteger
	{
		private static int GetByteLength(int nBits)
		{
			return (nBits + 8 - 1) / 8;
		}

		private NetBigInteger()
		{
		}

		private NetBigInteger(int signum, int[] mag, bool checkMag)
		{
			if (!checkMag)
			{
				this.m_sign = signum;
				this.m_magnitude = mag;
				return;
			}
			int i = 0;
			while (i < mag.Length && mag[i] == 0)
			{
				i++;
			}
			if (i == mag.Length)
			{
				this.m_magnitude = NetBigInteger.ZeroMagnitude;
				return;
			}
			this.m_sign = signum;
			if (i == 0)
			{
				this.m_magnitude = mag;
				return;
			}
			this.m_magnitude = new int[mag.Length - i];
			Array.Copy(mag, i, this.m_magnitude, 0, this.m_magnitude.Length);
		}

		public NetBigInteger(string value)
			: this(value, 10)
		{
		}

		public NetBigInteger(string str, int radix)
		{
			if (str.Length == 0)
			{
				throw new FormatException("Zero length BigInteger");
			}
			NumberStyles style;
			int chunk;
			NetBigInteger r;
			NetBigInteger rE;
			if (radix != 2)
			{
				if (radix != 10)
				{
					if (radix != 16)
					{
						throw new FormatException("Only bases 2, 10, or 16 allowed");
					}
					style = NumberStyles.AllowHexSpecifier;
					chunk = NetBigInteger.chunk16;
					r = NetBigInteger.radix16;
					rE = NetBigInteger.radix16E;
				}
				else
				{
					style = NumberStyles.Integer;
					chunk = NetBigInteger.chunk10;
					r = NetBigInteger.radix10;
					rE = NetBigInteger.radix10E;
				}
			}
			else
			{
				style = NumberStyles.Integer;
				chunk = NetBigInteger.chunk2;
				r = NetBigInteger.radix2;
				rE = NetBigInteger.radix2E;
			}
			int index = 0;
			this.m_sign = 1;
			if (str[0] == '-')
			{
				if (str.Length == 1)
				{
					throw new FormatException("Zero length BigInteger");
				}
				this.m_sign = -1;
				index = 1;
			}
			while (index < str.Length && int.Parse(str[index].ToString(), style) == 0)
			{
				index++;
			}
			if (index >= str.Length)
			{
				this.m_sign = 0;
				this.m_magnitude = NetBigInteger.ZeroMagnitude;
				return;
			}
			NetBigInteger b = NetBigInteger.Zero;
			int next = index + chunk;
			if (next <= str.Length)
			{
				string s;
				for (;;)
				{
					s = str.Substring(index, chunk);
					ulong i = ulong.Parse(s, style);
					NetBigInteger bi = NetBigInteger.createUValueOf(i);
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
				NetBigInteger bi2 = NetBigInteger.createUValueOf(j);
				if (b.m_sign > 0)
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
			this.m_magnitude = b.m_magnitude;
		}

		public NetBigInteger(byte[] bytes)
			: this(bytes, 0, bytes.Length)
		{
		}

		public NetBigInteger(byte[] bytes, int offset, int length)
		{
			if (length == 0)
			{
				throw new FormatException("Zero length BigInteger");
			}
			if ((sbyte)bytes[offset] >= 0)
			{
				this.m_magnitude = NetBigInteger.MakeMagnitude(bytes, offset, length);
				this.m_sign = ((this.m_magnitude.Length > 0) ? 1 : 0);
				return;
			}
			this.m_sign = -1;
			int end = offset + length;
			int iBval = offset;
			while (iBval < end && (sbyte)bytes[iBval] == -1)
			{
				iBval++;
			}
			if (iBval >= end)
			{
				this.m_magnitude = NetBigInteger.One.m_magnitude;
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
			this.m_magnitude = NetBigInteger.MakeMagnitude(inverse, 0, inverse.Length);
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
				return NetBigInteger.ZeroMagnitude;
			}
			int nInts = (end - firstSignificant + 3) / 4;
			int bCount = (end - firstSignificant) % 4;
			if (bCount == 0)
			{
				bCount = 4;
			}
			if (nInts < 1)
			{
				return NetBigInteger.ZeroMagnitude;
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

		public NetBigInteger(int sign, byte[] bytes)
			: this(sign, bytes, 0, bytes.Length)
		{
		}

		public NetBigInteger(int sign, byte[] bytes, int offset, int length)
		{
			if (sign < -1 || sign > 1)
			{
				throw new FormatException("Invalid sign value");
			}
			if (sign == 0)
			{
				this.m_magnitude = NetBigInteger.ZeroMagnitude;
				return;
			}
			this.m_magnitude = NetBigInteger.MakeMagnitude(bytes, offset, length);
			this.m_sign = ((this.m_magnitude.Length < 1) ? 0 : sign);
		}

		public NetBigInteger Abs()
		{
			if (this.m_sign < 0)
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

		public NetBigInteger Add(NetBigInteger value)
		{
			if (this.m_sign == 0)
			{
				return value;
			}
			if (this.m_sign == value.m_sign)
			{
				return this.AddToMagnitude(value.m_magnitude);
			}
			if (value.m_sign == 0)
			{
				return this;
			}
			if (value.m_sign < 0)
			{
				return this.Subtract(value.Negate());
			}
			return value.Subtract(this.Negate());
		}

		private NetBigInteger AddToMagnitude(int[] magToAdd)
		{
			int[] big;
			int[] small;
			if (this.m_magnitude.Length < magToAdd.Length)
			{
				big = magToAdd;
				small = this.m_magnitude;
			}
			else
			{
				big = this.m_magnitude;
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
			bigCopy = NetBigInteger.AddMagnitudes(bigCopy, small);
			return new NetBigInteger(this.m_sign, bigCopy, possibleOverflow);
		}

		public NetBigInteger And(NetBigInteger value)
		{
			if (this.m_sign == 0 || value.m_sign == 0)
			{
				return NetBigInteger.Zero;
			}
			int[] aMag = ((this.m_sign > 0) ? this.m_magnitude : this.Add(NetBigInteger.One).m_magnitude);
			int[] bMag = ((value.m_sign > 0) ? value.m_magnitude : value.Add(NetBigInteger.One).m_magnitude);
			bool resultNeg = this.m_sign < 0 && value.m_sign < 0;
			int resultLength = Math.Max(aMag.Length, bMag.Length);
			int[] resultMag = new int[resultLength];
			int aStart = resultMag.Length - aMag.Length;
			int bStart = resultMag.Length - bMag.Length;
			for (int i = 0; i < resultMag.Length; i++)
			{
				int aWord = ((i >= aStart) ? aMag[i - aStart] : 0);
				int bWord = ((i >= bStart) ? bMag[i - bStart] : 0);
				if (this.m_sign < 0)
				{
					aWord = ~aWord;
				}
				if (value.m_sign < 0)
				{
					bWord = ~bWord;
				}
				resultMag[i] = aWord & bWord;
				if (resultNeg)
				{
					resultMag[i] = ~resultMag[i];
				}
			}
			NetBigInteger result = new NetBigInteger(1, resultMag, true);
			if (resultNeg)
			{
				result = result.Not();
			}
			return result;
		}

		private int calcBitLength(int indx, int[] mag)
		{
			while (indx < mag.Length)
			{
				if (mag[indx] != 0)
				{
					int bitLength = 32 * (mag.Length - indx - 1);
					int firstMag = mag[indx];
					bitLength += NetBigInteger.BitLen(firstMag);
					if (this.m_sign < 0 && (firstMag & -firstMag) == firstMag)
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
				if (this.m_numBitLength == -1)
				{
					this.m_numBitLength = ((this.m_sign == 0) ? 0 : this.calcBitLength(0, this.m_magnitude));
				}
				return this.m_numBitLength;
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
			return this.m_sign > 0 && this.m_numBits == 1;
		}

		public int CompareTo(object obj)
		{
			return this.CompareTo((NetBigInteger)obj);
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
			return NetBigInteger.CompareNoLeadingZeroes(xIndx, x, yIndx, y);
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

		public int CompareTo(NetBigInteger value)
		{
			if (this.m_sign < value.m_sign)
			{
				return -1;
			}
			if (this.m_sign > value.m_sign)
			{
				return 1;
			}
			if (this.m_sign != 0)
			{
				return this.m_sign * NetBigInteger.CompareNoLeadingZeroes(0, this.m_magnitude, 0, value.m_magnitude);
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
			int xyCmp = NetBigInteger.CompareNoLeadingZeroes(xStart, x, yStart, y);
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
					c = NetBigInteger.ShiftLeft(y, shift);
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
					if (cBitLength < xBitLength || NetBigInteger.CompareNoLeadingZeroes(xStart, x, cStart, c) >= 0)
					{
						NetBigInteger.Subtract(xStart, x, cStart, c);
						NetBigInteger.AddMagnitudes(count, iCount);
						while (x[xStart] == 0)
						{
							if (++xStart == x.Length)
							{
								return count;
							}
						}
						xBitLength = 32 * (x.Length - xStart - 1) + NetBigInteger.BitLen(x[xStart]);
						if (xBitLength <= yBitLength)
						{
							if (xBitLength < yBitLength)
							{
								return count;
							}
							xyCmp = NetBigInteger.CompareNoLeadingZeroes(xStart, x, yStart, y);
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
						c = NetBigInteger.ShiftRightOneInPlace(cStart, c);
						cBitLength--;
						iCount = NetBigInteger.ShiftRightOneInPlace(iCountStart, iCount);
					}
					else
					{
						c = NetBigInteger.ShiftRightInPlace(cStart, c, shift);
						cBitLength -= shift;
						iCount = NetBigInteger.ShiftRightInPlace(iCountStart, iCount, shift);
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
				NetBigInteger.AddMagnitudes(count, NetBigInteger.One.m_magnitude);
				Array.Clear(x, xStart, x.Length - xStart);
			}
			return count;
		}

		public NetBigInteger Divide(NetBigInteger val)
		{
			if (val.m_sign == 0)
			{
				throw new ArithmeticException("Division by zero error");
			}
			if (this.m_sign == 0)
			{
				return NetBigInteger.Zero;
			}
			if (!val.QuickPow2Check())
			{
				int[] mag = (int[])this.m_magnitude.Clone();
				return new NetBigInteger(this.m_sign * val.m_sign, this.Divide(mag, val.m_magnitude), true);
			}
			NetBigInteger result = this.Abs().ShiftRight(val.Abs().BitLength - 1);
			if (val.m_sign != this.m_sign)
			{
				return result.Negate();
			}
			return result;
		}

		public NetBigInteger[] DivideAndRemainder(NetBigInteger val)
		{
			if (val.m_sign == 0)
			{
				throw new ArithmeticException("Division by zero error");
			}
			NetBigInteger[] biggies = new NetBigInteger[2];
			if (this.m_sign == 0)
			{
				biggies[0] = NetBigInteger.Zero;
				biggies[1] = NetBigInteger.Zero;
			}
			else if (val.QuickPow2Check())
			{
				int e = val.Abs().BitLength - 1;
				NetBigInteger quotient = this.Abs().ShiftRight(e);
				int[] remainder = this.LastNBits(e);
				biggies[0] = ((val.m_sign == this.m_sign) ? quotient : quotient.Negate());
				biggies[1] = new NetBigInteger(this.m_sign, remainder, true);
			}
			else
			{
				int[] remainder2 = (int[])this.m_magnitude.Clone();
				int[] quotient2 = this.Divide(remainder2, val.m_magnitude);
				biggies[0] = new NetBigInteger(this.m_sign * val.m_sign, quotient2, true);
				biggies[1] = new NetBigInteger(this.m_sign, remainder2, true);
			}
			return biggies;
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}
			NetBigInteger biggie = obj as NetBigInteger;
			if (biggie == null)
			{
				return false;
			}
			if (biggie.m_sign != this.m_sign || biggie.m_magnitude.Length != this.m_magnitude.Length)
			{
				return false;
			}
			for (int i = 0; i < this.m_magnitude.Length; i++)
			{
				if (biggie.m_magnitude[i] != this.m_magnitude[i])
				{
					return false;
				}
			}
			return true;
		}

		public NetBigInteger Gcd(NetBigInteger value)
		{
			if (value.m_sign == 0)
			{
				return this.Abs();
			}
			if (this.m_sign == 0)
			{
				return value.Abs();
			}
			NetBigInteger u = this;
			NetBigInteger v = value;
			while (v.m_sign != 0)
			{
				NetBigInteger r = u.Mod(v);
				u = v;
				v = r;
			}
			return u;
		}

		public override int GetHashCode()
		{
			int hc = this.m_magnitude.Length;
			if (this.m_magnitude.Length > 0)
			{
				hc ^= this.m_magnitude[0];
				if (this.m_magnitude.Length > 1)
				{
					hc ^= this.m_magnitude[this.m_magnitude.Length - 1];
				}
			}
			if (this.m_sign >= 0)
			{
				return hc;
			}
			return ~hc;
		}

		private NetBigInteger Inc()
		{
			if (this.m_sign == 0)
			{
				return NetBigInteger.One;
			}
			if (this.m_sign < 0)
			{
				return new NetBigInteger(-1, NetBigInteger.doSubBigLil(this.m_magnitude, NetBigInteger.One.m_magnitude), true);
			}
			return this.AddToMagnitude(NetBigInteger.One.m_magnitude);
		}

		public int IntValue
		{
			get
			{
				if (this.m_sign == 0)
				{
					return 0;
				}
				if (this.m_sign <= 0)
				{
					return -this.m_magnitude[this.m_magnitude.Length - 1];
				}
				return this.m_magnitude[this.m_magnitude.Length - 1];
			}
		}

		public NetBigInteger Max(NetBigInteger value)
		{
			if (this.CompareTo(value) <= 0)
			{
				return value;
			}
			return this;
		}

		public NetBigInteger Min(NetBigInteger value)
		{
			if (this.CompareTo(value) >= 0)
			{
				return value;
			}
			return this;
		}

		public NetBigInteger Mod(NetBigInteger m)
		{
			if (m.m_sign < 1)
			{
				throw new ArithmeticException("Modulus must be positive");
			}
			NetBigInteger biggie = this.Remainder(m);
			if (biggie.m_sign < 0)
			{
				return biggie.Add(m);
			}
			return biggie;
		}

		public NetBigInteger ModInverse(NetBigInteger m)
		{
			if (m.m_sign < 1)
			{
				throw new ArithmeticException("Modulus must be positive");
			}
			NetBigInteger x = new NetBigInteger();
			NetBigInteger gcd = NetBigInteger.ExtEuclid(this, m, x, null);
			if (!gcd.Equals(NetBigInteger.One))
			{
				throw new ArithmeticException("Numbers not relatively prime.");
			}
			if (x.m_sign < 0)
			{
				x.m_sign = 1;
				x.m_magnitude = NetBigInteger.doSubBigLil(m.m_magnitude, x.m_magnitude);
			}
			return x;
		}

		private static NetBigInteger ExtEuclid(NetBigInteger a, NetBigInteger b, NetBigInteger u1Out, NetBigInteger u2Out)
		{
			NetBigInteger u = NetBigInteger.One;
			NetBigInteger u2 = a;
			NetBigInteger v = NetBigInteger.Zero;
			NetBigInteger v2 = b;
			while (v2.m_sign > 0)
			{
				NetBigInteger[] q = u2.DivideAndRemainder(v2);
				NetBigInteger tmp = v.Multiply(q[0]);
				NetBigInteger tn = u.Subtract(tmp);
				u = v;
				v = tn;
				u2 = v2;
				v2 = q[1];
			}
			if (u1Out != null)
			{
				u1Out.m_sign = u.m_sign;
				u1Out.m_magnitude = u.m_magnitude;
			}
			if (u2Out != null)
			{
				NetBigInteger tmp2 = u.Multiply(a);
				tmp2 = u2.Subtract(tmp2);
				NetBigInteger res = tmp2.Divide(b);
				u2Out.m_sign = res.m_sign;
				u2Out.m_magnitude = res.m_magnitude;
			}
			return u2;
		}

		private static void ZeroOut(int[] x)
		{
			Array.Clear(x, 0, x.Length);
		}

		public NetBigInteger ModPow(NetBigInteger exponent, NetBigInteger m)
		{
			if (m.m_sign < 1)
			{
				throw new ArithmeticException("Modulus must be positive");
			}
			if (m.Equals(NetBigInteger.One))
			{
				return NetBigInteger.Zero;
			}
			if (exponent.m_sign == 0)
			{
				return NetBigInteger.One;
			}
			if (this.m_sign == 0)
			{
				return NetBigInteger.Zero;
			}
			int[] zVal = null;
			int[] yAccum = null;
			bool useMonty = (m.m_magnitude[m.m_magnitude.Length - 1] & 1) == 1;
			long mQ = 0L;
			if (useMonty)
			{
				mQ = m.GetMQuote();
				NetBigInteger tmp = this.ShiftLeft(32 * m.m_magnitude.Length).Mod(m);
				zVal = tmp.m_magnitude;
				useMonty = zVal.Length <= m.m_magnitude.Length;
				if (useMonty)
				{
					yAccum = new int[m.m_magnitude.Length + 1];
					if (zVal.Length < m.m_magnitude.Length)
					{
						int[] longZ = new int[m.m_magnitude.Length];
						zVal.CopyTo(longZ, longZ.Length - zVal.Length);
						zVal = longZ;
					}
				}
			}
			if (!useMonty)
			{
				if (this.m_magnitude.Length <= m.m_magnitude.Length)
				{
					zVal = new int[m.m_magnitude.Length];
					this.m_magnitude.CopyTo(zVal, zVal.Length - this.m_magnitude.Length);
				}
				else
				{
					NetBigInteger tmp2 = this.Remainder(m);
					zVal = new int[m.m_magnitude.Length];
					tmp2.m_magnitude.CopyTo(zVal, zVal.Length - tmp2.m_magnitude.Length);
				}
				yAccum = new int[m.m_magnitude.Length * 2];
			}
			int[] yVal = new int[m.m_magnitude.Length];
			for (int i = 0; i < exponent.m_magnitude.Length; i++)
			{
				int v = exponent.m_magnitude[i];
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
						NetBigInteger.MultiplyMonty(yAccum, yVal, yVal, m.m_magnitude, mQ);
					}
					else
					{
						NetBigInteger.Square(yAccum, yVal);
						this.Remainder(yAccum, m.m_magnitude);
						Array.Copy(yAccum, yAccum.Length - yVal.Length, yVal, 0, yVal.Length);
						NetBigInteger.ZeroOut(yAccum);
					}
					bits++;
					if (v < 0)
					{
						if (useMonty)
						{
							NetBigInteger.MultiplyMonty(yAccum, yVal, zVal, m.m_magnitude, mQ);
						}
						else
						{
							NetBigInteger.Multiply(yAccum, yVal, zVal);
							this.Remainder(yAccum, m.m_magnitude);
							Array.Copy(yAccum, yAccum.Length - yVal.Length, yVal, 0, yVal.Length);
							NetBigInteger.ZeroOut(yAccum);
						}
					}
					v <<= 1;
				}
				while (bits < 32)
				{
					if (useMonty)
					{
						NetBigInteger.MultiplyMonty(yAccum, yVal, yVal, m.m_magnitude, mQ);
					}
					else
					{
						NetBigInteger.Square(yAccum, yVal);
						this.Remainder(yAccum, m.m_magnitude);
						Array.Copy(yAccum, yAccum.Length - yVal.Length, yVal, 0, yVal.Length);
						NetBigInteger.ZeroOut(yAccum);
					}
					bits++;
				}
			}
			if (useMonty)
			{
				NetBigInteger.ZeroOut(zVal);
				zVal[zVal.Length - 1] = 1;
				NetBigInteger.MultiplyMonty(yAccum, yVal, zVal, m.m_magnitude, mQ);
			}
			NetBigInteger result = new NetBigInteger(1, yVal, true);
			if (exponent.m_sign <= 0)
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
			long gcd = NetBigInteger.FastExtEuclid(v, m, x);
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
			if (this.m_quote != -1L)
			{
				return this.m_quote;
			}
			if (this.m_magnitude.Length == 0 || (this.m_magnitude[this.m_magnitude.Length - 1] & 1) == 0)
			{
				return -1L;
			}
			long v = (long)(~this.m_magnitude[this.m_magnitude.Length - 1] | 1) & (long)((ulong)(-1));
			this.m_quote = NetBigInteger.FastModInverse(v, 4294967296L);
			return this.m_quote;
		}

		private static void MultiplyMonty(int[] a, int[] x, int[] y, int[] m, long mQuote)
		{
			if (m.Length == 1)
			{
				x[0] = (int)NetBigInteger.MultiplyMontyNIsOne((uint)x[0], (uint)y[0], (uint)m[0], (ulong)mQuote);
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
			if (NetBigInteger.CompareTo(0, a, 0, m) >= 0)
			{
				NetBigInteger.Subtract(0, a, 0, m);
			}
			Array.Copy(a, 1, x, 0, i);
		}

		private static uint MultiplyMontyNIsOne(uint x, uint y, uint m, ulong mQuote)
		{
			ulong um = (ulong)m;
			ulong prod = (ulong)x * (ulong)y;
			ulong u = (prod * mQuote) & NetBigInteger.UIMASK;
			ulong prod2 = u * um;
			ulong tmp = (prod & NetBigInteger.UIMASK) + (prod2 & NetBigInteger.UIMASK);
			ulong carry = (prod >> 32) + (prod2 >> 32) + (tmp >> 32);
			if (carry > um)
			{
				carry -= um;
			}
			return (uint)(carry & NetBigInteger.UIMASK);
		}

		public NetBigInteger Modulus(NetBigInteger val)
		{
			return this.Mod(val);
		}

		public NetBigInteger Multiply(NetBigInteger val)
		{
			if (this.m_sign == 0 || val.m_sign == 0)
			{
				return NetBigInteger.Zero;
			}
			if (val.QuickPow2Check())
			{
				NetBigInteger result = this.ShiftLeft(val.Abs().BitLength - 1);
				if (val.m_sign <= 0)
				{
					return result.Negate();
				}
				return result;
			}
			else
			{
				if (!this.QuickPow2Check())
				{
					int maxBitLength = this.BitLength + val.BitLength;
					int resLength = (maxBitLength + 32 - 1) / 32;
					int[] res = new int[resLength];
					if (val == this)
					{
						NetBigInteger.Square(res, this.m_magnitude);
					}
					else
					{
						NetBigInteger.Multiply(res, this.m_magnitude, val.m_magnitude);
					}
					return new NetBigInteger(this.m_sign * val.m_sign, res, true);
				}
				NetBigInteger result2 = val.ShiftLeft(this.Abs().BitLength - 1);
				if (this.m_sign <= 0)
				{
					return result2.Negate();
				}
				return result2;
			}
		}

		public NetBigInteger Negate()
		{
			if (this.m_sign == 0)
			{
				return this;
			}
			return new NetBigInteger(-this.m_sign, this.m_magnitude, false);
		}

		public NetBigInteger Not()
		{
			return this.Inc().Negate();
		}

		public NetBigInteger Pow(int exp)
		{
			if (exp < 0)
			{
				throw new ArithmeticException("Negative exponent");
			}
			if (exp == 0)
			{
				return NetBigInteger.One;
			}
			if (this.m_sign == 0 || this.Equals(NetBigInteger.One))
			{
				return this;
			}
			NetBigInteger y = NetBigInteger.One;
			NetBigInteger z = this;
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

		private int Remainder(int m)
		{
			long acc = 0L;
			for (int pos = 0; pos < this.m_magnitude.Length; pos++)
			{
				long posVal = (long)((ulong)this.m_magnitude[pos]);
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
			int xyCmp = NetBigInteger.CompareNoLeadingZeroes(xStart, x, yStart, y);
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
					c = NetBigInteger.ShiftLeft(y, shift);
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
					if (cBitLength < xBitLength || NetBigInteger.CompareNoLeadingZeroes(xStart, x, cStart, c) >= 0)
					{
						NetBigInteger.Subtract(xStart, x, cStart, c);
						while (x[xStart] == 0)
						{
							if (++xStart == x.Length)
							{
								return x;
							}
						}
						xBitLength = 32 * (x.Length - xStart - 1) + NetBigInteger.BitLen(x[xStart]);
						if (xBitLength <= yBitLength)
						{
							if (xBitLength < yBitLength)
							{
								return x;
							}
							xyCmp = NetBigInteger.CompareNoLeadingZeroes(xStart, x, yStart, y);
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
						c = NetBigInteger.ShiftRightOneInPlace(cStart, c);
						cBitLength--;
					}
					else
					{
						c = NetBigInteger.ShiftRightInPlace(cStart, c, shift);
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

		public NetBigInteger Remainder(NetBigInteger n)
		{
			if (n.m_sign == 0)
			{
				throw new ArithmeticException("Division by zero error");
			}
			if (this.m_sign == 0)
			{
				return NetBigInteger.Zero;
			}
			if (n.m_magnitude.Length == 1)
			{
				int val = n.m_magnitude[0];
				if (val > 0)
				{
					if (val == 1)
					{
						return NetBigInteger.Zero;
					}
					int rem = this.Remainder(val);
					if (rem != 0)
					{
						return new NetBigInteger(this.m_sign, new int[] { rem }, false);
					}
					return NetBigInteger.Zero;
				}
			}
			if (NetBigInteger.CompareNoLeadingZeroes(0, this.m_magnitude, 0, n.m_magnitude) < 0)
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
				result = (int[])this.m_magnitude.Clone();
				result = this.Remainder(result, n.m_magnitude);
			}
			return new NetBigInteger(this.m_sign, result, true);
		}

		private int[] LastNBits(int n)
		{
			if (n < 1)
			{
				return NetBigInteger.ZeroMagnitude;
			}
			int numWords = (n + 32 - 1) / 32;
			numWords = Math.Min(numWords, this.m_magnitude.Length);
			int[] result = new int[numWords];
			Array.Copy(this.m_magnitude, this.m_magnitude.Length - numWords, result, 0, numWords);
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

		public NetBigInteger ShiftLeft(int n)
		{
			if (this.m_sign == 0 || this.m_magnitude.Length == 0)
			{
				return NetBigInteger.Zero;
			}
			if (n == 0)
			{
				return this;
			}
			if (n < 0)
			{
				return this.ShiftRight(-n);
			}
			NetBigInteger result = new NetBigInteger(this.m_sign, NetBigInteger.ShiftLeft(this.m_magnitude, n), true);
			if (this.m_numBits != -1)
			{
				result.m_numBits = ((this.m_sign > 0) ? this.m_numBits : (this.m_numBits + n));
			}
			if (this.m_numBitLength != -1)
			{
				result.m_numBitLength = this.m_numBitLength + n;
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

		public NetBigInteger ShiftRight(int n)
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
					Array.Copy(this.m_magnitude, 0, res, 0, res.Length);
				}
				else
				{
					int numBits2 = 32 - numBits;
					int magPos = this.m_magnitude.Length - 1 - numInts;
					for (int i = resultLength - 1; i >= 0; i--)
					{
						res[i] = (int)((uint)this.m_magnitude[magPos--] >> (numBits & 31));
						if (magPos >= 0)
						{
							res[i] |= this.m_magnitude[magPos] << numBits2;
						}
					}
				}
				return new NetBigInteger(this.m_sign, res, false);
			}
			if (this.m_sign >= 0)
			{
				return NetBigInteger.Zero;
			}
			return NetBigInteger.One.Negate();
		}

		public int SignValue
		{
			get
			{
				return this.m_sign;
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

		public NetBigInteger Subtract(NetBigInteger n)
		{
			if (n.m_sign == 0)
			{
				return this;
			}
			if (this.m_sign == 0)
			{
				return n.Negate();
			}
			if (this.m_sign != n.m_sign)
			{
				return this.Add(n.Negate());
			}
			int compare = NetBigInteger.CompareNoLeadingZeroes(0, this.m_magnitude, 0, n.m_magnitude);
			if (compare == 0)
			{
				return NetBigInteger.Zero;
			}
			NetBigInteger bigun;
			NetBigInteger lilun;
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
			return new NetBigInteger(this.m_sign * compare, NetBigInteger.doSubBigLil(bigun.m_magnitude, lilun.m_magnitude), true);
		}

		private static int[] doSubBigLil(int[] bigMag, int[] lilMag)
		{
			int[] res = (int[])bigMag.Clone();
			return NetBigInteger.Subtract(0, res, 0, lilMag);
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
			if (this.m_sign != 0)
			{
				int nBits = ((unsigned && this.m_sign > 0) ? this.BitLength : (this.BitLength + 1));
				int nBytes = NetBigInteger.GetByteLength(nBits);
				byte[] bytes = new byte[nBytes];
				int magIndex = this.m_magnitude.Length;
				int bytesIndex = bytes.Length;
				if (this.m_sign > 0)
				{
					while (magIndex > 1)
					{
						uint mag = (uint)this.m_magnitude[--magIndex];
						bytes[--bytesIndex] = (byte)mag;
						bytes[--bytesIndex] = (byte)(mag >> 8);
						bytes[--bytesIndex] = (byte)(mag >> 16);
						bytes[--bytesIndex] = (byte)(mag >> 24);
					}
					uint lastMag;
					for (lastMag = (uint)this.m_magnitude[0]; lastMag > 255U; lastMag >>= 8)
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
						uint mag2 = (uint)(~(uint)this.m_magnitude[--magIndex]);
						if (carry)
						{
							carry = (mag2 += 1U) == 0U;
						}
						bytes[--bytesIndex] = (byte)mag2;
						bytes[--bytesIndex] = (byte)(mag2 >> 8);
						bytes[--bytesIndex] = (byte)(mag2 >> 16);
						bytes[--bytesIndex] = (byte)(mag2 >> 24);
					}
					uint lastMag2 = (uint)this.m_magnitude[0];
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
			return NetBigInteger.ZeroEncoding;
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
			if (this.m_magnitude == null)
			{
				return "null";
			}
			if (this.m_sign == 0)
			{
				return "0";
			}
			StringBuilder sb = new StringBuilder();
			if (radix == 16)
			{
				sb.Append(this.m_magnitude[0].ToString("x"));
				for (int i = 1; i < this.m_magnitude.Length; i++)
				{
					sb.Append(this.m_magnitude[i].ToString("x8"));
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
				Stack S = new Stack();
				NetBigInteger bs = NetBigInteger.ValueOf((long)radix);
				NetBigInteger u = this.Abs();
				while (u.m_sign != 0)
				{
					NetBigInteger b = u.Mod(bs);
					if (b.m_sign == 0)
					{
						S.Push("0");
					}
					else
					{
						S.Push(b.m_magnitude[0].ToString("d"));
					}
					u = u.Divide(bs);
				}
				while (S.Count != 0)
				{
					sb.Append((string)S.Pop());
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
			if (this.m_sign == -1)
			{
				s = "-" + s;
			}
			return s;
		}

		private static NetBigInteger createUValueOf(ulong value)
		{
			int msw = (int)(value >> 32);
			int lsw = (int)value;
			if (msw != 0)
			{
				return new NetBigInteger(1, new int[] { msw, lsw }, false);
			}
			if (lsw != 0)
			{
				NetBigInteger i = new NetBigInteger(1, new int[] { lsw }, false);
				if ((lsw & -lsw) == lsw)
				{
					i.m_numBits = 1;
				}
				return i;
			}
			return NetBigInteger.Zero;
		}

		private static NetBigInteger createValueOf(long value)
		{
			if (value >= 0L)
			{
				return NetBigInteger.createUValueOf((ulong)value);
			}
			if (value == -9223372036854775808L)
			{
				return NetBigInteger.createValueOf(~value).Not();
			}
			return NetBigInteger.createValueOf(-value).Negate();
		}

		public static NetBigInteger ValueOf(long value)
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
					return NetBigInteger.Zero;
				case 1:
					return NetBigInteger.One;
				case 2:
					return NetBigInteger.Two;
				case 3:
					return NetBigInteger.Three;
				}
			}
			if (value == 10L)
			{
				return NetBigInteger.Ten;
			}
			IL_0049:
			return NetBigInteger.createValueOf(value);
		}

		public int GetLowestSetBit()
		{
			if (this.m_sign == 0)
			{
				return -1;
			}
			int w = this.m_magnitude.Length;
			while (--w > 0 && this.m_magnitude[w] == 0)
			{
			}
			int word = this.m_magnitude[w];
			int b = (((word & 65535) == 0) ? (((word & 16711680) == 0) ? 7 : 15) : (((word & 255) == 0) ? 23 : 31));
			while (b > 0 && word << b != -2147483648)
			{
				b--;
			}
			return (this.m_magnitude.Length - w) * 32 - (b + 1);
		}

		public bool TestBit(int n)
		{
			if (n < 0)
			{
				throw new ArithmeticException("Bit position must not be negative");
			}
			if (this.m_sign < 0)
			{
				return !this.Not().TestBit(n);
			}
			int wordNum = n / 32;
			if (wordNum >= this.m_magnitude.Length)
			{
				return false;
			}
			int word = this.m_magnitude[this.m_magnitude.Length - 1 - wordNum];
			return ((word >> n % 32) & 1) > 0;
		}

		private const long IMASK = 4294967295L;

		private const int BitsPerByte = 8;

		private const int BitsPerInt = 32;

		private const int BytesPerInt = 4;

		private static readonly ulong UIMASK = (ulong)(-1);

		private static readonly int[] ZeroMagnitude = new int[0];

		private static readonly byte[] ZeroEncoding = new byte[0];

		public static readonly NetBigInteger Zero = new NetBigInteger(0, NetBigInteger.ZeroMagnitude, false);

		public static readonly NetBigInteger One = NetBigInteger.createUValueOf(1UL);

		public static readonly NetBigInteger Two = NetBigInteger.createUValueOf(2UL);

		public static readonly NetBigInteger Three = NetBigInteger.createUValueOf(3UL);

		public static readonly NetBigInteger Ten = NetBigInteger.createUValueOf(10UL);

		private static readonly int chunk2 = 1;

		private static readonly NetBigInteger radix2 = NetBigInteger.ValueOf(2L);

		private static readonly NetBigInteger radix2E = NetBigInteger.radix2.Pow(NetBigInteger.chunk2);

		private static readonly int chunk10 = 19;

		private static readonly NetBigInteger radix10 = NetBigInteger.ValueOf(10L);

		private static readonly NetBigInteger radix10E = NetBigInteger.radix10.Pow(NetBigInteger.chunk10);

		private static readonly int chunk16 = 16;

		private static readonly NetBigInteger radix16 = NetBigInteger.ValueOf(16L);

		private static readonly NetBigInteger radix16E = NetBigInteger.radix16.Pow(NetBigInteger.chunk16);

		private int m_sign;

		private int[] m_magnitude;

		private int m_numBits = -1;

		private int m_numBitLength = -1;

		private long m_quote = -1L;
	}
}
