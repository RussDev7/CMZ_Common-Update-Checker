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
			MD5HashProvider hasher = new MD5HashProvider();
			return new CrashID(hasher.Parse(idText));
		}

		public static CrashID FromInfo(string type, string message, string stackTrace)
		{
			MD5HashProvider hasher = new MD5HashProvider();
			UTF8Encoding encoding = new UTF8Encoding();
			string errorString = type + message + stackTrace;
			byte[] data = encoding.GetBytes(errorString);
			return new CrashID(hasher.CreateHash(data));
		}

		public override string ToString()
		{
			return this._hash.ToString();
		}

		private Hash _hash;
	}
}
