﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public struct VertexPositionNormalColor : IVertexType
	{
		public VertexDeclaration VertexDeclaration
		{
			get
			{
				return VertexPositionNormalColor.s_VertexDeclaration;
			}
		}

		public Vector3 Position;

		public Color Color;

		public Vector3 Normal;

		public Vector2 TextureCoord;

		public static VertexDeclaration s_VertexDeclaration = new VertexDeclaration(VertexPositionNormalColor.VertexElements);

		public static VertexElement[] VertexElements = new VertexElement[]
		{
			new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
			new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
			new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
			new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
		};
	}
}
