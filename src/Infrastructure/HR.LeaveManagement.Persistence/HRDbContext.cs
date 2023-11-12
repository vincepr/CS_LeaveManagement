using HR.LeaveManagement.Domain;
using HR.LeaveManagement.Domain.Common;
using HR.LeaveManagement.Persistence.Configuration.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.LeaveManagement.Persistence;

public class HrDbContext : DbContext
{
    public HrDbContext(DbContextOptions<HrDbContext> options)
        : base(options) { }
    
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<LeaveType> LeaveTypes { get; set; }
    public DbSet<LeaveAllocation> LeaveAllocations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrDbContext).Assembly);
        // again the typeof(HrDbContext).Assembly catches all the IEntityTypeConfiguration<...>
        // so we dont have to model.Builder.ApplyConfiguration(new LeaveTypeConfiguration()); for each one
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var item in ChangeTracker.Entries<BaseDomainEntity>())
        {
            // on every change we want to automatically set LastModifiedDate
            item.Entity.LastModifiedDate = DateTime.Now;
            
            // if we newly create the entry we want ot set DateCreate
            if (item.State == EntityState.Added)
            {
                item.Entity.CreatedBy = "current-user";
                item.Entity.DateCreated = DateTime.Now;
            }
            item.Entity.LastModifiedBy = "current-user";

        }
        // afterwards we just call the base SaveChangesAsync()
        return base.SaveChangesAsync(cancellationToken);
    }
}