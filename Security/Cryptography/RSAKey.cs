using System;
using System.Collections.Generic;
using System.IO;
using DNA.IO;
using DNA.Security.Cryptography.Crypto;
using DNA.Security.Cryptography.Crypto.Generators;
using DNA.Security.Cryptography.Crypto.Parameters;
using DNA.Security.Cryptography.Math;
using DNA.Security.Cryptography.Security;

namespace DNA.Security.Cryptography
{
	public class RSAKey
	{
		internal RsaKeyParameters Key
		{
			get
			{
				if (this._privateKeyParams != null)
				{
					return this._privateKeyParams;
				}
				return this._publicKeyParams;
			}
		}

		public bool IsPrivate
		{
			get
			{
				return this._privateKeyParams != null;
			}
		}

		public RSAKey PublicKey
		{
			get
			{
				if (this._privateKeyParams == null)
				{
					return this;
				}
				return new RSAKey(this._publicKeyParams);
			}
		}

		private RSAKey(RsaKeyParameters pubKey)
		{
			this._publicKeyParams = pubKey;
		}

		private RSAKey()
		{
		}

		public static RSAKey Generate(int size)
		{
			RSAKey ret = new RSAKey();
			RsaKeyPairGenerator rsaKeypairGenerator = new RsaKeyPairGenerator();
			rsaKeypairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), size));
			AsymmetricCipherKeyPair keyPair = rsaKeypairGenerator.GenerateKeyPair();
			ret._privateKeyParams = (RsaPrivateCrtKeyParameters)keyPair.Private;
			ret._publicKeyParams = (RsaKeyParameters)keyPair.Public;
			return ret;
		}

		public override string ToString()
		{
			HTFDocument doc = new HTFDocument();
			if (this._privateKeyParams != null)
			{
				doc.Children.Add(new HTFElement("Modulus", this._privateKeyParams.Modulus.ToString(16)));
				doc.Children.Add(new HTFElement("PublicExponent", this._privateKeyParams.PublicExponent.ToString(16)));
				doc.Children.Add(new HTFElement("PrivateExponent", this._privateKeyParams.Exponent.ToString(16)));
				doc.Children.Add(new HTFElement("P", this._privateKeyParams.P.ToString(16)));
				doc.Children.Add(new HTFElement("Q", this._privateKeyParams.Q.ToString(16)));
				doc.Children.Add(new HTFElement("DP", this._privateKeyParams.DP.ToString(16)));
				doc.Children.Add(new HTFElement("dQ", this._privateKeyParams.DQ.ToString(16)));
				doc.Children.Add(new HTFElement("qInv", this._privateKeyParams.QInv.ToString(16)));
			}
			else
			{
				doc.Children.Add(new HTFElement("Modulus", this._publicKeyParams.Modulus.ToString(16)));
				doc.Children.Add(new HTFElement("PublicExponent", this._publicKeyParams.Exponent.ToString(16)));
			}
			return doc.ToString();
		}

		public static RSAKey Parse(string str)
		{
			RSAKey ret = new RSAKey();
			HTFDocument doc = new HTFDocument();
			doc.LoadFromString(str);
			Dictionary<string, BigInteger> table = new Dictionary<string, BigInteger>();
			foreach (HTFElement ele in doc.Children)
			{
				table[ele.ID] = new BigInteger(ele.Value, 16);
			}
			if (table.ContainsKey("PrivateExponent"))
			{
				ret._privateKeyParams = new RsaPrivateCrtKeyParameters(table["Modulus"], table["PublicExponent"], table["PrivateExponent"], table["P"], table["Q"], table["DP"], table["dQ"], table["qInv"]);
				ret._publicKeyParams = new RsaKeyParameters(false, ret._privateKeyParams.Modulus, ret._privateKeyParams.PublicExponent);
			}
			else
			{
				ret._publicKeyParams = new RsaKeyParameters(false, table["Modulus"], table["PublicExponent"]);
			}
			return ret;
		}

		private static void WriteBigInt(BinaryWriter writer, BigInteger bigInt)
		{
			byte[] data = bigInt.ToByteArray();
			writer.Write(data.Length);
			writer.Write(data);
		}

		private static BigInteger ReadBigInt(BinaryReader reader)
		{
			int size = reader.ReadInt32();
			return new BigInteger(reader.ReadBytes(size));
		}

		public void Write(BinaryWriter writer)
		{
			if (this._privateKeyParams != null)
			{
				writer.Write(true);
				RSAKey.WriteBigInt(writer, this._privateKeyParams.Modulus);
				RSAKey.WriteBigInt(writer, this._privateKeyParams.PublicExponent);
				RSAKey.WriteBigInt(writer, this._privateKeyParams.Exponent);
				RSAKey.WriteBigInt(writer, this._privateKeyParams.P);
				RSAKey.WriteBigInt(writer, this._privateKeyParams.Q);
				RSAKey.WriteBigInt(writer, this._privateKeyParams.DP);
				RSAKey.WriteBigInt(writer, this._privateKeyParams.DQ);
				RSAKey.WriteBigInt(writer, this._privateKeyParams.QInv);
				return;
			}
			writer.Write(false);
			RSAKey.WriteBigInt(writer, this._publicKeyParams.Modulus);
			RSAKey.WriteBigInt(writer, this._publicKeyParams.Exponent);
		}

		public static RSAKey Read(BinaryReader reader)
		{
			RSAKey ret = new RSAKey();
			if (reader.ReadBoolean())
			{
				ret._privateKeyParams = new RsaPrivateCrtKeyParameters(RSAKey.ReadBigInt(reader), RSAKey.ReadBigInt(reader), RSAKey.ReadBigInt(reader), RSAKey.ReadBigInt(reader), RSAKey.ReadBigInt(reader), RSAKey.ReadBigInt(reader), RSAKey.ReadBigInt(reader), RSAKey.ReadBigInt(reader));
				ret._publicKeyParams = new RsaKeyParameters(false, ret._privateKeyParams.Modulus, ret._privateKeyParams.PublicExponent);
			}
			else
			{
				ret._publicKeyParams = new RsaKeyParameters(false, RSAKey.ReadBigInt(reader), RSAKey.ReadBigInt(reader));
			}
			return ret;
		}

		public byte[] ToByteArray()
		{
			MemoryStream stream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(stream);
			this.Write(writer);
			writer.Flush();
			return stream.ToArray();
		}

		public static RSAKey FromByteArray(byte[] data)
		{
			MemoryStream stream = new MemoryStream(data);
			BinaryReader reader = new BinaryReader(stream);
			return RSAKey.Read(reader);
		}

		private RsaPrivateCrtKeyParameters _privateKeyParams;

		private RsaKeyParameters _publicKeyParams;
	}
}
