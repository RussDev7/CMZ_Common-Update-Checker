using System;

namespace DNA.Drawing.UI
{
	public class SelectedMenuItemArgs : EventArgs
	{
		public SelectedMenuItemArgs(MenuItemElement control)
		{
			this.MenuItem = control;
		}

		public MenuItemElement MenuItem;
	}
}
