using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace DNA.Net.Lidgren
{
	public static class NetUtility
	{
		public static void ResolveAsync(string ipOrHost, int port, NetUtility.ResolveEndPointCallback callback)
		{
			NetUtility.ResolveAsync(ipOrHost, delegate(IPAddress adr)
			{
				if (adr == null)
				{
					callback(null);
					return;
				}
				callback(new IPEndPoint(adr, port));
			});
		}

		public static IPEndPoint Resolve(string ipOrHost, int port)
		{
			IPAddress adr = NetUtility.Resolve(ipOrHost);
			return new IPEndPoint(adr, port);
		}

		public static void ResolveAsync(string ipOrHost, NetUtility.ResolveAddressCallback callback)
		{
			NetUtility.LastResolveResult = SocketError.Success;
			if (string.IsNullOrEmpty(ipOrHost))
			{
				callback(null);
			}
			ipOrHost = ipOrHost.Trim();
			if (string.IsNullOrEmpty(ipOrHost))
			{
				callback(null);
			}
			IPAddress ipAddress = null;
			if (!IPAddress.TryParse(ipOrHost, out ipAddress))
			{
				try
				{
					IPHostEntry entry;
					Dns.BeginGetHostEntry(ipOrHost, delegate(IAsyncResult result)
					{
						try
						{
							entry = Dns.EndGetHostEntry(result);
						}
						catch (SocketException ex2)
						{
							NetUtility.LastResolveResult = ex2.SocketErrorCode;
							entry = null;
						}
						if (entry == null)
						{
							callback(null);
							return;
						}
						foreach (IPAddress ipCurrent in entry.AddressList)
						{
							if (ipCurrent.AddressFamily == AddressFamily.InterNetwork)
							{
								callback(ipCurrent);
								return;
							}
						}
						callback(null);
					}, null);
				}
				catch (SocketException ex)
				{
					NetUtility.LastResolveResult = ex.SocketErrorCode;
					callback(null);
				}
				return;
			}
			if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
			{
				callback(ipAddress);
				return;
			}
			throw new ArgumentException("This method will not currently resolve other than ipv4 addresses");
		}

		public static IPAddress Resolve(string ipOrHost)
		{
			NetUtility.LastResolveResult = SocketError.Success;
			if (string.IsNullOrEmpty(ipOrHost))
			{
				return null;
			}
			ipOrHost = ipOrHost.Trim();
			if (string.IsNullOrEmpty(ipOrHost))
			{
				return null;
			}
			IPAddress ipAddress = null;
			if (!IPAddress.TryParse(ipOrHost, out ipAddress))
			{
				IPAddress ipaddress;
				try
				{
					IPHostEntry entry = Dns.GetHostEntry(ipOrHost);
					if (entry == null)
					{
						ipaddress = null;
					}
					else
					{
						foreach (IPAddress ipCurrent in entry.AddressList)
						{
							if (ipCurrent.AddressFamily == AddressFamily.InterNetwork)
							{
								return ipCurrent;
							}
						}
						ipaddress = null;
					}
				}
				catch (SocketException ex)
				{
					NetUtility.LastResolveResult = ex.SocketErrorCode;
					ipaddress = null;
				}
				return ipaddress;
			}
			if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
			{
				return ipAddress;
			}
			return null;
		}

		private static NetworkInterface GetNetworkInterface()
		{
			if (IPGlobalProperties.GetIPGlobalProperties() == null)
			{
				return null;
			}
			NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
			if (nics == null || nics.Length < 1)
			{
				return null;
			}
			NetworkInterface best = null;
			foreach (NetworkInterface adapter in nics)
			{
				if (adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback && adapter.NetworkInterfaceType != NetworkInterfaceType.Unknown && adapter.Supports(NetworkInterfaceComponent.IPv4))
				{
					if (best == null)
					{
						best = adapter;
					}
					if (adapter.OperationalStatus == OperationalStatus.Up)
					{
						return adapter;
					}
				}
			}
			return best;
		}

		public static PhysicalAddress GetMacAddress()
		{
			NetworkInterface ni = NetUtility.GetNetworkInterface();
			if (ni == null)
			{
				return null;
			}
			return ni.GetPhysicalAddress();
		}

		public static string ToHexString(long data)
		{
			return NetUtility.ToHexString(BitConverter.GetBytes(data));
		}

		public static string ToHexString(byte[] data)
		{
			char[] c = new char[data.Length * 2];
			for (int i = 0; i < data.Length; i++)
			{
				byte b = (byte)(data[i] >> 4);
				c[i * 2] = (char)((b > 9) ? (b + 55) : (b + 48));
				b = data[i] & 15;
				c[i * 2 + 1] = (char)((b > 9) ? (b + 55) : (b + 48));
			}
			return new string(c);
		}

		public static IPAddress GetBroadcastAddress()
		{
			try
			{
				NetworkInterface ni = NetUtility.GetNetworkInterface();
				if (ni == null)
				{
					return null;
				}
				IPInterfaceProperties properties = ni.GetIPProperties();
				foreach (UnicastIPAddressInformation unicastAddress in properties.UnicastAddresses)
				{
					if (unicastAddress != null && unicastAddress.Address != null && unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
					{
						IPAddress mask = unicastAddress.IPv4Mask;
						byte[] ipAdressBytes = unicastAddress.Address.GetAddressBytes();
						byte[] subnetMaskBytes = mask.GetAddressBytes();
						if (ipAdressBytes.Length != subnetMaskBytes.Length)
						{
							throw new ArgumentException("Lengths of IP address and subnet mask do not match.");
						}
						byte[] broadcastAddress = new byte[ipAdressBytes.Length];
						for (int i = 0; i < broadcastAddress.Length; i++)
						{
							broadcastAddress[i] = ipAdressBytes[i] | (subnetMaskBytes[i] ^ byte.MaxValue);
						}
						return new IPAddress(broadcastAddress);
					}
				}
			}
			catch
			{
				return IPAddress.Broadcast;
			}
			return IPAddress.Broadcast;
		}

		public static IPAddress GetMyAddress(out IPAddress mask)
		{
			mask = null;
			NetworkInterface ni = NetUtility.GetNetworkInterface();
			if (ni == null)
			{
				mask = null;
				return null;
			}
			IPInterfaceProperties properties = ni.GetIPProperties();
			foreach (UnicastIPAddressInformation unicastAddress in properties.UnicastAddresses)
			{
				if (unicastAddress != null && unicastAddress.Address != null && unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
				{
					mask = unicastAddress.IPv4Mask;
					return unicastAddress.Address;
				}
			}
			return null;
		}

		public static bool IsLocal(IPEndPoint endPoint)
		{
			return endPoint != null && NetUtility.IsLocal(endPoint.Address);
		}

		public static bool IsLocal(IPAddress remote)
		{
			IPAddress mask;
			IPAddress local = NetUtility.GetMyAddress(out mask);
			if (mask == null)
			{
				return false;
			}
			uint maskBits = BitConverter.ToUInt32(mask.GetAddressBytes(), 0);
			uint remoteBits = BitConverter.ToUInt32(remote.GetAddressBytes(), 0);
			uint localBits = BitConverter.ToUInt32(local.GetAddressBytes(), 0);
			return (remoteBits & maskBits) == (localBits & maskBits);
		}

		[CLSCompliant(false)]
		public static int BitsToHoldUInt(uint value)
		{
			int bits = 1;
			while ((value >>= 1) != 0U)
			{
				bits++;
			}
			return bits;
		}

		public static int BytesToHoldBits(int numBits)
		{
			return (numBits + 7) / 8;
		}

		internal static uint SwapByteOrder(uint value)
		{
			return ((value & 4278190080U) >> 24) | ((value & 16711680U) >> 8) | ((value & 65280U) << 8) | ((value & 255U) << 24);
		}

		internal static ulong SwapByteOrder(ulong value)
		{
			return ((value & 18374686479671623680UL) >> 56) | ((value & 71776119061217280UL) >> 40) | ((value & 280375465082880UL) >> 24) | ((value & 1095216660480UL) >> 8) | ((value & (ulong)(-16777216)) << 8) | ((value & 16711680UL) << 24) | ((value & 65280UL) << 40) | ((value & 255UL) << 56);
		}

		internal static bool CompareElements(byte[] one, byte[] two)
		{
			if (one.Length != two.Length)
			{
				return false;
			}
			for (int i = 0; i < one.Length; i++)
			{
				if (one[i] != two[i])
				{
					return false;
				}
			}
			return true;
		}

		public static byte[] ToByteArray(string hexString)
		{
			byte[] retval = new byte[hexString.Length / 2];
			for (int i = 0; i < hexString.Length; i += 2)
			{
				retval[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
			}
			return retval;
		}

		public static string ToHumanReadable(long bytes)
		{
			if (bytes < 4000L)
			{
				return bytes + " bytes";
			}
			if (bytes < 1000000L)
			{
				return Math.Round((double)bytes / 1000.0, 2) + " kilobytes";
			}
			return Math.Round((double)bytes / 1000000.0, 2) + " megabytes";
		}

		internal static int RelativeSequenceNumber(int nr, int expected)
		{
			return (nr - expected + 1024 + 512) % 1024 - 512;
		}

		public static int GetWindowSize(NetDeliveryMethod method)
		{
			switch (method)
			{
			case NetDeliveryMethod.Unknown:
				return 0;
			case NetDeliveryMethod.Unreliable:
			case NetDeliveryMethod.UnreliableSequenced:
				return 128;
			default:
				switch (method)
				{
				case NetDeliveryMethod.ReliableUnordered:
				case NetDeliveryMethod.ReliableSequenced:
					break;
				default:
					if (method == NetDeliveryMethod.ReliableOrdered)
					{
						return 64;
					}
					break;
				}
				return 64;
			}
		}

		internal static void SortMembersList(MemberInfo[] list)
		{
			int h = 1;
			while (h * 3 + 1 <= list.Length)
			{
				h = 3 * h + 1;
			}
			while (h > 0)
			{
				for (int i = h - 1; i < list.Length; i++)
				{
					MemberInfo tmp = list[i];
					int j = i;
					while (j >= h && string.Compare(list[j - h].Name, tmp.Name, StringComparison.InvariantCulture) > 0)
					{
						list[j] = list[j - h];
						j -= h;
					}
					list[j] = tmp;
				}
				h /= 3;
			}
		}

		internal static NetDeliveryMethod GetDeliveryMethod(NetMessageType mtp)
		{
			if (mtp >= NetMessageType.UserReliableOrdered1)
			{
				return NetDeliveryMethod.ReliableOrdered;
			}
			if (mtp >= NetMessageType.UserReliableSequenced1)
			{
				return NetDeliveryMethod.ReliableSequenced;
			}
			if (mtp >= NetMessageType.UserReliableUnordered)
			{
				return NetDeliveryMethod.ReliableUnordered;
			}
			if (mtp >= NetMessageType.UserSequenced1)
			{
				return NetDeliveryMethod.UnreliableSequenced;
			}
			return NetDeliveryMethod.Unreliable;
		}

		public static string MakeCommaDelimitedList<T>(IList<T> list)
		{
			int cnt = list.Count;
			StringBuilder bdr = new StringBuilder(cnt * 5);
			for (int i = 0; i < cnt; i++)
			{
				StringBuilder stringBuilder = bdr;
				T t = list[i];
				stringBuilder.Append(t.ToString());
				if (i != cnt - 1)
				{
					bdr.Append(", ");
				}
			}
			return bdr.ToString();
		}

		public static SocketError LastResolveResult;

		public delegate void ResolveEndPointCallback(IPEndPoint endPoint);

		public delegate void ResolveAddressCallback(IPAddress adr);
	}
}
