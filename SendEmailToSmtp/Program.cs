using System;
using System.IO;
using MimeKit;

namespace SendEmailToSmtp
{
	public class Program
	{
		public static void Main()
		{
//			SystemNetMail.SendMailToMailtrap();
//			SystemNetMail.SendMailToRspkMailhogLocalHost();
//			SystemNetMail.SendMailToRspkMailhog();
//			new LimilabsSmtp().RequestDeliveryNotification();

			//Запросить параметры smtp сервера:
//			RunTestMailServer();

//			new DsnSmtpClient().SendMailToMailtrap();
//			new DsnSmtpClient().DownloadMessages();

//			Распарсить ответное письмо:
			/*MimeMessage message;
			using (Stream mailStream = File.OpenRead("C:\\TestMail.eml"))
			{
				message = MimeMessage.Load(mailStream);

				mailStream.Close();
			}
			new DsnSmtpClient().ProcessDeliveryStatusNotification(message);*/

			Console.WriteLine("Sent");
			Console.ReadLine();
		}

		/// <summary>
		/// Узнать возможности почтового сервера.
		/// </summary>
		private static void RunTestMailServer()
		{
			Console.WriteLine("Smtp:");
			AbpMailKitSender.PrintCapabilities();

			Console.WriteLine("Pop3:");
			AbpMailKitSender.PopPrintCapabilities();
		}
	}
}