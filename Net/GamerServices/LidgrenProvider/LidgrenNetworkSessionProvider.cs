using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using DNA.Net.Lidgren;
using DNA.Net.MatchMaking;

namespace DNA.Net.GamerServices.LidgrenProvider
{
	public class LidgrenNetworkSessionProvider : NetworkSessionProvider
	{
		public static NetworkSession CreateNetworkSession(NetworkSessionStaticProvider staticprovider)
		{
			LidgrenNetworkSessionProvider lidgrenNetworkSessionProvider = new LidgrenNetworkSessionProvider(staticprovider);
			NetworkSession networkSession = new NetworkSession(lidgrenNetworkSessionProvider);
			lidgrenNetworkSessionProvider._networkSession = networkSession;
			return networkSession;
		}

		protected LidgrenNetworkSessionProvider(NetworkSessionStaticProvider staticProvider)
			: base(staticProvider)
		{
		}

		public override void ReportClientJoined(string username)
		{
			try
			{
				this._staticProvider.NetworkSessionServices.ReportClientJoined(this.HostSessionInfo, username);
			}
			catch
			{
			}
		}

		public override void ReportClientLeft(string username)
		{
			try
			{
				this._staticProvider.NetworkSessionServices.ReportClientLeft(this.HostSessionInfo, username);
			}
			catch
			{
			}
		}

		public override void ReportSessionAlive()
		{
			try
			{
				this._staticProvider.NetworkSessionServices.ReportSessionAlive(this.HostSessionInfo);
			}
			catch
			{
			}
		}

		public override void UpdateHostSessionJoinPolicy(JoinGamePolicy joinGamePolicy)
		{
			bool flag = joinGamePolicy == JoinGamePolicy.Anyone;
			this.UpdateHostSession(null, null, new bool?(flag), null);
		}

