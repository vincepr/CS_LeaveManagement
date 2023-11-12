using HR.LeaveManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.LeaveManagement.Persistence.Configuration.Entities;

public class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.HasData(
            new LeaveType()
            {
                Id = 1,
                DefaultDays = 10,
                Name = "Vacation",
                CreatedBy = "root-user",
                LastModifiedBy = "root-user",
            },
            new LeaveType
            {
                Id = 2,
                DefaultDays = 0,
                Name = "Sick",
                CreatedBy = "root-user",
                LastModifiedBy = "root-user",
            });
    }
}