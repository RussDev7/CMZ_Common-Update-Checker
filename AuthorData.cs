using System;
using System.IO;
using System.Reflection;
using DNA.Security.Cryptography;

namespace DNA
{
	public class AuthorData
	{
		public int Version
		{
			get
			{
				return this._dataVersion;
			}
		}

		public byte[] Data
		{
			get
			{
				return this._rawData;
			}
		}

		public AuthorData()
		{
			Assembly assm = Assembly.GetExecutingAssembly();
			Stream stream = assm.GetManifestResourceStream("DNA.ADT.key");
			this._key = RSAKey.Read(new BinaryReader(stream));
		}

		public AuthorData(RSAKey publicKey)
		{
			this._key = publicKey;
		}

		public AuthorData(RSAKey privateKey, int dataVersion, byte[] data)
		{
			this._dataVersion = dataVersion;
			this._key = privateKey.PublicKey;
			this._rawData = data;
			MemoryStream stream = new MemoryStream();
			BinaryWriter dwriter = new BinaryWriter(stream);
			dwriter.Write(dataVersion);
			dwriter.Write(data.Length);
			dwriter.Write(data);
			dwriter.Flush();
			byte[] dataBlock = stream.ToArray();
			RSASignatureProvider signer = new RSASignatureProvider(new SHA256HashProvider(), privateKey);
			this._signature = signer.Sign(dataBlock);
		}

		public void Read(BinaryReader reader)
		{
			RSASignatureProvider signer = new RSASignatureProvider(new SHA256HashProvider(), this._key);
			if (reader.ReadInt32() != 1935893365 || reader.ReadInt32() != 1)
			{
				throw new Exception("Bad Data Format");
			}
			int dBlockLen = reader.ReadInt32();
			byte[] dataBlock = reader.ReadBytes(dBlockLen);
			int sigLen = reader.ReadInt32();
			byte[] sigData = reader.ReadBytes(sigLen);
			this._signature = signer.FromByteArray(sigData);
			if (!this._signature.Verify(signer, dataBlock))
			{
				throw new Exception("Data Corrupt");
			}
			MemoryStream stream = new MemoryStream(dataBlock);
			BinaryReader dblockReader = new BinaryReader(stream);
			this._dataVersion = dblockReader.ReadInt32();
			int dlen = dblockReader.ReadInt32();
			this._rawData = dblockReader.ReadBytes(dlen);
		}

		public void Write(BinaryWriter writer)
		{
			MemoryStream stream = new MemoryStream();
			BinaryWriter dwriter = new BinaryWriter(stream);
			dwriter.Write(this._dataVersion);
			dwriter.Write(this._rawData.Length);
			dwriter.Write(this._rawData);
			dwriter.Flush();
			byte[] dataBlock = stream.ToArray();
			writer.Write(1935893365);
			writer.Write(1);
			writer.Write(dataBlock.Length);
			writer.Write(dataBlock);
			writer.Write(this._signature.Data.Length);
			writer.Write(this._signature.Data);
		}

		private const int FileIdent = 1935893365;

		private const int FileVersion = 1;

		private Signature _signature;

		private int _dataVersion;

		private byte[] _rawData;

		private RSAKey _key;
	}
}
