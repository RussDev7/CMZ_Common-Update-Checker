using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	public static class ColorTools
	{
		public static Color Blend(Color c1, Color c2, float factor)
		{
			int num = (int)Math.Round((double)((float)c1.A * (1f - factor) + (float)c2.A * factor));
			int num2 = (int)Math.Round((double)((float)c1.R * (1f - factor) + (float)c2.R * factor));
			int num3 = (int)Math.Round((double)((float)c1.G * (1f - factor) + (float)c2.G * factor));
			int num4 = (int)Math.Round((double)((float)c1.B * (1f - factor) + (float)c2.B * factor));
			return new Color(num2, num3, num4, num);
		}

		public static Color FromAHSB(Angle h, float s, float b)
		{
			return ColorTools.FromAHSB(1f, h, s, b);
		}

		public static Color FromAHSL(Angle h, float s, float b)
		{
			return ColorTools.FromAHSL(1f, h, s, b);
		}

		public static Color FromAHSV(Angle h, float s, float b)
		{
			return ColorTools.FromAHSV(1f, h, s, b);
		}

		public static Color FromAHSB(float alpha, Angle hue, float sat, float brt)
		{
			return ColorTools.FromAHSL(alpha, hue, sat, brt);
		}

		public static Color FromAHSL(float alpha, Angle hue, float sat, float brt)
		{
			return ColorF.FromAHSL(alpha, hue, sat, brt).GetColor();
		}

		public static Color FromCMY(float c, float m, float y)
		{
			return ColorTools.FromACMY(1f, c, m, y);
		}

		public static Color FromACMY(float alpha, float c, float m, float y)
		{
			return ColorF.FromACMY(alpha, c, m, y).GetColor();
		}

		public static Color FromCMYK(float alpha, float c, float m, float y, float k)
		{
			return ColorTools.FromACMYK(1f, c, m, y, k);
		}

		public static Color FromACMYK(float alpha, float c, float m, float y, float k)
		{
			return ColorF.FromACMYK(alpha, c, m, y, k).GetColor();
		}

		public static void ToCMYK(Color color, out float c, out float m, out float y, out float k)
		{
			float num;
			ColorTools.ToACMYK(color, out num, out c, out m, out y, out k);
		}

		public static void ToCMY(Color color, out float c, out float m, out float y)
		{
			ColorF.FromColor(color).GetCMY(out c, out m, out y);
		}

		public static void ToACMY(Color color, out float alpha, out float c, out float m, out float y)
		{
			alpha = (float)color.A / 255f;
			ColorF.FromColor(color).GetCMY(out c, out m, out y);
		}

		public static void ToACMYK(Color color, out float alpha, out float c, out float m, out float y, out float k)
		{
			alpha = (float)color.A / 255f;
			ColorF.FromColor(color).GetCMYK(out c, out m, out y, out k);
		}

		public static void ToAHSL(Color color, out float alpha, out Angle h, out float s, out float l)
		{
			ColorF colorF = ColorF.FromColor(color);
			alpha = (float)color.A / 255f;
			colorF.GetHSL(out h, out s, out l);
		}

		public static void ToAHSV(Color color, out float alpha, out Angle h, out float s, out float v)
		{
			alpha = (float)color.A / 255f;
			ColorF.FromColor(color).GetHSV(out h, out s, out v);
		}

		public static Color FromAHSV(float alpha, Angle hue, float sat, float brt)
		{
			return ColorF.FromAHSV(alpha, hue, sat, brt).GetColor();
		}

		public static Color GetRandomColor(Random rnd)
		{
			return new Color(rnd.Next(255), rnd.Next(255), rnd.Next(255));
		}

		public static Color GetRandomColor(Random rnd, float saturation, float brightness)
		{
			Angle angle = Angle.FromRevolutions((float)rnd.NextDouble());
			return ColorTools.FromAHSV(angle, saturation, brightness);
		}

		public static Color Brighten(Color c, float factor)
		{
			return c.Brighten(factor);
		}

		public static Color Saturate(Color c, float factor)
		{
			return c.Saturate(factor);
		}
	}
}
