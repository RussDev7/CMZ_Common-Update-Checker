using System;

namespace DNA.Triggers
{
	public abstract class Trigger
	{
		public Trigger(bool oneShot)
		{
			this._oneShot = oneShot;
		}

		public bool Triggered
		{
			get
			{
				return this._triggered;
			}
		}

		protected abstract bool IsSastisfied();

		public virtual void OnTriggered()
		{
		}

		public virtual void Reset()
		{
			this._triggered = false;
		}

		public void Update()
		{
			if (this._oneShot && this._triggered)
			{
				return;
			}
			this.OnUpdate();
			bool sastisfied = this.IsSastisfied();
			if (!this._lastState && sastisfied)
			{
				this._triggered = true;
				this.OnTriggered();
			}
			this._lastState = sastisfied;
		}

		protected virtual void OnUpdate()
		{
		}

		private bool _oneShot;

		private bool _triggered;

		private bool _lastState;
	}
}
