using System;

namespace DNA.Drawing
{
	public struct TriangleVertexIndices
	{
		public TriangleVertexIndices(int a, int b, int c)
		{
			this.A = a;
			this.B = b;
			this.C = c;
		}

		public int A;

		public int B;

		public int C;
	}
}
