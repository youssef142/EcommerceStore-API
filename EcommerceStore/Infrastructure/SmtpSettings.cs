namespace EcommerceStore.Infrastructure
{
    /// <summary>Bound from the "Smtp" config section. Keep Username/Password in user-secrets or env vars.</summary>
    public class SmtpSettings
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "E-Commerce Store";
    }
}
