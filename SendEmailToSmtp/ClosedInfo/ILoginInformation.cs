using MailKit.Security;

namespace SendEmailToSmtp.ClosedInfo
{
	/// <summary>
	/// Базовые поля любой аутентификации на сервере
	/// </summary>
	public interface ILoginInformation: IMailSender
	{
		string UserName { get; set; }
		string Password { get; set; }
		string Host { get; set; }
		int SmtpPort { get; set; }
		int Pop3Port { get; set; }
		int ImapPort { get; set; }
		SecureSocketOptions SecureSocketOptions { get; set; }
	}

	/// <summary>
	/// Информация об отправителе, получателе и т.д.
	/// </summary>
	public interface IMailSender
	{
		string To { get; set; }
	}
}