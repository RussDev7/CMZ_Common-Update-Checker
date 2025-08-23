using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DNA.Collections;
using DNA.IO;
using DNA.IO.Checksums;
using DNA.Net.GamerServices;
using DNA.Reflection;

namespace DNA.Net
{
	public abstract class Message
	{
		public virtual bool Echo
		{
			get
			{
				return true;
			}
		}

		public NetworkGamer Sender
		{
			get
			{
				return this._sender;
			}
		}

		public byte MessageID
		{
			get
			{
				return Message._messageIDs[base.GetType()];
			}
		}

		static Message()
		{
			Message.PopulateMessageTypes();
		}

		protected abstract SendDataOptions SendDataOptions { get; }

		protected abstract void RecieveData(BinaryReader reader);

		protected abstract void SendData(BinaryWriter writer);

		protected static T GetSendInstance<T>() where T : Message
		{
			Type t = typeof(T);
			return (T)((object)Message._sendInstances[(int)Message._messageIDs[t]]);
		}

		private void DoSendInternal(NetworkGamer recipiant)
		{
			lock (Message._writer)
			{
				MemoryStream baseStream = (MemoryStream)Message._writeBufferStream.BaseStream;
				baseStream.Position = 0L;
				Message._writeBufferStream.Reset();
				Message._writer.Write(this.MessageID);
				this.SendData(Message._writer);
				Message._writer.Flush();
				byte checksum = Message._writeBufferStream.ChecksumValue;
				Message._writer.Write(checksum);
				Message._writer.Flush();
				if (!this._sender.HasLeftSession)
				{
					if (recipiant != null)
					{
						if (!recipiant.HasLeftSession)
						{
							((LocalNetworkGamer)this._sender).SendData(baseStream.GetBuffer(), 0, (int)baseStream.Position, this.SendDataOptions, recipiant);
						}
					}
					else
					{
						((LocalNetworkGamer)this._sender).SendData(baseStream.GetBuffer(), 0, (int)baseStream.Position, this.SendDataOptions);
					}
				}
			}
		}

		protected void DoSend(LocalNetworkGamer sender)
		{
			this._sender = sender;
			this.DoSendInternal(null);
		}

		protected void DoSend(LocalNetworkGamer sender, NetworkGamer recipiant)
		{
			this._sender = sender;
			this.DoSendInternal(recipiant);
		}

		private static bool TypeFilter(Type type)
		{
			return type.IsSubclassOf(typeof(Message)) && !type.IsAbstract;
		}

		private static void PopulateMessageTypes()
		{
			Message._messageTypes = ReflectionTools.GetTypes(new Filter<Type>(Message.TypeFilter));
			Message._messageIDs = new Dictionary<Type, byte>();
			byte i = 0;
			while ((int)i < Message._messageTypes.Length)
			{
				Message._messageIDs[Message._messageTypes[(int)i]] = i;
				i += 1;
			}
			Message._receiveInstance = new Message[Message._messageTypes.Length];
			Message._sendInstances = new Message[Message._messageTypes.Length];
			for (int j = 0; j < Message._messageTypes.Length; j++)
			{
				ConstructorInfo c = Message._messageTypes[j].GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);
				if (c == null)
				{
					throw new Exception(Message._messageTypes[j].Name + " Needs a private parameterless constructor");
				}
				Message._receiveInstance[j] = (Message)c.Invoke(new object[0]);
				Message._sendInstances[j] = (Message)c.Invoke(new object[0]);
			}
		}

		private static Message ReadMessage(NetworkGamer sender)
		{
			Message._readBufferStream.Reset();
			byte messageID = Message._reader.ReadByte();
			Message message = Message._receiveInstance[(int)messageID];
			message._sender = sender;
			message.RecieveData(Message._reader);
			byte checksum = Message._readBufferStream.ChecksumValue;
			byte fileChecksum = Message._reader.ReadByte();
			if (checksum != fileChecksum)
			{
				throw new Exception("CheckSum Error");
			}
			return message;
		}

		public static Message GetMessage(LocalNetworkGamer localGamer)
		{
			Message message;
			lock (Message._reader)
			{
				MemoryStream baseStream = (MemoryStream)Message._readBufferStream.BaseStream;
				int packetSize = 0;
				NetworkGamer sender;
				for (;;)
				{
					try
					{
						packetSize = localGamer.ReceiveData(Message.messageBuffer, out sender);
					}
					catch (ArgumentException)
					{
						Message.messageBuffer = new byte[Message.messageBuffer.Length * 2];
						continue;
					}
					break;
				}
				baseStream.Position = 0L;
				baseStream.Write(Message.messageBuffer, 0, packetSize);
				baseStream.Position = 0L;
				if (localGamer == sender)
				{
					message = Message.ReadMessage(sender);
				}
				else
				{
					try
					{
						message = Message.ReadMessage(sender);
					}
					catch (Exception ex)
					{
						throw new InvalidMessageException(sender, ex);
					}
				}
			}
			return message;
		}

		private static Message[] _sendInstances;

		private static Message[] _receiveInstance;

		private static Type[] _messageTypes;

		private static Dictionary<Type, byte> _messageIDs;

		private static ChecksumStream<byte> _writeBufferStream = new ChecksumStream<byte>(new MemoryStream(4096), new XOR8Checksum());

		private static BinaryWriter _writer = new BinaryWriter(Message._writeBufferStream);

		private static ChecksumStream<byte> _readBufferStream = new ChecksumStream<byte>(new MemoryStream(4096), new XOR8Checksum());

		private static BinaryReader _reader = new BinaryReader(Message._readBufferStream);

		protected NetworkGamer _sender;

		private static byte[] messageBuffer = new byte[4096];
	}
}
