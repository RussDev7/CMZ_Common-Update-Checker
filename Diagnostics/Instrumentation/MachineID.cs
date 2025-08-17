using System;
using System.Text;
using DNA.Security.Cryptography;

namespace DNA.Diagnostics.Instrumentation
{
	public class MachineID
	{
		public static MachineID LocalID
		{
			get
			{
				if (MachineID._localID == null)
				{
					MachineID._localID = new MachineID(MachineInfo.LocalInfo);
				}
				return MachineID._localID;
			}
		}

		public byte[] ToByteArray()
		{
			return (byte[])this._hash.Data.Clone();
		}

		public override string ToString()
		{
			return this._hash.ToString();
		}

		public MachineID(string midString)
		{
			MD5HashProvider md5HashProvider = new MD5HashProvider();
			this._hash = md5HashProvider.Parse(midString);
		}

		public MachineID(byte[] data)
		{
			MD5HashProvider md5HashProvider = new MD5HashProvider();
			this._hash = md5HashProvider.CreateHash(data);
		}

		public MachineID(MachineInfo info)
		{
			string text = "";
			if (info.Processors != null && info.Processors.Length > 0)
			{
				text = info.Processors[0].ProcessorID;
			}
			string text2 = "";
			foreach (NetworkAdapterInfo networkAdapterInfo in info.NetworkAdapters)
			{
				if (networkAdapterInfo.Physical && !string.IsNullOrEmpty(networkAdapterInfo.MACAddress))
				{
					text2 = networkAdapterInfo.MACAddress;
					break;
				}
			}
			string text3 = "";
			foreach (HardDiskInfo hardDiskInfo in info.HardDisks)
			{
				if (!string.IsNullOrEmpty(hardDiskInfo.SerialNumber))
				{
					if (hardDiskInfo.BusType == "ATA" || hardDiskInfo.BusType == "SCSI")
					{
						text3 = hardDiskInfo.SerialNumber;
						break;
					}
					if (string.IsNullOrEmpty(text3))
					{
						text3 = hardDiskInfo.SerialNumber;
					}
				}
			}
			string text4 = "";
			foreach (HardDiskInfo hardDiskInfo2 in info.HardDisks)
			{
				if (!string.IsNullOrEmpty(hardDiskInfo2.Model))
				{
					if (hardDiskInfo2.BusType == "ATA" || hardDiskInfo2.BusType == "SCSI")
					{
						text4 = hardDiskInfo2.Model;
						break;
					}
					if (string.IsNullOrEmpty(text4))
					{
						text4 = hardDiskInfo2.Model;
					}
				}
			}
			MD5HashProvider md5HashProvider = new MD5HashProvider();
			string text5 = text2 + text3 + text + text4;
			byte[] bytes = Encoding.UTF8.GetBytes(text5);
			this._hash = md5HashProvider.Compute(bytes);
		}

		private Hash _hash;

		private static MachineID _localID;
	}
}
