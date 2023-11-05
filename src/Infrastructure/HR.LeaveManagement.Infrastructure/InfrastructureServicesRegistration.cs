using HR.LeaveManagement.Application.Contracts.Infrastructure;
using HR.LeaveManagement.Application.Models.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HR.LeaveManagement.Infrastructure;

public static class InfrastructureServicesRegistration
{
    public static IServiceCollection ConfigureInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // whenever asked for EmailSettings the data from our Appsettings.json "EmailSettings" should be used
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        
        // Every request should get it's own EmailSender-instance -> transient.
        services.AddTransient<IEmailSender, EmailSender>();
        return services;
    }
}