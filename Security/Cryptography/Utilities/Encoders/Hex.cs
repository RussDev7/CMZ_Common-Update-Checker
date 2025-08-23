using System;
using System.IO;

namespace DNA.Security.Cryptography.Utilities.Encoders
{
	public sealed class Hex
	{
		private Hex()
		{
		}

		public static byte[] Encode(byte[] data)
		{
			return Hex.Encode(data, 0, data.Length);
		}

		public static byte[] Encode(byte[] data, int off, int length)
		{
			MemoryStream bOut = new MemoryStream(length * 2);
			Hex.encoder.Encode(data, off, length, bOut);
			return bOut.ToArray();
		}

		public static int Encode(byte[] data, Stream outStream)
		{
			return Hex.encoder.Encode(data, 0, data.Length, outStream);
		}

		public static int Encode(byte[] data, int off, int length, Stream outStream)
		{
			return Hex.encoder.Encode(data, off, length, outStream);
		}

		public static byte[] Decode(byte[] data)
		{
			MemoryStream bOut = new MemoryStream((data.Length + 1) / 2);
			Hex.encoder.Decode(data, 0, data.Length, bOut);
			return bOut.ToArray();
		}

		public static byte[] Decode(string data)
		{
			MemoryStream bOut = new MemoryStream((data.Length + 1) / 2);
			Hex.encoder.DecodeString(data, bOut);
			return bOut.ToArray();
		}

		public static int Decode(string data, Stream outStream)
		{
			return Hex.encoder.DecodeString(data, outStream);
		}

		private static readonly IEncoder encoder = new HexEncoder();
	}
}
