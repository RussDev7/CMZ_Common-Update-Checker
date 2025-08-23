using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using DNA.Net.GamerServices;

namespace DNA.Net.ReliableUDP
{
	public class ReliableUDPClient : IDisposable
	{
		public int PacketsReady
		{
			get
			{
				return this.DispatchQueue.Count;
			}
		}

		public int ReadPacket(byte[] buffer, int offset, out IPEndPoint endpoint)
		{
			if (this.PacketsReady == 0)
			{
				endpoint = null;
				return -1;
			}
			ReliableUDPClient.Packet packet = this.DispatchQueue.Dequeue();
			int datalen = packet.PayloadLength;
			Array.Copy(packet.PayloadBuffer, 0, buffer, offset, datalen);
			endpoint = packet.Sender.EndPoint;
			packet.Release();
			return datalen;
		}

		private ReliableUDPClient.Host GetHostFromEndpoint(IPEndPoint endPoint)
		{
			for (int i = 0; i < this.Hosts.Count; i++)
			{
				if (this.Hosts[i].EndPoint.Equals(endPoint))
				{
					return this.Hosts[i];
				}
			}
			ReliableUDPClient.Host host = new ReliableUDPClient.Host();
			host.EndPoint = endPoint;
			this.Hosts.Add(host);
			return host;
		}

		public ReliableUDPClient(int port)
		{
			IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
			this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			this._socket.Bind(endPoint);
			this._receiveStream = new MemoryStream(65535);
			this._receiveStream.SetLength((long)this._receiveStream.Capacity);
			this._receiveReader = new BinaryReader(this._receiveStream);
		}

		private static ReliableUDPClient.CommandWords ToCommandWord(SendDataOptions option)
		{
			switch (option)
			{
			case SendDataOptions.None:
				return ReliableUDPClient.CommandWords.Data;
			case SendDataOptions.Reliable:
				return ReliableUDPClient.CommandWords.DataReliable;
			case SendDataOptions.InOrder:
				return ReliableUDPClient.CommandWords.DataInOrder;
			case SendDataOptions.ReliableInOrder:
				return ReliableUDPClient.CommandWords.DataInOrderReliable;
			case SendDataOptions.Chat:
				return ReliableUDPClient.CommandWords.DataInOrderReliable;
			default:
				throw new Exception("Bad option");
			}
		}

		public void SendPacket(byte[] sendData, int offset, int length, IPEndPoint endPoint, SendDataOptions option)
		{
			ReliableUDPClient.Packet packet = ReliableUDPClient.Packet.Create(ReliableUDPClient.ToCommandWord(option));
			packet.SetData(sendData, offset, length);
			ReliableUDPClient.Host sendTo = this.GetHostFromEndpoint(endPoint);
			switch (option)
			{
			case SendDataOptions.None:
				packet.Send(sendTo);
				packet.Release();
				return;
			case SendDataOptions.Reliable:
				packet.SequenceNumber = sendTo.NextReliablePacketNumberSent++;
				lock (sendTo._unacknolegedReliablePackets)
				{
					sendTo._unacknolegedReliablePackets[packet.SequenceNumber] = packet;
				}
				packet.Send(sendTo);
				return;
			case SendDataOptions.InOrder:
				packet.SequenceNumber = sendTo.NextInOrderPacketNumberSent++;
				packet.Send(sendTo);
				packet.Release();
				return;
			case SendDataOptions.ReliableInOrder:
				packet.SequenceNumber = sendTo.NextReliableInOrderPacketNumberSent++;
				lock (sendTo._unacknolegedReliableInOrderPackets)
				{
					sendTo._unacknolegedReliableInOrderPackets[packet.SequenceNumber] = packet;
				}
				packet.Send(sendTo);
				return;
			default:
				return;
			}
		}

		private void SendNACK(ReliableUDPClient.Host sendTo)
		{
			ReliableUDPClient.Packet packet = ReliableUDPClient.Packet.Create(ReliableUDPClient.CommandWords.NACK);
			packet.Send(sendTo);
			packet.Release();
		}

