using System;
using DNA.Audio;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework.Audio;

namespace DNA.Net
{
	public class VoiceChat
	{
		static VoiceChat()
		{
			VoiceChat.BuildALawMap();
			Microphone @default = Microphone.Default;
		}

		public VoiceChat(LocalNetworkGamer gamer)
		{
			this._gamer = gamer;
			this._microphone = AudioTools.GetMic(gamer.SignedInGamer);
			this._microphone.BufferDuration = TimeSpan.FromMilliseconds(100.0);
			this.handler = new EventHandler<EventArgs>(this._microphone_BufferReady);
			this._microphone.BufferReady += this.handler;
			this._micBuffer = new byte[this._microphone.GetSampleSizeInBytes(this._microphone.BufferDuration)];
			this._playBuffer = new byte[this._micBuffer.Length];
			this._sendBuffer = new byte[this._micBuffer.Length / 2];
			this._playbackEffect = new DynamicSoundEffectInstance(this._microphone.SampleRate, AudioChannels.Mono);
			this._playbackEffect.SubmitBuffer(this._micBuffer);
			this._playbackEffect.Play();
		}

		private void _microphone_BufferReady(object sender, EventArgs e)
		{
			this._microphone.GetData(this._micBuffer);
			int samples = this._micBuffer.Length / 2;
			int readpos = 0;
			for (int i = 0; i < samples; i++)
			{
				short low = (short)this._micBuffer[readpos + 1];
				short high = (short)(this._micBuffer[readpos] << 8);
				short sample = low | high;
				this._sendBuffer[i] = VoiceChat._pcmToALawMap[(int)sample];
				readpos += 2;
			}
			VoiceChatMessage.Send(this._gamer, this._sendBuffer);
		}

		public void ProcessMessage(VoiceChatMessage message)
		{
			int writepos = 0;
			for (int i = 0; i < message.AudioBuffer.Length; i++)
			{
				short sample = VoiceChat._aLawToPcmMap[(int)message.AudioBuffer[i]];
				this._playBuffer[writepos++] = (byte)(sample & 255);
				this._playBuffer[writepos++] = (byte)(sample >> 8);
			}
			this._playbackEffect.SubmitBuffer(this._playBuffer);
		}

		private static void BuildALawMap()
		{
			VoiceChat._pcmToALawMap = new byte[65536];
			for (int i = -32768; i <= 32767; i++)
			{
				VoiceChat._pcmToALawMap[i & 65535] = VoiceChat.EncodeALawSample(i);
			}
			VoiceChat._aLawToPcmMap = new short[256];
			for (byte j = 0; j < 255; j += 1)
			{
				VoiceChat._aLawToPcmMap[(int)j] = VoiceChat.DecodeALawSample(j);
			}
		}

		private static byte EncodeALawSample(int pcm)
		{
			int sign = (pcm & 32768) >> 8;
			if (sign != 0)
			{
				pcm = -pcm;
			}
			if (pcm > 32767)
			{
				pcm = 32767;
			}
			int exponent = 7;
			int expMask = 16384;
			while ((pcm & expMask) == 0 && exponent > 0)
			{
				exponent--;
				expMask >>= 1;
			}
			int mantissa = (pcm >> (((exponent == 0) ? 4 : (exponent + 3)) & 31)) & 15;
			byte alaw = (byte)(sign | (exponent << 4) | mantissa);
			return alaw ^ 213;
		}

		private static short DecodeALawSample(byte alaw)
		{
			alaw ^= 213;
			int sign = (int)(alaw & 128);
			int exponent = (alaw & 112) >> 4;
			int data = (int)(alaw & 15);
			data <<= 4;
			data += 8;
			if (exponent != 0)
			{
				data += 256;
			}
			if (exponent > 1)
			{
				data <<= exponent - 1;
			}
			return (short)((sign == 0) ? data : (-(short)data));
		}

		private Microphone _microphone;

		private EventHandler<EventArgs> handler;

		private DynamicSoundEffectInstance _playbackEffect;

		private LocalNetworkGamer _gamer;

		private byte[] _micBuffer;

		private byte[] _sendBuffer;

		private byte[] _playBuffer;

		private static byte[] _pcmToALawMap;

		private static short[] _aLawToPcmMap;
	}
}
