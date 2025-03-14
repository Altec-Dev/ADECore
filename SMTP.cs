/*
 *Copyright(c) 2023 Ulf - Dirk Stockburger
*/

using System.Net;
using System.Net.Mail;

namespace ADEcore
{
    public class SMTP
    {
        public SMTP() { }

        public bool SendEMailViaSmtp(string recipient, string msg, ref CommandLineOptions.ServerModeOptions opts)
        {
            foreach(string r in Consts.emailFakeAccounts)
            {
                string tmp = recipient.Split(new string[] { "@" }, StringSplitOptions.None)[0];
                if (r.ToLower() == tmp.ToLower())
                {
                    return true; 
                }
            }

            bool ret = false;
            if (recipient == string.Empty) { return ret; }
            try
            {
                string sender = opts.SmtpSender;
                string username = opts.SmtpUser;
                string password = opts.SmtpPassword;
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(opts.SmtpServer);
                mail.From = new MailAddress(opts.SmtpSender);
                mail.To.Add(recipient);
                mail.Subject = opts.SmtpSubject;
                mail.Body = msg;
                SmtpServer.Port = Convert.ToInt32(opts.SmtpPort);
                SmtpServer.Credentials = new System.Net.NetworkCredential(opts.SmtpUser, opts.SmtpPassword);
                SmtpServer.EnableSsl = opts.SmtpEnableSSL;
                // Fixes the issue: Das Remotezertifikat ist laut Validierungsverfahren ungültig
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                SmtpServer.Send(mail);
                ret = true;
            }
            catch (Exception ex)
            {
                //ConsoleWriteErrorAndExit(116, @"Error sending the email: " + ex.Message);
            }
            return ret;
        }
    }
}
