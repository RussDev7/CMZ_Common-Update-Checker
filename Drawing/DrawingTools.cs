using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public static class DrawingTools
	{
		public static Vector3 Compute(this Ray ray, float t)
		{
			return ray.Position + ray.Direction * t;
		}

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

		public static void DrawLine(this GraphicsDevice graphicsDevice, Matrix view, Matrix projection, LineF3D line, Color color)
		{
			if (DrawingTools._wireFrameVerts == null)
			{
				DrawingTools._wireFrameVerts = new VertexPositionColor[2];
			}
			DrawingTools._wireFrameVerts[0].Color = color;
			DrawingTools._wireFrameVerts[0].Position = line.Start;
			DrawingTools._wireFrameVerts[1].Color = Color.White;
			DrawingTools._wireFrameVerts[1].Position = line.End;
			if (DrawingTools._wireFrameEffect == null)
			{
				DrawingTools._wireFrameEffect = new BasicEffect(graphicsDevice);
			}
			DrawingTools._wireFrameEffect.LightingEnabled = false;
			DrawingTools._wireFrameEffect.TextureEnabled = false;
			DrawingTools._wireFrameEffect.VertexColorEnabled = true;
			DrawingTools._wireFrameEffect.Projection = projection;
			DrawingTools._wireFrameEffect.View = view;
			DrawingTools._wireFrameEffect.World = Matrix.Identity;
			for (int i = 0; i < DrawingTools._wireFrameEffect.CurrentTechnique.Passes.Count; i++)
			{
				EffectPass pass = DrawingTools._wireFrameEffect.CurrentTechnique.Passes[i];
				pass.Apply();
				graphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, DrawingTools._wireFrameVerts, 0, 1);
			}
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

		public static void ExtractData(this Model mdl, List<Vector3> vtcs, List<TriangleVertexIndices> idcs, bool includeNoncoll)
		{
			Matrix i = Matrix.Identity;
			foreach (ModelMesh mm in mdl.Meshes)
			{
				i = mm.ParentBone.GetAbsoluteTransform();
				mm.ExtractModelMeshData(ref i, vtcs, idcs, includeNoncoll);
			}
		}

		public static Matrix GetAbsoluteTransform(this ModelBone bone)
		{
			if (bone == null)
			{
				return Matrix.Identity;
			}
			return bone.Transform * bone.Parent.GetAbsoluteTransform();
		}

		public static List<Triangle3D> ExtractModelTris(this Model model, bool includeNoncoll)
		{
			List<Triangle3D> polys = new List<Triangle3D>();
			Matrix[] transforms = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(transforms);
			for (int i = 0; i < model.Meshes.Count; i++)
			{
				ModelMesh mesh = model.Meshes[i];
				List<Vector3> verts = new List<Vector3>();
				List<TriangleVertexIndices> triangles = new List<TriangleVertexIndices>();
				mesh.ExtractModelMeshData(ref transforms[mesh.ParentBone.Index], verts, triangles, includeNoncoll);
				foreach (TriangleVertexIndices triv in triangles)
				{
					Triangle3D tri = new Triangle3D(verts[triv.A], verts[triv.B], verts[triv.C]);
					polys.Add(tri);
				}
			}
			return polys;
		}

		public static void ExtractModelMeshData(this ModelMesh mm, ref Matrix xform, List<Vector3> vertices, List<TriangleVertexIndices> indices, bool includeNoncoll)
		{
			foreach (ModelMeshPart mmp in mm.MeshParts)
			{
				if (!includeNoncoll)
				{
					EffectAnnotation annot = mmp.Effect.CurrentTechnique.Annotations["collide"];
					if (annot != null && !annot.GetValueBoolean())
					{
						Console.WriteLine("Ignoring model mesh part {1} because it's set to not collide.", mm.Name);
						continue;
					}
				}
				mmp.ExtractModelMeshPartData(ref xform, vertices, indices);
			}
		}

		public static void ExtractModelMeshPartData(this ModelMeshPart meshPart, ref Matrix transform, List<Vector3> vertices, List<TriangleVertexIndices> indices)
		{
			int offset = vertices.Count;
			VertexDeclaration declaration = meshPart.VertexBuffer.VertexDeclaration;
			VertexElement[] vertexElements = declaration.GetVertexElements();
			VertexElement vertexPosition = default(VertexElement);
			foreach (VertexElement vert in vertexElements)
			{
				if (vert.VertexElementUsage == VertexElementUsage.Position && vert.VertexElementFormat == VertexElementFormat.Vector3)
				{
					vertexPosition = vert;
					break;
				}
			}
			if (vertexPosition.VertexElementUsage != VertexElementUsage.Position || vertexPosition.VertexElementFormat != VertexElementFormat.Vector3)
			{
				throw new Exception("Model uses unsupported vertex format!");
			}
			Vector3[] allVertex = new Vector3[meshPart.NumVertices];
			meshPart.VertexBuffer.GetData<Vector3>(meshPart.VertexOffset * declaration.VertexStride + vertexPosition.Offset, allVertex, 0, meshPart.NumVertices, declaration.VertexStride);
			for (int i = 0; i != allVertex.Length; i++)
			{
				Vector3.Transform(ref allVertex[i], ref transform, out allVertex[i]);
			}
			vertices.AddRange(allVertex);
			if (meshPart.IndexBuffer.IndexElementSize != IndexElementSize.SixteenBits)
			{
				throw new Exception("Model uses 32-bit indices, which are not supported.");
			}
			short[] indexElements = new short[meshPart.PrimitiveCount * 3];
			meshPart.IndexBuffer.GetData<short>(meshPart.StartIndex * 2, indexElements, 0, meshPart.PrimitiveCount * 3);
			TriangleVertexIndices[] tvi = new TriangleVertexIndices[meshPart.PrimitiveCount];
			for (int j = 0; j != tvi.Length; j++)
			{
				tvi[j].A = (int)indexElements[j * 3] + offset;
				tvi[j].B = (int)indexElements[j * 3 + 1] + offset;
				tvi[j].C = (int)indexElements[j * 3 + 2] + offset;
			}
			indices.AddRange(tvi);
		}

		public static Plane PlaneFromPointNormal(Vector3 point, Vector3 normal)
		{
			normal.Normalize();
			float d = -(normal.X * point.X + normal.Y * point.Y + normal.Z * point.Z);
			return new Plane(normal, d);
		}

		public static Point CenterOf(this Rectangle r)
		{
			return new Point(r.X + r.Width / 2, r.Y + r.Height / 2);
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

		public static RectangleF GetBoundingRect(Vector2[] points)
		{
			Vector2 br;
			Vector2 tl = (br = points[0]);
			for (int i = 1; i < points.Length; i++)
			{
				if (points[i].X < tl.X)
				{
					tl.X = points[i].X;
				}
				if (points[i].Y < tl.Y)
				{
					tl.Y = points[i].Y;
				}
				if (points[i].X > br.X)
				{
					br.X = points[i].X;
				}
				if (points[i].Y > br.Y)
				{
					br.Y = points[i].Y;
				}
			}
			return new RectangleF(tl.X, tl.Y, br.X - tl.X, br.Y - tl.Y);
		}

		public static double Distance(this Point p1, Point p2)
		{
			int difx = p1.X - p2.X;
			int dify = p1.Y - p2.Y;
			return Math.Sqrt((double)(difx * difx + dify * dify));
		}

		public static int DistanceSquared(this Point p1, Point p2)
		{
			int difx = p1.X - p2.X;
			int dify = p1.Y - p2.Y;
			return difx * difx + dify * dify;
		}

		public static double DistanceSquared(this Vector2 p1, Vector2 p2)
		{
			float difx = p1.X - p2.X;
			float dify = p1.Y - p2.Y;
			return (double)(difx * difx + dify * dify);
		}

		public static double Distance(this Vector2 p1, Vector2 p2)
		{
			float difx = p1.X - p2.X;
			float dify = p1.Y - p2.Y;
			return Math.Sqrt((double)(difx * difx + dify * dify));
		}

		public static bool PointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
		{
			Vector2 v0 = c - a;
			Vector2 v = b - a;
			Vector2 v2 = p - a;
			float dot0 = Vector2.Dot(v0, v0);
			float dot = Vector2.Dot(v0, v);
			float dot2 = Vector2.Dot(v0, v2);
			float dot3 = Vector2.Dot(v, v);
			float dot4 = Vector2.Dot(v, v2);
			float invDenom = 1f / (dot0 * dot3 - dot * dot);
			float u = (dot3 * dot2 - dot * dot4) * invDenom;
			float v3 = (dot0 * dot4 - dot * dot2) * invDenom;
			return u >= 0f && v3 >= 0f && u + v3 <= 1f;
		}

		public static bool PointInTriangle(Point vert1, Point vert2, Point vert3, Point point)
		{
			Vector2 p = new Vector2((float)point.X, (float)point.Y);
			Vector2 a = new Vector2((float)vert1.X, (float)vert1.Y);
			Vector2 b = new Vector2((float)vert2.X, (float)vert2.Y);
			Vector2 c = new Vector2((float)vert3.X, (float)vert3.Y);
			return DrawingTools.PointInTriangle(a, b, c, p);
		}

		public static RectangleF FindBounds(IList<Vector2> points)
		{
			float minx = float.MaxValue;
			float miny = float.MaxValue;
			float maxx = float.MinValue;
			float maxy = float.MinValue;
			for (int i = 0; i < points.Count; i++)
			{
				Vector2 p = points[i];
				minx = Math.Min(p.X, minx);
				miny = Math.Min(p.Y, miny);
				maxx = Math.Max(p.X, maxx);
				maxy = Math.Max(p.Y, maxy);
			}
			return new RectangleF(minx, miny, maxx - minx, maxy - miny);
		}

		public static int[] GetConvexHullIndices(IList<Vector2> points)
		{
			if (points.Count <= 3)
			{
				int[] ret = new int[points.Count];
				for (int i = 0; i < points.Count; i++)
				{
					ret[i] = i;
				}
				return ret;
			}
			LinkedList<int> aboveList = new LinkedList<int>();
			LinkedList<int> belowList = new LinkedList<int>();
			LinkedList<int> hullpoints = new LinkedList<int>();
			int leftMost = -1;
			float leftVal = float.MaxValue;
			int rightMost = -1;
			float rightVal = float.MinValue;
			for (int j = 0; j < points.Count; j++)
			{
				Vector2 p = points[j];
				if (p.X <= leftVal)
				{
					leftMost = j;
					leftVal = p.X;
				}
				if (p.X >= rightVal)
				{
					rightMost = j;
					rightVal = p.X;
				}
			}
			Vector2 a = points[leftMost];
			Vector2 b = points[rightMost];
			Vector2 lineV = new Vector2(b.X - a.X, b.Y - a.Y);
			float biggest = float.MinValue;
			LinkedListNode<int> bigNode = null;
			float smallest = float.MaxValue;
			LinkedListNode<int> smallNode = null;
			for (int k = 0; k < points.Count; k++)
			{
				if (k != leftMost && k != rightMost)
				{
					Vector2 c = points[k];
					Vector2 v = new Vector2(a.X - c.X, a.Y - c.Y);
					float dist = lineV.Cross(v);
					if (dist > 0f)
					{
						LinkedListNode<int> node = aboveList.AddLast(k);
						if (dist > biggest)
						{
							biggest = dist;
							bigNode = node;
						}
					}
					if (dist < 0f)
					{
						LinkedListNode<int> node2 = belowList.AddLast(k);
						if (dist < smallest)
						{
							smallest = dist;
							smallNode = node2;
						}
					}
				}
			}
			LinkedListNode<int> leftNode = hullpoints.AddFirst(leftMost);
			LinkedListNode<int> rightNode = hullpoints.AddLast(rightMost);
			if (bigNode != null)
			{
				aboveList.Remove(bigNode);
			}
			if (smallNode != null)
			{
				belowList.Remove(smallNode);
			}
			if (bigNode != null)
			{
				hullpoints.AddAfter(leftNode, bigNode);
			}
			if (smallNode != null)
			{
				hullpoints.AddAfter(rightNode, smallNode);
			}
			if (bigNode != null)
			{
				DrawingTools.QuickHull(points, aboveList, hullpoints, rightNode, leftNode, bigNode);
			}
			if (smallNode != null)
			{
				DrawingTools.QuickHull(points, belowList, hullpoints, leftNode, rightNode, smallNode);
			}
			int[] outpoints = new int[hullpoints.Count];
			hullpoints.CopyTo(outpoints, 0);
			return outpoints;
		}

		private static void QuickHull(IList<Vector2> points, LinkedList<int> pointList, LinkedList<int> hull, LinkedListNode<int> aNode, LinkedListNode<int> bNode, LinkedListNode<int> cNode)
		{
			Vector2 a = points[aNode.Value];
			Vector2 b = points[bNode.Value];
			Vector2 c = points[cNode.Value];
			Vector2 v0 = new Vector2(c.X - a.X, c.Y - a.Y);
			Vector2 v = new Vector2(b.X - a.X, b.Y - a.Y);
			Vector2 v2 = new Vector2(c.X - b.X, c.Y - b.Y);
			float dot0 = Vector2.Dot(v0, v0);
			float dot = Vector2.Dot(v0, v);
			float dot2 = Vector2.Dot(v, v);
			float invDenom = 1f / (dot0 * dot2 - dot * dot);
			LinkedList<int> acPoints = new LinkedList<int>();
			LinkedList<int> bcPoints = new LinkedList<int>();
			LinkedListNode<int> highAC = null;
			LinkedListNode<int> highBC = null;
			float maxBC = float.MinValue;
			float maxAC = float.MinValue;
			foreach (int index in pointList)
			{
				Vector2 p = points[index];
				Vector2 v3 = new Vector2(p.X - a.X, p.Y - a.Y);
				float dot3 = Vector2.Dot(v0, v3);
				float dot4 = Vector2.Dot(v, v3);
				float u = (dot2 * dot3 - dot * dot4) * invDenom;
				float v4 = (dot0 * dot4 - dot * dot3) * invDenom;
				if (u >= 0f)
				{
					if (v4 <= 0f)
					{
						LinkedListNode<int> node = acPoints.AddLast(index);
						float dist = v0.Cross(v3);
						if (dist < 0f)
						{
							dist = 0f;
						}
						if (dist > maxAC)
						{
							maxAC = dist;
							highAC = node;
						}
					}
					else if (u + v4 >= 1f)
					{
						Vector2 v5 = new Vector2(b.X - p.X, b.Y - p.Y);
						LinkedListNode<int> node2 = bcPoints.AddLast(index);
						float dist2 = v2.Cross(v5);
						if (dist2 < 0f)
						{
							dist2 = 0f;
						}
						if (dist2 > maxBC)
						{
							maxBC = dist2;
							highBC = node2;
						}
					}
				}
			}
			if (highBC != null)
			{
				bcPoints.Remove(highBC);
				hull.AddAfter(bNode, highBC);
			}
			if (highAC != null)
			{
				acPoints.Remove(highAC);
				hull.AddAfter(cNode, highAC);
			}
			if (highBC != null)
			{
				DrawingTools.QuickHull(points, bcPoints, hull, cNode, bNode, highBC);
			}
			if (highAC != null)
			{
				DrawingTools.QuickHull(points, acPoints, hull, aNode, cNode, highAC);
			}
		}

		public static Vector2[] GetConvexHull(IList<Vector2> points)
		{
			int[] result = DrawingTools.GetConvexHullIndices(points);
			Vector2[] reta = new Vector2[result.Length];
			for (int i = 0; i < result.Length; i++)
			{
				reta[i] = points[result[i]];
			}
			return reta;
		}

		public static Color ModulateColors(Color c1, Color c2)
		{
			int r = (int)(c1.R * c2.R / byte.MaxValue);
			int g = (int)(c1.G * c2.G / byte.MaxValue);
			int b = (int)(c1.B * c2.B / byte.MaxValue);
			int a = (int)(c1.A * c2.A / byte.MaxValue);
			return new Color(r, g, b, a);
		}

		public static void Decompose(this Matrix m, out Vector3 translation, out Vector3 scale, out Quaternion rotation)
		{
			Matrix A = m;
			float[][] Q = DrawingTools.MakeArrayMatrix();
			float[][] S = DrawingTools.MakeArrayMatrix();
			float[][] U = DrawingTools.MakeArrayMatrix();
			float[][] AA = DrawingTools.ToArrayMatrix(ref A);
			translation = new Vector3(AA[0][3], AA[1][3], AA[2][3]);
			float det = DrawingTools.polar_decomp(AA, Q, S);
			float outDet;
			if ((double)det < 0.0)
			{
				DrawingTools.mat_copy_eq_neg(Q, Q, 3);
				outDet = -1f;
			}
			else
			{
				outDet = 1f;
			}
			rotation = DrawingTools.Qt_FromMatrix(Q);
			scale = DrawingTools.spect_decomp(S, U);
			Quaternion scaleRotation = DrawingTools.Qt_FromMatrix(U);
			Quaternion p = DrawingTools.snuggle(scaleRotation, ref scale);
			scaleRotation = DrawingTools.Qt_Mul(scaleRotation, p);
			if (outDet == -1f)
			{
				rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, 3.1415927f);
			}
		}

		private static float[][] ToArrayMatrix(ref Matrix m)
		{
			return new float[][]
			{
				new float[] { m.M11, m.M21, m.M31, m.M41 },
				new float[] { m.M12, m.M22, m.M32, m.M42 },
				new float[] { m.M13, m.M23, m.M33, m.M43 },
				new float[] { m.M14, m.M24, m.M34, m.M44 }
			};
		}

		private static float[][] MakeArrayMatrix()
		{
			float[][] f = new float[4][];
			float[][] array = f;
			int num = 0;
			float[] array2 = new float[4];
			array2[0] = 1f;
			array[num] = array2;
			float[][] array3 = f;
			int num2 = 1;
			float[] array4 = new float[4];
			array4[1] = 1f;
			array3[num2] = array4;
			float[][] array5 = f;
			int num3 = 2;
			float[] array6 = new float[4];
			array6[2] = 1f;
			array5[num3] = array6;
			f[3] = new float[] { 0f, 0f, 0f, 1f };
			return f;
		}

		private static void mat_pad(float[][] A)
		{
			A[3][0] = (A[0][3] = (A[3][1] = (A[1][3] = (A[3][2] = (A[2][3] = 0f)))));
			A[3][3] = 1f;
		}

		private static void mat_copy_eq(float[][] C, float[][] A, int n)
		{
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = A[i][j];
				}
			}
		}

		private static void mat_copy_eq_neg(float[][] C, float[][] A, int n)
		{
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = -A[i][j];
				}
			}
		}

		private static void mat_copy_minuseq(float[][] C, float[][] A, int n)
		{
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] -= A[i][j];
				}
			}
		}

		private static void mat_tpose(float[][] AT, float[][] A, int n)
		{
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					AT[i][j] = A[j][i];
				}
			}
		}

		private static void mat_binop(float[][] C, float sa, float[][] A, float sb, float[][] B, int n)
		{
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					C[i][j] = sa * A[i][j] + sb * B[i][j];
				}
			}
		}

		private static void mat_mult(float[][] A, float[][] B, float[][] AB)
		{
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					AB[i][j] = A[i][0] * B[0][j] + A[i][1] * B[1][j] + A[i][2] * B[2][j];
				}
			}
		}

		private static float vdot(float[] va, float[] vb)
		{
			return va[0] * vb[0] + va[1] * vb[1] + va[2] * vb[2];
		}

		private static void vcross(float[] va, float[] vb, float[] v)
		{
			v[0] = va[1] * vb[2] - va[2] * vb[1];
			v[1] = va[2] * vb[0] - va[0] * vb[2];
			v[2] = va[0] * vb[1] - va[1] * vb[0];
		}

		private static void adjoint_transpose(float[][] M, float[][] MadjT)
		{
			DrawingTools.vcross(M[1], M[2], MadjT[0]);
			DrawingTools.vcross(M[2], M[0], MadjT[1]);
			DrawingTools.vcross(M[0], M[1], MadjT[2]);
		}

		internal static Quaternion Qt_Conj(Quaternion q)
		{
			return new Quaternion
			{
				X = -q.X,
				Y = -q.Y,
				Z = -q.Z,
				W = q.W
			};
		}

		internal static Quaternion Qt_Mul(Quaternion qL, Quaternion qR)
		{
			return new Quaternion
			{
				W = qL.W * qR.W - qL.X * qR.X - qL.Y * qR.Y - qL.Z * qR.Z,
				X = qL.W * qR.X + qL.X * qR.W + qL.Y * qR.Z - qL.Z * qR.Y,
				Y = qL.W * qR.Y + qL.Y * qR.W + qL.Z * qR.X - qL.X * qR.Z,
				Z = qL.W * qR.Z + qL.Z * qR.W + qL.X * qR.Y - qL.Y * qR.X
			};
		}

		private static Quaternion Qt_Scale(Quaternion q, float w)
		{
			return new Quaternion
			{
				W = q.W * w,
				X = q.X * w,
				Y = q.Y * w,
				Z = q.Z * w
			};
		}

		private static Quaternion Qt_FromMatrix(float[][] mat)
		{
			Quaternion qu = default(Quaternion);
			float tr = mat[0][0] + mat[1][1] + mat[2][2];
			if ((double)tr >= 0.0)
			{
				float s = (float)Math.Sqrt((double)(tr + mat[3][3]));
				qu.W = s * 0.5f;
				s = 0.5f / s;
				qu.X = (mat[2][1] - mat[1][2]) * s;
				qu.Y = (mat[0][2] - mat[2][0]) * s;
				qu.Z = (mat[1][0] - mat[0][1]) * s;
			}
			else
			{
				int h = 0;
				if (mat[1][1] > mat[0][0])
				{
					h = 1;
				}
				if (mat[2][2] > mat[h][h])
				{
					h = 2;
				}
				switch (h)
				{
				case 0:
				{
					int I = 0;
					int J = 1;
					int K = 2;
					float s = (float)Math.Sqrt((double)(mat[I][I] - (mat[J][J] + mat[K][K]) + mat[3][3]));
					qu.X = s * 0.5f;
					s = 0.5f / s;
					qu.Y = (mat[I][J] + mat[J][I]) * s;
					qu.Z = (mat[K][I] + mat[I][K]) * s;
					qu.W = (mat[K][J] - mat[J][K]) * s;
					break;
				}
				case 1:
				{
					int I2 = 1;
					int J2 = 2;
					int K2 = 0;
					float s = (float)Math.Sqrt((double)(mat[I2][I2] - (mat[J2][J2] + mat[K2][K2]) + mat[3][3]));
					qu.Y = s * 0.5f;
					s = 0.5f / s;
					qu.Z = (mat[I2][J2] + mat[J2][I2]) * s;
					qu.X = (mat[K2][I2] + mat[I2][K2]) * s;
					qu.W = (mat[K2][J2] - mat[J2][K2]) * s;
					break;
				}
				case 2:
				{
					int I3 = 2;
					int J3 = 0;
					int K3 = 1;
					float s = (float)Math.Sqrt((double)(mat[I3][I3] - (mat[J3][J3] + mat[K3][K3]) + mat[3][3]));
					qu.Z = s * 0.5f;
					s = 0.5f / s;
					qu.X = (mat[I3][J3] + mat[J3][I3]) * s;
					qu.Y = (mat[K3][I3] + mat[I3][K3]) * s;
					qu.W = (mat[K3][J3] - mat[J3][K3]) * s;
					break;
				}
				}
			}
			if ((double)mat[3][3] != 1.0)
			{
				qu = DrawingTools.Qt_Scale(qu, 1f / (float)Math.Sqrt((double)mat[3][3]));
			}
			return qu;
		}

		private static float mat_norm(float[][] M, bool tpose)
		{
			float max = 0f;
			for (int i = 0; i < 3; i++)
			{
				float sum;
				if (tpose)
				{
					sum = Math.Abs(M[0][i]) + Math.Abs(M[1][i]) + Math.Abs(M[2][i]);
				}
				else
				{
					sum = Math.Abs(M[i][0]) + Math.Abs(M[i][1]) + Math.Abs(M[i][2]);
				}
				if (max < sum)
				{
					max = sum;
				}
			}
			return max;
		}

		private static float norm_inf(float[][] M)
		{
			return DrawingTools.mat_norm(M, false);
		}

		private static float norm_one(float[][] M)
		{
			return DrawingTools.mat_norm(M, true);
		}

		private static int find_max_col(float[][] M)
		{
			float max = 0f;
			int col = -1;
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					float abs = M[i][j];
					if ((double)abs < 0.0)
					{
						abs = -abs;
					}
					if (abs > max)
					{
						max = abs;
						col = j;
					}
				}
			}
			return col;
		}

		private static void make_reflector(float[] v, float[] u)
		{
			float s = (float)Math.Sqrt((double)DrawingTools.vdot(v, v));
			u[0] = v[0];
			u[1] = v[1];
			u[2] = v[2] + (((double)v[2] < 0.0) ? (-s) : s);
			s = (float)Math.Sqrt((double)(2f / DrawingTools.vdot(u, u)));
			u[0] = u[0] * s;
			u[1] = u[1] * s;
			u[2] = u[2] * s;
		}

		private static void reflect_cols(float[][] M, float[] u)
		{
			for (int i = 0; i < 3; i++)
			{
				float s = u[0] * M[0][i] + u[1] * M[1][i] + u[2] * M[2][i];
				for (int j = 0; j < 3; j++)
				{
					M[j][i] -= u[j] * s;
				}
			}
		}

		private static void reflect_rows(float[][] M, float[] u)
		{
			for (int i = 0; i < 3; i++)
			{
				float s = DrawingTools.vdot(u, M[i]);
				for (int j = 0; j < 3; j++)
				{
					M[i][j] -= u[j] * s;
				}
			}
		}

		private static void do_rank1(float[][] M, float[][] Q)
		{
			float[] v = new float[3];
			float[] v2 = new float[3];
			DrawingTools.mat_copy_eq(Q, DrawingTools.mat_id, 4);
			int col = DrawingTools.find_max_col(M);
			if (col < 0)
			{
				return;
			}
			v[0] = M[0][col];
			v[1] = M[1][col];
			v[2] = M[2][col];
			DrawingTools.make_reflector(v, v);
			DrawingTools.reflect_cols(M, v);
			v2[0] = M[2][0];
			v2[1] = M[2][1];
			v2[2] = M[2][2];
			DrawingTools.make_reflector(v2, v2);
			DrawingTools.reflect_rows(M, v2);
			float s = M[2][2];
			if ((double)s < 0.0)
			{
				Q[2][2] = -1f;
			}
			DrawingTools.reflect_cols(Q, v);
			DrawingTools.reflect_rows(Q, v2);
		}

		private static void do_rank2(float[][] M, float[][] MadjT, float[][] Q)
		{
			float[] v = new float[3];
			float[] v2 = new float[3];
			int col = DrawingTools.find_max_col(MadjT);
			if (col < 0)
			{
				DrawingTools.do_rank1(M, Q);
				return;
			}
			v[0] = MadjT[0][col];
			v[1] = MadjT[1][col];
			v[2] = MadjT[2][col];
			DrawingTools.make_reflector(v, v);
			DrawingTools.reflect_cols(M, v);
			DrawingTools.vcross(M[0], M[1], v2);
			DrawingTools.make_reflector(v2, v2);
			DrawingTools.reflect_rows(M, v2);
			float w = M[0][0];
			float x = M[0][1];
			float y = M[1][0];
			float z = M[1][1];
			if (w * z > x * y)
			{
				float c = z + w;
				float s = y - x;
				float d = (float)Math.Sqrt((double)(c * c + s * s));
				c /= d;
				s /= d;
				Q[0][0] = (Q[1][1] = c);
				Q[0][1] = -(Q[1][0] = s);
			}
			else
			{
				float c = z - w;
				float s = y + x;
				float d = (float)Math.Sqrt((double)(c * c + s * s));
				c /= d;
				s /= d;
				Q[0][0] = -(Q[1][1] = c);
				Q[0][1] = (Q[1][0] = s);
			}
			Q[0][2] = (Q[2][0] = (Q[1][2] = (Q[2][1] = 0f)));
			Q[2][2] = 1f;
			DrawingTools.reflect_cols(Q, v);
			DrawingTools.reflect_rows(Q, v2);
		}

		private static float polar_decomp(float[][] M, float[][] Q, float[][] S)
		{
			float[][] Mk = DrawingTools.MakeArrayMatrix();
			float[][] MadjTk = DrawingTools.MakeArrayMatrix();
			float[][] Ek = DrawingTools.MakeArrayMatrix();
			DrawingTools.mat_tpose(Mk, M, 3);
			float M_one = DrawingTools.norm_one(Mk);
			float M_inf = DrawingTools.norm_inf(Mk);
			float det;
			for (;;)
			{
				DrawingTools.adjoint_transpose(Mk, MadjTk);
				det = DrawingTools.vdot(Mk[0], MadjTk[0]);
				if ((double)det == 0.0)
				{
					break;
				}
				float MadjT_one = DrawingTools.norm_one(MadjTk);
				float MadjT_inf = DrawingTools.norm_inf(MadjTk);
				float gamma = (float)Math.Sqrt((double)((float)Math.Sqrt((double)(MadjT_one * MadjT_inf / (M_one * M_inf))) / Math.Abs(det)));
				float g = gamma * 0.5f;
				float g2 = 0.5f / (gamma * det);
				DrawingTools.mat_copy_eq(Ek, Mk, 3);
				DrawingTools.mat_binop(Mk, g, Mk, g2, MadjTk, 3);
				DrawingTools.mat_copy_minuseq(Ek, Mk, 3);
				float E_one = DrawingTools.norm_one(Ek);
				M_one = DrawingTools.norm_one(Mk);
				M_inf = DrawingTools.norm_inf(Mk);
				if (E_one <= M_one * 1E-06f)
				{
					goto IL_00E3;
				}
			}
			DrawingTools.do_rank2(Mk, MadjTk, Mk);
			IL_00E3:
			DrawingTools.mat_tpose(Q, Mk, 3);
			DrawingTools.mat_pad(Q);
			DrawingTools.mat_mult(Mk, M, S);
			DrawingTools.mat_pad(S);
			for (int i = 0; i < 3; i++)
			{
				for (int j = i; j < 3; j++)
				{
					S[i][j] = (S[j][i] = 0.5f * (S[i][j] + S[j][i]));
				}
			}
			return det;
		}

		private static Vector3 spect_decomp(float[][] S, float[][] U)
		{
			Vector3 kv = default(Vector3);
			float[] Diag = new float[3];
			float[] OffD = new float[3];
			int[] array = new int[3];
			array[0] = 1;
			array[1] = 2;
			int[] nxt = array;
			DrawingTools.mat_copy_eq(U, DrawingTools.mat_id, 4);
			Diag[0] = S[0][0];
			Diag[1] = S[1][1];
			Diag[2] = S[2][2];
			OffD[0] = S[1][2];
			OffD[1] = S[2][0];
			OffD[2] = S[0][1];
			for (int sweep = 20; sweep > 0; sweep--)
			{
				float sm = Math.Abs(OffD[0]) + Math.Abs(OffD[1]) + Math.Abs(OffD[2]);
				if ((double)sm == 0.0)
				{
					break;
				}
				for (int i = 2; i >= 0; i--)
				{
					int p = nxt[i];
					int q = nxt[p];
					float fabsOffDi = Math.Abs(OffD[i]);
					float g = 100f * fabsOffDi;
					if ((double)fabsOffDi > 0.0)
					{
						float h = Diag[q] - Diag[p];
						float fabsh = Math.Abs(h);
						float t;
						if (fabsh + g == fabsh)
						{
							t = OffD[i] / h;
						}
						else
						{
							float theta = 0.5f * h / OffD[i];
							t = 1f / (Math.Abs(theta) + (float)Math.Sqrt((double)(theta * theta + 1f)));
							if (theta < 0f)
							{
								t = -t;
							}
						}
						float c = 1f / (float)Math.Sqrt((double)(t * t + 1f));
						float s = t * c;
						float tau = s / (c + 1f);
						float ta = t * OffD[i];
						OffD[i] = 0f;
						Diag[p] -= ta;
						Diag[q] += ta;
						float OffDq = OffD[q];
						OffD[q] -= s * (OffD[p] + tau * OffD[q]);
						OffD[p] += s * (OffDq - tau * OffD[p]);
						for (int j = 2; j >= 0; j--)
						{
							float a = U[j][p];
							float b = U[j][q];
							U[j][p] -= s * (b + tau * a);
							U[j][q] += s * (a - tau * b);
						}
					}
				}
			}
			kv.X = Diag[0];
			kv.Y = Diag[1];
			kv.Z = Diag[2];
			return kv;
		}

		private static float sgn(int n, float v)
		{
			if (n == 0)
			{
				return v;
			}
			return -v;
		}

		private static void swap(float[] a, int i, int j)
		{
			a[3] = a[i];
			a[i] = a[j];
			a[j] = a[3];
		}

		private static void cycle(float[] a, int p)
		{
			if (p != 0)
			{
				a[3] = a[0];
				a[0] = a[1];
				a[1] = a[2];
				a[2] = a[3];
				return;
			}
			a[3] = a[2];
			a[2] = a[1];
			a[1] = a[0];
			a[0] = a[3];
		}

		private static Quaternion snuggle(Quaternion q, ref Vector3 k)
		{
			Quaternion p = default(Quaternion);
			float[] ka = new float[4];
			int turn = -1;
			ka[0] = k.X;
			ka[1] = k.Y;
			ka[2] = k.Z;
			if (ka[0] == ka[1])
			{
				if (ka[0] == ka[2])
				{
					turn = 3;
				}
				else
				{
					turn = 2;
				}
			}
			else if (ka[0] == ka[2])
			{
				turn = 1;
			}
			else if (ka[1] == ka[2])
			{
				turn = 0;
			}
			if (turn >= 0)
			{
				int[] neg = new int[3];
				float[] mag = new float[3];
				Quaternion qtoz;
				switch (turn)
				{
				case 0:
					q = DrawingTools.Qt_Mul(q, qtoz = DrawingTools.qxtoz);
					DrawingTools.swap(ka, 0, 2);
					break;
				case 1:
					q = DrawingTools.Qt_Mul(q, qtoz = DrawingTools.qytoz);
					DrawingTools.swap(ka, 1, 2);
					break;
				case 2:
					qtoz = DrawingTools.q0001;
					break;
				default:
					return DrawingTools.Qt_Conj(q);
				}
				q = DrawingTools.Qt_Conj(q);
				mag[0] = q.Z * q.Z + q.W * q.W - 0.5f;
				mag[1] = q.X * q.Z - q.Y * q.W;
				mag[2] = q.Y * q.Z + q.X * q.W;
				for (int i = 0; i < 3; i++)
				{
					neg[i] = ((mag[i] < 0f) ? 1 : 0);
					if (neg[i] != 0)
					{
						mag[i] = -mag[i];
					}
				}
				int win;
				if (mag[0] > mag[1])
				{
					if (mag[0] > mag[2])
					{
						win = 0;
					}
					else
					{
						win = 2;
					}
				}
				else if (mag[1] > mag[2])
				{
					win = 1;
				}
				else
				{
					win = 2;
				}
				switch (win)
				{
				case 0:
					if (neg[0] != 0)
					{
						p = DrawingTools.q1000;
					}
					else
					{
						p = DrawingTools.q0001;
					}
					break;
				case 1:
					if (neg[1] != 0)
					{
						p = DrawingTools.qppmm;
					}
					else
					{
						p = DrawingTools.qpppp;
					}
					DrawingTools.cycle(ka, 0);
					break;
				case 2:
					if (neg[2] != 0)
					{
						p = DrawingTools.qmpmm;
					}
					else
					{
						p = DrawingTools.qpppm;
					}
					DrawingTools.cycle(ka, 1);
					break;
				}
				Quaternion qp = DrawingTools.Qt_Mul(q, p);
				float t = (float)Math.Sqrt((double)(mag[win] + 0.5f));
				p = DrawingTools.Qt_Mul(p, new Quaternion(0f, 0f, -qp.Z / t, qp.W / t));
				p = DrawingTools.Qt_Mul(qtoz, DrawingTools.Qt_Conj(p));
			}
			else
			{
				float[] qa = new float[4];
				float[] pa = new float[4];
				int[] neg2 = new int[4];
				int par = 0;
				qa[0] = q.X;
				qa[1] = q.Y;
				qa[2] = q.Z;
				qa[3] = q.W;
				for (int i = 0; i < 4; i++)
				{
					pa[i] = 0f;
					neg2[i] = ((qa[i] < 0f) ? 1 : 0);
					if (neg2[i] != 0)
					{
						qa[i] = -qa[i];
					}
					par ^= neg2[i];
				}
				int lo;
				if (qa[0] > qa[1])
				{
					lo = 0;
				}
				else
				{
					lo = 1;
				}
				int hi;
				if (qa[2] > qa[3])
				{
					hi = 2;
				}
				else
				{
					hi = 3;
				}
				if (qa[lo] > qa[hi])
				{
					if (qa[lo ^ 1] > qa[hi])
					{
						hi = lo;
						lo ^= 1;
					}
					else
					{
						hi ^= lo;
						lo ^= hi;
						hi ^= lo;
					}
				}
				else if (qa[hi ^ 1] > qa[lo])
				{
					lo = hi ^ 1;
				}
				double all = (double)(qa[0] + qa[1] + qa[2] + qa[3]) * 0.5;
				double two = (double)((qa[hi] + qa[lo]) * 0.70710677f);
				double big = (double)qa[hi];
				if (all > two)
				{
					if (all > big)
					{
						for (int ii = 0; ii < 4; ii++)
						{
							pa[ii] = DrawingTools.sgn(neg2[ii], 0.5f);
						}
						DrawingTools.cycle(ka, par);
					}
					else
					{
						pa[hi] = DrawingTools.sgn(neg2[hi], 1f);
					}
				}
				else if (two > big)
				{
					pa[hi] = DrawingTools.sgn(neg2[hi], 0.70710677f);
					pa[lo] = DrawingTools.sgn(neg2[lo], 0.70710677f);
					if (lo > hi)
					{
						hi ^= lo;
						lo ^= hi;
						hi ^= lo;
					}
					int[] array = new int[3];
					array[0] = 1;
					array[1] = 2;
					int[] list = array;
					if (hi == 3)
					{
						hi = list[lo];
						lo = 3 - hi - lo;
					}
					DrawingTools.swap(ka, hi, lo);
				}
				else
				{
					pa[hi] = DrawingTools.sgn(neg2[hi], 1f);
				}
				p.X = -pa[0];
				p.Y = -pa[1];
				p.Z = -pa[2];
				p.W = pa[3];
			}
			k.X = ka[0];
			k.Y = ka[1];
			k.Z = ka[2];
			return p;
		}

		// Note: this type is marked as 'beforefieldinit'.
		static DrawingTools()
		{
			float[][] array = new float[4][];
			float[][] array2 = array;
			int num = 0;
			float[] array3 = new float[4];
			array3[0] = 1f;
			array2[num] = array3;
			float[][] array4 = array;
			int num2 = 1;
			float[] array5 = new float[4];
			array5[1] = 1f;
			array4[num2] = array5;
			float[][] array6 = array;
			int num3 = 2;
			float[] array7 = new float[4];
			array7[2] = 1f;
			array6[num3] = array7;
			array[3] = new float[] { 0f, 0f, 0f, 1f };
			DrawingTools.mat_id = array;
			DrawingTools.qxtoz = new Quaternion(0f, 0.70710677f, 0f, 0.70710677f);
			DrawingTools.qytoz = new Quaternion(0.70710677f, 0f, 0f, 0.70710677f);
			DrawingTools.qppmm = new Quaternion(0.5f, 0.5f, -0.5f, -0.5f);
			DrawingTools.qpppp = new Quaternion(0.5f, 0.5f, 0.5f, 0.5f);
			DrawingTools.qmpmm = new Quaternion(-0.5f, 0.5f, -0.5f, -0.5f);
			DrawingTools.qpppm = new Quaternion(0.5f, 0.5f, 0.5f, -0.5f);
			DrawingTools.q0001 = new Quaternion(0f, 0f, 0f, 1f);
			DrawingTools.q1000 = new Quaternion(1f, 0f, 0f, 0f);
		}

		private const int X = 0;

		private const int Y = 1;

		private const int Z = 2;

		private const int W = 3;

		private const float SQRTHALF = 0.70710677f;

		private static BasicEffect _wireFrameEffect;

		private static VertexPositionColor[] _wireFrameVerts;

		private static float[][] mat_id;

		private static readonly Quaternion qxtoz;

		private static readonly Quaternion qytoz;

		private static readonly Quaternion qppmm;

		private static readonly Quaternion qpppp;

		private static readonly Quaternion qmpmm;

		private static readonly Quaternion qpppm;

		private static readonly Quaternion q0001;

		private static readonly Quaternion q1000;
	}
}
