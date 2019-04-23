using System;

namespace SendEmailToSmtp
{
	public class Program
	{
		public static void Main()
		{
//			SystemNetMail.SendMailToMailtrap();
//			SystemNetMail.SendMailToRspkMailhogLocalHost();
//			SystemNetMail.SendMailToRspkMailhog();

			//Запросить параметры smtp сервера:
//			AbpMailKitSender.PrintCapabilities();

//			new DsnSmtpClient().SendMailToMailtrap();
			
			Console.WriteLine("Sent");
			Console.ReadLine();
		}
	}
}