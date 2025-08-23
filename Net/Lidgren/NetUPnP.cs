using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;

namespace DNA.Net.Lidgren
{
	public class NetUPnP
	{
		public UPnPStatus Status
		{
			get
			{
				return this.m_status;
			}
		}

		public NetUPnP(NetPeer peer)
		{
			this.m_peer = peer;
			this.m_discoveryResponseDeadline = float.MinValue;
		}

		internal void Discover(NetPeer peer)
		{
			string str = "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nST:upnp:rootdevice\r\nMAN:\"ssdp:discover\"\r\nMX:3\r\n\r\n";
			this.m_status = UPnPStatus.Discovering;
			byte[] arr = Encoding.UTF8.GetBytes(str);
			peer.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
			peer.RawSend(arr, 0, arr.Length, new IPEndPoint(IPAddress.Broadcast, 1900));
			peer.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, false);
			this.m_discoveryResponseDeadline = (float)NetTime.Now + 6f;
			this.m_status = UPnPStatus.Discovering;
		}

		internal void ExtractServiceUrl(string resp)
		{
			try
			{
				XmlDocument desc = new XmlDocument();
				desc.Load(WebRequest.Create(resp).GetResponse().GetResponseStream());
				XmlNamespaceManager nsMgr = new XmlNamespaceManager(desc.NameTable);
				nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
				XmlNode typen = desc.SelectSingleNode("//tns:device/tns:deviceType/text()", nsMgr);
				if (typen.Value.Contains("InternetGatewayDevice"))
				{
					this.m_serviceName = "WANIPConnection";
					XmlNode node = desc.SelectSingleNode("//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:" + this.m_serviceName + ":1\"]/tns:controlURL/text()", nsMgr);
					if (node == null)
					{
						this.m_serviceName = "WANPPPConnection";
						node = desc.SelectSingleNode("//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:" + this.m_serviceName + ":1\"]/tns:controlURL/text()", nsMgr);
						if (node == null)
						{
							return;
						}
					}
					this.m_serviceUrl = NetUPnP.CombineUrls(resp, node.Value);
					this.m_status = UPnPStatus.Available;
					this.m_discoveryComplete.Set();
				}
			}
			catch
			{
			}
		}

		private static string CombineUrls(string gatewayURL, string subURL)
		{
			if (subURL.Contains("http:") || subURL.Contains("."))
			{
				return subURL;
			}
			gatewayURL = gatewayURL.Replace("http://", "");
			int i = gatewayURL.IndexOf("/");
			if (i != -1)
			{
				gatewayURL = gatewayURL.Substring(0, i);
			}
			return "http://" + gatewayURL + subURL;
		}

		private bool CheckAvailability()
		{
			switch (this.m_status)
			{
			case UPnPStatus.Discovering:
				if (this.m_discoveryComplete.WaitOne(1000))
				{
					return true;
				}
				if (NetTime.Now > (double)this.m_discoveryResponseDeadline)
				{
					this.m_status = UPnPStatus.NotAvailable;
				}
				return false;
			case UPnPStatus.NotAvailable:
				return false;
			case UPnPStatus.Available:
				return true;
			default:
				return false;
			}
		}

		public bool ForwardPort(int port, string description)
		{
			if (!this.CheckAvailability())
			{
				return false;
			}
			IPAddress mask;
			IPAddress client = NetUtility.GetMyAddress(out mask);
			if (client == null)
			{
				return false;
			}
			try
			{
				this.SOAPRequest(this.m_serviceUrl, string.Concat(new string[]
				{
					"<u:AddPortMapping xmlns:u=\"urn:schemas-upnp-org:service:",
					this.m_serviceName,
					":1\"><NewRemoteHost></NewRemoteHost><NewExternalPort>",
					port.ToString(),
					"</NewExternalPort><NewProtocol>",
					ProtocolType.Udp.ToString().ToUpper(),
					"</NewProtocol><NewInternalPort>",
					port.ToString(),
					"</NewInternalPort><NewInternalClient>",
					client.ToString(),
					"</NewInternalClient><NewEnabled>1</NewEnabled><NewPortMappingDescription>",
					description,
					"</NewPortMappingDescription><NewLeaseDuration>0</NewLeaseDuration></u:AddPortMapping>"
				}), "AddPortMapping");
				Thread.Sleep(50);
			}
			catch (Exception ex)
			{
				this.m_peer.LogWarning("UPnP port forward failed: " + ex.Message);
				return false;
			}
			return true;
		}

		public bool DeleteForwardingRule(int port)
		{
			if (!this.CheckAvailability())
			{
				return false;
			}
			bool flag;
			try
			{
				this.SOAPRequest(this.m_serviceUrl, string.Concat(new object[]
				{
					"<u:DeletePortMapping xmlns:u=\"urn:schemas-upnp-org:service:",
					this.m_serviceName,
					":1\"><NewRemoteHost></NewRemoteHost><NewExternalPort>",
					port,
					"</NewExternalPort><NewProtocol>",
					ProtocolType.Udp.ToString().ToUpper(),
					"</NewProtocol></u:DeletePortMapping>"
				}), "DeletePortMapping");
				flag = true;
			}
			catch (Exception ex)
			{
				this.m_peer.LogWarning("UPnP delete forwarding rule failed: " + ex.Message);
				flag = false;
			}
			return flag;
		}

		public IPAddress GetExternalIP()
		{
			if (!this.CheckAvailability())
			{
				return null;
			}
			IPAddress ipaddress;
			try
			{
				XmlDocument xdoc = this.SOAPRequest(this.m_serviceUrl, "<u:GetExternalIPAddress xmlns:u=\"urn:schemas-upnp-org:service:" + this.m_serviceName + ":1\"></u:GetExternalIPAddress>", "GetExternalIPAddress");
				XmlNamespaceManager nsMgr = new XmlNamespaceManager(xdoc.NameTable);
				nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
				string IP = xdoc.SelectSingleNode("//NewExternalIPAddress/text()", nsMgr).Value;
				ipaddress = IPAddress.Parse(IP);
			}
			catch (Exception ex)
			{
				this.m_peer.LogWarning("Failed to get external IP: " + ex.Message);
				ipaddress = null;
			}
			return ipaddress;
		}

		private XmlDocument SOAPRequest(string url, string soap, string function)
		{
			string req = "<?xml version=\"1.0\"?><s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\"><s:Body>" + soap + "</s:Body></s:Envelope>";
			WebRequest r = WebRequest.Create(url);
			r.Method = "POST";
			byte[] b = Encoding.UTF8.GetBytes(req);
			r.Headers.Add("SOAPACTION", string.Concat(new string[] { "\"urn:schemas-upnp-org:service:", this.m_serviceName, ":1#", function, "\"" }));
			r.ContentType = "text/xml; charset=\"utf-8\"";
			r.ContentLength = (long)b.Length;
			r.GetRequestStream().Write(b, 0, b.Length);
			XmlDocument resp = new XmlDocument();
			WebResponse wres = r.GetResponse();
			Stream ress = wres.GetResponseStream();
			resp.Load(ress);
			return resp;
		}

		private const int c_discoveryTimeOutMillis = 1000;

		private string m_serviceUrl;

		private string m_serviceName = "";

		private NetPeer m_peer;

		private ManualResetEvent m_discoveryComplete = new ManualResetEvent(false);

		internal float m_discoveryResponseDeadline;

		private UPnPStatus m_status;
	}
}
