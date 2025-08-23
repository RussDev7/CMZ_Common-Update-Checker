using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using DNA.Diagnostics.Instrumentation;

namespace DNA.Runtime.InteropServices.Win32.IPHelper
{
	public static class IPHelperAPI
	{
		[DllImport("Iphlpapi.dll")]
		private static extern Win32ErrorCode GetAdaptersInfo(IntPtr data, ref uint pOutBufLen);

		public static NetworkAdapterInfo[] GetAdaptersInfo()
		{
			List<NetworkAdapterInfo> devices = new List<NetworkAdapterInfo>();
			uint size = 0U;
			if (IPHelperAPI.GetAdaptersInfo(IntPtr.Zero, ref size) == Win32ErrorCode.Success)
			{
				return new NetworkAdapterInfo[0];
			}
			GlobalBuffer buffer = new GlobalBuffer(size);
			Win32ErrorCode ret = IPHelperAPI.GetAdaptersInfo(buffer.Pointer, ref size);
			if (ret != Win32ErrorCode.Success)
			{
				throw new Win32Exception((int)ret);
			}
			Marshal.SizeOf(typeof(IPHelperAPI.IPAdapterInfo));
			IntPtr curPtr = buffer.Pointer;
			do
			{
				IPHelperAPI.IPAdapterInfo adapterInfo = (IPHelperAPI.IPAdapterInfo)Marshal.PtrToStructure(curPtr, typeof(IPHelperAPI.IPAdapterInfo));
				ulong macAddress = 0UL;
				int i = 0;
				while ((long)i < (long)((ulong)adapterInfo.AddressLength))
				{
					ulong orval = (ulong)adapterInfo.Address[i] << i * 8;
					macAddress |= orval;
					i++;
				}
				string macstring = "";
				for (int j = 0; j < 6; j++)
				{
					macstring += ((macAddress >> 8 * j) & 255UL).ToString("X2");
					if (j != 5)
					{
						macstring += ":";
					}
				}
				devices.Add(new NetworkAdapterInfo(macstring, new IPAddress[0], true));
				curPtr = adapterInfo.Next;
			}
			while (curPtr != IntPtr.Zero);
			return devices.ToArray();
		}

		private struct IPAddressString
		{
			public IntPtr Next;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
			public string IPAddress;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
			public string IpMask;
		}

		private struct IPAdapterInfo
		{
			public IntPtr Next;

			public uint ComboIndex;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string AdapterName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 132)]
			public string Description;

			public uint AddressLength;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] Address;

			public uint Index;

			public uint Type;

			public uint DhcpEnabled;

			public IntPtr CurrentIpAddress;

			public IPHelperAPI.IPAddressString IpAddressList;

			public IPHelperAPI.IPAddressString GatewayList;

			public IPHelperAPI.IPAddressString DhcpServer;

			public bool HaveWins;

			public IPHelperAPI.IPAddressString PrimaryWinsServer;

			public IPHelperAPI.IPAddressString SecondaryWinsServer;

			public uint LeaseObtained;

			public uint LeaseExpires;
		}
	}
}
