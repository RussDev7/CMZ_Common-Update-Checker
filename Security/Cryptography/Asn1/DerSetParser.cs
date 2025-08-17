using System;

namespace DNA.Security.Cryptography.Asn1
{
	public class DerSetParser : Asn1SetParser, IAsn1Convertible
	{
		internal DerSetParser(Asn1StreamParser parser)
		{
			this._parser = parser;
		}

		public IAsn1Convertible ReadObject()
		{
			return this._parser.ReadObject();
		}

		public Asn1Object ToAsn1Object()
		{
			return new DerSet(this._parser.ReadVector(), false);
		}

		private readonly Asn1StreamParser _parser;
	}
}
