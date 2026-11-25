using System.Net;
using EcommerceStore.Dtos;
using EcommerceStore.Infrastructure;
using EcommerceStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcommerceStore.Controllers
{
    [Route("api/contact")]
    public class ContactController : ApiControllerBase
    {
        private readonly IEmailService _email;
        private readonly IConfiguration _config;
        private readonly ILogger<ContactController> _logger;

        public ContactController(IEmailService email, IConfiguration config, ILogger<ContactController> logger)
        {
            _email = email;
            _config = config;
            _logger = logger;
        }

        [HttpPost]
        [EnableRateLimiting("contact")]
        public async Task<IActionResult> Send(ContactRequest request, CancellationToken cancellationToken)
        {
            var supportEmail = _config["Contact:SupportEmail"];
            if (string.IsNullOrWhiteSpace(supportEmail))
                throw new InvalidOperationException("Contact:SupportEmail is not configured.");

            // Strip line breaks from the subject (header injection) and HTML-encode everything user-supplied.
            var subject = "[Contact] " + request.Subject.ReplaceLineEndings(" ");
            var body =
                $"<p><strong>From:</strong> {WebUtility.HtmlEncode(request.Name)} &lt;{WebUtility.HtmlEncode(request.Email)}&gt;</p>" +
                $"<p>{WebUtility.HtmlEncode(request.Message).ReplaceLineEndings("<br>")}</p>";

            await _email.SendAsync(supportEmail, subject, body, request.Email, request.Name, cancellationToken);

            _logger.LogInformation("Contact form submitted by {Email}", request.Email);
            return Ok(new { message = "Thank you for contacting us! We'll get back to you soon." });
        }
    }
}
