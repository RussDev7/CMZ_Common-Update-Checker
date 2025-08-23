using System;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class SkySphere : Entity
	{
		private void CreateBall(GraphicsDevice device, float radius, int segments, Vector3 center, bool insideOut)
		{
			int verticalSegs = segments / 2;
			VertexPositionNormalTexture[] verts = new VertexPositionNormalTexture[segments * (verticalSegs - 1) + 2];
			int bottomV = verts.Length - 1;
			verts[0].Position = new Vector3(0f, radius, 0f);
			verts[0].Normal = new Vector3(0f, 1f, 0f);
			verts[0].TextureCoordinate = new Vector2(0f, 0f);
			verts[bottomV].Position = new Vector3(0f, -radius, 0f);
			verts[bottomV].Normal = new Vector3(0f, -1f, 0f);
			verts[bottomV].TextureCoordinate = new Vector2(0f, 1f);
			int vindex = 1;
			for (int lon = 1; lon < verticalSegs; lon++)
			{
				for (int lat = 0; lat < segments; lat++)
				{
					Angle lonAng = Angle.Lerp(Angle.FromDegrees(0f), Angle.FromDegrees(180f), (float)lon / (float)verticalSegs);
					Angle latAng = Angle.Lerp(Angle.FromDegrees(0f), Angle.FromDegrees(360f), (float)lat / (float)segments);
					verts[vindex].Normal = new Vector3((float)(latAng.Sin * lonAng.Sin), (float)lonAng.Cos, (float)(latAng.Cos * lonAng.Sin));
					verts[vindex].Position = center + verts[vindex].Normal * radius;
					verts[vindex].TextureCoordinate = new Vector2((float)lat / (float)segments, -((float)lonAng.Cos - 1f) / 2f);
					vindex++;
				}
			}
			short[] indicies = new short[segments * 6 * verticalSegs];
			int indexIndex = 0;
			for (int i = 0; i < segments; i++)
			{
				int startv = 1;
				int nextv = (i + 1) % segments;
				indicies[indexIndex++] = 0;
				indicies[indexIndex++] = (short)(nextv + startv);
				indicies[indexIndex++] = (short)(i + startv);
				startv = bottomV - segments;
				indicies[indexIndex++] = (short)bottomV;
				indicies[indexIndex++] = (short)(startv + i);
				indicies[indexIndex++] = (short)(startv + nextv);
			}
			for (int lon2 = 0; lon2 < verticalSegs - 2; lon2++)
			{
				int startv2 = lon2 * segments + 1;
				for (int j = 0; j < segments; j++)
				{
					int nextv2 = (j + 1) % segments;
					indicies[indexIndex++] = (short)(startv2 + j);
					indicies[indexIndex++] = (short)(startv2 + nextv2 + segments);
					indicies[indexIndex++] = (short)(startv2 + j + segments);
					indicies[indexIndex++] = (short)(startv2 + j);
					indicies[indexIndex++] = (short)(startv2 + nextv2);
					indicies[indexIndex++] = (short)(startv2 + nextv2 + segments);
				}
			}
			if (insideOut)
			{
				for (int k = 0; k < verts.Length; k++)
				{
					verts[k].Normal = -verts[k].Normal;
				}
				for (int l = 0; l < indicies.Length; l += 3)
				{
					short swap = indicies[l + 1];
					indicies[l + 1] = indicies[l + 2];
					indicies[l + 2] = swap;
				}
			}
			this._vertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), verts.Length, BufferUsage.WriteOnly);
			this._vertexBuffer.SetData<VertexPositionNormalTexture>(verts);
			this._indexBuffer = new IndexBuffer(device, typeof(short), indicies.Length, BufferUsage.WriteOnly);
			this._indexBuffer.SetData<short>(indicies);
		}

		public SkySphere(Game game, float radius, Vector3 center, int segments, Texture texture)
		{
			this.CreateBall(game.GraphicsDevice, radius, segments, center, true);
			this._effect = new DNAEffect(game.Content.Load<Effect>("SkySphere"));
			this._texture = texture;
			base.RasterizerState = RasterizerState.CullNone;
			base.DepthStencilState = DepthStencilState.None;
		}

		public SkySphere(GraphicsDevice device, float radius, Vector3 center, int segments, Effect effect, Texture texture)
		{
			this.CreateBall(device, radius, segments, center, true);
			this._effect = new DNAEffect(effect);
			this._texture = texture;
			base.RasterizerState = RasterizerState.CullNone;
			base.DepthStencilState = DepthStencilState.None;
		}

		protected virtual bool SetEffectParams(DNAEffect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			effect.World = world;
			effect.View = view;
			effect.Projection = projection;
			if (effect.Parameters["Texture"] != null)
			{
				effect.Parameters["Texture"].SetValue(this._texture);
			}
			return true;
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			view.Translation = Vector3.Zero;
			this.SetEffectParams(this._effect, gameTime, base.LocalToWorld, view, projection);
			device.SetVertexBuffer(this._vertexBuffer);
			device.Indices = this._indexBuffer;
			for (int i = 0; i < this._effect.CurrentTechnique.Passes.Count; i++)
			{
				EffectPass pass = this._effect.CurrentTechnique.Passes[i];
				pass.Apply();
				device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, this._vertexBuffer.VertexCount, 0, this._indexBuffer.IndexCount / 3);
			}
			base.Draw(device, gameTime, view, projection);
		}

		private DNAEffect _effect;

		private VertexBuffer _vertexBuffer;

		private IndexBuffer _indexBuffer;

		private Texture _texture;
	}
}
