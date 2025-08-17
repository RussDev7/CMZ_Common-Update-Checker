using System;

namespace DNA.Timers
{
	public class OneShotTimer : Timer
	{
		public float PercentComplete
		{
			get
			{
				return Math.Min(1f, (float)(base.ElaspedTime.TotalSeconds / this.MaxTime.TotalSeconds));
			}
		}

		public bool Expired
		{
			get
			{
				return base.ElaspedTime >= this.MaxTime;
			}
		}

		public OneShotTimer(TimeSpan time)
		{
			this.MaxTime = time;
		}

		protected override void OnUpdate(TimeSpan time)
		{
			base.OnUpdate(time);
			if (this.AutoReset && this.Expired)
			{
				base.Reset();
			}
		}

		public OneShotTimer(TimeSpan time, bool autoReset)
		{
			this.AutoReset = autoReset;
			this.MaxTime = time;
		}

		public TimeSpan MaxTime;

		public bool AutoReset;
	}
}
