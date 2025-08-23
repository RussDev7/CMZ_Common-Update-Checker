using System;
using System.IO;
using DNA.Net.GamerServices;

namespace DNA.Net
{
	public class ChatMessage : Message
	{
		private ChatMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, string message)
		{
			ChatMessage instance = DNA.Net.Message.GetSendInstance<ChatMessage>();
			instance.Message = message;
			instance.DoSend(from);
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.Chat;
			}
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.Message = reader.ReadString();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.Message);
		}

		public string Message = "";
	}
}
