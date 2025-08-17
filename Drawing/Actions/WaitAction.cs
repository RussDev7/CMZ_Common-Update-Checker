using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Actions
{
	public class WaitAction : State
	{
		public WaitAction(TimeSpan time)
		{
			this._endTime = time;
		}

		public override bool Complete
		{
			get
			{
				return this._elapsedTime > this._endTime;
			}
		}

		protected override void OnTick(DNAGame game, Entity actor, GameTime deltaT)
		{
			this._elapsedTime += deltaT.ElapsedGameTime;
			base.OnTick(game, actor, deltaT);
		}

		protected override void OnStart(Entity actor)
		{
			this._elapsedTime = TimeSpan.Zero;
		}

		private TimeSpan _endTime;

		private TimeSpan _elapsedTime;
	}
}
