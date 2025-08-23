using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DNA.Security.Cryptography.Math;

namespace DNA.Security.Cryptography.Asn1
{
	public class DerObjectIdentifier : Asn1Object
	{
		public static DerObjectIdentifier GetInstance(object obj)
		{
			if (obj == null || obj is DerObjectIdentifier)
			{
				return (DerObjectIdentifier)obj;
			}
			if (obj is Asn1OctetString)
			{
				return new DerObjectIdentifier(((Asn1OctetString)obj).GetOctets());
			}
			if (obj is Asn1TaggedObject)
			{
				return DerObjectIdentifier.GetInstance(((Asn1TaggedObject)obj).GetObject());
			}
			throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name, "obj");
		}

		public static DerObjectIdentifier GetInstance(Asn1TaggedObject obj, bool explicitly)
		{
			return DerObjectIdentifier.GetInstance(obj.GetObject());
		}

		public DerObjectIdentifier(string identifier)
		{
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (!DerObjectIdentifier.OidRegex.IsMatch(identifier))
			{
				throw new FormatException("string " + identifier + " not an OID");
			}
			this.identifier = identifier;
		}

		public string Id
		{
			get
			{
				return this.identifier;
			}
		}

		internal DerObjectIdentifier(byte[] bytes)
			: this(DerObjectIdentifier.MakeOidStringFromBytes(bytes))
		{
		}

		private void WriteField(Stream outputStream, long fieldValue)
		{
			if (fieldValue >= 128L)
			{
				if (fieldValue >= 16384L)
				{
					if (fieldValue >= 2097152L)
					{
						if (fieldValue >= 268435456L)
						{
							if (fieldValue >= 34359738368L)
							{
								if (fieldValue >= 4398046511104L)
								{
									if (fieldValue >= 562949953421312L)
									{
										if (fieldValue >= 72057594037927936L)
										{
											outputStream.WriteByte((byte)((fieldValue >> 56) | 128L));
										}
										outputStream.WriteByte((byte)((fieldValue >> 49) | 128L));
									}
									outputStream.WriteByte((byte)((fieldValue >> 42) | 128L));
								}
								outputStream.WriteByte((byte)((fieldValue >> 35) | 128L));
							}
							outputStream.WriteByte((byte)((fieldValue >> 28) | 128L));
						}
						outputStream.WriteByte((byte)((fieldValue >> 21) | 128L));
					}
					outputStream.WriteByte((byte)((fieldValue >> 14) | 128L));
				}
				outputStream.WriteByte((byte)((fieldValue >> 7) | 128L));
			}
			outputStream.WriteByte((byte)(fieldValue & 127L));
		}

		private void WriteField(Stream outputStream, BigInteger fieldValue)
		{
			int byteCount = (fieldValue.BitLength + 6) / 7;
			if (byteCount == 0)
			{
				outputStream.WriteByte(0);
				return;
			}
			BigInteger tmpValue = fieldValue;
			byte[] tmp = new byte[byteCount];
			for (int i = byteCount - 1; i >= 0; i--)
			{
				tmp[i] = (byte)((tmpValue.IntValue & 127) | 128);
				tmpValue = tmpValue.ShiftRight(7);
			}
			byte[] array = tmp;
			int num = byteCount - 1;
			array[num] &= 127;
			outputStream.Write(tmp, 0, tmp.Length);
		}

		internal override void Encode(DerOutputStream derOut)
		{
			OidTokenizer tok = new OidTokenizer(this.identifier);
			MemoryStream bOut = new MemoryStream();
			DerOutputStream dOut = new DerOutputStream(bOut);
			string token = tok.NextToken();
			int first = int.Parse(token);
			token = tok.NextToken();
			int second = int.Parse(token);
			this.WriteField(bOut, (long)(first * 40 + second));
			while (tok.HasMoreTokens)
			{
				token = tok.NextToken();
				if (token.Length < 18)
				{
					this.WriteField(bOut, long.Parse(token));
				}
				else
				{
					this.WriteField(bOut, new BigInteger(token));
				}
			}
			dOut.Close();
			derOut.WriteEncoded(6, bOut.ToArray());
		}

		protected override int Asn1GetHashCode()
		{
			return this.identifier.GetHashCode();
		}

		protected override bool Asn1Equals(Asn1Object asn1Object)
		{
			DerObjectIdentifier other = asn1Object as DerObjectIdentifier;
			return other != null && this.identifier.Equals(other.identifier);
		}

		public override string ToString()
		{
			return this.identifier;
		}

		private static string MakeOidStringFromBytes(byte[] bytes)
		{
			StringBuilder objId = new StringBuilder();
			long value = 0L;
			BigInteger bigValue = null;
			bool first = true;
			for (int i = 0; i != bytes.Length; i++)
			{
				int b = (int)bytes[i];
				if (value < 36028797018963968L)
				{
					value = value * 128L + (long)(b & 127);
					if ((b & 128) == 0)
					{
						if (first)
						{
							switch ((int)value / 40)
							{
							case 0:
								objId.Append('0');
								break;
							case 1:
								objId.Append('1');
								value -= 40L;
								break;
							default:
								objId.Append('2');
								value -= 80L;
								break;
							}
							first = false;
						}
						objId.Append('.');
						objId.Append(value);
						value = 0L;
					}
				}
				else
				{
					if (bigValue == null)
					{
						bigValue = BigInteger.ValueOf(value);
					}
					bigValue = bigValue.ShiftLeft(7);
					bigValue = bigValue.Or(BigInteger.ValueOf((long)(b & 127)));
					if ((b & 128) == 0)
					{
						objId.Append('.');
						objId.Append(bigValue);
						bigValue = null;
						value = 0L;
					}
				}
			}
			return objId.ToString();
		}

		private static readonly Regex OidRegex = new Regex("\\A[0-2](\\.[0-9]+)+\\z");

		private readonly string identifier;
	}
}
