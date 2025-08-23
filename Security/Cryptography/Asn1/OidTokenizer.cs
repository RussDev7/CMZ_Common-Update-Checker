using System;

namespace DNA.Security.Cryptography.Asn1
{
	public class OidTokenizer
	{
		public OidTokenizer(string oid)
		{
			this.oid = oid;
		}

		public bool HasMoreTokens
		{
			get
			{
				return this.index != -1;
			}
		}

		public string NextToken()
		{
			if (this.index == -1)
			{
				return null;
			}
			int end = this.oid.IndexOf('.', this.index);
			if (end == -1)
			{
				string lastToken = this.oid.Substring(this.index);
				this.index = -1;
				return lastToken;
			}
			string nextToken = this.oid.Substring(this.index, end - this.index);
			this.index = end + 1;
			return nextToken;
		}

		private string oid;

		private int index;
	}
}
