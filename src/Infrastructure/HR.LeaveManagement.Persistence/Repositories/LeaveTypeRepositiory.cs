using HR.LeaveManagement.Application.Contracts.Persistance;
using HR.LeaveManagement.Domain;

namespace HR.LeaveManagement.Persistence.Repositories;

public class LeaveTypeRepositiory : GenericRepository<LeaveType>, ILeaveTypeRepository
{
    public LeaveTypeRepositiory(HrDbContext dbContext) : base(dbContext)
    {
    }
}