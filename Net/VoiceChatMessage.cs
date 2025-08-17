using System;
using System.IO;
using DNA.Net.GamerServices;

namespace DNA.Net
{
	public class VoiceChatMessage : Message
	{
		private VoiceChatMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, byte[] _audioBuffer)
		{
			VoiceChatMessage sendInstance = Message.GetSendInstance<VoiceChatMessage>();
			sendInstance.AudioBuffer = _audioBuffer;
			sendInstance.DoSend(from);
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
			int num = reader.ReadInt32();
			if (this.AudioBuffer.Length != num)
			{
				this.AudioBuffer = new byte[num];
			}
			this.AudioBuffer = reader.ReadBytes(num);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.AudioBuffer.Length);
			writer.Write(this.AudioBuffer);
		}

		public byte[] AudioBuffer = new byte[0];
	}
}
