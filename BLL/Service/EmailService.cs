using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;

namespace BLL.Service
{
    public class EmailService
    {
        public static Tuple<int, string> SendEmailVerifikasi(string emailTo, string codeOtp)
        {
            DotNetEnv.Env.Load();
            string email = Environment.GetEnvironmentVariable("Email");
            string password = Environment.GetEnvironmentVariable("Password");
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("LoginApi", email));
            message.To.Add(MailboxAddress.Parse(emailTo));
            message.Body = new TextPart("plain")
            {
                Text = $@"
                Dear user: {emailTo}
                Here Your Verification Code : {codeOtp}
                Dont Tell Anyone <br/>
                Contact Us Email : {email}
                "
            };
            SmtpClient client = new SmtpClient();
            try
            {
                client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                client.Authenticate(email, password);
                client.Send(message);
                return Tuple.Create(1, "Success");
            }
            catch (Exception Ex)
            {
                return Tuple.Create(-1, Ex.Message);
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();

            }
        }
    }
}