		public override void UpdateHostSession(string serverName, bool? passwordProtected, bool? isPublic, NetworkSessionProperties sessionProps)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(serverName))
				{
					this.HostSessionInfo.Name = serverName;
				}
				if (passwordProtected != null)
				{
					this.HostSessionInfo.PasswordProtected = passwordProtected.Value;
				}
				if (isPublic != null)
				{
					this.HostSessionInfo.IsPublic = isPublic.Value;
				}
				if (sessionProps != null)
				{
					this.HostSessionInfo.SessionProperties = sessionProps;
				}
				this._staticProvider.NetworkSessionServices.UpdateHostSession(this.HostSessionInfo);
			}
			catch
			{
			}
		}

		public override void CloseNetworkSession()
		{
			try
			{
				this._staticProvider.NetworkSessionServices.CloseNetworkSession(this.HostSessionInfo);
			}
			catch
			{
			}
		}

		protected NetworkGamer AddRemoteGamer(Gamer gmr, NetConnection connection, bool isHost, byte playerGID)
		{
			NetworkGamer networkGamer = base.AddRemoteGamer(gmr, connection.m_remoteEndPoint.Address, isHost, playerGID);
			connection.Tag = networkGamer;
			networkGamer.NetConnectionObject = connection;
			return networkGamer;
		}

		public override void StartHost(NetworkSessionStaticProvider.BeginCreateSessionState sqs)
		{
			this._isHost = true;
			this._sessionID = MathTools.RandomInt();
			this._sessionType = sqs.SessionType;
			this._maxPlayers = sqs.MaxPlayers;
			this._signedInGamers = new List<SignedInGamer>(sqs.LocalGamers);
			this._gameName = sqs.NetworkGameName;
			this._properties = sqs.Properties;
			this._version = sqs.Version;
			if (!string.IsNullOrWhiteSpace(this._password))
			{
				this._password = sqs.Password;
			}
			if (this._sessionType != NetworkSessionType.Local)
			{
				NetPeerConfiguration netPeerConfiguration = new NetPeerConfiguration(this._gameName);
				netPeerConfiguration.Port = this._staticProvider.DefaultPort;
				netPeerConfiguration.AcceptIncomingConnections = true;
				netPeerConfiguration.MaximumConnections = sqs.MaxPlayers;
				netPeerConfiguration.NetworkThreadName = "Lidgren Network Host Thread";
				netPeerConfiguration.UseMessageRecycling = true;
				netPeerConfiguration.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
				netPeerConfiguration.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
				netPeerConfiguration.EnableMessageType(NetIncomingMessageType.StatusChanged);
				netPeerConfiguration.EnableUPnP = true;
				try
				{
					this._netSession = new NetPeer(netPeerConfiguration);
					this._netSession.Start();
				}
				catch (Exception ex)
				{
					sqs.ExceptionEncountered = ex;
					this._hostConnectionResult = NetworkSession.ResultCode.ExceptionThrown;
					this._netSession = null;
					return;
				}
				this._netSession.UPnP.ForwardPort(this._netSession.Port, this._gameName);
				try
				{
					IPAddress ipaddress = null;
					if (this._netSession.UPnP != null && this._netSession.UPnP.Status != UPnPStatus.NotAvailable)
					{
						ipaddress = this._netSession.UPnP.GetExternalIP();
					}
					if (ipaddress == null)
					{
						this._externalIPString = LidgrenExtensions.GetPublicIP();
						if (IPAddress.TryParse(this._externalIPString, out ipaddress))
						{
							this._externalIPString = this._externalIPString + ":" + this._netSession.Port.ToString();
						}
					}
					else
					{
						this._externalIPString = ipaddress.ToString() + ":" + this._netSession.Port.ToString();
					}
				}
				catch
				{
					this._externalIPString = CommonResources.Address_not_available;
				}
				try
				{
					IPAddress ipaddress = null;
					this._internalIPString = LidgrenExtensions.GetLanIPAddress();
					if (IPAddress.TryParse(this._internalIPString, out ipaddress))
					{
						this._internalIPString = this._internalIPString + ":" + this._netSession.Port.ToString();
					}
				}
				catch
				{
					this._internalIPString = CommonResources.Address_not_available;
				}
				CreateSessionInfo createSessionInfo = new CreateSessionInfo();
				createSessionInfo.MaxPlayers = this._maxPlayers;
				createSessionInfo.Name = sqs.ServerMessage;
				createSessionInfo.NetworkPort = this._netSession.Port;
				createSessionInfo.PasswordProtected = !string.IsNullOrWhiteSpace(this._password);
				createSessionInfo.SessionProperties = this.SessionProperties;
				createSessionInfo.JoinGamePolicy = JoinGamePolicy.Anyone;
				createSessionInfo.IsPublic = true;
				try
				{
					this.HostSessionInfo = this._staticProvider.NetworkSessionServices.CreateNetworkSession(createSessionInfo);
					goto IL_02DD;
				}
				catch (Exception ex2)
				{
					sqs.ExceptionEncountered = ex2;
					this._hostConnectionResult = NetworkSession.ResultCode.ExceptionThrown;
					this._netSession = null;
					return;
				}
			}
			this._netSession = null;
			IL_02DD:
			this._hostConnectionResult = NetworkSession.ResultCode.Succeeded;
			base.AddLocalGamer(this._signedInGamers[0], true, 0);
		}

		public override void StartClient(NetworkSessionStaticProvider.BeginJoinSessionState sqs)
		{
			this._isHost = false;
			this._sessionType = sqs.SessionType;
			this._sessionID = sqs.AvailableSession.SessionID;
			this._properties = sqs.AvailableSession.SessionProperties;
			this._maxPlayers = sqs.AvailableSession.MaxGamerCount;
			this._signedInGamers = new List<SignedInGamer>(sqs.LocalGamers);
			this._gameName = sqs.NetworkGameName;
			this._version = sqs.Version;
			this._hostConnectionResult = NetworkSession.ResultCode.Pending;
			if (this._sessionType != NetworkSessionType.Local)
			{
				NetPeerConfiguration netPeerConfiguration = new NetPeerConfiguration(this._gameName);
				netPeerConfiguration.AcceptIncomingConnections = false;
				netPeerConfiguration.MaximumConnections = 1;
				netPeerConfiguration.NetworkThreadName = "Lidgren Network Client Thread";
				netPeerConfiguration.UseMessageRecycling = true;
				netPeerConfiguration.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
				netPeerConfiguration.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
				netPeerConfiguration.EnableMessageType(NetIncomingMessageType.StatusChanged);
				this._netSession = new NetPeer(netPeerConfiguration);
				this._netSession.Start();
				Thread.Sleep(100);
				RequestConnectToHostMessage requestConnectToHostMessage = new RequestConnectToHostMessage();
				requestConnectToHostMessage.SessionID = this._sessionID;
				requestConnectToHostMessage.SessionProperties = this._properties;
				requestConnectToHostMessage.Password = sqs.Password;
				requestConnectToHostMessage.Gamer = this._signedInGamers[0];
				NetOutgoingMessage netOutgoingMessage = this._netSession.CreateMessage();
				netOutgoingMessage.Write(requestConnectToHostMessage, this._gameName, this._version);
				this._netSession.Connect(sqs.AvailableSession.HostEndPoint, netOutgoingMessage);
				return;
			}
			this._netSession = null;
		}

		private void SendRemoteData(NetOutgoingMessage msg, NetDeliveryMethod flags, NetworkGamer recipient)
		{
			NetConnection netConnection = (NetConnection)recipient.NetConnectionObject;
			if (netConnection != null)
			{
				netConnection.SendMessage(msg, flags, 0);
			}
		}

		private NetDeliveryMethod GetDeliveryMethodFromOptions(SendDataOptions options)
		{
			NetDeliveryMethod netDeliveryMethod = NetDeliveryMethod.Unknown;
			switch (options)
			{
			case SendDataOptions.None:
				netDeliveryMethod = NetDeliveryMethod.Unreliable;
				break;
			case SendDataOptions.Reliable:
				netDeliveryMethod = NetDeliveryMethod.ReliableUnordered;
				break;
			case SendDataOptions.InOrder:
				netDeliveryMethod = NetDeliveryMethod.UnreliableSequenced;
				break;
			case SendDataOptions.ReliableInOrder:
				netDeliveryMethod = NetDeliveryMethod.ReliableOrdered;
				break;
			}
			return netDeliveryMethod;
		}

		private void PrepareMessageForSending(SendDataOptions options, NetworkGamer recipient, out NetOutgoingMessage msg, out int channel, out NetConnection netConnection, out NetDeliveryMethod flags)
		{
			if (recipient.NetProxyObject)
			{
				msg = this._netSession.CreateMessage();
				flags = this.GetDeliveryMethodFromOptions(options);
				channel = 1;
				netConnection = (NetConnection)this._host.NetConnectionObject;
				msg.Write(3);
				msg.Write(recipient.Id);
				msg.Write((byte)flags);
				msg.Write(this._localPlayerGID);
				if (flags == NetDeliveryMethod.ReliableUnordered)
				{
					flags = NetDeliveryMethod.ReliableOrdered;
					return;
				}
			}
			else
			{
				NetConnection netConnection2 = (NetConnection)recipient.NetConnectionObject;
				if (netConnection2 != null)
				{
					msg = this._netSession.CreateMessage();
					flags = this.GetDeliveryMethodFromOptions(options);
					msg.Write(recipient.Id);
					msg.Write(this._localPlayerGID);
					channel = 0;
					netConnection = netConnection2;
					return;
				}
				msg = null;
				channel = 0;
				flags = NetDeliveryMethod.Unknown;
				netConnection = null;
			}
		}

		public override void SendRemoteData(byte[] data, SendDataOptions options, NetworkGamer recipient)
		{
			if (this._netSession != null)
			{
				NetOutgoingMessage netOutgoingMessage;
				int num;
				NetConnection netConnection;
				NetDeliveryMethod netDeliveryMethod;
				this.PrepareMessageForSending(options, recipient, out netOutgoingMessage, out num, out netConnection, out netDeliveryMethod);
				if (netConnection != null)
				{
					netOutgoingMessage.WriteArray(data);
					netConnection.SendMessage(netOutgoingMessage, netDeliveryMethod, num);
				}
			}
		}

		public override void SendRemoteData(byte[] data, int offset, int length, SendDataOptions options, NetworkGamer recipient)
		{
			if (this._netSession != null)
			{
				NetOutgoingMessage netOutgoingMessage;
				int num;
				NetConnection netConnection;
				NetDeliveryMethod netDeliveryMethod;
				this.PrepareMessageForSending(options, recipient, out netOutgoingMessage, out num, out netConnection, out netDeliveryMethod);
				if (netConnection != null)
				{
					netOutgoingMessage.WriteArray(data, offset, length);
					netConnection.SendMessage(netOutgoingMessage, netDeliveryMethod, num);
				}
			}
		}

		private void PrepareBroadcastMessageForSending(SendDataOptions options, out NetOutgoingMessage msg, out NetDeliveryMethod flags)
		{
			msg = this._netSession.CreateMessage();
			flags = this.GetDeliveryMethodFromOptions(options);
			msg.Write(4);
			msg.Write((byte)flags);
			msg.Write(this._localPlayerGID);
			if (flags == NetDeliveryMethod.ReliableUnordered)
			{
				flags = NetDeliveryMethod.ReliableOrdered;
			}
		}

		public override void BroadcastRemoteData(byte[] data, SendDataOptions options)
		{
			if (this._netSession != null)
			{
				NetConnection netConnection = this._host.NetConnectionObject as NetConnection;
				if (netConnection != null)
				{
					NetOutgoingMessage netOutgoingMessage;
					NetDeliveryMethod netDeliveryMethod;
					this.PrepareBroadcastMessageForSending(options, out netOutgoingMessage, out netDeliveryMethod);
					netOutgoingMessage.WriteArray(data);
					netConnection.SendMessage(netOutgoingMessage, netDeliveryMethod, 1);
				}
			}
		}

		public override void BroadcastRemoteData(byte[] data, int offset, int length, SendDataOptions options)
		{
			if (this._netSession != null)
			{
				NetConnection netConnection = this._host.NetConnectionObject as NetConnection;
				if (netConnection != null)
				{
					NetOutgoingMessage netOutgoingMessage;
					NetDeliveryMethod netDeliveryMethod;
					this.PrepareBroadcastMessageForSending(options, out netOutgoingMessage, out netDeliveryMethod);
					netOutgoingMessage.WriteArray(data, offset, length);
					netConnection.SendMessage(netOutgoingMessage, netDeliveryMethod, 1);
				}
			}
		}

		private bool HandleHostStatusChangedMessage(NetIncomingMessage msg)
		{
			bool flag = true;
			switch (msg.ReadByte())
			{
			case 5:
			{
				ConnectedMessage connectedMessage = new ConnectedMessage();
				connectedMessage.PlayerGID = this._nextPlayerGID;
				connectedMessage.SetPeerList(this._allGamers);
				NetOutgoingMessage netOutgoingMessage = this._netSession.CreateMessage();
				netOutgoingMessage.Write(1);
				netOutgoingMessage.Write(connectedMessage);
				msg.SenderConnection.SendMessage(netOutgoingMessage, NetDeliveryMethod.ReliableOrdered, 1);
				NetworkGamer networkGamer = this.AddRemoteGamer((Gamer)msg.SenderConnection.Tag, msg.SenderConnection, false, this._nextPlayerGID);
				this._nextPlayerGID += 1;
				using (List<NetConnection>.Enumerator enumerator = this._netSession.Connections.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						NetConnection netConnection = enumerator.Current;
						if (netConnection != msg.SenderConnection)
						{
							netOutgoingMessage = this._netSession.CreateMessage();
							netOutgoingMessage.Write(0);
							netOutgoingMessage.Write(networkGamer.Id);
							netOutgoingMessage.Write(networkGamer);
							netConnection.SendMessage(netOutgoingMessage, NetDeliveryMethod.ReliableOrdered, 1);
						}
					}
					return flag;
				}
				break;
			}
			case 6:
				goto IL_01CC;
			case 7:
				break;
			default:
				goto IL_01CC;
			}
			if (msg.SenderConnection.Tag == null)
			{
				return flag;
			}
			NetworkGamer networkGamer2 = msg.SenderConnection.Tag as NetworkGamer;
			if (networkGamer2 != null)
			{
				DropPeerMessage dropPeerMessage = new DropPeerMessage();
				dropPeerMessage.PlayerGID = networkGamer2.Id;
				foreach (NetConnection netConnection2 in this._netSession.Connections)
				{
					if (netConnection2 != msg.SenderConnection)
					{
						NetOutgoingMessage netOutgoingMessage2 = this._netSession.CreateMessage();
						netOutgoingMessage2.Write(2);
						netOutgoingMessage2.Write(dropPeerMessage);
						netConnection2.SendMessage(netOutgoingMessage2, NetDeliveryMethod.ReliableOrdered, 1);
					}
				}
				base.RemoveGamer(networkGamer2);
				return flag;
			}
			return flag;
			IL_01CC:
			flag = false;
			return flag;
		}

		private bool HandleHostSystemMessages(NetIncomingMessage msg)
		{
			bool flag = true;
			switch (msg.ReadByte())
			{
			case 3:
			{
				byte b = msg.ReadByte();
				NetDeliveryMethod netDeliveryMethod = (NetDeliveryMethod)msg.ReadByte();
				NetworkGamer networkGamer = this.FindGamerById(b);
				if (networkGamer != null)
				{
					byte b2 = msg.ReadByte();
					NetOutgoingMessage netOutgoingMessage = this._netSession.CreateMessage();
					netOutgoingMessage.Write(b);
					netOutgoingMessage.Write(b2);
					netOutgoingMessage.CopyByteArrayFrom(msg);
					NetConnection netConnection = (NetConnection)networkGamer.NetConnectionObject;
					netConnection.SendMessage(netOutgoingMessage, netDeliveryMethod, 0);
				}
				break;
			}
			case 4:
			{
				NetDeliveryMethod netDeliveryMethod = (NetDeliveryMethod)msg.ReadByte();
				byte b2 = msg.ReadByte();
				byte[] array = null;
				int num = msg.ReadInt32();
				int num2 = 0;
				bool flag2 = false;
				if (num > 0)
				{
					flag2 = msg.GetAlignedData(out array, out num2);
					if (!flag2)
					{
						array = msg.ReadBytes(num);
					}
				}
				LocalNetworkGamer localNetworkGamer = this.FindGamerById(0) as LocalNetworkGamer;
				if (localNetworkGamer != null)
				{
					NetworkGamer networkGamer2 = this.FindGamerById(b2);
					if (flag2)
					{
						localNetworkGamer.AppendNewDataPacket(array, num2, num, networkGamer2);
					}
					else
					{
						localNetworkGamer.AppendNewDataPacket(array, networkGamer2);
					}
					for (int i = 0; i < this._remoteGamers.Count; i++)
					{
						if (this._remoteGamers[i].Id != b2)
						{
							NetConnection netConnection2 = this._remoteGamers[i].NetConnectionObject as NetConnection;
							if (netConnection2 != null)
							{
								NetOutgoingMessage netOutgoingMessage2 = this._netSession.CreateMessage();
								netOutgoingMessage2.Write(this._remoteGamers[i].Id);
								netOutgoingMessage2.Write(b2);
								netOutgoingMessage2.Write(num);
								if (num > 0)
								{
									netOutgoingMessage2.Write(array, num2, num);
								}
								netConnection2.SendMessage(netOutgoingMessage2, netDeliveryMethod, 0);
							}
						}
					}
				}
				break;
			}
			}
			return flag;
		}

		private void HandleHostDiscoveryRequest(NetIncomingMessage msg)
		{
			HostDiscoveryRequestMessage hostDiscoveryRequestMessage = msg.ReadDiscoveryRequestMessage(this._gameName, this._version);
			HostDiscoveryResponseMessage hostDiscoveryResponseMessage = new HostDiscoveryResponseMessage();
			switch (hostDiscoveryRequestMessage.ReadResult)
			{
			case VersionCheckedMessage.ReadResultCode.Success:
				if (this.AllowConnectionCallback == null || this.AllowConnectionCallback(hostDiscoveryRequestMessage.PlayerID, msg.SenderEndPoint.Address))
				{
					hostDiscoveryResponseMessage.Result = NetworkSession.ResultCode.Succeeded;
				}
				else
				{
					hostDiscoveryResponseMessage.Result = NetworkSession.ResultCode.ConnectionDenied;
				}
				break;
			case VersionCheckedMessage.ReadResultCode.GameNameInvalid:
				hostDiscoveryResponseMessage.Result = NetworkSession.ResultCode.GameNamesDontMatch;
				break;
			case VersionCheckedMessage.ReadResultCode.LocalVersionIsHIgher:
				hostDiscoveryResponseMessage.Result = NetworkSession.ResultCode.ServerHasNewerVersion;
				break;
			case VersionCheckedMessage.ReadResultCode.LocalVersionIsLower:
				hostDiscoveryResponseMessage.Result = NetworkSession.ResultCode.ServerHasOlderVersion;
				break;
			}
			if (hostDiscoveryResponseMessage.Result == NetworkSession.ResultCode.Succeeded)
			{
				hostDiscoveryResponseMessage.RequestID = hostDiscoveryRequestMessage.RequestID;
				hostDiscoveryResponseMessage.SessionID = this._sessionID;
				hostDiscoveryResponseMessage.SessionProperties = this._properties;
				hostDiscoveryResponseMessage.CurrentPlayers = this._allGamers.Count;
				hostDiscoveryResponseMessage.MaxPlayers = this._maxPlayers;
				hostDiscoveryResponseMessage.Message = this._serverMessage;
				hostDiscoveryResponseMessage.HostUsername = this._host.Gamertag;
				hostDiscoveryResponseMessage.PasswordProtected = !string.IsNullOrWhiteSpace(this._password);
			}
			NetOutgoingMessage netOutgoingMessage = this._netSession.CreateMessage();
			netOutgoingMessage.Write(hostDiscoveryResponseMessage, this._gameName, this._version);
			this._netSession.SendDiscoveryResponse(netOutgoingMessage, msg.SenderEndPoint);
		}

		private void HandleHostConnectionApproval(NetIncomingMessage msg)
		{
			RequestConnectToHostMessage requestConnectToHostMessage = msg.ReadRequestConnectToHostMessage(this._gameName, this._version);
			if (requestConnectToHostMessage.ReadResult == VersionCheckedMessage.ReadResultCode.GameNameInvalid)
			{
				this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.GameNamesDontMatch);
				return;
			}
			if (requestConnectToHostMessage.ReadResult == VersionCheckedMessage.ReadResultCode.VersionInvalid)
			{
				this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.ServerHasOlderVersion);
				return;
			}
			if (requestConnectToHostMessage.ReadResult == VersionCheckedMessage.ReadResultCode.LocalVersionIsLower)
			{
				this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.ServerHasOlderVersion);
				return;
			}
			if (requestConnectToHostMessage.ReadResult == VersionCheckedMessage.ReadResultCode.LocalVersionIsHIgher)
			{
				this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.ServerHasNewerVersion);
				return;
			}
			if (!string.IsNullOrWhiteSpace(this._password) && (string.IsNullOrWhiteSpace(requestConnectToHostMessage.Password) || !requestConnectToHostMessage.Password.Equals(this._password)))
			{
				this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.IncorrectPassword);
				return;
			}
			if (this.AllowConnectionCallback != null && !this.AllowConnectionCallback(requestConnectToHostMessage.Gamer.PlayerID, msg.SenderEndPoint.Address))
			{
				this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.ConnectionDenied);
				return;
			}
			if (requestConnectToHostMessage.SessionProperties.Count != this._properties.Count)
			{
				this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.SessionPropertiesDontMatch);
				return;
			}
			for (int i = 0; i < requestConnectToHostMessage.SessionProperties.Count; i++)
			{
				if (requestConnectToHostMessage.SessionProperties[i] != this._properties[i])
				{
					this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.SessionPropertiesDontMatch);
					return;
				}
			}
			GamerCollection<NetworkGamer> allGamers = base.AllGamers;
			for (int j = 0; j < allGamers.Count; j++)
			{
				if (allGamers[j] != null && allGamers[j].Gamertag == requestConnectToHostMessage.Gamer.Gamertag)
				{
					this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.GamerAlreadyConnected);
					return;
				}
			}
			msg.SenderConnection.Tag = requestConnectToHostMessage.Gamer;
			msg.SenderConnection.Approve();
		}

		private bool HandleHostMessages(NetIncomingMessage msg)
		{
			bool flag = true;
			NetIncomingMessageType messageType = msg.MessageType;
			if (messageType <= NetIncomingMessageType.ConnectionApproval)
			{
				if (messageType == NetIncomingMessageType.StatusChanged)
				{
					return this.HandleHostStatusChangedMessage(msg);
				}
				if (messageType == NetIncomingMessageType.ConnectionApproval)
				{
					this.HandleHostConnectionApproval(msg);
					return flag;
				}
			}
			else if (messageType != NetIncomingMessageType.Data)
			{
				if (messageType == NetIncomingMessageType.DiscoveryRequest)
				{
					this.HandleHostDiscoveryRequest(msg);
					return flag;
				}
			}
			else
			{
				if (msg.SequenceChannel == 1)
				{
					return this.HandleHostSystemMessages(msg);
				}
				return false;
			}
			flag = false;
			return flag;
		}

		private void AddNewPeer(NetIncomingMessage msg)
		{
			byte b = msg.ReadByte();
			Gamer gamer = msg.ReadGamer();
			base.AddProxyGamer(gamer, false, b);
		}

		private bool HandleClientSystemMessages(NetIncomingMessage msg)
		{
			bool flag = true;
			InternalMessageTypes internalMessageTypes = (InternalMessageTypes)msg.ReadByte();
			NetworkGamer networkGamer = null;
			switch (internalMessageTypes)
			{
			case InternalMessageTypes.NewPeer:
				this.AddNewPeer(msg);
				break;
			case InternalMessageTypes.ResponseToConnection:
			{
				ConnectedMessage connectedMessage = msg.ReadConnectedMessage();
				base.AddLocalGamer(this._signedInGamers[0], false, connectedMessage.PlayerGID);
				for (int i = 0; i < connectedMessage.Peers.Length; i++)
				{
					if (connectedMessage.ids[i] == 0)
					{
						this.AddRemoteGamer(connectedMessage.Peers[i], msg.SenderConnection, true, 0);
					}
					else
					{
						base.AddProxyGamer(connectedMessage.Peers[i], false, connectedMessage.ids[i]);
					}
				}
				break;
			}
			case InternalMessageTypes.DropPeer:
			{
				DropPeerMessage dropPeerMessage = msg.ReadDropPeerMessage();
				if (this._idToGamer.TryGetValue(dropPeerMessage.PlayerGID, out networkGamer))
				{
					base.RemoveGamer(networkGamer);
				}
				break;
			}
			default:
				flag = false;
				break;
			}
			return flag;
		}

		private void HandleClientStatusChangedMessage(NetIncomingMessage msg)
		{
			switch (msg.ReadByte())
			{
			case 5:
				this._hostConnectionResult = NetworkSession.ResultCode.Succeeded;
				this._hostConnectionResultString = this._hostConnectionResult.ToString();
				return;
			case 6:
				break;
			case 7:
				base.HandleDisconnection(msg.ReadString());
				break;
			default:
				return;
			}
		}

		private bool HandleClientMessages(NetIncomingMessage msg)
		{
			bool flag = true;
			NetIncomingMessageType messageType = msg.MessageType;
			if (messageType != NetIncomingMessageType.StatusChanged)
			{
				if (messageType != NetIncomingMessageType.Data)
				{
					flag = false;
				}
				else
				{
					if (msg.SequenceChannel == 1)
					{
						return this.HandleClientSystemMessages(msg);
					}
					flag = false;
				}
			}
			else
			{
				this.HandleClientStatusChangedMessage(msg);
			}
			return flag;
		}

		private void FailConnection(NetConnection c, NetworkSession.ResultCode reason)
		{
			c.Deny(reason.ToString());
		}

		private bool HandleCommonMessages(NetIncomingMessage msg)
		{
			bool flag = true;
			NetIncomingMessageType messageType = msg.MessageType;
			if (messageType <= NetIncomingMessageType.VerboseDebugMessage)
			{
				if (messageType != NetIncomingMessageType.Data)
				{
					if (messageType == NetIncomingMessageType.VerboseDebugMessage)
					{
						return flag;
					}
				}
				else
				{
					byte b = msg.ReadByte();
					NetworkGamer networkGamer = this.FindGamerById(b);
					if (networkGamer == null)
					{
						return flag;
					}
					LocalNetworkGamer localNetworkGamer = networkGamer as LocalNetworkGamer;
					if (localNetworkGamer == null)
					{
						return flag;
					}
					byte b2 = msg.ReadByte();
					NetworkGamer networkGamer2 = this.FindGamerById(b2);
					if (networkGamer2 != null)
					{
						byte[] array = msg.ReadByteArray();
						localNetworkGamer.AppendNewDataPacket(array, networkGamer2);
						return flag;
					}
					return flag;
				}
			}
			else if (messageType == NetIncomingMessageType.DebugMessage || messageType == NetIncomingMessageType.WarningMessage || messageType == NetIncomingMessageType.ErrorMessage)
			{
				return flag;
			}
			flag = false;
			return flag;
		}

		public override void Update()
		{
			if (this._netSession != null)
			{
				NetIncomingMessage netIncomingMessage;
				while ((netIncomingMessage = this._netSession.ReadMessage()) != null)
				{
					if (!(this._isHost ? this.HandleHostMessages(netIncomingMessage) : this.HandleClientMessages(netIncomingMessage)))
					{
						bool flag = this.HandleCommonMessages(netIncomingMessage);
					}
					if (this._netSession == null)
					{
						return;
					}
					this._netSession.Recycle(netIncomingMessage);
				}
			}
		}

		public override void Dispose(bool disposeManagedObjects)
		{
			this._staticProvider.TaskScheduler.Exit();
			if (this._netSession != null)
			{
				int port = this._netSession.Port;
				this._netSession.Shutdown("Session Ended Normally");
				if (this.IsHost && this._netSession.UPnP != null)
				{
					this._netSession.UPnP.DeleteForwardingRule(port);
				}
				this._netSession = null;
			}
			base.Dispose(disposeManagedObjects);
		}

		protected NetPeer _netSession;

		protected int _sessionID;

		private List<NetworkGamer> _hostConnectedGamerList = new List<NetworkGamer>();

		private Stopwatch debugStopwatch;
	}
}
