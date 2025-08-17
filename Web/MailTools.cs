using System;
using System.Diagnostics;
using System.Net.Mail;

namespace DNA.Web
{
	public static class MailTools
	{
		public static void SendDefaultMailClientEmail(MailMessage message)
		{
			string text = string.Format("mailto:{0}?subject={1}&body={2}", message.To, message.Subject, message.Body);
			if (message.CC.Count > 0)
			{
				text += "&CC=";
			}
			for (int i = 0; i < message.CC.Count; i++)
			{
				MailAddress mailAddress = message.CC[i];
				text += mailAddress.Address;
				if (i < message.CC.Count - 1)
				{
					text += ";";
				}
			}
			if (message.Bcc.Count > 0)
			{
				text += "&BCC=";
			}
			for (int j = 0; j < message.Bcc.Count; j++)
			{
				MailAddress mailAddress2 = message.Bcc[j];
				text += mailAddress2.Address;
				if (j < message.Bcc.Count - 1)
				{
					text += ";";
				}
			}
			Process.Start(text);
		}
	}
}
