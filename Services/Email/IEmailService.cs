namespace Services.Email
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string name, string email, string phone, string subject, string message);
    }
}