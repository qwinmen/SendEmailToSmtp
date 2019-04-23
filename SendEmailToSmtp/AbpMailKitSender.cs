using System;
using MailKit.Net.Smtp;
using MailKit.Security;
using SendEmailToSmtp.ClosedInfo;

namespace SendEmailToSmtp
{
	public class AbpMailKitSender
	{
		/// <summary>
		///     Узнать возможности почтового сервера.
		/// </summary>
		public static void PrintCapabilities()
		{
			using (var client = new SmtpClient())
			{
				client.Connect("smtp.mailtrap.io", 2525, SecureSocketOptions.StartTls);

				if (client.Capabilities.HasFlag(SmtpCapabilities.Authentication))
				{
					var mechanisms = string.Join(", ", client.AuthenticationMechanisms);
					Console.WriteLine("The SMTP server supports the following SASL mechanisms: {0}", mechanisms);
					client.Authenticate(LoginInformation.MailtrapLogin.UserName,
						LoginInformation.MailtrapLogin.Password);
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