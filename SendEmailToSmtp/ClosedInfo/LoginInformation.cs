using System;
using MailKit.Security;

namespace SendEmailToSmtp.ClosedInfo
{
	//Открытая часть с примером реализации обьекта аутентификаций
	public partial class LoginInformation
	{
		public ILoginInformation GetExampleLoginInformation(SiteLoginInfo siteLoginInfo)
		{
			switch (siteLoginInfo)
			{
				case SiteLoginInfo.ExampleMailLogin:
					return new ExampleMailLogin();
				default:
					throw new ArgumentOutOfRangeException(nameof(siteLoginInfo), siteLoginInfo, null);
			}
		}

		/// <summary>
		///     Пример инициализаций
		/// </summary>
		public class ExampleMailLogin : ILoginInformation
		{
			public ExampleMailLogin()
			{
				UserName = "1111111";
				Password = "2222222";
				Host = "smtp.example.com";
				SmtpPort = 25;
				Pop3Port = 110;
				SecureSocketOptions = SecureSocketOptions.StartTls;
			}

			public string UserName { get; set; }
			public string Password { get; set; }
			public string Host { get; set; }
			public int SmtpPort { get; set; }
			public int Pop3Port { get; set; }
			public int ImapPort { get; set; }
			public SecureSocketOptions SecureSocketOptions { get; set; }
		}
	}
}