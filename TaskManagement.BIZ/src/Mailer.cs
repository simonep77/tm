using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TaskManagement.BIZ.src
{
    internal class Mailer
    {

        public static void Send(string mailfrom, string mailto, string mailcc, string mailbcc, string subj, string body, string attachs)
        {
            var ATT_SEPS = new char[] { ',', ';' };
            var ATT_MAIL_SEPS = new char[] { ',', ';', ' ' };
            var oSmtp = new System.Net.Mail.SmtpClient();

            using (var oMsg = new System.Net.Mail.MailMessage())
            {

                // Imposta dati
                oMsg.IsBodyHtml = true;
                oMsg.Subject = subj;
                oMsg.Body = body;

                // Indirizzi
                if (!string.IsNullOrWhiteSpace(mailfrom))
                {
                    oMsg.From = new System.Net.Mail.MailAddress(mailfrom);
                }

                if (!string.IsNullOrWhiteSpace(mailto))
                {
                    foreach (string sEmail in mailto.Split(ATT_MAIL_SEPS, StringSplitOptions.RemoveEmptyEntries))
                        oMsg.To.Add(new System.Net.Mail.MailAddress(sEmail));
                }

                if (!string.IsNullOrWhiteSpace(mailcc))
                {
                    foreach (string sEmail in mailcc.Split(ATT_MAIL_SEPS, StringSplitOptions.RemoveEmptyEntries))
                        oMsg.CC.Add(new System.Net.Mail.MailAddress(sEmail));
                }

                if (!string.IsNullOrWhiteSpace(mailbcc))
                {
                    foreach (string sEmail in mailbcc.Split(ATT_MAIL_SEPS, StringSplitOptions.RemoveEmptyEntries))
                        oMsg.Bcc.Add(new System.Net.Mail.MailAddress(sEmail));
                }

                // Attachments
                if (!string.IsNullOrWhiteSpace(attachs))
                {
                    foreach (string sFile in attachs.Split(ATT_SEPS, StringSplitOptions.RemoveEmptyEntries))
                        oMsg.Attachments.Add(new System.Net.Mail.Attachment(sFile));
                }

                // INVIA
                oSmtp.Send(oMsg);
            }

        }


    }
}
