using Services.Email;

namespace Services
{
    public static class Extensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration )
        {
            services.AddScoped<IEmailService, EmailService>();

            services.Configure<EmailConfiguration>(configuration.GetSection("mailConfiguration"));

            return services;
        }
    }
}