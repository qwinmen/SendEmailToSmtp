using Limilabs.Client.SMTP;
using Limilabs.Mail;
using Limilabs.Mail.Fluent;
using Limilabs.Mail.Headers;
using SendEmailToSmtp.ClosedInfo;

namespace SendEmailToSmtp
{
	//https://www.limilabs.com/
	public class LimilabsSmtp
	{
		//https://www.limilabs.com/blog/requesting-delivery-status-notifications-dsn
		public void RequestDeliveryNotification()
		{
			using (Smtp smtp = new Smtp())
			{
				smtp.Connect(LoginInformation.MailtrapLogin.Host, LoginInformation.MailtrapLogin.Port, false);
				smtp.UseBestLogin(LoginInformation.MailtrapLogin.UserName, LoginInformation.MailtrapLogin.Password);
				smtp.Configuration.DeliveryNotification =
					DeliveryNotificationOptions.OnFailure | DeliveryNotificationOptions.Delay | DeliveryNotificationOptions.OnSuccess;

				IMail email = Mail.Text("limbo lab test dsn")
					.From(new MailBox("test@ya.ru"))
					.To(new MailBox("component@gmail.com"))
					.Create();
				smtp.SendMessage(email);

				smtp.Close();
			}
		}
	}
}