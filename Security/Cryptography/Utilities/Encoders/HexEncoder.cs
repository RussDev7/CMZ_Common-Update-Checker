using System;
using System.IO;

namespace DNA.Security.Cryptography.Utilities.Encoders
{
	public class HexEncoder : IEncoder
	{
		static HexEncoder()
		{
			for (int i = 0; i < HexEncoder.encodingTable.Length; i++)
			{
				HexEncoder.decodingTable[(int)HexEncoder.encodingTable[i]] = (byte)i;
			}
			HexEncoder.decodingTable[65] = HexEncoder.decodingTable[97];
			HexEncoder.decodingTable[66] = HexEncoder.decodingTable[98];
			HexEncoder.decodingTable[67] = HexEncoder.decodingTable[99];
			HexEncoder.decodingTable[68] = HexEncoder.decodingTable[100];
			HexEncoder.decodingTable[69] = HexEncoder.decodingTable[101];
			HexEncoder.decodingTable[70] = HexEncoder.decodingTable[102];
		}

		public int Encode(byte[] data, int off, int length, Stream outStream)
		{
			for (int i = off; i < off + length; i++)
			{
				int v = (int)data[i];
				outStream.WriteByte(HexEncoder.encodingTable[v >> 4]);
				outStream.WriteByte(HexEncoder.encodingTable[v & 15]);
			}
			return length * 2;
		}

		private bool ignore(char c)
		{
			return c == '\n' || c == '\r' || c == '\t' || c == ' ';
		}

		public int Decode(byte[] data, int off, int length, Stream outStream)
		{
			int outLen = 0;
			int end = off + length;
			while (end > off && this.ignore((char)data[end - 1]))
			{
				end--;
			}
			int i = off;
			while (i < end)
			{
				while (i < end && this.ignore((char)data[i]))
				{
					i++;
				}
				byte b = HexEncoder.decodingTable[(int)data[i++]];
				while (i < end && this.ignore((char)data[i]))
				{
					i++;
				}
				byte b2 = HexEncoder.decodingTable[(int)data[i++]];
				outStream.WriteByte((byte)(((int)b << 4) | (int)b2));
				outLen++;
			}
			return outLen;
		}

		public int DecodeString(string data, Stream outStream)
		{
			int length = 0;
			int end = data.Length;
			while (end > 0 && this.ignore(data[end - 1]))
			{
				end--;
			}
			int i = 0;
			while (i < end)
			{
				while (i < end && this.ignore(data[i]))
				{
					i++;
				}
				byte b = HexEncoder.decodingTable[(int)data[i++]];
				while (i < end && this.ignore(data[i]))
				{
					i++;
				}
				byte b2 = HexEncoder.decodingTable[(int)data[i++]];
				outStream.WriteByte((byte)(((int)b << 4) | (int)b2));
				length++;
			}
			return length;
		}

		private static readonly byte[] encodingTable = new byte[]
		{
			48, 49, 50, 51, 52, 53, 54, 55, 56, 57,
			97, 98, 99, 100, 101, 102
		};

		internal static readonly byte[] decodingTable = new byte[128];
	}
}
