using System;
using System.Net;
using System.Net.Mail;

namespace TechShopBackendDotnet.OtherModels
{


    public class EmailService
    {
        public void SendEmail(string recipientEmail, string subject, string body)
        {
            try
            {
                var fromAddress = new MailAddress("thaibao528@gmail.com", "Techshop");
                var toAddress = new MailAddress(recipientEmail, "Recipient Name");
                const string fromPassword = "ewhq uqiu hnvu osxg";
                string host = "smtp.gmail.com";

                var smtp = new SmtpClient
                {
                    Host = host,
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                // Xử lý exception khi gửi email không thành công
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
    }

}
