﻿using System.Net;
using System.Net.Mail;
using System.Text;
using SendEmailToSmtp.ClosedInfo;

namespace SendEmailToSmtp
{
	public class SystemNetMail
	{
		public static ILoginInformation LoginInfo { get; set; }

		public SystemNetMail()
		{
			LoginInfo = new LoginInformation.ExampleMailLogin();
		}

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
		/// Отправка на реальный ящик
		/// </summary>
		public static void SendMailToIsapOnline()
		{
			var source = LoginInformation.GetLoginInformation(LoginType.IsapOnlineLogin);
			var client = new SmtpClient(source.Host, source.SmtpPort)
				{
					Credentials = new NetworkCredential(source.UserName, source.Password, source.Domain),
					EnableSsl = true,
					DeliveryMethod = SmtpDeliveryMethod.Network,
					UseDefaultCredentials = false,//если true, то username и passwd можно оставить пустыми
					Timeout = 1500,
				};
			client.Send("from@example.com", "мойпочтовыйадрессобака", "Hello world", "testbody");
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

			var client = new SmtpClient(LoginInfo.Host, LoginInfo.SmtpPort)
			{
				Credentials = new NetworkCredential(LoginInfo.UserName, LoginInfo.Password),
				EnableSsl = true,
			};
			
			client.Send(mailMessage);
		}
	}
}