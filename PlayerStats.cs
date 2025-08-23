using System;
using System.IO;

namespace DNA
{
	public abstract class PlayerStats
	{
		public abstract int Version { get; }

		public void Save(BinaryWriter writer)
		{
			writer.Write(1095783254);
			writer.Write(this.Version);
			if (this.GamerTag != null)
			{
				writer.Write(this.GamerTag);
			}
			else
			{
				writer.Write("<null>");
			}
			writer.Write(this.DateRecorded.Ticks);
			this.SaveData(writer);
		}

		protected abstract void SaveData(BinaryWriter writer);

		public void Load(BinaryReader reader)
		{
			int ident = reader.ReadInt32();
			if (ident != 1095783254)
			{
				throw new Exception();
			}
			int version = reader.ReadInt32();
			this.GamerTag = reader.ReadString();
			this.DateRecorded = new DateTime(reader.ReadInt64());
			this.LoadData(reader, version);
		}

		protected abstract void LoadData(BinaryReader reader, int version);

		private const int FileIdent = 1095783254;

		public string GamerTag;

		public DateTime DateRecorded;
	}
}
