﻿using System;

namespace DNA.Security.Cryptography.Asn1
{
	public class BerApplicationSpecificParser : IAsn1ApplicationSpecificParser, IAsn1Convertible
	{
		internal BerApplicationSpecificParser(int tag, Asn1StreamParser parser)
		{
			this.tag = tag;
			this.parser = parser;
		}

		public IAsn1Convertible ReadObject()
		{
			return this.parser.ReadObject();
		}

		public Asn1Object ToAsn1Object()
		{
			return new BerApplicationSpecific(this.tag, this.parser.ReadVector());
		}

		private readonly int tag;

		private readonly Asn1StreamParser parser;
	}
}
