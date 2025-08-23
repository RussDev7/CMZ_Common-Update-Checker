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
			MD5HashProvider hasher = new MD5HashProvider();
			this._hash = hasher.Parse(midString);
		}

		public MachineID(byte[] data)
		{
			MD5HashProvider hasher = new MD5HashProvider();
			this._hash = hasher.CreateHash(data);
		}

		public MachineID(MachineInfo info)
		{
			string procID = "";
			if (info.Processors != null && info.Processors.Length > 0)
			{
				procID = info.Processors[0].ProcessorID;
			}
			string mac = "";
			foreach (NetworkAdapterInfo network in info.NetworkAdapters)
			{
				if (network.Physical && !string.IsNullOrEmpty(network.MACAddress))
				{
					mac = network.MACAddress;
					break;
				}
			}
			string HDSerial = "";
			foreach (HardDiskInfo hdinfo in info.HardDisks)
			{
				if (!string.IsNullOrEmpty(hdinfo.SerialNumber))
				{
					if (hdinfo.BusType == "ATA" || hdinfo.BusType == "SCSI")
					{
						HDSerial = hdinfo.SerialNumber;
						break;
					}
					if (string.IsNullOrEmpty(HDSerial))
					{
						HDSerial = hdinfo.SerialNumber;
					}
				}
			}
			string HDModel = "";
			foreach (HardDiskInfo hdinfo2 in info.HardDisks)
			{
				if (!string.IsNullOrEmpty(hdinfo2.Model))
				{
					if (hdinfo2.BusType == "ATA" || hdinfo2.BusType == "SCSI")
					{
						HDModel = hdinfo2.Model;
						break;
					}
					if (string.IsNullOrEmpty(HDModel))
					{
						HDModel = hdinfo2.Model;
					}
				}
			}
			MD5HashProvider hasher = new MD5HashProvider();
			string hashstring = mac + HDSerial + procID + HDModel;
			byte[] strData = Encoding.UTF8.GetBytes(hashstring);
			this._hash = hasher.Compute(strData);
		}

		private Hash _hash;

		private static MachineID _localID;
	}
}
