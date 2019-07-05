using System;
using System.Linq;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MimeKit;
using SendEmailToSmtp.ClosedInfo;

namespace SendEmailToSmtp
{
	/// <summary>
	/// https://github.com/jstedfast/MailKit
	/// </summary>
	public class AbpMailKitSender
	{
		#region Capabilities POP

		public static void PopPrintCapabilities()
		{
			var loginInfo = new LoginInformation().GetLoginInformation(SiteLoginInfo.StageMoscow);

			using (var client = new Pop3Client ()) {
				client.Connect(/*"pop3.mailtrap.io"*/loginInfo.Host, loginInfo.Pop3Port, loginInfo.SecureSocketOptions);

				if (client.Capabilities.HasFlag (Pop3Capabilities.Sasl)) {
					var mechanisms = string.Join (", ", client.AuthenticationMechanisms);
					Console.WriteLine ("The POP3 server supports the following SASL mechanisms: {0}", mechanisms);
				}

				client.Authenticate(loginInfo.UserName, loginInfo.Password);

				if (client.Capabilities.HasFlag (Pop3Capabilities.Apop))
					Console.WriteLine ("The server supports APOP authentication.");

				if (client.Capabilities.HasFlag (Pop3Capabilities.Expire)) {
					if (client.ExpirePolicy > 0)
						Console.WriteLine ("The POP3 server automatically expires messages after {0} days", client.ExpirePolicy);
					else
						Console.WriteLine ("The POP3 server will never expire messages.");
				}

				if (client.Capabilities.HasFlag (Pop3Capabilities.LoginDelay))
					Console.WriteLine ("The minimum number of seconds between login attempts is {0}.", client.LoginDelay);

				if (client.Capabilities.HasFlag (Pop3Capabilities.Pipelining))
					Console.WriteLine ("The POP3 server can pipeline commands, so using client.GetMessages() will be faster.");

				if (client.Capabilities.HasFlag (Pop3Capabilities.Top))
					Console.WriteLine ("The POP3 server supports the TOP command, so it's possible to download message headers.");

				if (client.Capabilities.HasFlag (Pop3Capabilities.UIDL))
					Console.WriteLine ("The POP3 server supports the UIDL command which means we can track messages by UID.");

				client.Disconnect (true);
			}
		}

		#endregion

		/// <summary>
		///     Узнать возможности почтового сервера.
		/// </summary>
		public static void PrintCapabilities()
		{
			var loginInfo = new LoginInformation().GetLoginInformation(SiteLoginInfo.StageMoscow);

			using (var client = new SmtpClient())
			{
				client.Connect(loginInfo.Host, loginInfo.SmtpPort, loginInfo.SecureSocketOptions);

				if (client.Capabilities.HasFlag(SmtpCapabilities.Authentication))
				{
					var mechanisms = string.Join(", ", client.AuthenticationMechanisms);
					Console.WriteLine("The SMTP server supports the following SASL mechanisms: {0}", mechanisms);
					client.Authenticate(loginInfo.UserName, loginInfo.Password);
				}

				if (client.Capabilities.HasFlag(SmtpCapabilities.Size))
					Console.WriteLine("The SMTP server has a size restriction on messages: {0}.", client.MaxSize);

				if (client.Capabilities.HasFlag(SmtpCapabilities.Dsn))
					Console.WriteLine("The SMTP server supports delivery-status notifications.");

				if (client.Capabilities.HasFlag(SmtpCapabilities.EightBitMime))
					Console.WriteLine("The SMTP server supports Content-Transfer-Encoding: 8bit");

				if (client.Capabilities.HasFlag(SmtpCapabilities.BinaryMime))
					Console.WriteLine("The SMTP server supports Content-Transfer-Encoding: binary");

				if (client.Capabilities.HasFlag(SmtpCapabilities.UTF8))
					Console.WriteLine("The SMTP server supports UTF-8 in message headers.");

				client.Disconnect(true);
			}
		}

		/// <summary>
		/// Sending Messages
		/// </summary>
		public static void SendMailToStageMoscow()
		{
			var loginInfo = new LoginInformation().GetLoginInformation(SiteLoginInfo.StageMoscow);
			string fakeEmail = String.Format("63b978b8b8-ba6217@{1}", "inbox.mailtrap.io", "yandex.ru");

			var message = new MimeMessage();
			message.From.Add(new MailboxAddress("Stage mailer", loginInfo.UserName));
			message.To.Add(new MailboxAddress("Дмитрий", fakeEmail));
			message.Subject = "Hello wold";
			message.Body = new TextPart("plain")
			{
				Text = @"Тестовое сообщения для проверки smtp,
-- Abp hello wold"
			};

			using (var client = new DsnSmtpClient("smtpsend.log"))
			{
				// For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
				client.ServerCertificateValidationCallback = (s, c, h, e) => true;
				client.Connect(loginInfo.Host, loginInfo.SmtpPort, loginInfo.SecureSocketOptions);

				if (client.Capabilities.HasFlag(SmtpCapabilities.Authentication))
					client.Authenticate(loginInfo.UserName, loginInfo.Password);

				try
				{
					client.Send(message);
				}
				catch (SmtpCommandException ex)
				{
					Console.WriteLine("Error sending message: {0}", ex.Message);
					Console.WriteLine("\tStatusCode: {0}", ex.StatusCode);

					switch (ex.ErrorCode)
					{
						case SmtpErrorCode.RecipientNotAccepted:
							Console.WriteLine("\tRecipient not accepted: {0}", ex.Mailbox);
							break;
						case SmtpErrorCode.SenderNotAccepted:
							Console.WriteLine("\tSender not accepted: {0}", ex.Mailbox);
							break;
						case SmtpErrorCode.MessageNotAccepted:
							Console.WriteLine("\tMessage not accepted.");
							break;
					}
				}
				catch (SmtpProtocolException ex)
				{
					Console.WriteLine("Protocol error while sending message: {0}", ex.Message);
				}

				client.Disconnect(true);
			}
		}

