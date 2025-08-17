using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	public class State
	{
		public bool Started
		{
			get
			{
				return this._started;
			}
		}

		public virtual bool Complete
		{
			get
			{
				return this._started;
			}
		}

		public void Start(Entity entity)
		{
			this._started = true;
			this.OnStart(entity);
		}

		public void End(Entity entity)
		{
			this.OnEnd(entity);
		}

		public void Tick(DNAGame game, Entity entity, GameTime time)
		{
			this.OnTick(game, entity, time);
		}

		protected virtual void OnStart(Entity entity)
		{
		}

		protected virtual void OnEnd(Entity entity)
		{
		}

		protected virtual void OnTick(DNAGame game, Entity entity, GameTime deltaT)
		{
		}

		private bool _started;
	}
}
