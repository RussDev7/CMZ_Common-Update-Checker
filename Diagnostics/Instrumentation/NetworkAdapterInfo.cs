using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management;
using System.Net;

namespace DNA.Diagnostics.Instrumentation
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[Serializable]
	public class NetworkAdapterInfo
	{
		public bool Physical
		{
			get
			{
				return this._physical;
			}
		}

		public string MACAddress
		{
			get
			{
				return this._macAddress;
			}
		}

		public IPAddress[] IPAddresses
		{
			get
			{
				return this._ipAddreses;
			}
		}

		public override string ToString()
		{
			return this._macAddress;
		}

		public NetworkAdapterInfo(string macAddress, IPAddress[] ipaddresses, bool physical)
		{
			this._macAddress = macAddress;
			this._ipAddreses = ipaddresses;
			this._physical = physical;
		}

		public static NetworkAdapterInfo FromManagmentObject(ManagementObject mo, bool physical)
		{
			string text = (string)mo["MacAddress"];
			string[] array = (string[])mo["IPAddress"];
			List<IPAddress> list = new List<IPAddress>();
			if (array != null)
			{
				foreach (string text2 in array)
				{
					list.Add(IPAddress.Parse(text2));
				}
			}
			return new NetworkAdapterInfo(text, list.ToArray(), physical);
		}

		private IPAddress[] _ipAddreses;

		private bool _physical;

		private string _macAddress;
	}
}
