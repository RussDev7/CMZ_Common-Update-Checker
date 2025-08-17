using System;

namespace DNA.Timers
{
	public class Timer
	{
		public TimeSpan ElaspedTime
		{
			get
			{
				return this._elapsedTime;
			}
		}

		protected virtual void OnUpdate(TimeSpan time)
		{
		}

		public void Update(TimeSpan time)
		{
			this._elapsedTime += time;
			this.OnUpdate(time);
		}

		public void Reset()
		{
			this._elapsedTime = TimeSpan.Zero;
		}

		private TimeSpan _elapsedTime;
	}
}
