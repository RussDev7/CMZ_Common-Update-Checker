using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Profiling
{
	public class ProfilerPrimitiveBatch : IDisposable
	{
		public ProfilerPrimitiveBatch(GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null)
			{
				throw new ArgumentNullException("graphicsDevice");
			}
			this.device = graphicsDevice;
			this.basicEffect = new BasicEffect(graphicsDevice);
			this.basicEffect.VertexColorEnabled = true;
			this.basicEffect.Projection = ProfilerUtils._standard2DProjection;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !this.isDisposed)
			{
				if (this.basicEffect != null)
				{
					this.basicEffect.Dispose();
				}
				this.isDisposed = true;
			}
		}

		public void Begin(PrimitiveType primitiveType)
		{
			this.Begin(primitiveType, Matrix.Identity);
		}

		public void Begin(PrimitiveType primitiveType, Matrix mat)
		{
			if (this.hasBegun)
			{
				throw new InvalidOperationException("End must be called before Begin can be called again.");
			}
			if (primitiveType == PrimitiveType.LineStrip || primitiveType == PrimitiveType.TriangleStrip)
			{
				throw new NotSupportedException("The specified primitiveType is not supported by PrimitiveBatch.");
			}
			this.primitiveType = primitiveType;
			this.numVertsPerPrimitive = ProfilerPrimitiveBatch.NumVertsPerPrimitive(primitiveType);
			this.basicEffect.Projection = ProfilerUtils._standard2DProjection;
			this.basicEffect.View = mat;
			this.basicEffect.CurrentTechnique.Passes[0].Apply();
			this.hasBegun = true;
		}

		public void AddVertex(Vector2 vertex, Color color)
		{
			if (!this.hasBegun)
			{
				throw new InvalidOperationException("Begin must be called before AddVertex can be called.");
			}
			bool newPrimitive = this.positionInBuffer % this.numVertsPerPrimitive == 0;
			if (newPrimitive && this.positionInBuffer + this.numVertsPerPrimitive >= this.vertices.Length)
			{
				this.Flush();
			}
			this.vertices[this.positionInBuffer].Position = new Vector3(vertex, 0f);
			this.vertices[this.positionInBuffer].Color = color;
			this.positionInBuffer++;
		}

		public void End()
		{
			if (!this.hasBegun)
			{
				throw new InvalidOperationException("Begin must be called before End can be called.");
			}
			this.Flush();
			this.hasBegun = false;
		}

		private void Flush()
		{
			if (!this.hasBegun)
			{
				throw new InvalidOperationException("Begin must be called before Flush can be called.");
			}
			if (this.positionInBuffer == 0)
			{
				return;
			}
			int primitiveCount = this.positionInBuffer / this.numVertsPerPrimitive;
			this.device.DrawUserPrimitives<VertexPositionColor>(this.primitiveType, this.vertices, 0, primitiveCount);
			this.positionInBuffer = 0;
		}

		private static int NumVertsPerPrimitive(PrimitiveType primitive)
		{
			switch (primitive)
			{
			case PrimitiveType.TriangleList:
				return 3;
			case PrimitiveType.LineList:
				return 2;
			}
			throw new InvalidOperationException("primitive is not valid");
		}

		public void AddFilledBox(Vector2 coord, Vector2 size, Color color, bool centered)
		{
			this.AddFilledBox(ref coord, ref size, color, centered);
		}

		public void AddFilledBox(ref Vector2 coord, ref Vector2 size, Color color, bool centered)
		{
			Vector2 upperLeft;
			if (centered)
			{
				upperLeft = coord - size * 0.5f;
			}
			else
			{
				upperLeft = coord;
			}
			Vector2 lowerRight = upperLeft + size;
			if (this.hasBegun && this.primitiveType != PrimitiveType.TriangleList)
			{
				this.End();
			}
			if (!this.hasBegun)
			{
				this.Begin(PrimitiveType.TriangleList, this.basicEffect.View);
			}
			this.AddVertex(new Vector2(upperLeft.X, upperLeft.Y), color);
			this.AddVertex(new Vector2(lowerRight.X, upperLeft.Y), color);
			this.AddVertex(new Vector2(upperLeft.X, lowerRight.Y), color);
			this.AddVertex(new Vector2(upperLeft.X, lowerRight.Y), color);
			this.AddVertex(new Vector2(lowerRight.X, upperLeft.Y), color);
			this.AddVertex(new Vector2(lowerRight.X, lowerRight.Y), color);
		}

		public void AddLine(Vector2[] vectors, Color color, bool close)
		{
			if (this.hasBegun && this.primitiveType != PrimitiveType.LineList)
			{
				this.End();
			}
			if (!this.hasBegun)
			{
				this.Begin(PrimitiveType.LineList, this.basicEffect.View);
			}
			for (int i = 0; i < vectors.Length - 1; i++)
			{
				this.AddVertex(vectors[i], color);
				this.AddVertex(vectors[i + 1], color);
			}
			if (close)
			{
				this.AddVertex(vectors[vectors.Length - 1], color);
				this.AddVertex(vectors[0], color);
			}
		}

		public void DrawGraph(float[] values, int startIndex, Vector2 scale, Vector2 upperLeft, Vector2 size, Color color)
		{
			if (this.hasBegun && this.primitiveType != PrimitiveType.LineList)
			{
				this.End();
			}
			if (!this.hasBegun)
			{
				this.Begin(PrimitiveType.LineList, this.basicEffect.View);
			}
			int i = startIndex;
			float step = size.X / ((float)values.Length - 1f);
			Vector2 prev = Vector2.Zero;
			Vector2 next = new Vector2(upperLeft.X, 0f);
			for (int count = 0; count < values.Length; count++)
			{
				next.X += step;
				if (values[i] < scale.X)
				{
					next.Y = upperLeft.Y + size.Y;
				}
				else if (values[i] > scale.Y)
				{
					next.Y = upperLeft.Y;
				}
				else
				{
					next.Y = upperLeft.Y + size.Y * (1f - (values[i] - scale.X) / (scale.Y - scale.X));
				}
				if (count != 0)
				{
					this.AddVertex(prev, color);
					this.AddVertex(next, color);
				}
				prev = next;
				if (++i == values.Length)
				{
					i = 0;
				}
			}
		}

		public void DrawGraphBar(float value, Vector2 scale, Vector2 upperLeft, Vector2 size, Color color)
		{
			if (value < scale.X || value > scale.Y)
			{
				return;
			}
			if (this.hasBegun && this.primitiveType != PrimitiveType.LineList)
			{
				this.End();
			}
			if (!this.hasBegun)
			{
				this.Begin(PrimitiveType.LineList, this.basicEffect.View);
			}
			Vector2 startPoint = new Vector2(upperLeft.X, 0f);
			Vector2 stopPoint = new Vector2(upperLeft.X + size.X, 0f);
			startPoint.Y = upperLeft.Y + size.Y * (1f - (value - scale.X) / (scale.Y - scale.X));
			stopPoint.Y = startPoint.Y;
			this.AddVertex(startPoint, color);
			this.AddVertex(stopPoint, color);
		}

		public void DrawGraphVerticalAxis(Vector2 upperLeft, Vector2 size, Color color)
		{
			if (this.hasBegun && this.primitiveType != PrimitiveType.LineList)
			{
				this.End();
			}
			if (!this.hasBegun)
			{
				this.Begin(PrimitiveType.LineList, this.basicEffect.View);
			}
			this.AddVertex(upperLeft, color);
			this.AddVertex(new Vector2(upperLeft.X, upperLeft.Y + size.Y), color);
		}

		private const int DefaultBufferSize = 500;

		private VertexPositionColor[] vertices = new VertexPositionColor[500];

		private int positionInBuffer;

		private BasicEffect basicEffect;

		private GraphicsDevice device;

		private PrimitiveType primitiveType;

		private int numVertsPerPrimitive;

		private bool hasBegun;

		private bool isDisposed;
	}
}
