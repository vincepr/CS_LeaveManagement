using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace HR.LeaveManagement.Application;

public static class ApplicationServicesRegistration
{
    public static IServiceCollection ConfigureapplicatinSErvices(this IServiceCollection services)
    {
        // instead of:
        // // services.AddAutoMapper(typeof(MappingProfile));
        // we use:
        services.AddAutoMapper(System.Reflection.Assembly.GetExecutingAssembly());
        // this will traverse and automatically add every Mapping Profile that inherits : Profile 

        services.AddMediatR(System.Reflection.Assembly.GetExecutingAssembly());

        return services;
    }
}