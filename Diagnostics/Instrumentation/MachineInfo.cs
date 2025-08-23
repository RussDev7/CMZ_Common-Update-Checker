using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management;
using System.Net;
using System.Text;
using DNA.Runtime.InteropServices.Win32;
using DNA.Runtime.InteropServices.Win32.IPHelper;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Diagnostics.Instrumentation
{
	public class MachineInfo
	{
		public static MachineInfo LocalInfo
		{
			get
			{
				if (MachineInfo._localInfo == null)
				{
					MachineInfo._localInfo = new MachineInfo();
					MachineInfo._localInfo.GetLocalInfo();
				}
				else
				{
					MachineInfo._localInfo.UpdateLocalInfo();
				}
				return MachineInfo._localInfo;
			}
		}

		public bool ManagementSupported
		{
			get
			{
				return this._wmiSupported;
			}
		}

		public string MachineName
		{
			get
			{
				return this._machineName;
			}
		}

		public string CurrentUser
		{
			get
			{
				return this._currentUser;
			}
		}

		public OperatingSystem OSVersion
		{
			get
			{
				return this._osVersion;
			}
		}

		public bool IsOS64Bit
		{
			get
			{
				return this._64BitOS;
			}
		}

		public Version CLRVersion
		{
			get
			{
				return this._CLRVersion;
			}
		}

		public MemoryInfo MemoryInfo
		{
			get
			{
				return this._memoryInfo;
			}
		}

		public HardDiskInfo[] HardDisks
		{
			get
			{
				return this._hardDisks;
			}
		}

		public NetworkAdapterInfo[] NetworkAdapters
		{
			get
			{
				return this._networkAdapters;
			}
		}

		public ProcessorInfo[] Processors
		{
			get
			{
				return this._processors;
			}
		}

		public LogicalDriveInfo[] LogicalDrives
		{
			get
			{
				return this._logicalDrives;
			}
		}

		public string GetReport()
		{
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			try
			{
				sw.WriteLine("Computer Details:");
				sw.WriteLine("Machine Name:\t" + this.MachineName);
				sw.WriteLine("Current Time:\t" + DateTime.Now.ToString("F", CultureInfo.CreateSpecificCulture("en-US")));
				sw.WriteLine("Current User:\t" + this.CurrentUser);
				sw.WriteLine(string.Concat(new string[]
				{
					"OS:\t\t",
					this.OSVersion.Platform.ToString(),
					"(",
					this.OSVersion.Version.ToString(),
					")"
				}));
				sw.WriteLine("64bit:\t\t" + (this.IsOS64Bit ? "Yes" : "No"));
				sw.WriteLine("CLR:\t\t" + this.CLRVersion);
				sw.WriteLine();
				if (this._wmiSupported)
				{
					int load = (int)Math.Round((double)(this.MemoryInfo.MemoryLoad * 100f));
					sw.WriteLine("Memory:");
					sw.WriteLine("\tLoad:\t\t\t" + load.ToString() + "%");
					sw.WriteLine("\tWorking Set:\t\t" + this.MemoryInfo.WorkingSet.ToString("n0") + "\tbytes");
					sw.WriteLine("\tTotal:");
					sw.WriteLine("\t\tPhysical:\t\t" + this.MemoryInfo.TotalPhysical.ToString("n0") + "\tbytes");
					sw.WriteLine("\t\tPage File:\t\t" + this.MemoryInfo.TotalPageFile.ToString("n0") + "\tbytes");
					sw.WriteLine("\t\tVirtual:\t\t" + this.MemoryInfo.TotalVirtual.ToString("n0") + "\tbytes");
					sw.WriteLine("\tAvailible:");
					sw.WriteLine("\t\tPhysical:\t\t" + this.MemoryInfo.AvailiblePhysical.ToString("n0") + "\tbytes");
					sw.WriteLine("\t\tPage File:\t\t" + this.MemoryInfo.AvailiblePageFile.ToString("n0") + "\tbytes");
					sw.WriteLine("\t\tVirtual:\t\t" + this.MemoryInfo.AvailibleVirtual.ToString("n0") + "\tbytes");
					sw.WriteLine("\t\tExtended Virtual:\t" + this.MemoryInfo.AvailibleExtendedVirtual.ToString("n0") + "\tbytes");
					sw.WriteLine();
					sw.WriteLine("Proccessor Info:");
					sw.WriteLine("\tCount:\t" + this.Processors.Length);
					for (int i = 0; i < this.Processors.Length; i++)
					{
						ProcessorInfo pinfo = this.Processors[i];
						sw.WriteLine("\tProcessor[" + i.ToString() + "]:");
						sw.WriteLine("\t\tName:\t\t" + pinfo.Name);
						sw.WriteLine("\t\tManufacturer:\t" + pinfo.Manufacturer);
						sw.WriteLine("\t\tID:\t\t" + pinfo.ProcessorID);
						sw.WriteLine("\t\tClockSpeedID:\t" + pinfo.MaxClockSpeed.ToString("n0") + " Mhz");
						sw.WriteLine("\t\tCores:\t\t" + pinfo.NumberOfCores.ToString());
						sw.WriteLine("\t\tLogicalProcessors:\t" + pinfo.NumberOfLogicalProcessors.ToString());
						string idstr = "NoID";
						if (pinfo.UniqueID != null)
						{
							idstr = pinfo.UniqueID;
						}
						sw.WriteLine("\t\tUniqueID:\t" + idstr);
					}
					sw.WriteLine();
					sw.WriteLine("HardDisks:");
					sw.WriteLine("\tCount:\t" + this.HardDisks.Length.ToString());
					for (int j = 0; j < this.HardDisks.Length; j++)
					{
						HardDiskInfo hdinfo = this.HardDisks[j];
						sw.WriteLine("\tDisk[" + j.ToString() + "]:");
						sw.WriteLine("\t\tModel:\t\t\t" + hdinfo.Model);
						sw.WriteLine("\t\tBusType:\t\t\t" + hdinfo.BusType);
						string serialStr = "No Serial Number";
						if (hdinfo.SerialNumber != null)
						{
							serialStr = hdinfo.SerialNumber;
						}
						sw.WriteLine("\t\tSerial Number:\t\t" + serialStr);
						string friendSerialStr = "No Serial Number";
						if (hdinfo.SerialNumber != null)
						{
							friendSerialStr = hdinfo.FriendlySerialNumber;
						}
						sw.WriteLine("\t\tFriendly Serial Number:\t" + friendSerialStr);
					}
					sw.WriteLine();
					sw.WriteLine("Logical Drives:");
					sw.WriteLine("\tCount:" + this.LogicalDrives.Length);
					for (int k = 0; k < this.LogicalDrives.Length; k++)
					{
						LogicalDriveInfo driveInfo = this.LogicalDrives[k];
						sw.WriteLine("\t" + driveInfo.Name);
						sw.WriteLine("\t\tType:\t\t" + driveInfo.DriveType.ToString());
						sw.WriteLine("\t\tTotal Size:\t" + driveInfo.Size.ToString("n0") + " bytes");
						sw.WriteLine("\t\tFree Space:\t" + driveInfo.FreeSpace.ToString("n0") + " bytes");
					}
					sw.WriteLine();
					sw.WriteLine("Network Adapters:");
					sw.WriteLine("\tCount:" + this.NetworkAdapters.Length.ToString());
					for (int l = 0; l < this.NetworkAdapters.Length; l++)
					{
						NetworkAdapterInfo netInfo = this.NetworkAdapters[l];
						sw.WriteLine("\tAdapter[" + l.ToString() + "]:");
						sw.WriteLine("\t\tMAC Address:\t" + netInfo.MACAddress);
						sw.WriteLine("\t\tIP Addresses:");
						foreach (IPAddress adr in netInfo.IPAddresses)
						{
							sw.WriteLine("\t\t\t" + adr.ToString());
						}
					}
				}
				else
				{
					sw.WriteLine("WMI Not Supported");
				}
				sw.WriteLine();
				sw.WriteLine("Video Hardware Details:");
				int adapterCount = GraphicsAdapter.Adapters.Count;
				sw.WriteLine("\tAdapter Count:\t" + adapterCount.ToString());
				for (int deviceID = 0; deviceID < adapterCount; deviceID++)
				{
					GraphicsAdapter details = GraphicsAdapter.Adapters[deviceID];
					sw.WriteLine("\tAdapter[" + deviceID.ToString() + "]:");
					sw.WriteLine("\t\tName:\t\t" + details.DeviceName);
					sw.WriteLine("\t\tDescription:\t" + details.Description);
					sw.WriteLine("\t\tRevision:\t\t" + details.Revision.ToString());
					sw.WriteLine("\t\tHiDef Support:\t" + (details.IsProfileSupported(GraphicsProfile.HiDef) ? "Yes" : "No"));
					sw.WriteLine("\t\tReach Support:\t" + (details.IsProfileSupported(GraphicsProfile.Reach) ? "Yes" : "No"));
					sw.WriteLine("\t\tID:\t\t" + details.DeviceId.ToString());
					sw.WriteLine("\t\tVendor ID:\t" + details.VendorId.ToString("X"));
					sw.WriteLine("\t\tSub System ID:\t" + details.SubSystemId.ToString("X"));
					sw.WriteLine("\t\tWideScreen:\t" + (details.IsWideScreen ? "Yes" : "No"));
					sw.WriteLine();
					sw.WriteLine("\tSupported Display Modes:");
					foreach (DisplayMode mode in details.SupportedDisplayModes)
					{
						sw.WriteLine("\t\t" + mode.ToString());
					}
					sw.WriteLine();
					sw.WriteLine("\tCurrent Display Mode:" + details.CurrentDisplayMode.ToString());
					sw.WriteLine();
					sw.WriteLine();
				}
			}
			catch (Exception e)
			{
				sw.WriteLine(e.Message + "\n" + e.StackTrace);
			}
			sw.Flush();
			return sb.ToString();
		}

		private static int CompareProcsByMemory(Process p1, Process p2)
		{
			return p2.WorkingSet64.CompareTo(p1.WorkingSet64);
		}

		public override string ToString()
		{
			return this.GetReport();
		}

		private void UpdateLocalInfo()
		{
			this.GetMemory();
		}

		private void GetLocalInfo()
		{
			this._machineName = Environment.MachineName;
			this._currentUser = Environment.UserDomainName + "\\" + Environment.UserName;
			this._osVersion = Environment.OSVersion;
			this._CLRVersion = Environment.Version;
			this._64BitOS = Environment.Is64BitOperatingSystem;
			try
			{
				this.GetProcessors();
				this.GetLogicalDrives();
				this._wmiSupported = true;
			}
			catch
			{
				this._logicalDrives = new LogicalDriveInfo[0];
				this._processors = new ProcessorInfo[0];
				this._wmiSupported = false;
			}
			this.GetHardDisks();
			this.GetNetworkAdapters();
			this.UpdateLocalInfo();
			this._machineID = new MachineID(this);
		}

		private void GetProcessors()
		{
			List<ProcessorInfo> cpus = new List<ProcessorInfo>();
			ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
			foreach (ManagementBaseObject managementBaseObject in searcher.Get())
			{
				ManagementObject mo = (ManagementObject)managementBaseObject;
				cpus.Add(new ProcessorInfo(mo));
			}
			this._processors = cpus.ToArray();
		}

		private void GetMemory()
		{
			Win32MemoryStatusEx status = Win32API.GlobalMemoryStatus();
			this._memoryInfo = new MemoryInfo(status);
		}

		private void GetHardDisks()
		{
			List<HardDiskInfo> hdCollection = new List<HardDiskInfo>();
			if (this._wmiSupported)
			{
				ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
				using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = searcher.Get().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ManagementBaseObject managementBaseObject = enumerator.Current;
						ManagementObject wmi_HD = (ManagementObject)managementBaseObject;
						hdCollection.Add(HardDiskInfo.FromManagmentObject(wmi_HD));
					}
					goto IL_008A;
				}
			}
			for (int i = 0; i < 16; i++)
			{
				string driveName = "\\\\.\\PhysicalDrive" + i.ToString();
				try
				{
					hdCollection.Add(HardDiskInfo.GetPhysicalDriveInfo(driveName));
				}
				catch
				{
				}
			}
			IL_008A:
			this._hardDisks = hdCollection.ToArray();
		}

		private void GetLogicalDrives()
		{
			List<LogicalDriveInfo> hdCollection = new List<LogicalDriveInfo>();
			ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");
			foreach (ManagementBaseObject managementBaseObject in searcher.Get())
			{
				ManagementObject wmi_HD = (ManagementObject)managementBaseObject;
				hdCollection.Add(new LogicalDriveInfo(wmi_HD));
			}
			this._logicalDrives = hdCollection.ToArray();
		}

		private static void PrintObjectProperties(ManagementObject mo)
		{
			foreach (PropertyData pd in mo.Properties)
			{
				if (pd.Value != null)
				{
					Console.WriteLine(pd.Name + " = " + pd.Value.ToString());
				}
			}
			Console.WriteLine();
		}

		private void GetNetworkAdapters()
		{
			if (this._wmiSupported)
			{
				ManagementClass nicClass = new ManagementClass("Win32_NetworkAdapter");
				ManagementObjectCollection nicMocs = nicClass.GetInstances();
				Dictionary<string, ManagementObject> physicalAdapters = new Dictionary<string, ManagementObject>();
				Dictionary<string, bool> isPhysical = new Dictionary<string, bool>();
				foreach (ManagementBaseObject managementBaseObject in nicMocs)
				{
					ManagementObject nicMo = (ManagementObject)managementBaseObject;
					try
					{
						ushort type = (ushort)nicMo["AdapterTypeID"];
						if (type != 0)
						{
							continue;
						}
					}
					catch
					{
						continue;
					}
					bool isRealAdapter = false;
					string devID = (string)nicMo["DeviceID"];
					try
					{
						isRealAdapter = (bool)nicMo["PhysicalAdapter"];
						if (!isRealAdapter)
						{
							continue;
						}
					}
					catch
					{
						ManagementObjectSearcher searcher = new ManagementObjectSearcher("ASSOCIATORS OF {Win32_NetworkAdapter.DeviceID='" + devID + "'} WHERE ResultClass = Win32_PnPEntity");
						ManagementObjectCollection nicAss = searcher.Get();
						if (nicAss.Count != 0)
						{
							isRealAdapter = true;
						}
					}
					if (isRealAdapter)
					{
						string macAddress = (string)nicMo["MacAddress"];
						physicalAdapters[macAddress] = nicMo;
						ManagementObjectSearcher searcher2 = new ManagementObjectSearcher("ASSOCIATORS OF {Win32_NetworkAdapter.DeviceID='" + devID + "'} WHERE ResultClass = Win32_IRQResource");
						ManagementObjectCollection nicAss2 = searcher2.Get();
						if (nicAss2.Count != 0)
						{
							isPhysical[macAddress] = true;
						}
						else if (!isPhysical.ContainsKey(macAddress))
						{
							isPhysical[macAddress] = false;
						}
					}
				}
				nicClass = new ManagementClass("Win32_NetworkAdapterConfiguration");
				nicMocs = nicClass.GetInstances();
				Dictionary<string, NetworkAdapterInfo> netInfos = new Dictionary<string, NetworkAdapterInfo>();
				foreach (ManagementBaseObject managementBaseObject2 in nicMocs)
				{
					ManagementObject mo = (ManagementObject)managementBaseObject2;
					try
					{
						string macAddress2 = (string)mo["MacAddress"];
						if (physicalAdapters.ContainsKey(macAddress2))
						{
							NetworkAdapterInfo netInfo = NetworkAdapterInfo.FromManagmentObject(mo, isPhysical[macAddress2]);
							NetworkAdapterInfo existingInfo;
							if (netInfos.TryGetValue(netInfo.MACAddress, out existingInfo))
							{
								if (netInfo.IPAddresses.Length > existingInfo.IPAddresses.Length)
								{
									netInfos[netInfo.MACAddress] = netInfo;
								}
							}
							else
							{
								netInfos[netInfo.MACAddress] = netInfo;
							}
						}
					}
					catch
					{
					}
				}
				NetworkAdapterInfo[] finalInfos = new NetworkAdapterInfo[netInfos.Values.Count];
				netInfos.Values.CopyTo(finalInfos, 0);
				this._networkAdapters = finalInfos;
				return;
			}
			this._networkAdapters = IPHelperAPI.GetAdaptersInfo();
		}

		private MachineInfo()
		{
		}

		private const int MaxIDEDrives = 16;

		private static MachineInfo _localInfo;

		private bool _wmiSupported;

		private MemoryInfo _memoryInfo;

		private HardDiskInfo[] _hardDisks = new HardDiskInfo[0];

		private LogicalDriveInfo[] _logicalDrives = new LogicalDriveInfo[0];

		private NetworkAdapterInfo[] _networkAdapters = new NetworkAdapterInfo[0];

		private ProcessorInfo[] _processors = new ProcessorInfo[0];

		private string _machineName;

		private string _currentUser;

		private Version _CLRVersion;

		private OperatingSystem _osVersion;

		private bool _64BitOS;

		private MachineID _machineID;
	}
}
