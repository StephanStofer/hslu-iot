using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace IotGui.Data
{
    public class MailService : IMailService
    {
        public void SendAlertMail(string device, string value)
        {
            try
            {
                var fromAddress = new MailAddress("altert.group02@gmail.com", "IoT Monitoring");
                var toAddress = new MailAddress("altert.group02@gmail.com", "IoT Monitoring");
                const string fromPassword = "IOTGroup02";
                const string subject = "Alert: IoT systems alert";
                string body = $"Found critical {device} values: {value}.\nSee monitoring tool for more information.";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Mail sending error: {e.Message}");
            }
        }
    }
}