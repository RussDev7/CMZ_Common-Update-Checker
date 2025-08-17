using System;
using System.Text;
using DNA.Security.Cryptography;

namespace DNA.Diagnostics.IssueReporting
{
	public class CrashID
	{
		private CrashID(Hash hash)
		{
			this._hash = hash;
		}

		public static CrashID Parse(string idText)
		{
			MD5HashProvider md5HashProvider = new MD5HashProvider();
			return new CrashID(md5HashProvider.Parse(idText));
		}

		public static CrashID FromInfo(string type, string message, string stackTrace)
		{
			MD5HashProvider md5HashProvider = new MD5HashProvider();
			UTF8Encoding utf8Encoding = new UTF8Encoding();
			string text = type + message + stackTrace;
			byte[] bytes = utf8Encoding.GetBytes(text);
			return new CrashID(md5HashProvider.CreateHash(bytes));
		}

		public override string ToString()
		{
			return this._hash.ToString();
		}

		private Hash _hash;
	}
}
