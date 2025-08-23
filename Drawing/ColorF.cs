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
			float r = this.Red * factor;
			float g = this.Green * factor;
			float b = this.Blue * factor;
			return ColorF.FromARGB(this.Alpha, r, g, b);
		}

		public ColorF Saturate(float factor)
		{
			Angle hue;
			float s;
			float v;
			this.GetHSV(out hue, out s, out v);
			s *= factor;
			return ColorF.FromAHSV(this.Alpha, hue, s, v);
		}

		public ColorF AdjustBrightness(float factor)
		{
			float r = this.Red * factor;
			float g = this.Green * factor;
			float b = this.Blue * factor;
			return ColorF.FromARGB(this.Alpha, r, g, b);
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
			float m2 = ((brt <= 0.5f) ? (brt * (1f + sat)) : (brt + sat - brt * sat));
			float m3 = 2f * brt - m2;
			float blue;
			float green;
			float red;
			if (sat == 0f)
			{
				blue = brt;
				green = brt;
				red = brt;
			}
			else
			{
				float hueDegrees = hue.Degrees;
				red = ColorF.ColVal(m3, m2, hueDegrees + 120f);
				green = ColorF.ColVal(m3, m2, hueDegrees);
				blue = ColorF.ColVal(m3, m2, hueDegrees - 120f);
			}
			return ColorF.FromARGB(alpha, red, green, blue);
		}

		public static ColorF FromHSV(Angle hue, float sat, float brt)
		{
			return ColorF.FromAHSV(1f, hue, sat, brt);
		}

		public static ColorF FromAHSV(float alpha, Angle h, float sat, float brt)
		{
			float blue;
			float green;
			float red;
			if (sat == 0f)
			{
				blue = brt;
				green = brt;
				red = brt;
			}
			else
			{
				h.Normalize();
				float hue = h.Degrees;
				hue /= 60f;
				int i = (int)Math.Floor((double)hue);
				float f = hue - (float)i;
				float p = brt * (1f - sat);
				float q = brt * (1f - sat * f);
				float t = brt * (1f - sat * (1f - f));
				switch (i)
				{
				case 0:
					red = brt;
					green = t;
					blue = p;
					break;
				case 1:
					red = q;
					green = brt;
					blue = p;
					break;
				case 2:
					red = p;
					green = brt;
					blue = t;
					break;
				case 3:
					red = p;
					green = q;
					blue = brt;
					break;
				case 4:
					red = t;
					green = p;
					blue = brt;
					break;
				case 5:
					red = brt;
					green = p;
					blue = q;
					break;
				default:
					throw new Exception("Hue Out of Range");
				}
			}
			return ColorF.FromARGB(alpha, red, green, blue);
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
			float max = MathTools.Max(this.Red, this.Green, this.Blue);
			float min = MathTools.Min(this.Red, this.Green, this.Blue);
			l = (max + min) / 2f;
			float delta = max - min;
			if (delta == 0f)
			{
				s = 0f;
				h = Angle.FromDegrees(0f);
				return;
			}
			if (l < 0.5f)
			{
				s = delta / (max + min);
			}
			else
			{
				s = delta / (2f - (max + min));
			}
			float hdegs;
			if (this.Red == max)
			{
				hdegs = (this.Green - this.Blue) / delta;
			}
			else if (this.Green == max)
			{
				hdegs = 2f + (this.Blue - this.Red) / delta;
			}
			else if (this.Blue == max)
			{
				hdegs = 4f + (this.Red - this.Green) / delta;
			}
			else
			{
				hdegs = 0f;
			}
			hdegs *= 60f;
			if (hdegs < 0f)
			{
				hdegs += 360f;
			}
			h = Angle.FromDegrees(hdegs);
		}

		public void GetHSV(out Angle h, out float s, out float v)
		{
			float r = this.Red;
			float g = this.Green;
			float b = this.Blue;
			float max = MathTools.Max(r, g, b);
			float min = MathTools.Min(r, g, b);
			v = max;
			float delta = max - min;
			if (max == 0f || delta == 0f)
			{
				h = Angle.FromDegrees(0f);
				s = 0f;
				return;
			}
			s = delta / max;
			float hdegs;
			if (r == max)
			{
				hdegs = (g - b) / delta;
			}
			else if (g == max)
			{
				hdegs = 2f + (b - r) / delta;
			}
			else
			{
				hdegs = 4f + (r - g) / delta;
			}
			hdegs *= 60f;
			if (hdegs < 0f)
			{
				hdegs += 360f;
			}
			h = Angle.FromDegrees(hdegs);
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
			Color c = new Color(this.Red, this.Green, this.Blue, this.Alpha);
			return c;
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
			string[] split = strval.Split(new char[] { ',' });
			float r = float.Parse(split[0]);
			float g = float.Parse(split[1]);
			float b = float.Parse(split[2]);
			float a = float.Parse(split[3]);
			return ColorF.FromARGB(a, r, g, b);
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