		private void SendNACKRIO(ReliableUDPClient.Host sendTo)
		{
			ReliableUDPClient.Packet packet = ReliableUDPClient.Packet.Create(ReliableUDPClient.CommandWords.NACKRIO);
			packet.Send(sendTo);
			packet.Release();
		}

		public void FlushSendBuffers()
		{
			for (int i = 0; i < this.Hosts.Count; i++)
			{
				lock (this.Hosts[i]._sendWriter)
				{
					this.Hosts[i]._sendWriter.Flush();
					if (this.Hosts[i]._sendStream.Position > 0L)
					{
						this._socket.SendTo(this.Hosts[i]._sendStream.GetBuffer(), 0, (int)this.Hosts[i]._sendStream.Position, SocketFlags.None, this.Hosts[i].EndPoint);
						this.Hosts[i]._sendStream.Position = 0L;
					}
				}
			}
		}

		public void ReceivePackets()
		{
			lock (this._receiveStream)
			{
				this._receiveStream.Position = 0L;
				EndPoint endPoint = new IPEndPoint(0L, 0);
				int packetLength = this._socket.ReceiveFrom(this._receiveStream.GetBuffer(), ref endPoint);
				ReliableUDPClient.Host sender = this.GetHostFromEndpoint((IPEndPoint)endPoint);
				bool checkNack = false;
				bool checkNackRIO = false;
				bool sendNack = false;
				bool sendNackRIO = false;
				IL_0415:
				while (this._receiveStream.Position < (long)packetLength)
				{
					ReliableUDPClient.Packet packet = ReliableUDPClient.Packet.Read(sender, this._receiveReader);
					if (this._pakectLossRand.Decide(this.SimulatePacketLoss))
					{
						packet.Release();
					}
					else
					{
						switch (packet.PacketType)
						{
						case ReliableUDPClient.CommandWords.Data:
							this.DispatchQueue.Enqueue(packet);
							continue;
						case ReliableUDPClient.CommandWords.DataReliable:
						{
							checkNack = true;
							LinkedListNode<int> node = sender.ReliablePacketsReceived.First;
							ReliableUDPClient.Packet ackPacket = ReliableUDPClient.Packet.Create(ReliableUDPClient.CommandWords.ACK);
							ackPacket.AckNumber = packet.SequenceNumber;
							ackPacket.Send(sender);
							ackPacket.Release();
							LinkedListNode<int> nextNode;
							for (;;)
							{
								nextNode = node.Next;
								if (packet.SequenceNumber <= node.Value)
								{
									goto Block_6;
								}
								if (nextNode == null)
								{
									goto Block_7;
								}
								if (nextNode.Value > packet.SequenceNumber)
								{
									goto Block_8;
								}
								node = nextNode;
							}
							IL_01DF:
							while (sender.ReliablePacketsReceived.First.Next != null)
							{
								if (sender.ReliablePacketsReceived.First.Value != sender.ReliablePacketsReceived.First.Next.Value - 1)
								{
									break;
								}
								sender.ReliablePacketsReceived.RemoveFirst();
							}
							continue;
							Block_8:
							node = sender.ReliablePacketsReceived.AddBefore(nextNode, packet.SequenceNumber);
							this.DispatchQueue.Enqueue(packet);
							goto IL_01DF;
							Block_7:
							node = sender.ReliablePacketsReceived.AddLast(packet.SequenceNumber);
							this.DispatchQueue.Enqueue(packet);
							goto IL_01DF;
							Block_6:
							packet.Release();
							goto IL_01DF;
						}
						case ReliableUDPClient.CommandWords.DataInOrder:
							if (packet.SequenceNumber > packet.Sender.LastInOrderPacketNumberReceived)
							{
								packet.Sender.LastInOrderPacketNumberReceived = packet.SequenceNumber;
								this.DispatchQueue.Enqueue(packet);
								continue;
							}
							packet.Release();
							continue;
						case ReliableUDPClient.CommandWords.DataInOrderReliable:
						{
							ReliableUDPClient.Packet ackPacket2 = ReliableUDPClient.Packet.Create(ReliableUDPClient.CommandWords.ACKRIO);
							ackPacket2.AckNumber = packet.SequenceNumber;
							ackPacket2.Send(sender);
							ackPacket2.Release();
							checkNackRIO = true;
							LinkedListNode<ReliableUDPClient.Packet> node2 = sender.ReliableInOrderPacketsReceived.First;
							if (packet.SequenceNumber <= sender.LastRIOPacketDispatched)
							{
								packet.Release();
								continue;
							}
							while (node2 != null)
							{
								if (packet.SequenceNumber == node2.Value.SequenceNumber)
								{
									packet.Release();
								}
								else
								{
									if (packet.SequenceNumber >= node2.Value.SequenceNumber)
									{
										node2 = node2.Next;
										continue;
									}
									sender.ReliableInOrderPacketsReceived.AddBefore(node2, packet);
								}
								IL_0310:
								while (sender.ReliableInOrderPacketsReceived.First != null)
								{
									if (sender.ReliableInOrderPacketsReceived.First.Value.SequenceNumber != sender.LastRIOPacketDispatched + 1)
									{
										break;
									}
									ReliableUDPClient.Packet toDispatch = sender.ReliableInOrderPacketsReceived.First.Value;
									sender.LastRIOPacketDispatched = toDispatch.SequenceNumber;
									this.DispatchQueue.Enqueue(toDispatch);
									sender.ReliableInOrderPacketsReceived.RemoveFirst();
								}
								goto IL_0415;
							}
							sender.ReliableInOrderPacketsReceived.AddLast(packet);
							goto IL_0310;
						}
						case ReliableUDPClient.CommandWords.ACK:
							lock (sender._unacknolegedReliablePackets)
							{
								ReliableUDPClient.Packet rioPacket;
								if (sender._unacknolegedReliablePackets.TryGetValue(packet.AckNumber, out rioPacket))
								{
									sender._unacknolegedReliablePackets.Remove(packet.AckNumber);
									rioPacket.Release();
								}
								packet.Release();
								continue;
							}
							break;
						case ReliableUDPClient.CommandWords.ACKRIO:
							break;
						case ReliableUDPClient.CommandWords.NACK:
							goto IL_03F7;
						case ReliableUDPClient.CommandWords.NACKRIO:
							sendNackRIO = true;
							packet.Release();
							continue;
						default:
							throw new NotImplementedException();
						}
						lock (sender._unacknolegedReliableInOrderPackets)
						{
							ReliableUDPClient.Packet rioPacket2;
							if (sender._unacknolegedReliableInOrderPackets.TryGetValue(packet.AckNumber, out rioPacket2))
							{
								sender._unacknolegedReliableInOrderPackets.Remove(packet.AckNumber);
								rioPacket2.Release();
							}
							packet.Release();
							continue;
						}
						IL_03F7:
						sendNack = true;
						packet.Release();
					}
				}
				if (checkNack && sender.ReliablePacketsReceived.Count > 1)
				{
					this.SendNACK(sender);
				}
				if (checkNackRIO && sender.ReliableInOrderPacketsReceived.Count > 0)
				{
					this.SendNACKRIO(sender);
				}
				if (sendNack)
				{
					lock (sender._unacknolegedReliablePackets)
					{
						foreach (KeyValuePair<int, ReliableUDPClient.Packet> pair in sender._unacknolegedReliablePackets)
						{
							pair.Value.Send(sender);
						}
					}
				}
				if (sendNackRIO)
				{
					lock (sender._unacknolegedReliableInOrderPackets)
					{
						foreach (KeyValuePair<int, ReliableUDPClient.Packet> pair2 in sender._unacknolegedReliableInOrderPackets)
						{
							pair2.Value.Send(sender);
						}
					}
				}
			}
		}

