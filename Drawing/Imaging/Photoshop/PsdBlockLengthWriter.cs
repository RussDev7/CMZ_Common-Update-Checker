using System;

namespace DNA.Drawing.Imaging.Photoshop
{
	internal class PsdBlockLengthWriter : IDisposable
	{
		public PsdBlockLengthWriter(PsdBinaryWriter writer)
		{
			this.writer = writer;
			this.lengthPosition = writer.BaseStream.Position;
			writer.Write(4277010157U);
			this.startPosition = writer.BaseStream.Position;
		}

		public void Write()
		{
			long endPosition = this.writer.BaseStream.Position;
			this.writer.BaseStream.Position = this.lengthPosition;
			long length = endPosition - this.startPosition;
			this.writer.Write((uint)length);
			this.writer.BaseStream.Position = endPosition;
		}

		public void Dispose()
		{
			if (!this.disposed)
			{
				this.Write();
				this.disposed = true;
			}
		}

		private bool disposed;

		private long lengthPosition;

		private long startPosition;

		private PsdBinaryWriter writer;
	}
}
