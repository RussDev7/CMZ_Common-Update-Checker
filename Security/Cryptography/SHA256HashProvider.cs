﻿using System;
using System.Globalization;
using DNA.Security.Cryptography.Crypto;
using DNA.Security.Cryptography.Crypto.Digests;

namespace DNA.Security.Cryptography
{
	public class SHA256HashProvider : GenericHashProvider
	{
		public override HashProcess BeginHash()
		{
			return new SHA256HashProvider.SHA256HashProcess();
		}

		public override Hash Parse(string s)
		{
			byte[] data = new byte[s.Length / 2];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = byte.Parse(s.Substring(i * 2, 2), NumberStyles.HexNumber);
			}
			return new SHA256HashProvider.SHA256Hash(data);
		}

		internal override IDigest GetHashAlgorythim()
		{
			return new Sha256Digest();
		}

		public override Hash CreateHash(byte[] data)
		{
			return new SHA256HashProvider.SHA256Hash(data);
		}

		public override Hash GetFileHash(string path)
		{
			throw new NotImplementedException();
		}

		private class SHA256HashProcess : HashProcess
		{
			public SHA256HashProcess()
				: base(new Sha256Digest())
			{
			}

			protected override Hash CreateHash(byte[] data)
			{
				return new SHA256HashProvider.SHA256Hash(data);
			}
		}

		private class SHA256Hash : Hash
		{
			public SHA256Hash(byte[] data)
				: base(data)
			{
			}
		}
	}
}
