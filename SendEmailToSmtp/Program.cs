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
			{
//				AbpMailKitSender.PrintCapabilities();
//				AbpMailKitSender.PopPrintCapabilities();
			}

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
	}
}