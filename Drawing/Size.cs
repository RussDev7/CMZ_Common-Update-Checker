using System;

namespace DNA.Drawing
{
	public struct Size
	{
		public Size(int width, int height)
		{
			this.Width = width;
			this.Height = height;
		}

		public override int GetHashCode()
		{
			return this.Width.GetHashCode() ^ this.Height.GetHashCode();
		}

		public static bool operator !=(Size a, Size b)
		{
			return a.Width != b.Width || a.Height != b.Height;
		}

		public static bool operator ==(Size a, Size b)
		{
			return a.Width == b.Width && a.Height == b.Height;
		}

		public int Width;

		public int Height;
	}
}
