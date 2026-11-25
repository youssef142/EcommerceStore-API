using EcommerceStore.Infrastructure;
using EcommerceStore.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EcommerceStore.Services
{
    /// <summary>
    /// SMTP sender. All settings come from configuration (section "Smtp") - nothing is hard-coded.
    /// In Development, if SMTP isn't configured the email is logged instead of sent.
    /// </summary>
    public class EmailSender : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly IHostEnvironment _env;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<SmtpSettings> settings, IHostEnvironment env, ILogger<EmailSender> logger)
        {
            _settings = settings.Value;
            _env = env;
            _logger = logger;
        }

        public async Task SendAsync(
            string to,
            string subject,
            string htmlBody,
            string? replyToEmail = null,
            string? replyToName = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.Username) || string.IsNullOrWhiteSpace(_settings.Password))
            {
                if (_env.IsDevelopment())
                {
                    _logger.LogWarning(
                        "SMTP is not configured - email NOT sent. To: {To} | Subject: {Subject} | Body: {Body}",
                        to, subject, htmlBody);
                    return;
                }

                throw new InvalidOperationException("SMTP is not configured (Smtp:Username / Smtp:Password).");
            }

            var fromEmail = string.IsNullOrWhiteSpace(_settings.FromEmail) ? _settings.Username : _settings.FromEmail;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(to));
            if (!string.IsNullOrWhiteSpace(replyToEmail))
                message.ReplyTo.Add(new MailboxAddress(replyToName ?? replyToEmail, replyToEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, cancellationToken);
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            await smtp.SendAsync(new SmtpSendRequest(message), cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent to {To}", to);
        }
    }
}
