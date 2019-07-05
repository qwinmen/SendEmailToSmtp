using System;
using MailKit;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MimeKit;
using SendEmailToSmtp.ClosedInfo;

namespace SendEmailToSmtp
{
	//https://stackoverflow.com/questions/45027910/get-the-delivery-status-of-email-with-mimekit-mailkit-library
	//Получить статус доставки электронной почты с помощью библиотеки mimekit/mailkit.
	//Это скажет SMTP-серверу отправлять вам электронные письма о статусе доставки каждого отправленного вами сообщения.
	public class DsnSmtpClient : SmtpClient
	{
		private readonly ILoginInformation _loginInfo;

		public DsnSmtpClient(string logFileName)
			: base(new ProtocolLogger(logFileName))
		{
			_loginInfo = new LoginInformation().GetLoginInformation(SiteLoginInfo.Mailtrap);
		}

		public DsnSmtpClient()
			: base()
		{
			_loginInfo = new LoginInformation().GetLoginInformation(SiteLoginInfo.Mailtrap);
		}

		/// <summary>
		/// Забрать письма с ящика.
		/// Удалить письма в ящике.
		/// https://github.com/jstedfast/MailKit/blob/master/Documentation/Examples/Pop3Examples.cs
		/// </summary>
		public void DownloadMessages()
		{
			using (var client = new Pop3Client(new ProtocolLogger("pop3.log")))
			{
				client.Connect("pop3.mailtrap.io", _loginInfo.Pop3Port, _loginInfo.SecureSocketOptions);
				client.Authenticate(_loginInfo.UserName, _loginInfo.Password);

				for (int i = 0; i < client.Count; i++)
				{
					var message = client.GetMessage(i);

					// write the message to a file
					message.WriteTo($"{i}.msg");

					// mark the message for deletion
					client.DeleteMessage(i);
				}

				client.Disconnect(true);
			}
		}

		/// <summary>
		/// Отправить почту при помощи MailKit. Смотри тут https://mailtrap.io/
		/// </summary>
		public void SendMailToMailtrap()
		{
			string fakeEmail = "63b978b8b8-ba6217@inbox.mailtrap.io";
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress("Stage mailer", _loginInfo.UserName));
			message.To.Add(new MailboxAddress("Дмитрий", _loginInfo.To));
			message.Subject = "Тестирование dsn";
			message.Body = new TextPart("plain")
			{
				Text = @"Тестовое сообщения для проверки smtp,
-- Abp hello wold"
			};
			DeliveryStatusNotification? dsnStatus = GetDeliveryStatusNotifications(message, new MailboxAddress(_loginInfo.To));
			Console.WriteLine("Типы уведомлений о состоянии доставки, требуемые для указанного почтового ящика получателя: {0}", dsnStatus);
			
			using (var client = new DsnSmtpClient("SendMailToMailtrap.log"))
			{
				client.Connect(_loginInfo.Host, _loginInfo.SmtpPort, _loginInfo.SecureSocketOptions);

				if (client.Capabilities.HasFlag(SmtpCapabilities.Authentication))
					client.Authenticate(_loginInfo.UserName, _loginInfo.Password);
				if (client.Capabilities.HasFlag(SmtpCapabilities.Dsn))
					Console.WriteLine("The SMTP server supports delivery-status notifications.");
				
				string envelopeId = GetEnvelopeId(message);
				Console.WriteLine("Идентификатор конверта, который будет использоваться с DSN: {0}", envelopeId);

				string command = string.Format("RCPT TO:<{0}>", _loginInfo.To);
				if ((client.Capabilities & SmtpCapabilities.Dsn) != 0)
				{
					var notify = GetDeliveryStatusNotifications(message, new MailboxAddress(_loginInfo.To));

					if (notify.HasValue)
						command += " NOTIFY=" + (notify.Value);
					Console.WriteLine("Проверка корректного выставления флага dsn: {0}", command);
				}

				client.MessageSent += ClientOnMessageSent;
				client.Send(message);
				
				client.Disconnect(true);
			}
		}

		private void ClientOnMessageSent(object sender, MessageSentEventArgs e)
		{
			Console.WriteLine("Сообщение отослано.");
		}
		
		/// <summary>
		///     Получить идентификатор конверта, который будет использоваться с DSN.
		/// </summary>
		/// <remarks>
		///     <para>
		///         Идентификатор конверта, если он не пустой, полезен при определении того, для какого сообщения было отправлено
		///         уведомление о состоянии доставки.
		///     </para>
		///     <para>
		///         Идентификатор конверта должен быть уникальным и иметь длину до 100 символов, но должен состоять только из
		///         печатных символов ASCII и не содержать пробелов.
		///     </para>
		///     <para>For more information, see rfc3461, section 4.4.</para>
		/// </remarks>
		/// <returns>Идентификатор конверта.</returns>
		/// <param name="message">The message.</param>
		protected override string GetEnvelopeId(MimeMessage message)
		{
			// Поскольку вы захотите иметь возможность сопоставлять любой идентификатор, который вы возвращаете здесь, с сообщением,
			// очевидным идентификатором, который нужно использовать, является, вероятно, значение MessageId.
			return message.MessageId;
		}

		/// <summary>
		///     Получите типы уведомлений о состоянии доставки, требуемые для указанного почтового ящика получателя.
		/// </summary>
		/// <remarks>
		///     Получает типы уведомлений о состоянии доставки, требуемые для указанного почтового ящика получателя.
		/// </remarks>
		/// <returns>Желаемый тип уведомления о статусе доставки.</returns>
		/// <param name="message">Сообщение отправляется.</param>
		/// <param name="mailbox">Почтовый ящик.</param>
		protected override DeliveryStatusNotification? GetDeliveryStatusNotifications(MimeMessage message,
			MailboxAddress mailbox)
		{
			// В этом примере мы хотим получать уведомления только о сбоях доставки в почтовый ящик.
			// Если вы также хотите получать уведомления о задержках или успешных поставках,
			// просто поразрядно - или любую комбинацию флагов, о которой вы хотите получать уведомления.
			return DeliveryStatusNotification.Failure/* | DeliveryStatusNotification.Delay | DeliveryStatusNotification.Success*/;
		}
	}
}