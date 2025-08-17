using System;

namespace DNA
{
	public static class GameConsole
	{
		public static void SetControl(IConsole control)
		{
			GameConsole._control = control;
		}

		public static void Write(char value)
		{
			if (GameConsole._control != null)
			{
				GameConsole._control.Write(value);
			}
		}

		public static void Write(string value)
		{
			if (GameConsole._control != null)
			{
				GameConsole._control.Write(value);
			}
		}

		public static void WriteLine(string value)
		{
			if (GameConsole._control != null)
			{
				GameConsole._control.WriteLine(value);
			}
		}

		public static void WriteLine()
		{
			GameConsole._control.WriteLine();
		}

		private static IConsole _control;
	}
}
