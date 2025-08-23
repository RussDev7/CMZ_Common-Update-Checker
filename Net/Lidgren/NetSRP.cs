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
			string one = NetUtility.ToHexString(NetSRP.N.ToByteArrayUnsigned());
			string two = NetUtility.ToHexString(NetSRP.g.ToByteArrayUnsigned());
			string ccstr = one + two.PadLeft(one.Length, '0');
			byte[] cc = NetUtility.ToByteArray(ccstr);
			HashAlgorithm sha = NetSRP.GetHashAlgorithm();
			byte[] ccHashed = sha.ComputeHash(cc);
			return new NetBigInteger(NetUtility.ToHexString(ccHashed), 16);
		}

		public static byte[] CreateRandomSalt()
		{
			byte[] retval = new byte[16];
			NetRandom.Instance.NextBytes(retval);
			return retval;
		}

		public static byte[] CreateRandomEphemeral()
		{
			byte[] retval = new byte[32];
			NetRandom.Instance.NextBytes(retval);
			return retval;
		}

		public static byte[] ComputePrivateKey(string username, string password, byte[] salt)
		{
			HashAlgorithm sha = NetSRP.GetHashAlgorithm();
			byte[] tmp = Encoding.UTF8.GetBytes(username + ":" + password);
			byte[] innerHash = sha.ComputeHash(tmp);
			byte[] total = new byte[innerHash.Length + salt.Length];
			Buffer.BlockCopy(salt, 0, total, 0, salt.Length);
			Buffer.BlockCopy(innerHash, 0, total, salt.Length, innerHash.Length);
			return new NetBigInteger(NetUtility.ToHexString(sha.ComputeHash(total)), 16).ToByteArrayUnsigned();
		}

		public static byte[] ComputeServerVerifier(byte[] privateKey)
		{
			NetBigInteger x = new NetBigInteger(NetUtility.ToHexString(privateKey), 16);
			NetBigInteger serverVerifier = NetSRP.g.ModPow(x, NetSRP.N);
			return serverVerifier.ToByteArrayUnsigned();
		}

		public static byte[] Hash(byte[] data)
		{
			HashAlgorithm sha = NetSRP.GetHashAlgorithm();
			return sha.ComputeHash(data);
		}

		public static byte[] ComputeClientEphemeral(byte[] clientPrivateEphemeral)
		{
			NetBigInteger a = new NetBigInteger(NetUtility.ToHexString(clientPrivateEphemeral), 16);
			NetBigInteger retval = NetSRP.g.ModPow(a, NetSRP.N);
			return retval.ToByteArrayUnsigned();
		}

		public static byte[] ComputeServerEphemeral(byte[] serverPrivateEphemeral, byte[] verifier)
		{
			NetBigInteger b = new NetBigInteger(NetUtility.ToHexString(serverPrivateEphemeral), 16);
			NetBigInteger v = new NetBigInteger(NetUtility.ToHexString(verifier), 16);
			NetBigInteger bb = NetSRP.g.ModPow(b, NetSRP.N);
			NetBigInteger kv = v.Multiply(NetSRP.k);
			NetBigInteger B = kv.Add(bb).Mod(NetSRP.N);
			return B.ToByteArrayUnsigned();
		}

		public static byte[] ComputeU(byte[] clientPublicEphemeral, byte[] serverPublicEphemeral)
		{
			string one = NetUtility.ToHexString(clientPublicEphemeral);
			string two = NetUtility.ToHexString(serverPublicEphemeral);
			int len = 66;
			string ccstr = one.PadLeft(len, '0') + two.PadLeft(len, '0');
			byte[] cc = NetUtility.ToByteArray(ccstr);
			HashAlgorithm sha = NetSRP.GetHashAlgorithm();
			byte[] ccHashed = sha.ComputeHash(cc);
			return new NetBigInteger(NetUtility.ToHexString(ccHashed), 16).ToByteArrayUnsigned();
		}

		public static byte[] ComputeServerSessionValue(byte[] clientPublicEphemeral, byte[] verifier, byte[] udata, byte[] serverPrivateEphemeral)
		{
			NetBigInteger A = new NetBigInteger(NetUtility.ToHexString(clientPublicEphemeral), 16);
			NetBigInteger v = new NetBigInteger(NetUtility.ToHexString(verifier), 16);
			NetBigInteger u = new NetBigInteger(NetUtility.ToHexString(udata), 16);
			NetBigInteger b = new NetBigInteger(NetUtility.ToHexString(serverPrivateEphemeral), 16);
			NetBigInteger retval = v.ModPow(u, NetSRP.N).Multiply(A).Mod(NetSRP.N)
				.ModPow(b, NetSRP.N)
				.Mod(NetSRP.N);
			return retval.ToByteArrayUnsigned();
		}

		public static byte[] ComputeClientSessionValue(byte[] serverPublicEphemeral, byte[] xdata, byte[] udata, byte[] clientPrivateEphemeral)
		{
			NetBigInteger B = new NetBigInteger(NetUtility.ToHexString(serverPublicEphemeral), 16);
			NetBigInteger x = new NetBigInteger(NetUtility.ToHexString(xdata), 16);
			NetBigInteger u = new NetBigInteger(NetUtility.ToHexString(udata), 16);
			NetBigInteger a = new NetBigInteger(NetUtility.ToHexString(clientPrivateEphemeral), 16);
			NetBigInteger bx = NetSRP.g.ModPow(x, NetSRP.N);
			NetBigInteger btmp = B.Add(NetSRP.N.Multiply(NetSRP.k)).Subtract(bx.Multiply(NetSRP.k)).Mod(NetSRP.N);
			return btmp.ModPow(x.Multiply(u).Add(a), NetSRP.N).ToByteArrayUnsigned();
		}

		public static NetXtea CreateEncryption(byte[] sessionValue)
		{
			HashAlgorithm sha = NetSRP.GetHashAlgorithm();
			byte[] hash = sha.ComputeHash(sessionValue);
			byte[] key = new byte[16];
			for (int i = 0; i < 16; i++)
			{
				key[i] = hash[i];
				for (int j = 1; j < hash.Length / 16; j++)
				{
					byte[] array = key;
					int num = i;
					array[num] ^= hash[i + j * 16];
				}
			}
			return new NetXtea(key);
		}

		private static readonly NetBigInteger N = new NetBigInteger("0115b8b692e0e045692cf280b436735c77a5a9e8a9e7ed56c965f87db5b2a2ece3", 16);

		private static readonly NetBigInteger g = NetBigInteger.Two;

		private static readonly NetBigInteger k = NetSRP.ComputeMultiplier();
	}
}
