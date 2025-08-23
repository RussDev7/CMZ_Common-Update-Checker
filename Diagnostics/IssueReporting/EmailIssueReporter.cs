using System;
using System.Net.Mail;
using System.Windows.Forms;
using DNA.Web;

namespace DNA.Diagnostics.IssueReporting
{
	public class EmailIssueReporter : IssueReporter
	{
		public EmailIssueReporter(string name, Version version, DateTime gameStartTime)
		{
			this._gameName = name;
			this._version = version;
			this._startTime = gameStartTime;
		}

		public override void ReportCrash(Exception e)
		{
			DateTime.UtcNow - this._startTime;
			Cursor.Show();
			EmailBugForm form = new EmailBugForm();
			form.CrashInfo = this.GetErrorString(e);
			if (form.ShowDialog() == DialogResult.OK)
			{
				MailTools.SendDefaultMailClientEmail(new MailMessage
				{
					To = { "bugs@digitaldnagames.com" },
					Subject = this._gameName + " Crash",
					Body = form.CrashInfo.Replace("\n", Environment.NewLine),
					IsBodyHtml = false
				});
			}
			base.ReportCrash(e);
		}

		private string GetErrorString(Exception e)
		{
			TimeSpan _runTime = DateTime.UtcNow - this._startTime;
			string result = "Crash Info:";
			string name = e.GetType().Name;
			object obj = result;
			result = string.Concat(new object[] { obj, this._gameName, ", Version ", this._version, "\n" });
			string text = result;
			result = string.Concat(new string[]
			{
				text,
				this._startTime.ToString("MM/dd/yy HH:mm"),
				" ",
				_runTime.ToString(),
				"\n\n"
			});
			result = result + e.Message + "\n\n";
			result = result + e.GetType().Name + "\n\n";
			string[] lines = e.StackTrace.Split(new char[] { '\n' });
			foreach (string line in lines)
			{
				string tline = line.Trim();
				result = result + tline + "\n";
			}
			return result;
		}

		private string _gameName;

		private Version _version;

		private DateTime _startTime;
	}
}
