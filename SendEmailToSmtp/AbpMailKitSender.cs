using System;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using SendEmailToSmtp.ClosedInfo;

namespace SendEmailToSmtp
{
	public class AbpMailKitSender
	{
		#region Capabilities POP

		public static void PopPrintCapabilities()
		{
			var loginInfo = new LoginInformation().GetLoginInformation(SiteLoginInfo.Mailtrap);

			using (var client = new Pop3Client ()) {
				client.Connect("pop3.mailtrap.io", loginInfo.Pop3Port, loginInfo.SecureSocketOptions);

				if (client.Capabilities.HasFlag (Pop3Capabilities.Sasl)) {
					var mechanisms = string.Join (", ", client.AuthenticationMechanisms);
					Console.WriteLine ("The POP3 server supports the following SASL mechanisms: {0}", mechanisms);
				}

				client.Authenticate(loginInfo.UserName, loginInfo.Password);

				if (client.Capabilities.HasFlag (Pop3Capabilities.Apop))
					Console.WriteLine ("The server supports APOP authentication.");

				if (client.Capabilities.HasFlag (Pop3Capabilities.Expire)) {
					if (client.ExpirePolicy > 0)
						Console.WriteLine ("The POP3 server automatically expires messages after {0} days", client.ExpirePolicy);
					else
						Console.WriteLine ("The POP3 server will never expire messages.");
				}

				if (client.Capabilities.HasFlag (Pop3Capabilities.LoginDelay))
					Console.WriteLine ("The minimum number of seconds between login attempts is {0}.", client.LoginDelay);

				if (client.Capabilities.HasFlag (Pop3Capabilities.Pipelining))
					Console.WriteLine ("The POP3 server can pipeline commands, so using client.GetMessages() will be faster.");

				if (client.Capabilities.HasFlag (Pop3Capabilities.Top))
					Console.WriteLine ("The POP3 server supports the TOP command, so it's possible to download message headers.");

				if (client.Capabilities.HasFlag (Pop3Capabilities.UIDL))
					Console.WriteLine ("The POP3 server supports the UIDL command which means we can track messages by UID.");

				client.Disconnect (true);
			}
		}

		#endregion

		/// <summary>
		///     Узнать возможности почтового сервера.
		/// </summary>
		public static void PrintCapabilities()
		{
			var loginInfo = new LoginInformation().GetLoginInformation(SiteLoginInfo.Mailtrap);

			using (var client = new SmtpClient())
			{
				client.Connect(loginInfo.Host, loginInfo.SmtpPort, loginInfo.SecureSocketOptions);

				if (client.Capabilities.HasFlag(SmtpCapabilities.Authentication))
				{
					var mechanisms = string.Join(", ", client.AuthenticationMechanisms);
					Console.WriteLine("The SMTP server supports the following SASL mechanisms: {0}", mechanisms);
					client.Authenticate(loginInfo.UserName, loginInfo.Password);
				}

				if (client.Capabilities.HasFlag(SmtpCapabilities.Size))
					Console.WriteLine("The SMTP server has a size restriction on messages: {0}.", client.MaxSize);

				if (client.Capabilities.HasFlag(SmtpCapabilities.Dsn))
					Console.WriteLine("The SMTP server supports delivery-status notifications.");

				if (client.Capabilities.HasFlag(SmtpCapabilities.EightBitMime))
					Console.WriteLine("The SMTP server supports Content-Transfer-Encoding: 8bit");

				if (client.Capabilities.HasFlag(SmtpCapabilities.BinaryMime))
					Console.WriteLine("The SMTP server supports Content-Transfer-Encoding: binary");

				if (client.Capabilities.HasFlag(SmtpCapabilities.UTF8))
					Console.WriteLine("The SMTP server supports UTF-8 in message headers.");

				client.Disconnect(true);
			}
		}
	}
}