		~ReliableUDPClient()
		{
			if (!this._disposed)
			{
				this.Dispose();
			}
			GC.SuppressFinalize(this);
		}

		public void Dispose()
		{
			if (!this._disposed)
			{
				this._socket.Dispose();
				this._disposed = true;
			}
		}

		public const int MaxPacketSize = 65535;

		public float SimulatePacketLoss;

		public Random _pakectLossRand = new Random();

		private Socket _socket;

		private MemoryStream _receiveStream;

		private BinaryReader _receiveReader;

		private List<ReliableUDPClient.Packet> ReliablePackets = new List<ReliableUDPClient.Packet>();

		private List<ReliableUDPClient.Host> Hosts = new List<ReliableUDPClient.Host>();

		private Queue<ReliableUDPClient.Packet> DispatchQueue = new Queue<ReliableUDPClient.Packet>();

		private bool _disposed;

		private enum CommandWords : byte
		{
			Data,
			DataReliable,
			DataInOrder,
			DataInOrderReliable,
			ACK,
			ACKRIO,
			NACK,
			NACKRIO,
			HeartBeat,
			Disconnect
		}

		private class Host
		{
			public Host()
			{
				this.ReliablePacketsReceived.AddLast(0);
				this._sendWriter = new BinaryWriter(this._sendStream);
			}