		/// <summary>
		/// Удалить сообщения по IMAP
		/// </summary>
		public static void ImapDeleteMailFromStageMoscow()
		{
			var loginInfo = new LoginInformation().GetLoginInformation(SiteLoginInfo.StageMoscow);
			using (var client = new ImapClient(new ProtocolLogger("smtpdelete.log")))
			{
				// For demo-purposes, accept all SSL certificates
				client.ServerCertificateValidationCallback = (s, c, h, e) => true;
				client.Connect(loginInfo.Host, 993, true);
				client.Authenticate(loginInfo.UserName, loginInfo.Password);

				// The Inbox folder is always available on all IMAP servers...
				IMailFolder inbox = client.Inbox;
				inbox.Open(FolderAccess.ReadWrite);

				SpecialFolder folderNamespace = SpecialFolder.Trash;
				inbox.MoveTo(14, client.GetFolder(folderNamespace));

				client.Disconnect(true);
			}
		}

		/// <summary>
		/// Получить сообщения по IMAP
		/// </summary>
		public static void ImapGetMailFromStageMoscow()
		{
			var loginInfo = new LoginInformation().GetLoginInformation(SiteLoginInfo.StageMoscow);

			using (var client = new ImapClient(new ProtocolLogger("smtpget.log")))
			{
				// For demo-purposes, accept all SSL certificates
				client.ServerCertificateValidationCallback = (s, c, h, e) => true;
				client.Connect(loginInfo.Host, 993, true);
				client.Authenticate(loginInfo.UserName, loginInfo.Password);

				// The Inbox folder is always available on all IMAP servers...
				var inbox = client.Inbox;
				inbox.Open(FolderAccess.ReadOnly);
				
				//Вывести поверхностую информацию без полной загрузки писем с сервера:
				foreach (IMessageSummary summary in inbox.Fetch(0, -1, MessageSummaryItems.Envelope)) {
					Console.WriteLine ("[summary] {0:D2}: {1}", summary.Index, summary.Envelope.Subject);
				}

				Console.WriteLine();

				foreach (IMessageSummary summary in inbox.Fetch(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Body))
				{
					if (summary.TextBody != null)
					{
						// this will download *just* the text/plain part
						var text = inbox.GetBodyPart(summary.UniqueId, summary.TextBody);
						Console.WriteLine("[summary text] {0:D2}: {1}", summary.Index, text);
					}

					if (summary.HtmlBody != null)
					{
						// this will download *just* the text/html part
						var html = inbox.GetBodyPart(summary.UniqueId, summary.HtmlBody);
						Console.WriteLine("[summary html] {0:D2}: {1}", summary.Index, html);
					}
					// if you'd rather grab, say, an image attachment... it might look something like this:
					if (summary.Body is BodyPartMultipart multipart)
					{
						var attachment = multipart.BodyParts.OfType<BodyPartBasic>().FirstOrDefault(x => x.FileName == "logo.jpg");
						if (attachment != null)
						{
							// this will download *just* the attachment
							var part = inbox.GetBodyPart(summary.UniqueId, attachment);
						}
					}
				}

				Console.WriteLine("Total messages: {0}", inbox.Count);
				Console.WriteLine("Recent messages: {0}", inbox.Recent);

				for (int i = 0; i < inbox.Count; i++)
				{
					var message = inbox.GetMessage(i);
					Console.WriteLine("Subject: {0}", message.Subject);
				}
				
				client.Disconnect(true);
			}
		}

		/// <summary>
		/// Получить сообщения по POP3
		/// </summary>
		public static void Pop3GetMailFromStageMoscow()
		{
			var loginInfo = new LoginInformation().GetLoginInformation(SiteLoginInfo.StageMoscow);

			using (var client = new Pop3Client(new ProtocolLogger("pop3get.log")))
			{
				// For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
				client.ServerCertificateValidationCallback = (s, c, h, e) => true;
				client.Connect(loginInfo.Host, loginInfo.Pop3Port, false);
				client.Authenticate(loginInfo.UserName, loginInfo.Password);
				
				for (int i = 0; i < client.Count; i++)
				{
					var message = client.GetMessage(i);
					Console.WriteLine("Subject: {0}", message.Subject);
				}

				client.Disconnect(true);
			}
		}
	}
}