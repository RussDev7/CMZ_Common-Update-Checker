using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using DNA.Net.Lidgren;

namespace DNA.Net.GamerServices
{
	public static class LidgrenExtensions
	{
		public static void Write(this NetBuffer msg, NetworkSessionProperties props)
		{
			msg.Write(props.Count);
			for (int c = 0; c < props.Count; c++)
			{
				int? i = props[c];
				if (i == null)
				{
					msg.Write(false);
				}
				else
				{
					msg.Write(true);
					msg.Write(i.Value);
				}
			}
		}

		public static NetworkSessionProperties ReadSessionProps(this NetBuffer msg)
		{
			NetworkSessionProperties result = new NetworkSessionProperties();
			int count = msg.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				if (msg.ReadBoolean())
				{
					result[i] = new int?(msg.ReadInt32());
				}
				else
				{
					result[i] = null;
				}
			}
			return result;
		}

		public static void Write(this NetBuffer msg, HostDiscoveryRequestMessage value, string validGameName, int validNetworkVersion)
		{
			value.Write(msg, validGameName, validNetworkVersion);
			msg.Write(value.RequestID);
			msg.Write(value.PlayerID);
		}

		public static HostDiscoveryRequestMessage ReadDiscoveryRequestMessage(this NetBuffer msg, string validGameName, int validNetworkVersion)
		{
			HostDiscoveryRequestMessage result = new HostDiscoveryRequestMessage();
			if (result.ReadAndValidate(msg, validGameName, validNetworkVersion))
			{
				result.RequestID = msg.ReadInt32();
				result.PlayerID = msg.ReadPlayerID();
			}
			return result;
		}

		public static void Write(this NetBuffer msg, HostDiscoveryResponseMessage hdrm, string validGameName, int validNetworkVersion)
		{
			hdrm.Write(msg, validGameName, validNetworkVersion);
			msg.Write((int)hdrm.Result);
			if (hdrm.Result == NetworkSession.ResultCode.Succeeded)
			{
				msg.Write((byte)hdrm.CurrentPlayers);
				msg.Write((byte)hdrm.MaxPlayers);
				msg.Write(hdrm.Message);
				msg.Write(hdrm.HostUsername);
				msg.Write(hdrm.RequestID);
				msg.Write(hdrm.SessionID);
				if (hdrm.PasswordProtected)
				{
					msg.Write(1);
				}
				else
				{
					msg.Write(0);
				}
				msg.Write(hdrm.SessionProperties);
			}
		}

		public static HostDiscoveryResponseMessage ReadDiscoveryResponseMessage(this NetBuffer msg, string validGameName, int validNetworkVersion)
		{
			HostDiscoveryResponseMessage result = new HostDiscoveryResponseMessage();
			if (result.ReadAndValidate(msg, validGameName, validNetworkVersion))
			{
				try
				{
					result.Result = (NetworkSession.ResultCode)msg.ReadInt32();
					if (result.Result == NetworkSession.ResultCode.Succeeded)
					{
						result.CurrentPlayers = (int)msg.ReadByte();
						result.MaxPlayers = (int)msg.ReadByte();
						result.Message = msg.ReadString();
						result.HostUsername = msg.ReadString();
						result.RequestID = msg.ReadInt32();
						result.SessionID = msg.ReadInt32();
						int pp = msg.ReadInt32();
						if (pp == 1)
						{
							result.PasswordProtected = true;
						}
						else
						{
							result.PasswordProtected = false;
						}
						result.SessionProperties = msg.ReadSessionProps();
					}
				}
				catch
				{
					result.Result = NetworkSession.ResultCode.VersionIsInvalid;
				}
			}
			return result;
		}

		public static void Write(this NetBuffer msg, RequestConnectToHostMessage cam, string validGameName, int validNetworkVersion)
		{
			cam.Write(msg, validGameName, validNetworkVersion);
			msg.Write(cam.SessionID);
			if (string.IsNullOrEmpty(cam.Password))
			{
				msg.Write(0);
			}
			else
			{
				msg.Write(1);
				msg.Write(cam.Password);
			}
			msg.Write(cam.Gamer);
			msg.Write(cam.SessionProperties);
		}

		public static RequestConnectToHostMessage ReadRequestConnectToHostMessage(this NetBuffer msg, string validGameName, int validVersion)
		{
			RequestConnectToHostMessage result = new RequestConnectToHostMessage();
			if (result.ReadAndValidate(msg, validGameName, validVersion))
			{
				result.SessionID = msg.ReadInt32();
				int pp = msg.ReadInt32();
				if (pp == 1)
				{
					result.Password = msg.ReadString();
				}
				else
				{
					result.Password = null;
				}
				result.Gamer = msg.ReadGamer();
				result.SessionProperties = msg.ReadSessionProps();
			}
			return result;
		}

		public static void Write(this NetBuffer msg, DropPeerMessage data)
		{
			msg.Write(data.PlayerGID);
		}

		public static DropPeerMessage ReadDropPeerMessage(this NetBuffer msg)
		{
			return new DropPeerMessage
			{
				PlayerGID = msg.ReadByte()
			};
		}

