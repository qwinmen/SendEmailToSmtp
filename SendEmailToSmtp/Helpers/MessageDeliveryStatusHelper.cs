using System;
using System.Linq;
using MimeKit;

namespace SendEmailToSmtp.Helpers
{
	// Author: Jeffrey Stedfast <jeff@xamarin.com>
	// https://github.com/jstedfast/MailKit/

	public class MessageDeliveryStatusHelper
	{
		#region ProcessDeliveryStatusNotification

		/// <summary>
		/// Распарсить переданное сообщение
		/// </summary>
		/// <param name="message"></param>
		public void ProcessDeliveryStatusNotification(MimeMessage message)
		{
			MultipartReport report = message.Body as MultipartReport;

			if (report == null || report.ReportType == null || !report.ReportType.Equals("delivery-status", StringComparison.OrdinalIgnoreCase))
			{
				// это не уведомление о статусе доставки ...
				Console.WriteLine("Переданное сообщение не содержит Dsn тела.");
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

		#endregion
	}
}