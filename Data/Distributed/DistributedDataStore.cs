using System;
using System.Collections.Generic;
using System.IO;

namespace DNA.Data.Distributed
{
	public class DistributedDataStore
	{
		public void Set(DistributedRecord record)
		{
			this._records[record.ID] = record;
		}

		public void Remove(DistributedRecord record)
		{
			this._records.Remove(record.ID);
		}

		public void Remove(Guid id)
		{
			this._records.Remove(id);
		}

		public void Commit(Stream stream, string user)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(this._records.Count);
			MemoryStream tempStream = new MemoryStream();
			foreach (KeyValuePair<Guid, DistributedRecord> pair in this._records)
			{
				tempStream.Position = 0L;
				DistributedRecord record = pair.Value;
				writer.Write(record.RecordTypeID);
				record.Serialize(tempStream, user);
				byte[] buffer = tempStream.GetBuffer();
				writer.Write(buffer, 0, (int)tempStream.Position);
			}
		}

		private Dictionary<Guid, DistributedRecord> _records = new Dictionary<Guid, DistributedRecord>();
	}
}
