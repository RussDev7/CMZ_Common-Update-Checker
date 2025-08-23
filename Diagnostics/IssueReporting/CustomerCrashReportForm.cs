using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DNA.Diagnostics.IssueReporting
{
	public partial class CustomerCrashReportForm : Form
	{
		public CustomerCrashReportForm(CrashID cid)
		{
			this.InitializeComponent();
			string url = "http:\\\\services.digitaldnagames.com\\Support.aspx";
			if (cid != null)
			{
				url = url + "?CrashID=" + cid.ToString();
			}
			this.webBrowser1.Navigate(url);
		}
	}
}
