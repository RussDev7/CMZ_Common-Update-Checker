using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Actions
{
	public class CallbackState : State
	{
		public event CallbackState.StateCallBack Callback;

		public CallbackState(CallbackState.StateCallBack callback, object data)
		{
			this.Callback += callback;
			this._data = data;
		}

		public override bool Complete
		{
			get
			{
				return this._finished;
			}
		}

		protected override void OnTick(DNAGame game, Entity entity, GameTime deltaT)
		{
			this._finished = true;
			if (this.Callback != null)
			{
				this._finished = this.Callback(entity, this, this._data, deltaT);
			}
			base.OnTick(game, entity, deltaT);
		}

		private bool _finished;

		private object _data;

		public delegate bool StateCallBack(Entity e, CallbackState state, object data, GameTime time);
	}
}