		public static void Write(this NetBuffer msg, ConnectedMessage cm)
		{
			msg.Write(cm.PlayerGID);
			msg.WriteArray(cm.Peers);
			msg.WriteArray(cm.ids);
		}

		public static ConnectedMessage ReadConnectedMessage(this NetBuffer msg)
		{
			return new ConnectedMessage
			{
				PlayerGID = msg.ReadByte(),
				Peers = msg.ReadGamerArray(),
				ids = msg.ReadByteArray()
			};
		}

		public static void Write(this NetBuffer msg, Gamer gamer)
		{
			msg.Write(gamer.PlayerID);
			msg.Write(gamer.Gamertag);
		}

		public static Gamer ReadGamer(this NetBuffer msg)
		{
			PlayerID pid = msg.ReadPlayerID();
			string gamertag = msg.ReadString();
			return new SimpleGamer(pid, gamertag);
		}

		public static void WriteArray(this NetBuffer msg, Gamer[] data)
		{
			if (data == null)
			{
				msg.Write(-1);
				return;
			}
			msg.Write(data.Length);
			for (int i = 0; i < data.Length; i++)
			{
				msg.Write(data[i]);
			}
		}

		public static Gamer[] ReadGamerArray(this NetBuffer msg)
		{
			Gamer[] result = null;
			int numGamers = msg.ReadInt32();
			if (numGamers > -1)
			{
				result = new SimpleGamer[numGamers];
				for (int i = 0; i < numGamers; i++)
				{
					result[i] = msg.ReadGamer();
				}
			}
			return result;
		}

		public static void Write(this NetBuffer msg, PlayerID id)
		{
			msg.WriteArray(id.Data);
		}

		public static PlayerID ReadPlayerID(this NetBuffer msg)
		{
			PlayerID playerID;
			try
			{
				byte[] data = msg.ReadByteArray();
				playerID = new PlayerID(data);
			}
			catch
			{
				playerID = null;
			}
			return playerID;
		}

		public static void WriteArray(this NetBuffer msg, IPEndPoint[] data)
		{
			if (data == null)
			{
				msg.Write(-1);
				return;
			}
			msg.Write(data.Length);
			for (int i = 0; i < data.Length; i++)
			{
				msg.Write(data[i]);
			}
		}

		public static IPEndPoint[] ReadIPEndPointArray(this NetBuffer msg)
		{
			int num = msg.ReadInt32();
			if (num == -1)
			{
				return null;
			}
			IPEndPoint[] result = new IPEndPoint[num];
			for (int i = 0; i < num; i++)
			{
				result[i] = msg.ReadIPEndPoint();
			}
			return result;
		}

		public static void CopyByteArrayFrom(this NetBuffer msg, NetBuffer src)
		{
			int size = src.ReadInt32();
			msg.Write(size);
			if (size > 0)
			{
				msg.CopyBytesFrom(src, size);
			}
		}

		public static void WriteArray(this NetBuffer msg, byte[] data)
		{
			if (data == null)
			{
				msg.Write(-1);
				return;
			}
			msg.Write(data.Length);
			if (data.Length > 0)
			{
				msg.Write(data);
			}
		}

		public static void WriteArray(this NetBuffer msg, byte[] data, int offset, int length)
		{
			if (data == null)
			{
				msg.Write(-1);
				return;
			}
			msg.Write(length);
			if (length > 0)
			{
				msg.Write(data, offset, length);
			}
		}

		public static byte[] ReadByteArray(this NetBuffer msg)
		{
			int i = msg.ReadInt32();
			switch (i)
			{
			case -1:
				return null;
			case 0:
				return new byte[0];
			default:
				return msg.ReadBytes(i);
			}
		}

		public static string GetPublicIP()
		{
			if (!NetworkInterface.GetIsNetworkAvailable())
			{
				return CommonResources.Not_connected_to_internet;
			}
			string direction = "";
			WebRequest request = WebRequest.Create("http://checkip.dyndns.org");
			request.Timeout = 4000;
			for (int i = 0; i < 5; i++)
			{
				try
				{
					using (WebResponse response = request.GetResponse())
					{
						using (StreamReader stream = new StreamReader(response.GetResponseStream()))
						{
							direction = stream.ReadToEnd();
						}
					}
					int first = direction.IndexOf("Address: ") + 9;
					int last = direction.LastIndexOf("</body>");
					direction = direction.Substring(first, last - first);
					i = 5;
				}
				catch (WebException we)
				{
					direction = string.Concat(new object[]
					{
						CommonResources.Error_getting_address_dyndns_returned,
						": ",
						we.Status.ToString(),
						" ",
						we.Response
					});
				}
			}
			return direction;
		}

		public static string GetLanIPAddress()
		{
			if (!NetworkInterface.GetIsNetworkAvailable())
			{
				return CommonResources.No_network_is_available;
			}
			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress result = host.AddressList.FirstOrDefault((IPAddress ip) => ip.AddressFamily == AddressFamily.InterNetwork);
			if (result != null)
			{
				return result.ToString();
			}
			return CommonResources.Couldn_t_get_local_address;
		}
	}
}
