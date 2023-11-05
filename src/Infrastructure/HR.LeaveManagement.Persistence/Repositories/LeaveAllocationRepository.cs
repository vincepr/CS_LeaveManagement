using HR.LeaveManagement.Application.Persistence.Contracts;
using HR.LeaveManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace HR.LeaveManagement.Persistence.Repositories;

public class LeaveAllocationRepository : GenericRepository<LeaveAllocation>, ILeaveAllocationRepository
{
    private readonly HrDbContext _dbContext;
    protected LeaveAllocationRepository(HrDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LeaveAllocation?> GetLeaveAllocationWithDetails(int id)
    {
        var leaveAllocation = await _dbContext.LeaveAllocations
            .Include(la => la.LeaveType)
            .FirstOrDefaultAsync();
        return leaveAllocation;
    }

    public async Task<List<LeaveAllocation>> GetLeaveAllocationsWithDetails()
    {
        return await _dbContext.LeaveAllocations
            .Include(la => la.LeaveType)
            .ToListAsync();
    }
}