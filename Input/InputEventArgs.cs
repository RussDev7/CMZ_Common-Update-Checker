using System;
using Microsoft.Xna.Framework;

namespace DNA.Input
{
	public class InputEventArgs : EventArgs
	{
		public InputEventArgs(InputManager inputManger, GameTime time, bool continueProcessing)
		{
			this.InputManager = inputManger;
			this.GameTime = time;
			this.ContiuneProcessing = continueProcessing;
		}

		public InputManager InputManager;

		public GameTime GameTime;

		public bool ContiuneProcessing;
	}
}
