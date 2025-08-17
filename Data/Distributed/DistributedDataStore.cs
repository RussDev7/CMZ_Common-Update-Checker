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
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(this._records.Count);
			MemoryStream memoryStream = new MemoryStream();
			foreach (KeyValuePair<Guid, DistributedRecord> keyValuePair in this._records)
			{
				memoryStream.Position = 0L;
				DistributedRecord value = keyValuePair.Value;
				binaryWriter.Write(value.RecordTypeID);
				value.Serialize(memoryStream, user);
				byte[] buffer = memoryStream.GetBuffer();
				binaryWriter.Write(buffer, 0, (int)memoryStream.Position);
			}
		}

		private Dictionary<Guid, DistributedRecord> _records = new Dictionary<Guid, DistributedRecord>();
	}
}
