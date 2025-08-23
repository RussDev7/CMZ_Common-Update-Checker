using System;
using System.ComponentModel;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using DNA.Runtime.InteropServices;
using DNA.Runtime.InteropServices.Win32;

namespace DNA.Diagnostics.Instrumentation
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[Serializable]
	public class HardDiskInfo
	{
		private static string GetInterfaceString(HardDiskInfo.StorageBusType sbt)
		{
			switch (sbt)
			{
			case HardDiskInfo.StorageBusType.Scsi:
				return "SCSI";
			case HardDiskInfo.StorageBusType.Atapi:
				return "ATAPI";
			case HardDiskInfo.StorageBusType.Ata:
				return "ATA";
			case HardDiskInfo.StorageBusType._1394:
				return "1394";
			case HardDiskInfo.StorageBusType.Ssa:
				return "SSA";
			case HardDiskInfo.StorageBusType.Fibre:
				return "FIBRE";
			case HardDiskInfo.StorageBusType.Usb:
				return "USB";
			case HardDiskInfo.StorageBusType.RAID:
				return "RAID";
			default:
				return "Unknown";
			}
		}

		public string Model
		{
			get
			{
				return this._model;
			}
		}

		public string BusType
		{
			get
			{
				return this._busType;
			}
		}

		public string SerialNumber
		{
			get
			{
				return this._serial;
			}
		}

		public string FriendlySerialNumber
		{
			get
			{
				return HardDiskInfo.FlipAndCodeBytes(this._serial);
			}
		}

		public ulong Size
		{
			get
			{
				return this._size;
			}
		}

		private static string FlipAndCodeBytes(string str)
		{
			StringBuilder sb = new StringBuilder();
			int strlen = str.Length;
			for (int i = 0; i < strlen; i += 4)
			{
				for (int j = 2; j >= 0; j -= 2)
				{
					int sum = 0;
					for (int k = 0; k < 2; k++)
					{
						sum <<= 4;
						if (i + k + j >= strlen)
						{
							break;
						}
						char c = char.ToLower(str[i + k + j]);
						if (char.IsWhiteSpace(c))
						{
							c = '0';
						}
						char c2 = c;
						switch (c2)
						{
						case '0':
							sum = sum;
							break;
						case '1':
							sum++;
							break;
						case '2':
							sum += 2;
							break;
						case '3':
							sum += 3;
							break;
						case '4':
							sum += 4;
							break;
						case '5':
							sum += 5;
							break;
						case '6':
							sum += 6;
							break;
						case '7':
							sum += 7;
							break;
						case '8':
							sum += 8;
							break;
						case '9':
							sum += 9;
							break;
						default:
							switch (c2)
							{
							case 'a':
								sum += 10;
								break;
							case 'b':
								sum += 11;
								break;
							case 'c':
								sum += 12;
								break;
							case 'd':
								sum += 13;
								break;
							case 'e':
								sum += 14;
								break;
							case 'f':
								sum += 15;
								break;
							default:
								return str;
							}
							break;
						}
					}
					if (sum != 0)
					{
						sb.Append((char)sum);
					}
				}
			}
			string result = sb.ToString();
			return result.Trim();
		}

		public HardDiskInfo(string model, string type, string serial, ulong size)
		{
			this._model = model;
			this._busType = type;
			this._serial = serial;
			this._size = size;
		}

		public static HardDiskInfo GetPhysicalDriveInfo(string physicalDriveName)
		{
			int size = Marshal.SizeOf(typeof(HardDiskInfo.StoragePropertyQuery));
			GlobalBuffer inbuffer = new GlobalBuffer(size);
			GlobalBuffer outbuffer = new GlobalBuffer(10000);
			IntPtr hPhysicalDriveIOCTL = Win32API.CreateFile(physicalDriveName, Win32FileAccess.Default, Win32FileShare.ShareRead | Win32FileShare.ShareWrite, null, CreationDisposition.OpenExisting, Win32FileFlags.None, IntPtr.Zero);
			Marshal.StructureToPtr(new HardDiskInfo.StoragePropertyQuery
			{
				PropertyId = HardDiskInfo.StoragePropertyID.DeviceProperty,
				QueryType = HardDiskInfo.StorageQueryType.StandardQuery
			}, inbuffer.Pointer, true);
			uint cbBytesReturned = Win32API.DeviceIoControl(hPhysicalDriveIOCTL, IOControlCode.StorageQueryProperty, inbuffer.Pointer, inbuffer.Size, outbuffer.Pointer, outbuffer.Size, null);
			HardDiskInfo.StorageDeviceDescriptor descrip = (HardDiskInfo.StorageDeviceDescriptor)Marshal.PtrToStructure(outbuffer.Pointer, typeof(HardDiskInfo.StorageDeviceDescriptor));
			byte[] moutbuffer = new byte[cbBytesReturned];
			Marshal.Copy(outbuffer.Pointer, moutbuffer, 0, (int)cbBytesReturned);
			if (descrip.VendorIdOffset > 0U)
			{
				Marshal.PtrToStringAnsi(new IntPtr((long)outbuffer.Pointer + (long)((ulong)descrip.VendorIdOffset)));
			}
			string productID = "";
			if (descrip.ProductIdOffset > 0U)
			{
				productID = Marshal.PtrToStringAnsi(new IntPtr((long)outbuffer.Pointer + (long)((ulong)descrip.ProductIdOffset)));
			}
			if (descrip.ProductRevisionOffset > 0U)
			{
				Marshal.PtrToStringAnsi(new IntPtr((long)outbuffer.Pointer + (long)((ulong)descrip.ProductRevisionOffset)));
			}
			string serial = "";
			if (descrip.SerialNumberOffset > 0U)
			{
				serial = Marshal.PtrToStringAnsi(new IntPtr((long)outbuffer.Pointer + (long)((ulong)descrip.SerialNumberOffset)));
			}
			serial = HardDiskInfo.FlipAndCodeBytes(serial);
			Win32API.CloseHandle(hPhysicalDriveIOCTL);
			return new HardDiskInfo(productID, HardDiskInfo.GetInterfaceString(descrip.BusType), serial, 0UL);
		}

		public static HardDiskInfo FromManagmentObject(ManagementObject wmi_HD)
		{
			HardDiskInfo hdinfo = null;
			string busType = "Unknown";
			try
			{
				object obj = wmi_HD["DeviceID"];
				wmi_HD["DeviceID"].ToString();
				hdinfo = HardDiskInfo.GetPhysicalDriveInfo(wmi_HD["DeviceID"].ToString());
				busType = hdinfo._busType;
			}
			catch
			{
				try
				{
					busType = wmi_HD["InterfaceType"].ToString();
				}
				catch
				{
				}
			}
			string serial = "";
			try
			{
				object serialObj = wmi_HD["SerialNumber"];
				if (serialObj != null)
				{
					serial = serialObj.ToString();
				}
			}
			catch
			{
				if (hdinfo != null)
				{
					serial = hdinfo.SerialNumber;
				}
			}
			string model = "";
			try
			{
				object modelObj = wmi_HD["Model"];
				if (modelObj != null)
				{
					model = modelObj.ToString();
				}
			}
			catch
			{
				if (hdinfo != null)
				{
					model = hdinfo.Model;
				}
			}
			ulong size = 0UL;
			try
			{
				size = (ulong)wmi_HD["Size"];
			}
			catch
			{
				if (hdinfo != null)
				{
					size = hdinfo.Size;
				}
			}
			return new HardDiskInfo(model, busType, serial, size);
		}

		public override string ToString()
		{
			return "HardDrive: " + this.Model.ToString();
		}

		private string _model;

		private string _busType;

		private string _serial;

		private ulong _size;

		private enum StorageQueryType : uint
		{
			StandardQuery,
			ExistsQuery,
			MaskQuery,
			QueryMaxDefined
		}

		private enum StoragePropertyID : uint
		{
			DeviceProperty,
			AdapterProperty
		}

		private struct StoragePropertyQuery
		{
			public HardDiskInfo.StoragePropertyID PropertyId;

			public HardDiskInfo.StorageQueryType QueryType;

			private byte AdditionalParameters;
		}

		private enum StorageBusType : uint
		{
			Unknown,
			Scsi,
			Atapi,
			Ata,
			_1394,
			Ssa,
			Fibre,
			Usb,
			RAID
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct StorageDeviceDescriptor
		{
			public uint Version;

			public uint Size;

			public byte DeviceType;

			public byte DeviceTypeModifier;

			[MarshalAs(UnmanagedType.I1)]
			public bool RemovableMedia;

			[MarshalAs(UnmanagedType.I1)]
			public bool CommandQueueing;

			public uint VendorIdOffset;

			public uint ProductIdOffset;

			public uint ProductRevisionOffset;

			public uint SerialNumberOffset;

			public HardDiskInfo.StorageBusType BusType;

			public uint RawPropertiesLength;

			public byte RawDeviceProperties;
		}
	}
}
