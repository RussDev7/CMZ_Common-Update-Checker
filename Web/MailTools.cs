using System;
using System.Diagnostics;
using System.Net.Mail;

namespace DNA.Web
{
	public static class MailTools
	{
		public static void SendDefaultMailClientEmail(MailMessage message)
		{
			string smessage = string.Format("mailto:{0}?subject={1}&body={2}", message.To, message.Subject, message.Body);
			if (message.CC.Count > 0)
			{
				smessage += "&CC=";
			}
			for (int i = 0; i < message.CC.Count; i++)
			{
				MailAddress address = message.CC[i];
				smessage += address.Address;
				if (i < message.CC.Count - 1)
				{
					smessage += ";";
				}
			}
			if (message.Bcc.Count > 0)
			{
				smessage += "&BCC=";
			}
			for (int j = 0; j < message.Bcc.Count; j++)
			{
				MailAddress address2 = message.Bcc[j];
				smessage += address2.Address;
				if (j < message.Bcc.Count - 1)
				{
					smessage += ";";
				}
			}
			Process.Start(smessage);
		}
	}
}
