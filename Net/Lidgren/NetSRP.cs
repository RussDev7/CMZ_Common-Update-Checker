using System;
using System.Security.Cryptography;
using System.Text;

namespace DNA.Net.Lidgren
{
	public static class NetSRP
	{
		private static HashAlgorithm GetHashAlgorithm()
		{
			return SHA256.Create();
		}

		private static NetBigInteger ComputeMultiplier()
		{
			string text = NetUtility.ToHexString(NetSRP.N.ToByteArrayUnsigned());
			string text2 = NetUtility.ToHexString(NetSRP.g.ToByteArrayUnsigned());
			string text3 = text + text2.PadLeft(text.Length, '0');
			byte[] array = NetUtility.ToByteArray(text3);
			HashAlgorithm hashAlgorithm = NetSRP.GetHashAlgorithm();
			byte[] array2 = hashAlgorithm.ComputeHash(array);
			return new NetBigInteger(NetUtility.ToHexString(array2), 16);
		}

		public static byte[] CreateRandomSalt()
		{
			byte[] array = new byte[16];
			NetRandom.Instance.NextBytes(array);
			return array;
		}

		public static byte[] CreateRandomEphemeral()
		{
			byte[] array = new byte[32];
			NetRandom.Instance.NextBytes(array);
			return array;
		}

		public static byte[] ComputePrivateKey(string username, string password, byte[] salt)
		{
			HashAlgorithm hashAlgorithm = NetSRP.GetHashAlgorithm();
			byte[] bytes = Encoding.UTF8.GetBytes(username + ":" + password);
			byte[] array = hashAlgorithm.ComputeHash(bytes);
			byte[] array2 = new byte[array.Length + salt.Length];
			Buffer.BlockCopy(salt, 0, array2, 0, salt.Length);
			Buffer.BlockCopy(array, 0, array2, salt.Length, array.Length);
			return new NetBigInteger(NetUtility.ToHexString(hashAlgorithm.ComputeHash(array2)), 16).ToByteArrayUnsigned();
		}

		public static byte[] ComputeServerVerifier(byte[] privateKey)
		{
			NetBigInteger netBigInteger = new NetBigInteger(NetUtility.ToHexString(privateKey), 16);
			NetBigInteger netBigInteger2 = NetSRP.g.ModPow(netBigInteger, NetSRP.N);
			return netBigInteger2.ToByteArrayUnsigned();
		}

		public static byte[] Hash(byte[] data)
		{
			HashAlgorithm hashAlgorithm = NetSRP.GetHashAlgorithm();
			return hashAlgorithm.ComputeHash(data);
		}

		public static byte[] ComputeClientEphemeral(byte[] clientPrivateEphemeral)
		{
			NetBigInteger netBigInteger = new NetBigInteger(NetUtility.ToHexString(clientPrivateEphemeral), 16);
			NetBigInteger netBigInteger2 = NetSRP.g.ModPow(netBigInteger, NetSRP.N);
			return netBigInteger2.ToByteArrayUnsigned();
		}

		public static byte[] ComputeServerEphemeral(byte[] serverPrivateEphemeral, byte[] verifier)
		{
			NetBigInteger netBigInteger = new NetBigInteger(NetUtility.ToHexString(serverPrivateEphemeral), 16);
			NetBigInteger netBigInteger2 = new NetBigInteger(NetUtility.ToHexString(verifier), 16);
			NetBigInteger netBigInteger3 = NetSRP.g.ModPow(netBigInteger, NetSRP.N);
			NetBigInteger netBigInteger4 = netBigInteger2.Multiply(NetSRP.k);
			NetBigInteger netBigInteger5 = netBigInteger4.Add(netBigInteger3).Mod(NetSRP.N);
			return netBigInteger5.ToByteArrayUnsigned();
		}

		public static byte[] ComputeU(byte[] clientPublicEphemeral, byte[] serverPublicEphemeral)
		{
			string text = NetUtility.ToHexString(clientPublicEphemeral);
			string text2 = NetUtility.ToHexString(serverPublicEphemeral);
			int num = 66;
			string text3 = text.PadLeft(num, '0') + text2.PadLeft(num, '0');
			byte[] array = NetUtility.ToByteArray(text3);
			HashAlgorithm hashAlgorithm = NetSRP.GetHashAlgorithm();
			byte[] array2 = hashAlgorithm.ComputeHash(array);
			return new NetBigInteger(NetUtility.ToHexString(array2), 16).ToByteArrayUnsigned();
		}

		public static byte[] ComputeServerSessionValue(byte[] clientPublicEphemeral, byte[] verifier, byte[] udata, byte[] serverPrivateEphemeral)
		{
			NetBigInteger netBigInteger = new NetBigInteger(NetUtility.ToHexString(clientPublicEphemeral), 16);
			NetBigInteger netBigInteger2 = new NetBigInteger(NetUtility.ToHexString(verifier), 16);
			NetBigInteger netBigInteger3 = new NetBigInteger(NetUtility.ToHexString(udata), 16);
			NetBigInteger netBigInteger4 = new NetBigInteger(NetUtility.ToHexString(serverPrivateEphemeral), 16);
			NetBigInteger netBigInteger5 = netBigInteger2.ModPow(netBigInteger3, NetSRP.N).Multiply(netBigInteger).Mod(NetSRP.N)
				.ModPow(netBigInteger4, NetSRP.N)
				.Mod(NetSRP.N);
			return netBigInteger5.ToByteArrayUnsigned();
		}

		public static byte[] ComputeClientSessionValue(byte[] serverPublicEphemeral, byte[] xdata, byte[] udata, byte[] clientPrivateEphemeral)
		{
			NetBigInteger netBigInteger = new NetBigInteger(NetUtility.ToHexString(serverPublicEphemeral), 16);
			NetBigInteger netBigInteger2 = new NetBigInteger(NetUtility.ToHexString(xdata), 16);
			NetBigInteger netBigInteger3 = new NetBigInteger(NetUtility.ToHexString(udata), 16);
			NetBigInteger netBigInteger4 = new NetBigInteger(NetUtility.ToHexString(clientPrivateEphemeral), 16);
			NetBigInteger netBigInteger5 = NetSRP.g.ModPow(netBigInteger2, NetSRP.N);
			NetBigInteger netBigInteger6 = netBigInteger.Add(NetSRP.N.Multiply(NetSRP.k)).Subtract(netBigInteger5.Multiply(NetSRP.k)).Mod(NetSRP.N);
			return netBigInteger6.ModPow(netBigInteger2.Multiply(netBigInteger3).Add(netBigInteger4), NetSRP.N).ToByteArrayUnsigned();
		}

		public static NetXtea CreateEncryption(byte[] sessionValue)
		{
			HashAlgorithm hashAlgorithm = NetSRP.GetHashAlgorithm();
			byte[] array = hashAlgorithm.ComputeHash(sessionValue);
			byte[] array2 = new byte[16];
			for (int i = 0; i < 16; i++)
			{
				array2[i] = array[i];
				for (int j = 1; j < array.Length / 16; j++)
				{
					byte[] array3 = array2;
					int num = i;
					array3[num] ^= array[i + j * 16];
				}
			}
			return new NetXtea(array2);
		}

		private static readonly NetBigInteger N = new NetBigInteger("0115b8b692e0e045692cf280b436735c77a5a9e8a9e7ed56c965f87db5b2a2ece3", 16);

		private static readonly NetBigInteger g = NetBigInteger.Two;

		private static readonly NetBigInteger k = NetSRP.ComputeMultiplier();
	}
}
