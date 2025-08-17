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
			StringBuilder stringBuilder = new StringBuilder();
			int length = str.Length;
			for (int i = 0; i < length; i += 4)
			{
				for (int j = 2; j >= 0; j -= 2)
				{
					int num = 0;
					for (int k = 0; k < 2; k++)
					{
						num <<= 4;
						if (i + k + j >= length)
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
							num = num;
							break;
						case '1':
							num++;
							break;
						case '2':
							num += 2;
							break;
						case '3':
							num += 3;
							break;
						case '4':
							num += 4;
							break;
						case '5':
							num += 5;
							break;
						case '6':
							num += 6;
							break;
						case '7':
							num += 7;
							break;
						case '8':
							num += 8;
							break;
						case '9':
							num += 9;
							break;
						default:
							switch (c2)
							{
							case 'a':
								num += 10;
								break;
							case 'b':
								num += 11;
								break;
							case 'c':
								num += 12;
								break;
							case 'd':
								num += 13;
								break;
							case 'e':
								num += 14;
								break;
							case 'f':
								num += 15;
								break;
							default:
								return str;
							}
							break;
						}
					}
					if (num != 0)
					{
						stringBuilder.Append((char)num);
					}
				}
			}
			string text = stringBuilder.ToString();
			return text.Trim();
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
			int num = Marshal.SizeOf(typeof(HardDiskInfo.StoragePropertyQuery));
			GlobalBuffer globalBuffer = new GlobalBuffer(num);
			GlobalBuffer globalBuffer2 = new GlobalBuffer(10000);
			IntPtr intPtr = Win32API.CreateFile(physicalDriveName, Win32FileAccess.Default, Win32FileShare.ShareRead | Win32FileShare.ShareWrite, null, CreationDisposition.OpenExisting, Win32FileFlags.None, IntPtr.Zero);
			Marshal.StructureToPtr(new HardDiskInfo.StoragePropertyQuery
			{
				PropertyId = HardDiskInfo.StoragePropertyID.DeviceProperty,
				QueryType = HardDiskInfo.StorageQueryType.StandardQuery
			}, globalBuffer.Pointer, true);
			uint num2 = Win32API.DeviceIoControl(intPtr, IOControlCode.StorageQueryProperty, globalBuffer.Pointer, globalBuffer.Size, globalBuffer2.Pointer, globalBuffer2.Size, null);
			HardDiskInfo.StorageDeviceDescriptor storageDeviceDescriptor = (HardDiskInfo.StorageDeviceDescriptor)Marshal.PtrToStructure(globalBuffer2.Pointer, typeof(HardDiskInfo.StorageDeviceDescriptor));
			byte[] array = new byte[num2];
			Marshal.Copy(globalBuffer2.Pointer, array, 0, (int)num2);
			if (storageDeviceDescriptor.VendorIdOffset > 0U)
			{
				Marshal.PtrToStringAnsi(new IntPtr((long)globalBuffer2.Pointer + (long)((ulong)storageDeviceDescriptor.VendorIdOffset)));
			}
			string text = "";
			if (storageDeviceDescriptor.ProductIdOffset > 0U)
			{
				text = Marshal.PtrToStringAnsi(new IntPtr((long)globalBuffer2.Pointer + (long)((ulong)storageDeviceDescriptor.ProductIdOffset)));
			}
			if (storageDeviceDescriptor.ProductRevisionOffset > 0U)
			{
				Marshal.PtrToStringAnsi(new IntPtr((long)globalBuffer2.Pointer + (long)((ulong)storageDeviceDescriptor.ProductRevisionOffset)));
			}
			string text2 = "";
			if (storageDeviceDescriptor.SerialNumberOffset > 0U)
			{
				text2 = Marshal.PtrToStringAnsi(new IntPtr((long)globalBuffer2.Pointer + (long)((ulong)storageDeviceDescriptor.SerialNumberOffset)));
			}
			text2 = HardDiskInfo.FlipAndCodeBytes(text2);
			Win32API.CloseHandle(intPtr);
			return new HardDiskInfo(text, HardDiskInfo.GetInterfaceString(storageDeviceDescriptor.BusType), text2, 0UL);
		}

		public static HardDiskInfo FromManagmentObject(ManagementObject wmi_HD)
		{
			HardDiskInfo hardDiskInfo = null;
			string text = "Unknown";
			try
			{
				object obj = wmi_HD["DeviceID"];
				wmi_HD["DeviceID"].ToString();
				hardDiskInfo = HardDiskInfo.GetPhysicalDriveInfo(wmi_HD["DeviceID"].ToString());
				text = hardDiskInfo._busType;
			}
			catch
			{
				try
				{
					text = wmi_HD["InterfaceType"].ToString();
				}
				catch
				{
				}
			}
			string text2 = "";
			try
			{
				object obj2 = wmi_HD["SerialNumber"];
				if (obj2 != null)
				{
					text2 = obj2.ToString();
				}
			}
			catch
			{
				if (hardDiskInfo != null)
				{
					text2 = hardDiskInfo.SerialNumber;
				}
			}
			string text3 = "";
			try
			{
				object obj3 = wmi_HD["Model"];
				if (obj3 != null)
				{
					text3 = obj3.ToString();
				}
			}
			catch
			{
				if (hardDiskInfo != null)
				{
					text3 = hardDiskInfo.Model;
				}
			}
			ulong num = 0UL;
			try
			{
				num = (ulong)wmi_HD["Size"];
			}
			catch
			{
				if (hardDiskInfo != null)
				{
					num = hardDiskInfo.Size;
				}
			}
			return new HardDiskInfo(text3, text, text2, num);
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
