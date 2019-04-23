using System;
using System.Linq;
using MailKit;
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
		public DsnSmtpClient()
		{
		}

		/// <summary>
		/// Отправить почту при помощи MailKit. Смотри тут https://mailtrap.io/
		/// </summary>
		public void SendMailToMailtrap()
		{
			InternetAddress mailbox = new MailboxAddress("to@example.com");
			MimeMessage message = new MimeMessage
			{
				From = { InternetAddress.Parse(ParserOptions.Default, "from@example.com") },
				To = { mailbox },
				Subject = "Hello world",
				Body = new TextPart
				{
					Text = "Olololololo body.",
				},
			};

			using (var client = new SmtpClient())
			{
				client.Connect("smtp.mailtrap.io", 2525, SecureSocketOptions.StartTls);
				if (client.Capabilities.HasFlag(SmtpCapabilities.Authentication))
					client.Authenticate(LoginInformation.MailtrapLogin.UserName,
						LoginInformation.MailtrapLogin.Password);
				
				client.Send(message);
				
				string envelopeId = GetEnvelopeId(message);
				Console.WriteLine("Получить идентификатор конверта, который будет использоваться с DSN: {0}", envelopeId);

				DeliveryStatusNotification? dsnStatus = GetDeliveryStatusNotifications(message, new MailboxAddress("bala@bolka.ya"));
				Console.WriteLine("Получите типы уведомлений о состоянии доставки, требуемые для указанного почтового ящика получателя: {0}", dsnStatus);

				ProcessDeliveryStatusNotification(message);
				client.Disconnect(true);
			}
		}

		public void ProcessDeliveryStatusNotification(MimeMessage message)
		{
			MultipartReport report = message.Body as MultipartReport;

			if (report == null || report.ReportType == null || !report.ReportType.Equals("delivery-status", StringComparison.OrdinalIgnoreCase))
			{
				// это не уведомление о статусе доставки ...
				return;
			}

			// process the report
			foreach (var mds in report.OfType<MessageDeliveryStatus>())
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
			return DeliveryStatusNotification.Failure;
		}
	}
}