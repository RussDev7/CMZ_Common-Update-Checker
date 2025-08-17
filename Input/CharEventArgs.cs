using System;
using Microsoft.Xna.Framework;

namespace DNA.Input
{
	public class CharEventArgs : EventArgs
	{
		public CharEventArgs(char c, GameTime time, bool continueProcessing)
		{
			this.PressedChar = c;
			this.GameTime = time;
			this.ContiuneProcessing = continueProcessing;
		}

		public char PressedChar;

		public GameTime GameTime;

		public bool ContiuneProcessing;
	}
}
