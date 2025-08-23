using System;
using System.Collections.Generic;
using System.IO;
using DNA.Collections;
using DNA.Reflection;
using DNA.Security.Cryptography;

namespace DNA.Data.Distributed
{
	public abstract class DistributedRecord
	{
		public int RecordTypeID
		{
			get
			{
				if (DistributedRecord._recordIDs == null)
				{
					DistributedRecord.PopulateMessageTypes();
				}
				return DistributedRecord._recordIDs[base.GetType()];
			}
		}

		public Guid ID
		{
			get
			{
				return this._id;
			}
		}

		protected abstract void SerializeData(Stream stream);

		protected abstract void DeserializeData(Stream stream);

		public void Serialize(Stream stream, string user)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			MemoryStream memStream = new MemoryStream();
			this.SerializeData(memStream);
			Hash newHash = this.hasher.Compute(memStream.GetBuffer(), 0L, memStream.Position);
			newHash != this._hash;
			this._hash = newHash;
			writer.Write(memStream.GetBuffer(), 0, (int)memStream.Position);
			writer.Write(this._hash.Data.Length);
			writer.Write(this._hash.Data);
			writer.Write(this.Name);
		}

		public DistributedRecord(string name, string creator)
		{
			this.Name = name;
		}

		private static bool TypeFilter(Type type)
		{
			return type.IsSubclassOf(typeof(DistributedRecord)) && !type.IsAbstract;
		}

		private static void PopulateMessageTypes()
		{
			DistributedRecord._recordTypes = ReflectionTools.GetTypes(new Filter<Type>(DistributedRecord.TypeFilter));
			DistributedRecord._recordIDs = new Dictionary<Type, int>();
			for (int i = 0; i < DistributedRecord._recordTypes.Length; i++)
			{
				DistributedRecord._recordIDs[DistributedRecord._recordTypes[i]] = i;
			}
		}

		private IHashProvider hasher = new MD5HashProvider();

		private Guid _id = Guid.NewGuid();

		private Hash _hash;

		private string Name;

		private static Type[] _recordTypes;

		private static Dictionary<Type, int> _recordIDs;
	}
}
