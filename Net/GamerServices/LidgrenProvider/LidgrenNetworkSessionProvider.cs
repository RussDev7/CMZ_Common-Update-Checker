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
			LidgrenNetworkSessionProvider provider = new LidgrenNetworkSessionProvider(staticprovider);
			NetworkSession result = new NetworkSession(provider);
			provider._networkSession = result;
			return result;
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
			bool ispublic = joinGamePolicy == JoinGamePolicy.Anyone;
			this.UpdateHostSession(null, null, new bool?(ispublic), null);
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
			NetworkGamer result = base.AddRemoteGamer(gmr, connection.m_remoteEndPoint.Address, isHost, playerGID);
			connection.Tag = result;
			result.NetConnectionObject = connection;
			return result;
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
				NetPeerConfiguration npc = new NetPeerConfiguration(this._gameName);
				npc.Port = this._staticProvider.DefaultPort;
				npc.AcceptIncomingConnections = true;
				npc.MaximumConnections = sqs.MaxPlayers;
				npc.NetworkThreadName = "Lidgren Network Host Thread";
				npc.UseMessageRecycling = true;
				npc.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
				npc.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
				npc.EnableMessageType(NetIncomingMessageType.StatusChanged);
				npc.EnableUPnP = true;
				try
				{
					this._netSession = new NetPeer(npc);
					this._netSession.Start();
				}
				catch (Exception e)
				{
					sqs.ExceptionEncountered = e;
					this._hostConnectionResult = NetworkSession.ResultCode.ExceptionThrown;
					this._netSession = null;
					return;
				}
				this._netSession.UPnP.ForwardPort(this._netSession.Port, this._gameName);
				try
				{
					IPAddress ipa = null;
					if (this._netSession.UPnP != null && this._netSession.UPnP.Status != UPnPStatus.NotAvailable)
					{
						ipa = this._netSession.UPnP.GetExternalIP();
					}
					if (ipa == null)
					{
						this._externalIPString = LidgrenExtensions.GetPublicIP();
						if (IPAddress.TryParse(this._externalIPString, out ipa))
						{
							this._externalIPString = this._externalIPString + ":" + this._netSession.Port.ToString();
						}
					}
					else
					{
						this._externalIPString = ipa.ToString() + ":" + this._netSession.Port.ToString();
					}
				}
				catch
				{
					this._externalIPString = CommonResources.Address_not_available;
				}
				try
				{
					IPAddress ipa = null;
					this._internalIPString = LidgrenExtensions.GetLanIPAddress();
					if (IPAddress.TryParse(this._internalIPString, out ipa))
					{
						this._internalIPString = this._internalIPString + ":" + this._netSession.Port.ToString();
					}
				}
				catch
				{
					this._internalIPString = CommonResources.Address_not_available;
				}
				CreateSessionInfo sessionInfo = new CreateSessionInfo();
				sessionInfo.MaxPlayers = this._maxPlayers;
				sessionInfo.Name = sqs.ServerMessage;
				sessionInfo.NetworkPort = this._netSession.Port;
				sessionInfo.PasswordProtected = !string.IsNullOrWhiteSpace(this._password);
				sessionInfo.SessionProperties = this.SessionProperties;
				sessionInfo.JoinGamePolicy = JoinGamePolicy.Anyone;
				sessionInfo.IsPublic = true;
				try
				{
					this.HostSessionInfo = this._staticProvider.NetworkSessionServices.CreateNetworkSession(sessionInfo);
					goto IL_02DD;
				}
				catch (Exception e2)
				{
					sqs.ExceptionEncountered = e2;
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
				NetPeerConfiguration npc = new NetPeerConfiguration(this._gameName);
				npc.AcceptIncomingConnections = false;
				npc.MaximumConnections = 1;
				npc.NetworkThreadName = "Lidgren Network Client Thread";
				npc.UseMessageRecycling = true;
				npc.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
				npc.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
				npc.EnableMessageType(NetIncomingMessageType.StatusChanged);
				this._netSession = new NetPeer(npc);
				this._netSession.Start();
				Thread.Sleep(100);
				RequestConnectToHostMessage crm = new RequestConnectToHostMessage();
				crm.SessionID = this._sessionID;
				crm.SessionProperties = this._properties;
				crm.Password = sqs.Password;
				crm.Gamer = this._signedInGamers[0];
				NetOutgoingMessage nom = this._netSession.CreateMessage();
				nom.Write(crm, this._gameName, this._version);
				this._netSession.Connect(sqs.AvailableSession.HostEndPoint, nom);
				return;
			}
			this._netSession = null;
		}

		private void SendRemoteData(NetOutgoingMessage msg, NetDeliveryMethod flags, NetworkGamer recipient)
		{
			NetConnection c = (NetConnection)recipient.NetConnectionObject;
			if (c != null)
			{
				c.SendMessage(msg, flags, 0);
			}
		}

		private NetDeliveryMethod GetDeliveryMethodFromOptions(SendDataOptions options)
		{
			NetDeliveryMethod flags = NetDeliveryMethod.Unknown;
			switch (options)
			{
			case SendDataOptions.None:
				flags = NetDeliveryMethod.Unreliable;
				break;
			case SendDataOptions.Reliable:
				flags = NetDeliveryMethod.ReliableUnordered;
				break;
			case SendDataOptions.InOrder:
				flags = NetDeliveryMethod.UnreliableSequenced;
				break;
			case SendDataOptions.ReliableInOrder:
				flags = NetDeliveryMethod.ReliableOrdered;
				break;
			}
			return flags;
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
				NetConnection c = (NetConnection)recipient.NetConnectionObject;
				if (c != null)
				{
					msg = this._netSession.CreateMessage();
					flags = this.GetDeliveryMethodFromOptions(options);
					msg.Write(recipient.Id);
					msg.Write(this._localPlayerGID);
					channel = 0;
					netConnection = c;
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
				NetOutgoingMessage msg;
				int channel;
				NetConnection netConnection;
				NetDeliveryMethod flags;
				this.PrepareMessageForSending(options, recipient, out msg, out channel, out netConnection, out flags);
				if (netConnection != null)
				{
					msg.WriteArray(data);
					netConnection.SendMessage(msg, flags, channel);
				}
			}
		}

		public override void SendRemoteData(byte[] data, int offset, int length, SendDataOptions options, NetworkGamer recipient)
		{
			if (this._netSession != null)
			{
				NetOutgoingMessage msg;
				int channel;
				NetConnection netConnection;
				NetDeliveryMethod flags;
				this.PrepareMessageForSending(options, recipient, out msg, out channel, out netConnection, out flags);
				if (netConnection != null)
				{
					msg.WriteArray(data, offset, length);
					netConnection.SendMessage(msg, flags, channel);
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
					NetOutgoingMessage msg;
					NetDeliveryMethod flags;
					this.PrepareBroadcastMessageForSending(options, out msg, out flags);
					msg.WriteArray(data);
					netConnection.SendMessage(msg, flags, 1);
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
					NetOutgoingMessage msg;
					NetDeliveryMethod flags;
					this.PrepareBroadcastMessageForSending(options, out msg, out flags);
					msg.WriteArray(data, offset, length);
					netConnection.SendMessage(msg, flags, 1);
				}
			}
		}

		private bool HandleHostStatusChangedMessage(NetIncomingMessage msg)
		{
			bool messageHandled = true;
			switch (msg.ReadByte())
			{
			case 5:
			{
				ConnectedMessage cm = new ConnectedMessage();
				cm.PlayerGID = this._nextPlayerGID;
				cm.SetPeerList(this._allGamers);
				NetOutgoingMessage omsg = this._netSession.CreateMessage();
				omsg.Write(1);
				omsg.Write(cm);
				msg.SenderConnection.SendMessage(omsg, NetDeliveryMethod.ReliableOrdered, 1);
				NetworkGamer newGamer = this.AddRemoteGamer((Gamer)msg.SenderConnection.Tag, msg.SenderConnection, false, this._nextPlayerGID);
				this._nextPlayerGID += 1;
				using (List<NetConnection>.Enumerator enumerator = this._netSession.Connections.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						NetConnection nc = enumerator.Current;
						if (nc != msg.SenderConnection)
						{
							omsg = this._netSession.CreateMessage();
							omsg.Write(0);
							omsg.Write(newGamer.Id);
							omsg.Write(newGamer);
							nc.SendMessage(omsg, NetDeliveryMethod.ReliableOrdered, 1);
						}
					}
					return messageHandled;
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
				return messageHandled;
			}
			NetworkGamer g = msg.SenderConnection.Tag as NetworkGamer;
			if (g != null)
			{
				DropPeerMessage dropPeer = new DropPeerMessage();
				dropPeer.PlayerGID = g.Id;
				foreach (NetConnection nc2 in this._netSession.Connections)
				{
					if (nc2 != msg.SenderConnection)
					{
						NetOutgoingMessage om = this._netSession.CreateMessage();
						om.Write(2);
						om.Write(dropPeer);
						nc2.SendMessage(om, NetDeliveryMethod.ReliableOrdered, 1);
					}
				}
				base.RemoveGamer(g);
				return messageHandled;
			}
			return messageHandled;
			IL_01CC:
			messageHandled = false;
			return messageHandled;
		}

		private bool HandleHostSystemMessages(NetIncomingMessage msg)
		{
			bool result = true;
			switch (msg.ReadByte())
			{
			case 3:
			{
				byte recipientId = msg.ReadByte();
				NetDeliveryMethod flags = (NetDeliveryMethod)msg.ReadByte();
				NetworkGamer recipient = this.FindGamerById(recipientId);
				if (recipient != null)
				{
					byte senderId = msg.ReadByte();
					NetOutgoingMessage omsg = this._netSession.CreateMessage();
					omsg.Write(recipientId);
					omsg.Write(senderId);
					omsg.CopyByteArrayFrom(msg);
					NetConnection c = (NetConnection)recipient.NetConnectionObject;
					c.SendMessage(omsg, flags, 0);
				}
				break;
			}
			case 4:
			{
				NetDeliveryMethod flags = (NetDeliveryMethod)msg.ReadByte();
				byte senderId = msg.ReadByte();
				byte[] data = null;
				int dataSize = msg.ReadInt32();
				int offset = 0;
				bool dataIsAligned = false;
				if (dataSize > 0)
				{
					dataIsAligned = msg.GetAlignedData(out data, out offset);
					if (!dataIsAligned)
					{
						data = msg.ReadBytes(dataSize);
					}
				}
				LocalNetworkGamer host = this.FindGamerById(0) as LocalNetworkGamer;
				if (host != null)
				{
					NetworkGamer sender = this.FindGamerById(senderId);
					if (dataIsAligned)
					{
						host.AppendNewDataPacket(data, offset, dataSize, sender);
					}
					else
					{
						host.AppendNewDataPacket(data, sender);
					}
					for (int i = 0; i < this._remoteGamers.Count; i++)
					{
						if (this._remoteGamers[i].Id != senderId)
						{
							NetConnection c2 = this._remoteGamers[i].NetConnectionObject as NetConnection;
							if (c2 != null)
							{
								NetOutgoingMessage omsg2 = this._netSession.CreateMessage();
								omsg2.Write(this._remoteGamers[i].Id);
								omsg2.Write(senderId);
								omsg2.Write(dataSize);
								if (dataSize > 0)
								{
									omsg2.Write(data, offset, dataSize);
								}
								c2.SendMessage(omsg2, flags, 0);
							}
						}
					}
				}
				break;
			}
			}
			return result;
		}

		private void HandleHostDiscoveryRequest(NetIncomingMessage msg)
		{
			HostDiscoveryRequestMessage hdrm = msg.ReadDiscoveryRequestMessage(this._gameName, this._version);
			HostDiscoveryResponseMessage response = new HostDiscoveryResponseMessage();
			switch (hdrm.ReadResult)
			{
			case VersionCheckedMessage.ReadResultCode.Success:
				if (this.AllowConnectionCallback == null || this.AllowConnectionCallback(hdrm.PlayerID, msg.SenderEndPoint.Address))
				{
					response.Result = NetworkSession.ResultCode.Succeeded;
				}
				else
				{
					response.Result = NetworkSession.ResultCode.ConnectionDenied;
				}
				break;
			case VersionCheckedMessage.ReadResultCode.GameNameInvalid:
				response.Result = NetworkSession.ResultCode.GameNamesDontMatch;
				break;
			case VersionCheckedMessage.ReadResultCode.LocalVersionIsHIgher:
				response.Result = NetworkSession.ResultCode.ServerHasNewerVersion;
				break;
			case VersionCheckedMessage.ReadResultCode.LocalVersionIsLower:
				response.Result = NetworkSession.ResultCode.ServerHasOlderVersion;
				break;
			}
			if (response.Result == NetworkSession.ResultCode.Succeeded)
			{
				response.RequestID = hdrm.RequestID;
				response.SessionID = this._sessionID;
				response.SessionProperties = this._properties;
				response.CurrentPlayers = this._allGamers.Count;
				response.MaxPlayers = this._maxPlayers;
				response.Message = this._serverMessage;
				response.HostUsername = this._host.Gamertag;
				response.PasswordProtected = !string.IsNullOrWhiteSpace(this._password);
			}
			NetOutgoingMessage om = this._netSession.CreateMessage();
			om.Write(response, this._gameName, this._version);
			this._netSession.SendDiscoveryResponse(om, msg.SenderEndPoint);
		}

		private void HandleHostConnectionApproval(NetIncomingMessage msg)
		{
			RequestConnectToHostMessage crm = msg.ReadRequestConnectToHostMessage(this._gameName, this._version);
			if (crm.ReadResult == VersionCheckedMessage.ReadResultCode.GameNameInvalid)
			{
				this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.GameNamesDontMatch);
				return;
			}
			if (crm.ReadResult == VersionCheckedMessage.ReadResultCode.VersionInvalid)
			{
				this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.ServerHasOlderVersion);
				return;
			}
			if (crm.ReadResult == VersionCheckedMessage.ReadResultCode.LocalVersionIsLower)
			{
				this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.ServerHasOlderVersion);
				return;
			}
			if (crm.ReadResult == VersionCheckedMessage.ReadResultCode.LocalVersionIsHIgher)
			{
				this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.ServerHasNewerVersion);
				return;
			}
			if (!string.IsNullOrWhiteSpace(this._password) && (string.IsNullOrWhiteSpace(crm.Password) || !crm.Password.Equals(this._password)))
			{
				this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.IncorrectPassword);
				return;
			}
			if (this.AllowConnectionCallback != null && !this.AllowConnectionCallback(crm.Gamer.PlayerID, msg.SenderEndPoint.Address))
			{
				this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.ConnectionDenied);
				return;
			}
			if (crm.SessionProperties.Count != this._properties.Count)
			{
				this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.SessionPropertiesDontMatch);
				return;
			}
			for (int i = 0; i < crm.SessionProperties.Count; i++)
			{
				if (crm.SessionProperties[i] != this._properties[i])
				{
					this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.SessionPropertiesDontMatch);
					return;
				}
			}
			GamerCollection<NetworkGamer> gamers = base.AllGamers;
			for (int j = 0; j < gamers.Count; j++)
			{
				if (gamers[j] != null && gamers[j].Gamertag == crm.Gamer.Gamertag)
				{
					this.FailConnection(msg.SenderConnection, NetworkSession.ResultCode.GamerAlreadyConnected);
					return;
				}
			}
			msg.SenderConnection.Tag = crm.Gamer;
			msg.SenderConnection.Approve();
		}

		private bool HandleHostMessages(NetIncomingMessage msg)
		{
			bool messageHandled = true;
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
					return messageHandled;
				}
			}
			else if (messageType != NetIncomingMessageType.Data)
			{
				if (messageType == NetIncomingMessageType.DiscoveryRequest)
				{
					this.HandleHostDiscoveryRequest(msg);
					return messageHandled;
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
			messageHandled = false;
			return messageHandled;
		}

		private void AddNewPeer(NetIncomingMessage msg)
		{
			byte id = msg.ReadByte();
			Gamer newGamer = msg.ReadGamer();
			base.AddProxyGamer(newGamer, false, id);
		}

		private bool HandleClientSystemMessages(NetIncomingMessage msg)
		{
			bool result = true;
			InternalMessageTypes msgType = (InternalMessageTypes)msg.ReadByte();
			NetworkGamer g = null;
			switch (msgType)
			{
			case InternalMessageTypes.NewPeer:
				this.AddNewPeer(msg);
				break;
			case InternalMessageTypes.ResponseToConnection:
			{
				ConnectedMessage cm = msg.ReadConnectedMessage();
				base.AddLocalGamer(this._signedInGamers[0], false, cm.PlayerGID);
				for (int i = 0; i < cm.Peers.Length; i++)
				{
					if (cm.ids[i] == 0)
					{
						this.AddRemoteGamer(cm.Peers[i], msg.SenderConnection, true, 0);
					}
					else
					{
						base.AddProxyGamer(cm.Peers[i], false, cm.ids[i]);
					}
				}
				break;
			}
			case InternalMessageTypes.DropPeer:
			{
				DropPeerMessage dropPeer = msg.ReadDropPeerMessage();
				if (this._idToGamer.TryGetValue(dropPeer.PlayerGID, out g))
				{
					base.RemoveGamer(g);
				}
				break;
			}
			default:
				result = false;
				break;
			}
			return result;
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
			bool messageHandled = true;
			NetIncomingMessageType messageType = msg.MessageType;
			if (messageType != NetIncomingMessageType.StatusChanged)
			{
				if (messageType != NetIncomingMessageType.Data)
				{
					messageHandled = false;
				}
				else
				{
					if (msg.SequenceChannel == 1)
					{
						return this.HandleClientSystemMessages(msg);
					}
					messageHandled = false;
				}
			}
			else
			{
				this.HandleClientStatusChangedMessage(msg);
			}
			return messageHandled;
		}

		private void FailConnection(NetConnection c, NetworkSession.ResultCode reason)
		{
			c.Deny(reason.ToString());
		}

		private bool HandleCommonMessages(NetIncomingMessage msg)
		{
			bool messageHandled = true;
			NetIncomingMessageType messageType = msg.MessageType;
			if (messageType <= NetIncomingMessageType.VerboseDebugMessage)
			{
				if (messageType != NetIncomingMessageType.Data)
				{
					if (messageType == NetIncomingMessageType.VerboseDebugMessage)
					{
						return messageHandled;
					}
				}
				else
				{
					byte pid = msg.ReadByte();
					NetworkGamer gamer = this.FindGamerById(pid);
					if (gamer == null)
					{
						return messageHandled;
					}
					LocalNetworkGamer localGamer = gamer as LocalNetworkGamer;
					if (localGamer == null)
					{
						return messageHandled;
					}
					byte senderid = msg.ReadByte();
					NetworkGamer ng = this.FindGamerById(senderid);
					if (ng != null)
					{
						byte[] data = msg.ReadByteArray();
						localGamer.AppendNewDataPacket(data, ng);
						return messageHandled;
					}
					return messageHandled;
				}
			}
			else if (messageType == NetIncomingMessageType.DebugMessage || messageType == NetIncomingMessageType.WarningMessage || messageType == NetIncomingMessageType.ErrorMessage)
			{
				return messageHandled;
			}
			messageHandled = false;
			return messageHandled;
		}

		public override void Update()
		{
			if (this._netSession != null)
			{
				NetIncomingMessage msg;
				while ((msg = this._netSession.ReadMessage()) != null)
				{
					if (!(this._isHost ? this.HandleHostMessages(msg) : this.HandleClientMessages(msg)))
					{
						bool messageHandled = this.HandleCommonMessages(msg);
					}
					if (this._netSession == null)
					{
						return;
					}
					this._netSession.Recycle(msg);
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
