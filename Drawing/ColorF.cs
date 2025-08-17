using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	[Serializable]
	public struct ColorF
	{
		public float Red
		{
			get
			{
				return this._red;
			}
		}

		public float Green
		{
			get
			{
				return this._green;
			}
		}

		public float Blue
		{
			get
			{
				return this._blue;
			}
		}

		public float Alpha
		{
			get
			{
				return this._alpha;
			}
		}

		public static ColorF FromRGB(float r, float g, float b)
		{
			return new ColorF(1f, r, g, b);
		}

		public static ColorF FromARGB(float a, float r, float g, float b)
		{
			return new ColorF(a, r, g, b);
		}

		public static ColorF FromColor(Color col)
		{
			return new ColorF(col);
		}

		public Vector3 ToVector3()
		{
			return new Vector3(this._red, this._green, this._blue);
		}

		public Vector4 ToVector4()
		{
			return new Vector4(this._red, this._green, this._blue, this._alpha);
		}

		public static implicit operator Color(ColorF col)
		{
			return col.GetColor();
		}

		public static implicit operator ColorF(Color col)
		{
			return ColorF.FromColor(col);
		}

		public ColorF Brighten(float factor)
		{
			float num = this.Red * factor;
			float num2 = this.Green * factor;
			float num3 = this.Blue * factor;
			return ColorF.FromARGB(this.Alpha, num, num2, num3);
		}

		public ColorF Saturate(float factor)
		{
			Angle angle;
			float num;
			float num2;
			this.GetHSV(out angle, out num, out num2);
			num *= factor;
			return ColorF.FromAHSV(this.Alpha, angle, num, num2);
		}

		public ColorF AdjustBrightness(float factor)
		{
			float num = this.Red * factor;
			float num2 = this.Green * factor;
			float num3 = this.Blue * factor;
			return ColorF.FromARGB(this.Alpha, num, num2, num3);
		}

		private static float ColVal(float n1, float n2, float hue)
		{
			if (hue < 60f)
			{
				return n1 + (n2 - n1) * hue / 60f;
			}
			if (hue < 180f)
			{
				return n2;
			}
			if (hue < 240f)
			{
				return n1 + (n2 - n1) * (240f - hue) / 60f;
			}
			return n1;
		}

		public static ColorF FromHSL(Angle hue, float sat, float brt)
		{
			return ColorF.FromAHSL(1f, hue, sat, brt);
		}

		public static ColorF FromAHSL(float alpha, Angle hue, float sat, float brt)
		{
			hue.Normalize();
			float num = ((brt <= 0.5f) ? (brt * (1f + sat)) : (brt + sat - brt * sat));
			float num2 = 2f * brt - num;
			float num3;
			float num4;
			float num5;
			if (sat == 0f)
			{
				num3 = brt;
				num4 = brt;
				num5 = brt;
			}
			else
			{
				float num6 = hue.Degrees;
				num5 = ColorF.ColVal(num2, num, num6 + 120f);
				num4 = ColorF.ColVal(num2, num, num6);
				num3 = ColorF.ColVal(num2, num, num6 - 120f);
			}
			return ColorF.FromARGB(alpha, num5, num4, num3);
		}

		public static ColorF FromHSV(Angle hue, float sat, float brt)
		{
			return ColorF.FromAHSV(1f, hue, sat, brt);
		}

		public static ColorF FromAHSV(float alpha, Angle h, float sat, float brt)
		{
			float num;
			float num2;
			float num3;
			if (sat == 0f)
			{
				num = brt;
				num2 = brt;
				num3 = brt;
			}
			else
			{
				h.Normalize();
				float num4 = h.Degrees;
				num4 /= 60f;
				int num5 = (int)Math.Floor((double)num4);
				float num6 = num4 - (float)num5;
				float num7 = brt * (1f - sat);
				float num8 = brt * (1f - sat * num6);
				float num9 = brt * (1f - sat * (1f - num6));
				switch (num5)
				{
				case 0:
					num3 = brt;
					num2 = num9;
					num = num7;
					break;
				case 1:
					num3 = num8;
					num2 = brt;
					num = num7;
					break;
				case 2:
					num3 = num7;
					num2 = brt;
					num = num9;
					break;
				case 3:
					num3 = num7;
					num2 = num8;
					num = brt;
					break;
				case 4:
					num3 = num9;
					num2 = num7;
					num = brt;
					break;
				case 5:
					num3 = brt;
					num2 = num7;
					num = num8;
					break;
				default:
					throw new Exception("Hue Out of Range");
				}
			}
			return ColorF.FromARGB(alpha, num3, num2, num);
		}

		public static ColorF FromVector3(Vector3 vector)
		{
			return new ColorF(vector);
		}

		public static ColorF FromVector4(Vector4 vector)
		{
			return new ColorF(vector);
		}

		public static ColorF FromCMY(float c, float m, float y)
		{
			return ColorF.FromACMY(1f, c, m, y);
		}

		public static ColorF FromACMY(float a, float c, float m, float y)
		{
			return ColorF.FromARGB(a, 1f - c, 1f - m, 1f - y);
		}

		public static ColorF FromCMYK(float c, float m, float y, float k)
		{
			return ColorF.FromACMYK(1f, c, m, y, k);
		}

		public static ColorF FromACMYK(float a, float c, float m, float y, float k)
		{
			c = c * (1f - k) + k;
			m = m * (1f - k) + k;
			y = y * (1f - k) + k;
			return ColorF.FromACMY(a, c, m, y);
		}

		public void GetHSL(out Angle h, out float s, out float l)
		{
			float num = MathTools.Max(this.Red, this.Green, this.Blue);
			float num2 = MathTools.Min(this.Red, this.Green, this.Blue);
			l = (num + num2) / 2f;
			float num3 = num - num2;
			if (num3 == 0f)
			{
				s = 0f;
				h = Angle.FromDegrees(0f);
				return;
			}
			if (l < 0.5f)
			{
				s = num3 / (num + num2);
			}
			else
			{
				s = num3 / (2f - (num + num2));
			}
			float num4;
			if (this.Red == num)
			{
				num4 = (this.Green - this.Blue) / num3;
			}
			else if (this.Green == num)
			{
				num4 = 2f + (this.Blue - this.Red) / num3;
			}
			else if (this.Blue == num)
			{
				num4 = 4f + (this.Red - this.Green) / num3;
			}
			else
			{
				num4 = 0f;
			}
			num4 *= 60f;
			if (num4 < 0f)
			{
				num4 += 360f;
			}
			h = Angle.FromDegrees(num4);
		}

		public void GetHSV(out Angle h, out float s, out float v)
		{
			float red = this.Red;
			float green = this.Green;
			float blue = this.Blue;
			float num = MathTools.Max(red, green, blue);
			float num2 = MathTools.Min(red, green, blue);
			v = num;
			float num3 = num - num2;
			if (num == 0f || num3 == 0f)
			{
				h = Angle.FromDegrees(0f);
				s = 0f;
				return;
			}
			s = num3 / num;
			float num4;
			if (red == num)
			{
				num4 = (green - blue) / num3;
			}
			else if (green == num)
			{
				num4 = 2f + (blue - red) / num3;
			}
			else
			{
				num4 = 4f + (red - green) / num3;
			}
			num4 *= 60f;
			if (num4 < 0f)
			{
				num4 += 360f;
			}
			h = Angle.FromDegrees(num4);
		}

		public void GetCMY(out float c, out float m, out float y)
		{
			c = 1f - this.Red;
			m = 1f - this.Green;
			y = 1f - this.Blue;
		}

		public void GetCMYK(out float c, out float m, out float y, out float k)
		{
			c = 1f - this.Red;
			m = 1f - this.Green;
			y = 1f - this.Blue;
			k = 1f;
			if (c < k)
			{
				k = c;
			}
			if (m < k)
			{
				k = m;
			}
			if (y < k)
			{
				k = y;
			}
			c = (c - k) / (1f - k);
			m = (m - k) / (1f - k);
			y = (y - k) / (1f - k);
		}

		public Color GetColor()
		{
			Color color = new Color(this.Red, this.Green, this.Blue, this.Alpha);
			return color;
		}

		public static ColorF Lerp(ColorF a, ColorF b, float factor)
		{
			return new ColorF(a.Alpha * factor + b.Alpha * (1f - factor), a.Red * factor + b.Red * (1f - factor), a.Green * factor + b.Green * (1f - factor), a.Blue * factor + b.Blue * (1f - factor));
		}

		private ColorF(Color col)
		{
			this._red = (float)col.R / 255f;
			this._green = (float)col.G / 255f;
			this._blue = (float)col.B / 255f;
			this._alpha = (float)col.A / 255f;
		}

		private ColorF(float a, float r, float g, float b)
		{
			this._red = r;
			this._green = g;
			this._blue = b;
			this._alpha = a;
		}

		private ColorF(Vector3 vector)
		{
			this._red = vector.X;
			this._green = vector.Y;
			this._blue = vector.Z;
			this._alpha = 1f;
		}

		private ColorF(Vector4 vector)
		{
			this._red = vector.X;
			this._green = vector.Y;
			this._blue = vector.Z;
			this._alpha = vector.W;
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				this.Red.ToString(),
				",",
				this.Green.ToString(),
				",",
				this.Blue.ToString(),
				",",
				this.Alpha.ToString()
			});
		}

		public static ColorF Parse(string strval)
		{
			string[] array = strval.Split(new char[] { ',' });
			float num = float.Parse(array[0]);
			float num2 = float.Parse(array[1]);
			float num3 = float.Parse(array[2]);
			float num4 = float.Parse(array[3]);
			return ColorF.FromARGB(num4, num, num2, num3);
		}

		public override bool Equals(object obj)
		{
			return obj.GetType() == typeof(ColorF) && this == (ColorF)obj;
		}

		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}

		public static bool operator ==(ColorF a, ColorF b)
		{
			throw new NotImplementedException();
		}

		public static bool operator !=(ColorF a, ColorF b)
		{
			throw new NotImplementedException();
		}

		private float _red;

		private float _green;

		private float _blue;

		private float _alpha;
	}
}
