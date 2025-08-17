using System;

namespace DNA.Drawing.UI
{
	public class SelectedEventArgs : EventArgs
	{
		public SelectedEventArgs()
		{
		}

		public SelectedEventArgs(object tag)
		{
			this.Tag = tag;
		}

		public object Tag;
	}
}
