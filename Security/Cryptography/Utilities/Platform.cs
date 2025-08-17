using System;
using System.Security;

namespace DNA.Security.Cryptography.Utilities
{
	internal sealed class Platform
	{
		private Platform()
		{
		}

		internal static Exception CreateNotImplementedException(string message)
		{
			return new NotImplementedException(message);
		}

		internal static string GetEnvironmentVariable(string variable)
		{
			string text;
			try
			{
				text = Environment.GetEnvironmentVariable(variable);
			}
			catch (SecurityException)
			{
				text = null;
			}
			return text;
		}

		private static string GetNewLine()
		{
			return Environment.NewLine;
		}

		internal static readonly string NewLine = Platform.GetNewLine();
	}
}
