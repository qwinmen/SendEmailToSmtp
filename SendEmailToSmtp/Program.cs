using System;
using System.IO;
using System.Threading.Tasks;
using MimeKit;
using SendEmailToSmtp.Helpers;

namespace SendEmailToSmtp
{
	public class Program
	{
		public static void Main()
		{
//			AbpMailKitSender.ImapDeleteMailFromStageMoscow();
//			AbpMailKitSender.SendMailToStageMoscow();
//			AbpMailKitSender.ImapGetMailFromStageMoscow();
//			AbpMailKitSender.Pop3GetMailFromStageMoscow();

//			SystemNetMail.SendMailToMailtrap();
//			SystemNetMail.SendMailToRspkMailhogLocalHost();
//			SystemNetMail.SendMailToRspkMailhog();

//			new LimilabsSmtp().RequestDeliveryNotification();

//			new DsnSmtpClient().SendMailToMailtrap();
//			new DsnSmtpClient().DownloadMessages();

			//Запросить параметры smtp сервера:
//			RunTestMailServer();

			//Распарсить ответное письмо:
//			ParsLocalEmailDsn();
			
			Console.WriteLine("Sent");
			Console.ReadLine();
		}

		/// <summary>
		/// Распарсить локальное письмо-отчет сформированный Dsn роботом сервера
		/// </summary>
		private static void ParsLocalEmailDsn()
		{
			MimeMessage message;
			using (Stream mailStream = File.OpenRead("C:\\TestMail.eml"))
			{
				message = MimeMessage.Load(mailStream);

				mailStream.Close();
			}

			new MessageDeliveryStatusHelper().ProcessDeliveryStatusNotification(message);
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