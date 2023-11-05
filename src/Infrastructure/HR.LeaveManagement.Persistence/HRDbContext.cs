using HR.LeaveManagement.Domain;
using HR.LeaveManagement.Domain.Common;
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
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var item in ChangeTracker.Entries<BaseDomainEntity>())
        {
            // on every change we want to automatically set LastModifiedDate
            item.Entity.LastModifiedDate = DateTime.Now;
            
            // if we newly create the entry we want ot set DateCreate
            if (item.State == EntityState.Added)
                item.Entity.DateCreated = DateTime.Now;
        }
        // afterwards we just call the base SaveChangesAsync()
        return base.SaveChangesAsync(cancellationToken);
    }
}