using HR.LeaveManagement.Application.Contracts.Persistance;
using HR.LeaveManagement.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HR.LeaveManagement.Persistence;

public static class PersistenceServicesRegistration
{
    public static IServiceCollection ConfigurePersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<HrDbContext>(options =>
        {
            if ("Development" == Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") )
            {
                options.UseSqlite("Data Source = ./hr_sqlite");
                Console.WriteLine("PersistenceServicesRegistration.cs -> USING SQLITE while in Development");
            }
            else
            {
                options.UseSqlServer(configuration.GetConnectionString("LeaveManagementConnectionString"));
                Console.WriteLine("PersistenceServicesRegistration.cs -> USING SqlServer while in Production");
            }
        });
        
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        
        services.AddScoped<ILeaveAllocationRepository, LeaveAllocationRepository>();
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<ILeaveTypeRepository, LeaveTypeRepositiory>();

        return services;
    }
}