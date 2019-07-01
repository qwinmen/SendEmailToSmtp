using System;
using System.Linq;
using System.Threading;
using MailKit;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MailKit.Security;
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

		public DsnSmtpClient()
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
			InternetAddress mailbox = new MailboxAddress("test@ya.ru");
			MimeMessage message = new MimeMessage
			{
				From = {InternetAddress.Parse(ParserOptions.Default, "no-reply@moscow.ru")},
				To = {mailbox},
				Subject = "Тестирование Dsn.",
				Body = new TextPart
				{
					Text = $"Тестирование dsn. Time {DateTime.Now}",
				},
				Headers =
				{
					{"Return-Receipt-To", "test@ya.ru"},
					{"Disposition-Notification-To", "test@ya.ru"},
				},
			};
			
			using (var client = new SmtpClient())
			{
				client.Connect(_loginInfo.Host, _loginInfo.SmtpPort, _loginInfo.SecureSocketOptions);

				if (client.Capabilities.HasFlag(SmtpCapabilities.Authentication))
					client.Authenticate(_loginInfo.UserName, _loginInfo.Password);

				client.MessageSent +=ClientOnMessageSent;
				client.Send(message);
				
				string envelopeId = GetEnvelopeId(message);
				Console.WriteLine("Получить идентификатор конверта, который будет использоваться с DSN: {0}", envelopeId);
				
				DeliveryStatusNotification? dsnStatus = GetDeliveryStatusNotifications(message, new MailboxAddress("test@ya.ru"));
				Console.WriteLine("Типы уведомлений о состоянии доставки, требуемые для указанного почтового ящика получателя: {0}", dsnStatus);

				Thread.Sleep(10000);
				client.NoOp();
				
				client.Disconnect(true);
			}
		}

		private void ClientOnMessageSent(object sender, MessageSentEventArgs e)
		{
			Console.WriteLine("Сообщение отослано.");
		}


		public void ProcessDeliveryStatusNotification(MimeMessage message)
		{
			MultipartReport report = message.Body as MultipartReport;

			if (report == null || report.ReportType == null || !report.ReportType.Equals("delivery-status", StringComparison.OrdinalIgnoreCase))
			{
				// это не уведомление о статусе доставки ...
				Console.WriteLine("Отсутствует уведомление о статусе доставки.");
				return;
			}

			// process the report
			foreach (MessageDeliveryStatus mds in report.OfType<MessageDeliveryStatus>())
			{
				// process the status groups - each status group represents a different recipient

				// The first status group contains information about the message
				string envelopeId = mds.StatusGroups[0]["Original-Envelope-Id"];

				// all of the other status groups contain per-recipient information
				for (int i = 1; i < mds.StatusGroups.Count; i++)
				{
					string recipient = mds.StatusGroups[i]["Original-Recipient"];
					string action = mds.StatusGroups[i]["Action"];

					if (recipient == null)
						recipient = mds.StatusGroups[i]["Final-Recipient"];

					// the recipient string should be in the form: "rfc822;user@domain.com"
					int index = recipient.IndexOf(';');
					string address = recipient.Substring(index + 1);

					switch (action)
					{
						case "failed":
							Console.WriteLine("Delivery of message {0} failed for {1}", envelopeId, address);
							break;
						case "delayed":
							Console.WriteLine("Delivery of message {0} has been delayed for {1}", envelopeId, address);
							break;
						case "delivered":
							Console.WriteLine("Delivery of message {0} has been delivered to {1}", envelopeId, address);
							break;
						case "relayed":
							Console.WriteLine("Delivery of message {0} has been relayed for {1}", envelopeId, address);
							break;
						case "expanded":
							Console.WriteLine(
								"Delivery of message {0} has been delivered to {1} and relayed to the the expanded recipients",
								envelopeId, address);
							break;
					}
				}
			}
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
			return DeliveryStatusNotification.Failure | DeliveryStatusNotification.Delay | DeliveryStatusNotification.Success;
		}
	}
}