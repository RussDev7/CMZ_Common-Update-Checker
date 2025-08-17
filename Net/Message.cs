﻿using System;
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
			Type typeFromHandle = typeof(T);
			return (T)((object)Message._sendInstances[(int)Message._messageIDs[typeFromHandle]]);
		}

		private void DoSendInternal(NetworkGamer recipiant)
		{
			lock (Message._writer)
			{
				MemoryStream memoryStream = (MemoryStream)Message._writeBufferStream.BaseStream;
				memoryStream.Position = 0L;
				Message._writeBufferStream.Reset();
				Message._writer.Write(this.MessageID);
				this.SendData(Message._writer);
				Message._writer.Flush();
				byte checksumValue = Message._writeBufferStream.ChecksumValue;
				Message._writer.Write(checksumValue);
				Message._writer.Flush();
				if (!this._sender.HasLeftSession)
				{
					if (recipiant != null)
					{
						if (!recipiant.HasLeftSession)
						{
							((LocalNetworkGamer)this._sender).SendData(memoryStream.GetBuffer(), 0, (int)memoryStream.Position, this.SendDataOptions, recipiant);
						}
					}
					else
					{
						((LocalNetworkGamer)this._sender).SendData(memoryStream.GetBuffer(), 0, (int)memoryStream.Position, this.SendDataOptions);
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
			byte b = 0;
			while ((int)b < Message._messageTypes.Length)
			{
				Message._messageIDs[Message._messageTypes[(int)b]] = b;
				b += 1;
			}
			Message._receiveInstance = new Message[Message._messageTypes.Length];
			Message._sendInstances = new Message[Message._messageTypes.Length];
			for (int i = 0; i < Message._messageTypes.Length; i++)
			{
				ConstructorInfo constructor = Message._messageTypes[i].GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);
				if (constructor == null)
				{
					throw new Exception(Message._messageTypes[i].Name + " Needs a private parameterless constructor");
				}
				Message._receiveInstance[i] = (Message)constructor.Invoke(new object[0]);
				Message._sendInstances[i] = (Message)constructor.Invoke(new object[0]);
			}
		}

		private static Message ReadMessage(NetworkGamer sender)
		{
			Message._readBufferStream.Reset();
			byte b = Message._reader.ReadByte();
			Message message = Message._receiveInstance[(int)b];
			message._sender = sender;
			message.RecieveData(Message._reader);
			byte checksumValue = Message._readBufferStream.ChecksumValue;
			byte b2 = Message._reader.ReadByte();
			if (checksumValue != b2)
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
				MemoryStream memoryStream = (MemoryStream)Message._readBufferStream.BaseStream;
				int num = 0;
				NetworkGamer networkGamer;
				for (;;)
				{
					try
					{
						num = localGamer.ReceiveData(Message.messageBuffer, out networkGamer);
					}
					catch (ArgumentException)
					{
						Message.messageBuffer = new byte[Message.messageBuffer.Length * 2];
						continue;
					}
					break;
				}
				memoryStream.Position = 0L;
				memoryStream.Write(Message.messageBuffer, 0, num);
				memoryStream.Position = 0L;
				if (localGamer == networkGamer)
				{
					message = Message.ReadMessage(networkGamer);
				}
				else
				{
					try
					{
						message = Message.ReadMessage(networkGamer);
					}
					catch (Exception ex)
					{
						throw new InvalidMessageException(networkGamer, ex);
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
