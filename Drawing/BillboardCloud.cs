using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class BillboardCloud : Entity
	{
		public BillboardCloud(Game game)
		{
			this._game = game;
			this.LoadParticleEffect();
		}

		private void LoadParticleEffect()
		{
			if (BillboardCloud._effect == null)
			{
				BillboardCloud._effect = this._game.Content.Load<Effect>("Billboard");
			}
			this._cardEffect = BillboardCloud._effect.Clone();
		}

		private void Initialize(GraphicsDevice device, int cards)
		{
			this._vertexBuffer = new VertexBuffer(device, BillboardVertex.VertexDeclaration, cards * 4, BufferUsage.WriteOnly);
			this._indexBuffer = new IndexBuffer(device, typeof(uint), cards * 6, BufferUsage.WriteOnly);
		}

		public void Clear()
		{
			if (!this._started)
			{
				throw new Exception("Must Start Set to Clear");
			}
			this.billboardVerticies.Clear();
		}

		public void BeginSet(BillBoardMode mode)
		{
			if (this._started)
			{
				throw new Exception("Batch Already Started");
			}
			this._started = true;
			this._billboardMode = mode;
		}

		public void EndSet(GraphicsDevice device)
		{
			if (!this._started)
			{
				throw new Exception("End Before Start");
			}
			this.Initialize(device, this.billboardVerticies.Count / 4);
			this.SetData();
			this._started = false;
		}

		private void SetData()
		{
			int cards = this.billboardVerticies.Count / 4;
			uint[] indices = new uint[cards * 6];
			for (int i = 0; i < cards; i++)
			{
				indices[i * 6] = (uint)(i * 4);
				indices[i * 6 + 1] = (uint)(i * 4 + 1);
				indices[i * 6 + 2] = (uint)(i * 4 + 2);
				indices[i * 6 + 3] = (uint)(i * 4);
				indices[i * 6 + 4] = (uint)(i * 4 + 2);
				indices[i * 6 + 5] = (uint)(i * 4 + 3);
			}
			this._indexBuffer.SetData<uint>(indices);
			this._vertexBuffer.SetData<BillboardVertex>(this.billboardVerticies.ToArray());
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			this.Draw(device, base.LocalToWorld, view, projection);
			base.Draw(device, gameTime, view, projection);
		}

		public void Draw(GraphicsDevice device, Matrix world, Matrix view, Matrix projection)
		{
			if (this._started)
			{
				throw new Exception("Must finsih set before drawing");
			}
			EffectParameterCollection parameters = this._cardEffect.Parameters;
			device.BlendState = BlendState.AlphaBlend;
			device.BlendState = BlendState.Opaque;
			device.DepthStencilState = DepthStencilState.DepthRead;
			device.DepthStencilState = DepthStencilState.Default;
			device.RasterizerState = RasterizerState.CullNone;
			parameters["View"].SetValue(view);
			parameters["Projection"].SetValue(projection);
			parameters["ViewportScale"].SetValue(new Vector2(1f / device.Viewport.AspectRatio, 1f));
			parameters["Texture"].SetValue(this._texture);
			switch (this._billboardMode)
			{
			case BillBoardMode.ScreenAligned:
				this._cardEffect.CurrentTechnique = this._cardEffect.Techniques["ScreenAlignedBillboards"];
				break;
			case BillBoardMode.AxisAligned:
				this._cardEffect.CurrentTechnique = this._cardEffect.Techniques["AxisAlignedBillboards"];
				break;
			default:
				throw new Exception("Unknown Mode");
			}
			int cardCount = this.billboardVerticies.Count / 4;
			device.SetVertexBuffer(this._vertexBuffer);
			device.Indices = this._indexBuffer;
			for (int i = 0; i < this._cardEffect.CurrentTechnique.Passes.Count; i++)
			{
				EffectPass pass = this._cardEffect.CurrentTechnique.Passes[i];
				pass.Apply();
				device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, cardCount * 4, 0, cardCount * 2);
			}
			device.SetVertexBuffer(null);
		}

		public void DrawCard(Texture texture, Vector3 position, Vector3 axis, Vector2 scale, Color color)
		{
			this._texture = texture;
			this.billboardVerticies.Add(new BillboardVertex(position, scale, axis, new Vector2(0f, 0f), color));
			this.billboardVerticies.Add(new BillboardVertex(position, scale, axis, new Vector2(1f, 0f), color));
			this.billboardVerticies.Add(new BillboardVertex(position, scale, axis, new Vector2(1f, 1f), color));
			this.billboardVerticies.Add(new BillboardVertex(position, scale, axis, new Vector2(0f, 1f), color));
		}

		private BillBoardMode _billboardMode = BillBoardMode.AxisAligned;

		private List<BillboardVertex> billboardVerticies = new List<BillboardVertex>();

		private VertexBuffer _vertexBuffer;

		private IndexBuffer _indexBuffer;

		private Effect _cardEffect;

		private Texture _texture;

		public static Effect _effect;

		private Game _game;

		private bool _started;
	}
}
