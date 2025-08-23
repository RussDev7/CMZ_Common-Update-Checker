using System;

namespace DNA.IO.Compression.Zip.Compression
{
	public class Deflater
	{
		public Deflater()
			: this(Deflater.DefaultCompression, false)
		{
		}

		public Deflater(int lvl)
			: this(lvl, false)
		{
		}

		public Deflater(int level, bool noZlibHeaderOrFooter)
		{
			if (level == Deflater.DefaultCompression)
			{
				level = 6;
			}
			else if (level < Deflater.NoCompression || level > Deflater.BestCompression)
			{
				throw new ArgumentOutOfRangeException("level");
			}
			this.pending = new DeflaterPending();
			this.engine = new DeflaterEngine(this.pending);
			this.noZlibHeaderOrFooter = noZlibHeaderOrFooter;
			this.SetStrategy(DeflateStrategy.Default);
			this.SetLevel(level);
			this.Reset();
		}

		public void Reset()
		{
			this.state = (this.noZlibHeaderOrFooter ? Deflater.BUSY_STATE : Deflater.INIT_STATE);
			this.totalOut = 0L;
			this.pending.Reset();
			this.engine.Reset();
		}

		public int Adler
		{
			get
			{
				return this.engine.Adler;
			}
		}

		public int TotalIn
		{
			get
			{
				return this.engine.TotalIn;
			}
		}

		public long TotalOut
		{
			get
			{
				return this.totalOut;
			}
		}

		public void Flush()
		{
			this.state |= Deflater.IS_FLUSHING;
		}

		public void Finish()
		{
			this.state |= Deflater.IS_FLUSHING | Deflater.IS_FINISHING;
		}

		public bool IsFinished
		{
			get
			{
				return this.state == Deflater.FINISHED_STATE && this.pending.IsFlushed;
			}
		}

		public bool IsNeedingInput
		{
			get
			{
				return this.engine.NeedsInput();
			}
		}

		public void SetInput(byte[] input)
		{
			this.SetInput(input, 0, input.Length);
		}

		public void SetInput(byte[] input, int off, int len)
		{
			if ((this.state & Deflater.IS_FINISHING) != 0)
			{
				throw new InvalidOperationException("finish()/end() already called");
			}
			this.engine.SetInput(input, off, len);
		}

		public void SetLevel(int lvl)
		{
			if (lvl == Deflater.DefaultCompression)
			{
				lvl = 6;
			}
			else if (lvl < Deflater.NoCompression || lvl > Deflater.BestCompression)
			{
				throw new ArgumentOutOfRangeException("lvl");
			}
			if (this.level != lvl)
			{
				this.level = lvl;
				this.engine.SetLevel(lvl);
			}
		}

		public int GetLevel()
		{
			return this.level;
		}

		public void SetStrategy(DeflateStrategy strategy)
		{
			this.engine.Strategy = strategy;
		}

		public int Deflate(byte[] output)
		{
			return this.Deflate(output, 0, output.Length);
		}

		public int Deflate(byte[] output, int offset, int length)
		{
			int origLength = length;
			if (this.state == Deflater.CLOSED_STATE)
			{
				throw new InvalidOperationException("Deflater closed");
			}
			if (this.state < Deflater.BUSY_STATE)
			{
				int header = Deflater.Deflated + 112 << 8;
				int level_flags = this.level - 1 >> 1;
				if (level_flags < 0 || level_flags > 3)
				{
					level_flags = 3;
				}
				header |= level_flags << 6;
				if ((this.state & Deflater.IS_SETDICT) != 0)
				{
					header |= 32;
				}
				header += 31 - header % 31;
				this.pending.WriteShortMSB(header);
				if ((this.state & Deflater.IS_SETDICT) != 0)
				{
					int chksum = this.engine.Adler;
					this.engine.ResetAdler();
					this.pending.WriteShortMSB(chksum >> 16);
					this.pending.WriteShortMSB(chksum & 65535);
				}
				this.state = Deflater.BUSY_STATE | (this.state & (Deflater.IS_FLUSHING | Deflater.IS_FINISHING));
			}
			for (;;)
			{
				int count = this.pending.Flush(output, offset, length);
				offset += count;
				this.totalOut += (long)count;
				length -= count;
				if (length == 0 || this.state == Deflater.FINISHED_STATE)
				{
					goto IL_021C;
				}
				if (!this.engine.Deflate((this.state & Deflater.IS_FLUSHING) != 0, (this.state & Deflater.IS_FINISHING) != 0))
				{
					if (this.state == Deflater.BUSY_STATE)
					{
						break;
					}
					if (this.state == Deflater.FLUSHING_STATE)
					{
						if (this.level != Deflater.NoCompression)
						{
							for (int neededbits = 8 + (-this.pending.BitCount & 7); neededbits > 0; neededbits -= 10)
							{
								this.pending.WriteBits(2, 10);
							}
						}
						this.state = Deflater.BUSY_STATE;
					}
					else if (this.state == Deflater.FINISHING_STATE)
					{
						this.pending.AlignToByte();
						if (!this.noZlibHeaderOrFooter)
						{
							int adler = this.engine.Adler;
							this.pending.WriteShortMSB(adler >> 16);
							this.pending.WriteShortMSB(adler & 65535);
						}
						this.state = Deflater.FINISHED_STATE;
					}
				}
			}
			return origLength - length;
			IL_021C:
			return origLength - length;
		}

		public void SetDictionary(byte[] dict)
		{
			this.SetDictionary(dict, 0, dict.Length);
		}

		public void SetDictionary(byte[] dict, int offset, int length)
		{
			if (this.state != Deflater.INIT_STATE)
			{
				throw new InvalidOperationException();
			}
			this.state = Deflater.SETDICT_STATE;
			this.engine.SetDictionary(dict, offset, length);
		}

		public static int BestCompression = 9;

		public static int BestSpeed = 1;

		public static int DefaultCompression = -1;

		public static int NoCompression = 0;

		public static int Deflated = 8;

		private static int IS_SETDICT = 1;

		private static int IS_FLUSHING = 4;

		private static int IS_FINISHING = 8;

		private static int INIT_STATE = 0;

		private static int SETDICT_STATE = 1;

		private static int BUSY_STATE = 16;

		private static int FLUSHING_STATE = 20;

		private static int FINISHING_STATE = 28;

		private static int FINISHED_STATE = 30;

		private static int CLOSED_STATE = 127;

		private int level;

		private bool noZlibHeaderOrFooter;

		private int state;

		private long totalOut;

		private DeflaterPending pending;

		private DeflaterEngine engine;
	}
}
