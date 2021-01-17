using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace A2_Project
{
	public static class EmailManagement
	{
		/// <summary>
		/// Sends an email
		/// </summary>
		/// <param name="recipient">Who the email should be sent to</param>
		/// <param name="body">The body of the email as HTML</param>
		private static void SendEmail(string recipient, string subject, string body)
		{
			MailMessage message = new MailMessage();
			SmtpClient smtp = new SmtpClient();
			message.From = new MailAddress("atempmailfortestingcsharp@gmail.com", "JD Dog Care");
			message.To.Add(new MailAddress(recipient));
			message.Subject = subject;
			message.IsBodyHtml = true;
			message.Body = body;
			smtp.Port = 587;
			smtp.Host = "smtp.gmail.com";
			smtp.EnableSsl = true;
			smtp.UseDefaultCredentials = false;
			smtp.Credentials = new NetworkCredential("atempmailfortestingcsharp@gmail.com", FileAccess.GetEmailPassword());
			smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
			smtp.SendMailAsync(message);
		}

		#region 2FA
		/// <summary>
		/// Generates a random string of characters of size "size", default 8
		/// </summary>
		public static string GenerateRandomKey(int size = 8)
		{
			// The selection of characters to be used to generate the key
			char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
			byte[] data = new byte[4 * size];
			using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
			{
				crypto.GetBytes(data);
			}
			StringBuilder result = new StringBuilder(size);
			for (int i = 0; i < size; i++)
			{
				var rnd = BitConverter.ToUInt32(data, i * 4);
				var idx = rnd % chars.Length;

				result.Append(chars[idx]);
			}

			return result.ToString();
		}

		/// <summary>
		/// Sends an email containing a randomly generated key to act as 2FA
		/// </summary>
		/// <returns>The key that was generated</returns>
		public static string Send2FAEmail(string recipient)
		{
			// TODO: Body of 2FA email should be proper HTML, and contain more than just the key itself
			string key = GenerateRandomKey();
			SendEmail(recipient, "Authentication Email", key);
			return key;
		}
		#endregion 2FA

		#region Invoices
		/// <summary>
		/// Sends the contents of the DataTable as an email
		/// </summary>
		public static void SendInvoiceEmail(string recipient, DataTable table, string[] tableHeaders, string[] contactData)
		{
			SendEmail(recipient, "Monthly Invoice", GenerateInvoiceHTML(DataTableToStringArrays(table), tableHeaders, contactData));
		}

		private static string[][] DataTableToStringArrays(DataTable table)
		{
			List<string[]> data = new List<string[]>();
			foreach (DataRow r in table.Rows)
			{
				data.Add(r.ItemArray.Select(o => o.ToString()).ToArray());
			}
			return data.ToArray();
		}

		/// <summary>
		/// Generates the body HTML for an invoice email
		/// </summary>
		private static string GenerateInvoiceHTML(string[][] tableArr, string[] headers, string[] contactData)
		{
			string header = @"
<!doctype html>
<html>
	<head>
		<meta charset=""utf-8"">
		<title> JD Dog Care - Invoice </title>
		<style>
			.invoice-box {
				max-width: 800px;
				margin: auto;
				padding: 30px;
				border: 1px solid #eee;
				box-shadow: 0 0 10px rgba(0, 0, 0, .15);
				font-size: 16px;
				line-height: 24px;
				font-family: 'Helvetica Neue', 'Helvetica', Helvetica, Arial, sans-serif;
				color: #555;
			}

			.invoice-box table {
				width: 100%;
				line-height: inherit;
				text-align: left;
			}

			.invoice-box table td {
				padding: 5px;
				vertical-align: top;
			}

			.invoice-box table tr td:nth-child(2) {
				text-align: right;
			}

			.invoice-box table tr.top table td {
				padding-bottom: 20px;
			}

			.invoice-box table tr.top table td.title {
				font-size: 45px;
				line-height: 45px;
				color: #333;
			}

			.invoice-box table tr.information table td {
				padding-bottom: 40px;
			}

			.invoice-box table tr.heading td {
				background: #eee;
				border-bottom: 1px solid #ddd;
				font-weight: bold;
			}

			.invoice-box table tr.details td {
				padding-bottom: 20px;
			}

			.invoice-box table tr.item td{
				border-bottom: 1px solid #eee;
			}

			.invoice-box table tr.item.last td {
				border-bottom: none;
			}

			.invoice-box table tr.total td:nth-child(2) {
				border-top: 2px solid #eee;
				font-weight: bold;
			}
			@media only screen and (max-width: 600px) {
				.invoice-box table tr.top table td {
					width: 100%;
					display: block;
					text-align: center;
				}

				.invoice-box table tr.information table td {
					width: 100%;
					display: block;
					text-align: center;
				}
			}
		</style>
	</head>";

			string body = String.Format(@"
	<body>
		<div class=""invoice-box"">
			<table cellpadding=""0"" cellspacing=""0"">
				<tr class=""top"">
					<td colspan=""2"">
						<table>
							<tr>
								<td class=""title"">
									<img src=""https://drive.google.com/uc?export=view&id=1ib9GQTLzSi6WNdHJTDkUE-N2nPLeGbeW"" style=""width:100%; max-width:300px;"">
									</td>
								<td>
									Invoice #: {0}<br>
									Created: {1}<br>
									Due: ???
								</td>
							</tr>
						</table>
					</td>
				</tr>", new string[] { "1", DateTime.Now.ToString("dd/MM/yyyy") });

			body += String.Format(@"
				<tr class=""information"">
					<td colspan=""2"">
						<table>
							<tr>
								<td>
									JD Dog Care<br>
									164 Kilbroney Rd<br>
									Newtown, Rostrevor
								</td>
								<td>
									{0}<br>
									{1}<br>
									{2}
								</td>
							</tr>
						</table>
					</td>
				</tr>

				<tr class=""heading"">
					<td>Payment Method</td>
					<td>Cheque No</td>
				</tr>

				<tr class=""details"">
					<td>{3}</td>
					<td>1000</td>
				</tr>
			</table>", contactData[0], contactData[1], contactData[2], contactData[3]);

			body += @"
			<table cellpadding=""0"" cellspacing=""0"">
				<tr class=""heading"">";
			for (int i = 0; i < headers.Length; i++)
				body += String.Format("<td>{0}</td>", headers[i]);
			body += "</tr>";

			int total = 0;
			for (int i = 0; i < tableArr.Length - 1; i++)
			{
				body += @"<tr class=""item"">";
				for (int j = 0; j < tableArr[i].Length; j++)
					body += String.Format("<td>{0}</td>", tableArr[i][j]);
				body += "</tr>";
			}
			body += @"<tr class=""item last"">";
			for (int j = 0; j < tableArr[tableArr.Length - 1].Length; j++)
				body += String.Format("<td>{0}</td>", tableArr[tableArr.Length - 1][j]);
			body += "</tr>";
			body += String.Format(@"<tr class=""total""><td></td><td>Total: {0}</td></tr>", total);
			body += @"
			</table>
		</div>
	</body>
</html>";
			return header + body;
		}
		#endregion Invoices
	}
}
