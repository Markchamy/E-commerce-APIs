using Backend.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

[Route("admin/api/2024-01")]
[ApiController]
public class JobApplicationController : ControllerBase
{
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitJobApplication([FromForm] JobApplicationModel application)
    {
        // Build the email message
        var emailMessage = $"New Job Application:\n\n" +
                           $"First Name: {application.FirstName}\n" +
                           $"Last Name: {application.LastName}\n" +
                           $"Email: {application.Email}\n" +
                           $"Phone Number: {application.PhoneNumber}\n" +
                           $"Mode of Transportation: {application.TransportationMode}\n" +
                           $"Home Address: {application.HomeAddress}\n" +
                           $"Cover Letter: {application.CoverLetter ?? "N/A"}";

        try
        {
            // Send email
            await SendEmail(application.Email, application.FirstName, application.LastName, emailMessage, application.Resume);

            return Ok("Application submitted successfully!");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to send email: {ex.Message}");
        }
    }
    private async Task SendEmail(string userEmail, string firstName, string lastName, string message, IFormFile? resume)
    {
        try
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("zouxkhoueiry12213@gmail.com", "fesa wscw qylu vqex"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("zouxkhoueiry12213@gmail.com"),
                Subject = $"New Job Application from {firstName} {lastName}",
                Body = message,
                IsBodyHtml = false,
            };

            // Send to admin or HR email (you can change this)
            mailMessage.To.Add("joseph.elkhoueiry@akikis.com");

            // Set user's email as "Reply-To", so replies go directly to the applicant
            mailMessage.ReplyToList.Add(new MailAddress(userEmail));

            // Attach resume if present
            if (resume != null)
            {
                var attachment = new Attachment(resume.OpenReadStream(), resume.FileName);
                mailMessage.Attachments.Add(attachment);
            }

            // Send the email
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (SmtpException smtpEx)
        {
            var detailedMessage = smtpEx.InnerException?.Message ?? smtpEx.Message;
            throw new Exception($"Failed to send email: {detailedMessage}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to send email: {ex.Message}");
        }
    }

}
