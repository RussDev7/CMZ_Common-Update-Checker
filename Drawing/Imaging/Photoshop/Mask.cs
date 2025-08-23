using System;
using System.Collections.Specialized;
using System.Drawing;

namespace DNA.Drawing.Imaging.Photoshop
{
	public class Mask
	{
		public Layer Layer { get; private set; }

		public Rectangle Rect { get; private set; }

		public byte DefaultColor { get; set; }

		public bool PositionIsRelative
		{
			get
			{
				return this.flags[Mask.positionIsRelativeBit];
			}
			set
			{
				this.flags[Mask.positionIsRelativeBit] = value;
			}
		}

		public bool Disabled
		{
			get
			{
				return this.flags[Mask.disabledBit];
			}
			set
			{
				this.flags[Mask.disabledBit] = value;
			}
		}

		public bool InvertOnBlendBit
		{
			get
			{
				return this.flags[Mask.invertOnBlendBit];
			}
			set
			{
				this.flags[Mask.invertOnBlendBit] = value;
			}
		}

		public byte[] ImageData { get; set; }

		internal Mask(Layer layer)
		{
			this.Layer = layer;
		}

		internal Mask(PsdBinaryReader reader, Layer layer)
		{
			this.Layer = layer;
			uint maskLength = reader.ReadUInt32();
			if (maskLength <= 0U)
			{
				return;
			}
			long startPosition = reader.BaseStream.Position;
			Rectangle rect = default(Rectangle);
			rect.Y = reader.ReadInt32();
			rect.X = reader.ReadInt32();
			rect.Height = reader.ReadInt32() - rect.Y;
			rect.Width = reader.ReadInt32() - rect.X;
			this.Rect = rect;
			this.DefaultColor = reader.ReadByte();
			byte flagsByte = reader.ReadByte();
			this.flags = new BitVector32((int)flagsByte);
			if (maskLength == 36U)
			{
				new BitVector32((int)reader.ReadByte());
				reader.ReadByte();
				Rectangle realRect = default(Rectangle);
				realRect.Y = reader.ReadInt32();
				realRect.X = reader.ReadInt32();
				realRect.Height = reader.ReadInt32() - rect.Y;
				realRect.Width = reader.ReadInt32() - rect.X;
			}
			reader.BaseStream.Position = startPosition + (long)((ulong)maskLength);
		}

		public void Save(PsdBinaryWriter writer)
		{
			if (this.Rect.IsEmpty)
			{
				writer.Write(0U);
				return;
			}
			using (new PsdBlockLengthWriter(writer))
			{
				writer.Write(this.Rect.Top);
				writer.Write(this.Rect.Left);
				writer.Write(this.Rect.Bottom);
				writer.Write(this.Rect.Right);
				writer.Write(this.DefaultColor);
				writer.Write((byte)this.flags.Data);
				writer.Write(0);
				writer.Write(0);
			}
		}

		private static int positionIsRelativeBit = BitVector32.CreateMask();

		private static int disabledBit = BitVector32.CreateMask(Mask.positionIsRelativeBit);

		private static int invertOnBlendBit = BitVector32.CreateMask(Mask.disabledBit);

		private BitVector32 flags = default(BitVector32);
	}
}
