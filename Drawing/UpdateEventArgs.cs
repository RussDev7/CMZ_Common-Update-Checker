using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	public class UpdateEventArgs : EventArgs
	{
		public UpdateEventArgs()
		{
		}

		public UpdateEventArgs(GameTime gameTime)
		{
			this.GameTime = gameTime;
		}

		public GameTime GameTime;
	}
}
