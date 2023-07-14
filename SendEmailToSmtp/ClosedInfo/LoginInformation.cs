using MailKit.Security;

namespace SendEmailToSmtp.ClosedInfo
{
	//Открытая часть с примером реализации обьекта аутентификаций
	public partial class LoginInformation
	{
		/// <summary>
		///     Пример инициализаций
		/// </summary>
		public class ExampleMailLogin : ILoginInformation
		{
			public LoginType LoginType => LoginType.ExampleMailLogin;
			public string UserName => "1111111";
			public string Password => "2222222";
			public string Host { get; set; } = "smtp.example.com";
			public string Domain { get; set; } = "example.com";
			public int SmtpPort { get; set; } = 25;
			public int Pop3Port { get; set; } = 110;
			public int ImapPort { get; set; }
			public int Port { get; set; }
			public SecureSocketOptions SecureSocketOptions { get; set; } = SecureSocketOptions.StartTls;
			public string To { get; set; } = "test@to.com";
		}
	}
}