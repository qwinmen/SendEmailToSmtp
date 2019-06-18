using System.Net;
using System.Net.Mail;
using System.Text;
using SendEmailToSmtp.ClosedInfo;

namespace SendEmailToSmtp
{
	public class SystemNetMail
	{
		/// <summary>
		/// Смотри тут https://mailhog.isap.team/
		/// </summary>
		public static void SendMailToRspkMailhog()
		{
			var client = new SmtpClient("192.168.161.117", 1025)
			{
				Credentials = new NetworkCredential("", ""),
				EnableSsl = false,
				DeliveryMethod = SmtpDeliveryMethod.Network,
				UseDefaultCredentials = true,//если true, то username и passwd можно оставить пустыми
				Timeout = 1500,
			};
			client.Send("from@example.com", "to@example.com", "Hello world", "testbody");
		}

		/// <summary>
		/// Смотри тут http://localhost:8025/#
		/// </summary>
		public static void SendMailToRspkMailhogLocalHost()
		{
			var client = new SmtpClient("localhost", 1025)
			{
				Credentials = new NetworkCredential("", ""),
				EnableSsl = false,
				DeliveryMethod = SmtpDeliveryMethod.Network,
				UseDefaultCredentials = true,//если true, то username и passwd можно оставить пустыми
				Timeout = 500,
			};
			client.Send("from@example.com", "to@example.com", "Hello world", "testbody");
		}

		/// <summary>
		/// Смотри тут https://mailtrap.io/
		/// </summary>
		public static void SendMailToMailtrap()
		{
			MailMessage mailMessage = new MailMessage
			{
				From = new MailAddress("no-reply@moscow.ru"),
				To = {new MailAddress("test@yandex.ru")},
				Subject = "Hello wold",
				Body = "Проверка dsn",
				DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess,
				BodyEncoding = Encoding.UTF8,
				SubjectEncoding = Encoding.UTF8,
				Headers =
				{
					{"Return-Receipt-To", "test@yandex.ru"},
					{"Disposition-Notification-To", "test@yandex.ru"},
				},
			};

			var client = new SmtpClient(LoginInformation.MailtrapLogin.Host, LoginInformation.MailtrapLogin.Port)
			{
				Credentials = new NetworkCredential(LoginInformation.MailtrapLogin.UserName, LoginInformation.MailtrapLogin.Password),
				EnableSsl = true,
			};
			
			client.Send(mailMessage);
		}
	}
}