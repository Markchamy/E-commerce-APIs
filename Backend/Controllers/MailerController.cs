using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [Route("admin/api/2024-01")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] MailerModel mailerModel)
        {
            // Validate incoming request data
            if (!ModelState.IsValid || string.IsNullOrEmpty(mailerModel.Email) || string.IsNullOrEmpty(mailerModel.Message))
            {
                return BadRequest("Email and message are required.");
            }

            try
            {
                // Build the email message content
                var emailMessage = $@"
                    <html>
                        <body style='font-family: Arial, sans-serif; color: #333;'>
                            <h2 style='color: #2c3e50;'>New Contact Form Message</h2>
                            <hr style='border-color: #ecf0f1;'>
                            <h4 style='color: #2980b9;'>Contact Details</h4>
                            <p><strong>Name:</strong> {mailerModel.Name ?? "N/A"}</p>
                            <p><strong>Email:</strong> {mailerModel.Email}</p>
                            <p><strong>Phone Number:</strong> {mailerModel.PhoneNumber ?? "N/A"}</p>
                            <hr style='border-color: #ecf0f1;'>
                            <h4 style='color: #2980b9;'>Message</h4>
                            <p>{mailerModel.Message}</p>
                            <hr style='border-color: #ecf0f1;'>
                        </body>
                    </html>";



                // Send email using SMTP
                await SendEmail(mailerModel.Email, emailMessage);

                // Return success response
                return Ok(new { message = "Your message has been sent successfully!" });
            }
            catch (Exception ex)
            {
                // Handle exceptions related to sending the email
                return StatusCode(500, $"Failed to send the message: {ex.Message}");
            }
        }

        // SMTP email sending method
        private async Task SendEmail(string userEmail, string message)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("zouxkhoueiry12213@gmail.com", "clwr plxw mbhn vabg"), // Replace with your email and app password
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("zouxkhoueiry12213@gmail.com"), // Your company's email
                Subject = "New Contact Form Submission",
                Body = message,
                IsBodyHtml = true,
            };

            // Send to the company's email
            mailMessage.To.Add("joseph.elkhoueiry@akikis.com"); // Replace with your company's contact email

            // Optionally add a BCC to yourself
            mailMessage.Bcc.Add("joseph.el.khoueiryy@gmail.com");

            // Send the email
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