			public IPEndPoint EndPoint = new IPEndPoint(IPAddress.Any, 0);

			public DateTime LastSentTo = DateTime.Now;

			public DateTime LastHeardFrom = DateTime.Now;

			public int LastInOrderPacketNumberReceived;

			public LinkedList<int> ReliablePacketsReceived = new LinkedList<int>();

			public LinkedList<ReliableUDPClient.Packet> ReliableInOrderPacketsReceived = new LinkedList<ReliableUDPClient.Packet>();

			public List<ReliableUDPClient.Packet> InOrderBuffer = new List<ReliableUDPClient.Packet>();

			public MemoryStream _sendStream = new MemoryStream(65535);

			public BinaryWriter _sendWriter;

			public int NextInOrderPacketNumberSent = 1;

			public int NextReliablePacketNumberSent = 1;

			public int NextReliableInOrderPacketNumberSent = 1;

			public Dictionary<int, ReliableUDPClient.Packet> _unacknolegedReliablePackets = new Dictionary<int, ReliableUDPClient.Packet>();

			public Dictionary<int, ReliableUDPClient.Packet> _unacknolegedReliableInOrderPackets = new Dictionary<int, ReliableUDPClient.Packet>();

			public int LastRIOPacketDispatched;
		}

		private class Packet
		{
			public ReliableUDPClient.CommandWords PacketType
			{
				get
				{
					return this._packetType;
				}
			}

			public byte[] PayloadBuffer
			{
				get
				{
					return this._playloadStream.GetBuffer();
				}
			}

			public int PayloadLength
			{
				get
				{
					return (int)this._playloadStream.Length;
				}
			}

			public static ReliableUDPClient.Packet Read(ReliableUDPClient.Host sender, BinaryReader reader)
			{
				ReliableUDPClient.CommandWords word = (ReliableUDPClient.CommandWords)reader.ReadByte();
				ReliableUDPClient.Packet packet = ReliableUDPClient.Packet.Create(word);
				packet.Receive(sender, reader);
				return packet;
			}

			public static ReliableUDPClient.Packet Create(ReliableUDPClient.CommandWords packetType)
			{
				ReliableUDPClient.Packet packet;
				lock (ReliableUDPClient.Packet.PacketGrave)
				{
					if (ReliableUDPClient.Packet.PacketGrave.Count > 0)
					{
						packet = ReliableUDPClient.Packet.PacketGrave.Dequeue();
					}
					else
					{
						packet = new ReliableUDPClient.Packet();
					}
				}
				packet._packetType = packetType;
				return packet;
			}

			private Packet()
			{
				this._playloadStream = new MemoryStream(4096);
			}

