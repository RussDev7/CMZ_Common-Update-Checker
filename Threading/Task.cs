using System;

namespace DNA.Threading
{
	public abstract class Task
	{
		public TaskStatus Status
		{
			get
			{
				return this._status;
			}
			protected set
			{
				this._status = value;
			}
		}

		public Exception Exception
		{
			get
			{
				return this._exception;
			}
			protected set
			{
				this._exception = value;
			}
		}

		private bool Failed
		{
			get
			{
				return this.Status == TaskStatus.Failed;
			}
		}

		private bool Completed
		{
			get
			{
				return this.Status == TaskStatus.Failed || this.Status == TaskStatus.Compelete;
			}
		}

		private TaskStatus _status;

		private Exception _exception;
	}
}
