using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Actions
{
	public class MoveToState : State
	{
		public MoveToState(Vector3 finalLocaiton, TimeSpan time)
		{
			this._totalTime = time;
			this._endPosition = finalLocaiton;
		}

		public override bool Complete
		{
			get
			{
				return this._currentTime >= this._totalTime;
			}
		}

		protected override void OnStart(Entity entity)
		{
			this._currentTime = TimeSpan.Zero;
			this._startPosition = entity.LocalPosition;
			base.OnStart(entity);
		}

		protected override void OnTick(DNAGame game, Entity entity, GameTime deltaT)
		{
			this._currentTime += deltaT.ElapsedGameTime;
			if (this._currentTime > this._totalTime)
			{
				this._currentTime = this._totalTime;
			}
			float num = (float)((this._totalTime.TotalSeconds - this._currentTime.TotalSeconds) / this._totalTime.TotalSeconds);
			entity.LocalPosition = Vector3.Lerp(this._endPosition, this._startPosition, num);
			base.OnTick(game, entity, deltaT);
		}

		private Vector3 _startPosition;

		private Vector3 _endPosition;

		private TimeSpan _totalTime;

		private TimeSpan _currentTime;
	}
}
