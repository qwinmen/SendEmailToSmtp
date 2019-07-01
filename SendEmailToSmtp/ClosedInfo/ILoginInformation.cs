using MailKit.Security;

namespace SendEmailToSmtp.ClosedInfo
{
	public interface ILoginInformation
	{
		string UserName { get; set; }
		string Password { get; set; }
		string Host { get; set; }
		int SmtpPort { get; set; }
		int Pop3Port { get; set; }
		int ImapPort { get; set; }
		SecureSocketOptions SecureSocketOptions { get; set; }
	}
}