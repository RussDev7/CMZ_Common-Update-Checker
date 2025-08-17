using System;

namespace DNA.Drawing.Actions
{
	public class WaitForAction : State
	{
		public override bool Complete
		{
			get
			{
				return this._otherAction.Complete;
			}
		}

		public WaitForAction(State otherACtion)
		{
			this._otherAction = otherACtion;
		}

		private State _otherAction;
	}
}
