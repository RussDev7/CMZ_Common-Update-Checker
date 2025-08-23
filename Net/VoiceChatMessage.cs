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
			VoiceChatMessage Instance = Message.GetSendInstance<VoiceChatMessage>();
			Instance.AudioBuffer = _audioBuffer;
			Instance.DoSend(from);
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
			int samples = reader.ReadInt32();
			if (this.AudioBuffer.Length != samples)
			{
				this.AudioBuffer = new byte[samples];
			}
			this.AudioBuffer = reader.ReadBytes(samples);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.AudioBuffer.Length);
			writer.Write(this.AudioBuffer);
		}

		public byte[] AudioBuffer = new byte[0];
	}
}
