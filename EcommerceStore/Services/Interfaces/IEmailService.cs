namespace EcommerceStore.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(
            string to,
            string subject,
            string htmlBody,
            string? replyToEmail = null,
            string? replyToName = null,
            CancellationToken cancellationToken = default);
    }
}
