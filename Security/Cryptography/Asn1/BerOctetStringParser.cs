using System;
using System.IO;
using DNA.Security.Cryptography.Utilities.IO;

namespace DNA.Security.Cryptography.Asn1
{
	public class BerOctetStringParser : Asn1OctetStringParser, IAsn1Convertible
	{
		internal BerOctetStringParser(Asn1StreamParser parser)
		{
			this._parser = parser;
		}

		public Stream GetOctetStream()
		{
			return new ConstructedOctetStream(this._parser);
		}

		public Asn1Object ToAsn1Object()
		{
			Asn1Object asn1Object;
			try
			{
				asn1Object = new BerOctetString(Streams.ReadAll(this.GetOctetStream()));
			}
			catch (IOException e)
			{
				throw new InvalidOperationException("IOException converting stream to byte array: " + e.Message, e);
			}
			return asn1Object;
		}

		private readonly Asn1StreamParser _parser;
	}
}
