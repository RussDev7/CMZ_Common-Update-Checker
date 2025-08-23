using System;
using System.Net;
using System.Net.Sockets;

namespace DNA.Net
{
	public static class NetTools
	{
		public static IPAddress MaptoIP6(this IPAddress adr)
		{
			if (adr == null)
			{
				return null;
			}
			if (adr.AddressFamily == AddressFamily.InterNetworkV6)
			{
				return adr;
			}
			if (adr.AddressFamily == AddressFamily.InterNetwork)
			{
				byte[] ip4bytes = adr.GetAddressBytes();
				byte[] array = new byte[16];
				array[10] = byte.MaxValue;
				array[11] = byte.MaxValue;
				byte[] ip6bytes = array;
				ip6bytes[12] = ip4bytes[0];
				ip6bytes[13] = ip4bytes[1];
				ip6bytes[14] = ip4bytes[2];
				ip6bytes[15] = ip4bytes[3];
				return new IPAddress(ip6bytes);
			}
			throw new ApplicationException("Cannot convert this address");
		}

		public static IPAddress MaptoIPV4(this IPAddress adr)
		{
			if (adr == null)
			{
				return null;
			}
			if (adr.AddressFamily == AddressFamily.InterNetwork)
			{
				return adr;
			}
			if (adr.AddressFamily == AddressFamily.InterNetworkV6)
			{
				byte[] ip6bytes = adr.GetAddressBytes();
				return new IPAddress(new byte[]
				{
					ip6bytes[12],
					ip6bytes[13],
					ip6bytes[14],
					ip6bytes[15]
				});
			}
			throw new ApplicationException("Cannot convert this address");
		}
	}
}
