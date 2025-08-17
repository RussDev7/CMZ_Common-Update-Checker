using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class DrawEventArgs : EventArgs
	{
		public DrawEventArgs()
		{
		}

		public DrawEventArgs(GraphicsDevice device, GameTime gameTime)
		{
			this.Device = device;
			this.GameTime = gameTime;
		}

		public GraphicsDevice Device;

		public GameTime GameTime;
	}
}
