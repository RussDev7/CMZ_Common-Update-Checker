using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public static class DrawingTools
	{
		public static void SplitText(StringBuilder text, StringBuilder outputText, SpriteFont font, int maxWidth)
		{
			outputText.Length = 0;
			StringBuilder sb = new StringBuilder();
			StringBuilder sb2 = new StringBuilder();
			float sizeOfLine = (float)maxWidth;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				sb.Append(c);
				if (char.IsSeparator(c))
				{
					if (font.MeasureString(sb).X > sizeOfLine)
					{
						outputText.Append(sb2);
						outputText.Append('\n');
						sb.Remove(0, sb2.Length);
					}
					sb2.Length = 0;
					sb2.Append(sb);
				}
			}
			while (font.MeasureString(sb).X > sizeOfLine)
			{
				outputText.Append(sb2);
				outputText.Append('\n');
				sb.Remove(0, sb2.Length);
			}
			outputText.Append(sb);
		}

		public static Color ModulateColors(Color c1, Color c2)
		{
			int r = (int)(c1.R * c2.R / byte.MaxValue);
			int g = (int)(c1.G * c2.G / byte.MaxValue);
			int b = (int)(c1.B * c2.B / byte.MaxValue);
			int a = (int)(c1.A * c2.A / byte.MaxValue);
			return new Color(r, g, b, a);
		}

		public static Plane PlaneFromPointNormal(Vector3 point, Vector3 normal)
		{
			normal.Normalize();
			float d = -(normal.X * point.X + normal.Y * point.Y + normal.Z * point.Z);
			return new Plane(normal, d);
		}

		public static void GenerateComplementBasis(out Vector3 u, out Vector3 v, Vector3 w)
		{
			v = default(Vector3);
			u = default(Vector3);
			float invLength;
			if (Math.Abs(w.X) >= Math.Abs(w.Y))
			{
				invLength = 1f / (float)Math.Sqrt((double)(w.X * w.X + w.Z * w.Z));
				u.X = -w.Z * invLength;
				u.Y = 0f;
				u.Z = w.X * invLength;
				v.X = w.Y * u.Z;
				v.Y = w.Z * u.X - w.X * u.Z;
				v.Z = -w.Y * u.X;
				return;
			}
			invLength = 1f / (float)Math.Sqrt((double)(w.Y * w.Y + w.Z * w.Z));
			u.X = 0f;
			u.Y = w.Z * invLength;
			u.Z = -w.Y * invLength;
			v.X = w.Y * u.Z - w.Z * u.Y;
			v.Y = -w.X * u.Z;
			v.Z = w.X * u.Y;
		}

		public static int Intersects(this Ray ray, Capsule capsule, out float? t1, out float? t2)
		{
			float rayLen = ray.Direction.Length();
			t1 = null;
			t2 = null;
			Vector3 W = capsule.Segment.Direction;
			Vector3 U;
			Vector3 V;
			DrawingTools.GenerateComplementBasis(out U, out V, W);
			float rSqr = capsule.Radius * capsule.Radius;
			float extent = capsule.Segment.Length / 2f;
			Vector3 origin = ray.Position;
			Vector3 dir = ray.Direction / rayLen;
			float tolerance = 0.001f;
			Vector3 diff = origin - capsule.Segment.Center;
			Vector3 P = new Vector3(Vector3.Dot(U, diff), Vector3.Dot(V, diff), Vector3.Dot(W, diff));
			float dz = Vector3.Dot(W, dir);
			if (Math.Abs(dz) >= 1f - tolerance)
			{
				float radialSqrDist = rSqr - P.X * P.X - P.Y * P.Y;
				if (radialSqrDist < 0f)
				{
					return 0;
				}
				float zOffset = (float)Math.Sqrt((double)radialSqrDist) + extent;
				if (dz > 0f)
				{
					t1 = new float?(-P.Z - zOffset);
					t2 = new float?(-P.Z + zOffset);
					float? num = t1;
					float num2 = rayLen;
					t1 = ((num != null) ? new float?(num.GetValueOrDefault() / num2) : null);
					float? num3 = t2;
					float num4 = rayLen;
					t2 = ((num3 != null) ? new float?(num3.GetValueOrDefault() / num4) : null);
				}
				else
				{
					t1 = new float?(P.Z - zOffset);
					t2 = new float?(P.Z + zOffset);
					float? num5 = t1;
					float num6 = rayLen;
					t1 = ((num5 != null) ? new float?(num5.GetValueOrDefault() / num6) : null);
					float? num7 = t2;
					float num8 = rayLen;
					t2 = ((num7 != null) ? new float?(num7.GetValueOrDefault() / num8) : null);
				}
				return 2;
			}
			else
			{
				Vector3 D = new Vector3(Vector3.Dot(U, dir), Vector3.Dot(V, dir), dz);
				float a0 = P.X * P.X + P.Y * P.Y - rSqr;
				float a = P.X * D.X + P.Y * D.Y;
				float a2 = D.X * D.X + D.Y * D.Y;
				float discr = a * a - a0 * a2;
				if (discr < 0f)
				{
					return 0;
				}
				if (discr > tolerance)
				{
					float root = (float)Math.Sqrt((double)discr);
					float inv = 1f / a2;
					float tValue = (-a - root) * inv;
					float zValue = P.Z + tValue * D.Z;
					if (Math.Abs(zValue) <= extent)
					{
						if (t1 == null)
						{
							t1 = new float?(tValue / rayLen);
						}
						else
						{
							t2 = new float?(tValue / rayLen);
						}
					}
					tValue = (-a + root) * inv;
					zValue = P.Z + tValue * D.Z;
					if (Math.Abs(zValue) <= extent)
					{
						if (t1 == null)
						{
							t1 = new float?(tValue / rayLen);
						}
						else
						{
							t2 = new float?(tValue / rayLen);
						}
					}
					if (t1 != null && t2 != null)
					{
						return 2;
					}
				}
				else
				{
					float tValue = -a / a2;
					float zValue = P.Z + tValue * D.Z;
					if (Math.Abs(zValue) <= extent)
					{
						t1 = new float?(tValue / rayLen);
						return 1;
					}
				}
				float PZpE = P.Z + extent;
				a += PZpE * D.Z;
				a0 += PZpE * PZpE;
				discr = a * a - a0;
				if (discr > tolerance)
				{
					float root = (float)Math.Sqrt((double)discr);
					float tValue = -a - root;
					float zValue = P.Z + tValue * D.Z;
					if (zValue <= -extent)
					{
						if (t1 == null)
						{
							t1 = new float?(tValue / rayLen);
						}
						else
						{
							t2 = new float?(tValue / rayLen);
						}
						if (t1 != null && t2 != null)
						{
							float? num9 = t1;
							float? num10 = t2;
							if (num9.GetValueOrDefault() > num10.GetValueOrDefault() && ((num9 != null) & (num10 != null)))
							{
								float? save = t1;
								t1 = t2;
								t2 = save;
							}
							return 2;
						}
					}
					tValue = -a + root;
					zValue = P.Z + tValue * D.Z;
					if (zValue <= -extent)
					{
						if (t1 == null)
						{
							t1 = new float?(tValue / rayLen);
						}
						else
						{
							t2 = new float?(tValue / rayLen);
						}
						if (t1 != null && t2 != null)
						{
							float? num11 = t1;
							float? num12 = t2;
							if (num11.GetValueOrDefault() > num12.GetValueOrDefault() && ((num11 != null) & (num12 != null)))
							{
								float? save2 = t1;
								t1 = t2;
								t2 = save2;
							}
							return 2;
						}
					}
				}
				else if (Math.Abs(discr) <= tolerance)
				{
					float tValue = -a;
					float zValue = P.Z + tValue * D.Z;
					if (zValue <= -extent)
					{
						if (t1 == null)
						{
							t1 = new float?(tValue / rayLen);
						}
						else
						{
							t2 = new float?(tValue / rayLen);
						}
						if (t1 != null && t2 != null)
						{
							float? num13 = t1;
							float? num14 = t2;
							if (num13.GetValueOrDefault() > num14.GetValueOrDefault() && ((num13 != null) & (num14 != null)))
							{
								float? save3 = t1;
								t1 = t2;
								t2 = save3;
							}
							return 2;
						}
					}
				}
				a -= 2f * extent * D.Z;
				a0 -= 4f * extent * P.Z;
				discr = a * a - a0;
				if (discr > tolerance)
				{
					float root = (float)Math.Sqrt((double)discr);
					float tValue = -a - root;
					float zValue = P.Z + tValue * D.Z;
					if (zValue >= extent)
					{
						if (t1 == null)
						{
							t1 = new float?(tValue / rayLen);
						}
						else
						{
							t2 = new float?(tValue / rayLen);
						}
						if (t1 != null && t2 != null)
						{
							float? num15 = t1;
							float? num16 = t2;
							if (num15.GetValueOrDefault() > num16.GetValueOrDefault() && ((num15 != null) & (num16 != null)))
							{
								float? save4 = t1;
								t1 = t2;
								t2 = save4;
							}
							return 2;
						}
					}
					tValue = -a + root;
					zValue = P.Z + tValue * D.Z;
					if (zValue >= extent)
					{
						if (t1 == null)
						{
							t1 = new float?(tValue / rayLen);
						}
						else
						{
							t2 = new float?(tValue / rayLen);
						}
						if (t1 != null && t2 != null)
						{
							float? num17 = t1;
							float? num18 = t2;
							if (num17.GetValueOrDefault() > num18.GetValueOrDefault() && ((num17 != null) & (num18 != null)))
							{
								float? save5 = t1;
								t1 = t2;
								t2 = save5;
							}
							return 2;
						}
					}
				}
				else if (Math.Abs(discr) <= tolerance)
				{
					float tValue = -a;
					float zValue = P.Z + tValue * D.Z;
					if (zValue >= extent)
					{
						if (t1 == null)
						{
							t1 = new float?(tValue / rayLen);
						}
						else
						{
							t2 = new float?(tValue / rayLen);
						}
						if (t1 != null && t2 != null)
						{
							float? num19 = t1;
							float? num20 = t2;
							if (num19.GetValueOrDefault() > num20.GetValueOrDefault() && ((num19 != null) & (num20 != null)))
							{
								float? save6 = t1;
								t1 = t2;
								t2 = save6;
							}
							return 2;
						}
					}
				}
				int quant = 0;
				if (t1 != null)
				{
					quant++;
				}
				if (t2 != null)
				{
					quant++;
				}
				return quant;
			}
		}

		public static void SplitText(string text, StringBuilder outputText, SpriteFont font, int maxWidth)
		{
			outputText.Length = 0;
			StringBuilder sb = new StringBuilder();
			StringBuilder sb2 = new StringBuilder();
			float sizeOfLine = (float)maxWidth;
			foreach (char c in text)
			{
				sb.Append(c);
				if (char.IsSeparator(c))
				{
					if (font.MeasureString(sb).X > sizeOfLine)
					{
						outputText.Append(sb2);
						outputText.Append('\n');
						sb.Remove(0, sb2.Length);
					}
					sb2.Length = 0;
					sb2.Append(sb);
				}
			}
			while (font.MeasureString(sb).X > sizeOfLine)
			{
				outputText.Append(sb2);
				outputText.Append('\n');
				sb.Remove(0, sb2.Length);
			}
			outputText.Append(sb);
		}

		public static void DrawOutlinedText(this SpriteBatch spriteBatch, SpriteFont font, StringBuilder builder, Point location, Color textColor, Color outlineColor, int outlineWidth, float scale, float rotation, Vector2 orgin)
		{
			spriteBatch.DrawString(font, builder, new Vector2((float)location.X + (float)outlineWidth * scale, (float)location.Y + (float)outlineWidth * scale), outlineColor, rotation, orgin, scale, SpriteEffects.None, 1f);
			spriteBatch.DrawString(font, builder, new Vector2((float)location.X - (float)outlineWidth * scale, (float)location.Y - (float)outlineWidth * scale), outlineColor, rotation, orgin, scale, SpriteEffects.None, 1f);
			spriteBatch.DrawString(font, builder, new Vector2((float)location.X - (float)outlineWidth * scale, (float)location.Y + (float)outlineWidth * scale), outlineColor, rotation, orgin, scale, SpriteEffects.None, 1f);
			spriteBatch.DrawString(font, builder, new Vector2((float)location.X + (float)outlineWidth * scale, (float)location.Y - (float)outlineWidth * scale), outlineColor, rotation, orgin, scale, SpriteEffects.None, 1f);
			spriteBatch.DrawString(font, builder, new Vector2((float)location.X, (float)location.Y), textColor, rotation, orgin, scale, SpriteEffects.None, 1f);
		}

		public static void DrawOutlinedText(this SpriteBatch spriteBatch, SpriteFont font, string text, Point location, Color textColor, Color outlineColor, int outlineWidth, float scale, float rotation, Vector2 orgin)
		{
			spriteBatch.DrawString(font, text, new Vector2((float)location.X + (float)outlineWidth * scale, (float)location.Y + (float)outlineWidth * scale), outlineColor, rotation, orgin, scale, SpriteEffects.None, 1f);
			spriteBatch.DrawString(font, text, new Vector2((float)location.X - (float)outlineWidth * scale, (float)location.Y - (float)outlineWidth * scale), outlineColor, rotation, orgin, scale, SpriteEffects.None, 1f);
			spriteBatch.DrawString(font, text, new Vector2((float)location.X - (float)outlineWidth * scale, (float)location.Y + (float)outlineWidth * scale), outlineColor, rotation, orgin, scale, SpriteEffects.None, 1f);
			spriteBatch.DrawString(font, text, new Vector2((float)location.X + (float)outlineWidth * scale, (float)location.Y - (float)outlineWidth * scale), outlineColor, rotation, orgin, scale, SpriteEffects.None, 1f);
			spriteBatch.DrawString(font, text, new Vector2((float)location.X, (float)location.Y), textColor, rotation, orgin, scale, SpriteEffects.None, 1f);
		}

		public static void DrawOutlinedText(this SpriteBatch spriteBatch, SpriteFont font, StringBuilder builder, Point location, Color textColor, Color outlineColor, int outlineWidth)
		{
			spriteBatch.DrawString(font, builder, new Vector2((float)(location.X + outlineWidth), (float)(location.Y + outlineWidth)), outlineColor);
			spriteBatch.DrawString(font, builder, new Vector2((float)(location.X - outlineWidth), (float)(location.Y - outlineWidth)), outlineColor);
			spriteBatch.DrawString(font, builder, new Vector2((float)(location.X - outlineWidth), (float)(location.Y + outlineWidth)), outlineColor);
			spriteBatch.DrawString(font, builder, new Vector2((float)(location.X + outlineWidth), (float)(location.Y - outlineWidth)), outlineColor);
			spriteBatch.DrawString(font, builder, new Vector2((float)location.X, (float)location.Y), textColor);
		}

		public static void DrawOutlinedText(this SpriteBatch spriteBatch, SpriteFont font, string text, Point location, Color textColor, Color outlineColor, int outlineWidth)
		{
			spriteBatch.DrawString(font, text, new Vector2((float)(location.X + outlineWidth), (float)(location.Y + outlineWidth)), outlineColor);
			spriteBatch.DrawString(font, text, new Vector2((float)(location.X - outlineWidth), (float)(location.Y - outlineWidth)), outlineColor);
			spriteBatch.DrawString(font, text, new Vector2((float)(location.X - outlineWidth), (float)(location.Y + outlineWidth)), outlineColor);
			spriteBatch.DrawString(font, text, new Vector2((float)(location.X + outlineWidth), (float)(location.Y - outlineWidth)), outlineColor);
			spriteBatch.DrawString(font, text, new Vector2((float)location.X, (float)location.Y), textColor);
		}

		public static void DrawOutlinedText(this SpriteBatch spriteBatch, SpriteFont font, StringBuilder builder, Vector2 location, Color textColor, Color outlineColor, int outlineWidth, float scale, float rotation, Vector2 orgin)
		{
			spriteBatch.DrawString(font, builder, new Vector2(location.X + (float)outlineWidth * scale, location.Y + (float)outlineWidth * scale), outlineColor, rotation, orgin, scale, SpriteEffects.None, 1f);
			spriteBatch.DrawString(font, builder, new Vector2(location.X - (float)outlineWidth * scale, location.Y - (float)outlineWidth * scale), outlineColor, rotation, orgin, scale, SpriteEffects.None, 1f);
			spriteBatch.DrawString(font, builder, new Vector2(location.X - (float)outlineWidth * scale, location.Y + (float)outlineWidth * scale), outlineColor, rotation, orgin, scale, SpriteEffects.None, 1f);
			spriteBatch.DrawString(font, builder, new Vector2(location.X + (float)outlineWidth * scale, location.Y - (float)outlineWidth * scale), outlineColor, rotation, orgin, scale, SpriteEffects.None, 1f);
			spriteBatch.DrawString(font, builder, new Vector2(location.X, location.Y), textColor, rotation, orgin, scale, SpriteEffects.None, 1f);
		}

		public static void DrawOutlinedText(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 location, Color textColor, Color outlineColor, int outlineWidth, float scale, float rotation, Vector2 orgin)
		{
			spriteBatch.DrawString(font, text, new Vector2(location.X + (float)outlineWidth * scale, location.Y + (float)outlineWidth * scale), outlineColor, rotation, orgin, scale, SpriteEffects.None, 1f);
			spriteBatch.DrawString(font, text, new Vector2(location.X - (float)outlineWidth * scale, location.Y - (float)outlineWidth * scale), outlineColor, rotation, orgin, scale, SpriteEffects.None, 1f);
			spriteBatch.DrawString(font, text, new Vector2(location.X - (float)outlineWidth * scale, location.Y + (float)outlineWidth * scale), outlineColor, rotation, orgin, scale, SpriteEffects.None, 1f);
			spriteBatch.DrawString(font, text, new Vector2(location.X + (float)outlineWidth * scale, location.Y - (float)outlineWidth * scale), outlineColor, rotation, orgin, scale, SpriteEffects.None, 1f);
			spriteBatch.DrawString(font, text, new Vector2(location.X, location.Y), textColor, rotation, orgin, scale, SpriteEffects.None, 1f);
		}

		public static void DrawOutlinedText(this SpriteBatch spriteBatch, SpriteFont font, StringBuilder builder, Vector2 location, Color textColor, Color outlineColor, int outlineWidth)
		{
			spriteBatch.DrawString(font, builder, new Vector2(location.X + (float)outlineWidth, location.Y + (float)outlineWidth), outlineColor);
			spriteBatch.DrawString(font, builder, new Vector2(location.X - (float)outlineWidth, location.Y - (float)outlineWidth), outlineColor);
			spriteBatch.DrawString(font, builder, new Vector2(location.X - (float)outlineWidth, location.Y + (float)outlineWidth), outlineColor);
			spriteBatch.DrawString(font, builder, new Vector2(location.X + (float)outlineWidth, location.Y - (float)outlineWidth), outlineColor);
			spriteBatch.DrawString(font, builder, new Vector2(location.X, location.Y), textColor);
		}

		public static void DrawOutlinedText(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 location, Color textColor, Color outlineColor, int outlineWidth)
		{
			spriteBatch.DrawString(font, text, new Vector2(location.X + (float)outlineWidth, location.Y + (float)outlineWidth), outlineColor);
			spriteBatch.DrawString(font, text, new Vector2(location.X - (float)outlineWidth, location.Y - (float)outlineWidth), outlineColor);
			spriteBatch.DrawString(font, text, new Vector2(location.X - (float)outlineWidth, location.Y + (float)outlineWidth), outlineColor);
			spriteBatch.DrawString(font, text, new Vector2(location.X + (float)outlineWidth, location.Y - (float)outlineWidth), outlineColor);
			spriteBatch.DrawString(font, text, new Vector2(location.X, location.Y), textColor);
		}
	}
}
