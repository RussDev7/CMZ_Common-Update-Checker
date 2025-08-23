using System;
using DNA.Security;

namespace DNA
{
	public class GVerifier
	{
		public bool Verify(string data)
		{
			for (int i = 0; i < this.checkStr.Length; i++)
			{
				string name = SecurityTools.DecryptString(GVerifier.Key, this.checkStr[i]);
				if (data == name)
				{
					return false;
				}
			}
			return true;
		}

		private static byte[] Key = new byte[]
		{
			223, 11, 100, 213, 74, 199, 64, 73, 109, 173,
			136, 21, 16, 234, 243, 33, 234, 239, 126, 140,
			232, 186, 72, 153, 134, 6, 91, 196, 117, 38,
			142, 13
		};

		private byte[][] checkStr = new byte[][]
		{
			new byte[]
			{
				80, 48, 100, 35, 217, 166, 5, byte.MaxValue, 156, 43,
				183, 49, 83, 201, 226, 121
			},
			new byte[]
			{
				190, 20, 122, 60, 196, 121, 161, 167, 21, 243,
				138, 80, 240, 76, 169, 142
			}
		};
	}
}
