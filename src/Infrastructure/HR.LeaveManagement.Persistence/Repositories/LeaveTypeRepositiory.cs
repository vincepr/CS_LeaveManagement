using HR.LeaveManagement.Application.Persistence.Contracts;
using HR.LeaveManagement.Domain;

namespace HR.LeaveManagement.Persistence.Repositories;

public class LeaveTypeRepositiory : GenericRepository<LeaveType>, ILeaveTypeRepository
{
    public LeaveTypeRepositiory(HrDbContext dbContext) : base(dbContext)
    {
    }
}