			private void Receive(ReliableUDPClient.Host sender, BinaryReader reader)
			{
				this.Sender = sender;
				switch (this._packetType)
				{
				case ReliableUDPClient.CommandWords.Data:
					this.ReadPayload(reader);
					return;
				case ReliableUDPClient.CommandWords.DataReliable:
					this.SequenceNumber = reader.ReadInt32();
					this.ReadPayload(reader);
					return;
				case ReliableUDPClient.CommandWords.DataInOrder:
					this.SequenceNumber = reader.ReadInt32();
					this.ReadPayload(reader);
					return;
				case ReliableUDPClient.CommandWords.DataInOrderReliable:
					this.SequenceNumber = reader.ReadInt32();
					this.ReadPayload(reader);
					return;
				case ReliableUDPClient.CommandWords.ACK:
					this.AckNumber = reader.ReadInt32();
					return;
				case ReliableUDPClient.CommandWords.ACKRIO:
					this.AckNumber = reader.ReadInt32();
					return;
				case ReliableUDPClient.CommandWords.NACK:
				case ReliableUDPClient.CommandWords.NACKRIO:
					return;
				default:
					throw new NotImplementedException();
				}
			}

			public override string ToString()
			{
				return "Packet #" + this.SequenceNumber.ToString();
			}

			private void ReadPayload(BinaryReader reader)
			{
				int len = (int)reader.ReadUInt16();
				this._playloadStream.SetLength((long)len);
				reader.Read(this._playloadStream.GetBuffer(), 0, (int)this._playloadStream.Length);
			}

			private void WritePayload(BinaryWriter writer)
			{
				if (this._playloadStream.Position > 65535L)
				{
					throw new Exception("Packet too Big");
				}
				ushort len = (ushort)this._playloadStream.Position;
				writer.Write(len);
				writer.Write(this._playloadStream.GetBuffer(), 0, (int)this._playloadStream.Position);
			}

			public void Send(ReliableUDPClient.Host sendTo)
			{
				this.LastSent = DateTime.Now;
				sendTo.LastSentTo = DateTime.Now;
				BinaryWriter writer = sendTo._sendWriter;
				lock (writer)
				{
					writer.Write((byte)this._packetType);
					switch (this.PacketType)
					{
					case ReliableUDPClient.CommandWords.Data:
						this.WritePayload(sendTo._sendWriter);
						break;
					case ReliableUDPClient.CommandWords.DataReliable:
						writer.Write(this.SequenceNumber);
						this.WritePayload(sendTo._sendWriter);
						break;
					case ReliableUDPClient.CommandWords.DataInOrder:
						writer.Write(this.SequenceNumber);
						this.WritePayload(sendTo._sendWriter);
						break;
					case ReliableUDPClient.CommandWords.DataInOrderReliable:
						writer.Write(this.SequenceNumber);
						this.WritePayload(sendTo._sendWriter);
						break;
					case ReliableUDPClient.CommandWords.ACK:
						writer.Write(this.AckNumber);
						break;
					case ReliableUDPClient.CommandWords.ACKRIO:
						writer.Write(this.AckNumber);
						break;
					case ReliableUDPClient.CommandWords.NACK:
					case ReliableUDPClient.CommandWords.NACKRIO:
						break;
					default:
						throw new NotImplementedException();
					}
				}
			}

			private void Reset()
			{
				this.AckNumber = -1;
				this._playloadStream.Position = 0L;
				this.LastSent = DateTime.Now;
			}

			public void SetData(byte[] data, int offset, int length)
			{
				this._playloadStream.Position = 0L;
				this._playloadStream.Write(data, offset, length);
			}

			public void Release()
			{
				this.Reset();
				lock (ReliableUDPClient.Packet.PacketGrave)
				{
					ReliableUDPClient.Packet.PacketGrave.Enqueue(this);
				}
			}

			private static Queue<ReliableUDPClient.Packet> PacketGrave = new Queue<ReliableUDPClient.Packet>();

			private ReliableUDPClient.CommandWords _packetType;

			public ReliableUDPClient.Host Sender;

			public DateTime LastSent;

			public int SequenceNumber;

			private MemoryStream _playloadStream;

			public int AckNumber;
		}
	}
}
