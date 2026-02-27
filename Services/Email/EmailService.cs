using System.Collections.Concurrent;
using System.Net;
using System.Net.Mail;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

namespace Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly RateLimiter _rateLimiter;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _userSemaphores = new();

        public EmailService(IOptions<EmailConfiguration> emailConfiguration)
        {
            _emailConfig = emailConfiguration.Value ?? throw new ArgumentNullException(nameof(emailConfiguration));
            
            _rateLimiter = new TokenBucketRateLimiter(
                new TokenBucketRateLimiterOptions
                {
                    QueueLimit = 100,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                    TokensPerPeriod = 10,
                    TokenLimit = 10,
                    AutoReplenishment = true
                });
        }

        public async Task<bool> SendEmailAsync(string name, string email, string phone, string subject, string message)
        {
            var semaphore = _userSemaphores.GetOrAdd(email, _ => new SemaphoreSlim(1, 1));
            
            await semaphore.WaitAsync();

            try
            {
                using var lease = await _rateLimiter.AcquireAsync(1);

                if (!lease.IsAcquired)
                {
                    return false;
                }

                using var client = new SmtpClient(_emailConfig.SmtpServer, _emailConfig.Port)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_emailConfig.Username, _emailConfig.Password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailConfig.From),
                    Subject = subject,
                    Body = $"<p><strong>Namn:</strong> {name}</p>" +
                        $"<p><strong>Email:</strong> {email}</p>" +
                        $"<p><strong>Telefon:</strong> {phone}</p>" +
                        $"<br>" +
                        $"<p><strong>Meddelande:</strong></p>" +
                        $"<p>{message}</p>",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(_emailConfig.To);

                //await HandleAttachement(attachments, mailMessage);

                await client.SendMailAsync(mailMessage);
                
                await Task.Delay(1000);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                
                return false;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}