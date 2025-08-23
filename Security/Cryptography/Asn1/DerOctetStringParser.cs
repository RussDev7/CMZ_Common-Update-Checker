using System;
using System.IO;

namespace DNA.Security.Cryptography.Asn1
{
	public class DerOctetStringParser : Asn1OctetStringParser, IAsn1Convertible
	{
		internal DerOctetStringParser(DefiniteLengthInputStream stream)
		{
			this.stream = stream;
		}

		public Stream GetOctetStream()
		{
			return this.stream;
		}

		public Asn1Object ToAsn1Object()
		{
			Asn1Object asn1Object;
			try
			{
				asn1Object = new DerOctetString(this.stream.ToArray());
			}
			catch (IOException e)
			{
				throw new InvalidOperationException("IOException converting stream to byte array: " + e.Message, e);
			}
			return asn1Object;
		}

		private readonly DefiniteLengthInputStream stream;
	}
}
