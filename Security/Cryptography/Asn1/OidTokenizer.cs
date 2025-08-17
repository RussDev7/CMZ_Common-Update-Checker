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
			int num = this.oid.IndexOf('.', this.index);
			if (num == -1)
			{
				string text = this.oid.Substring(this.index);
				this.index = -1;
				return text;
			}
			string text2 = this.oid.Substring(this.index, num - this.index);
			this.index = num + 1;
			return text2;
		}

		private string oid;

		private int index;
	}
}
