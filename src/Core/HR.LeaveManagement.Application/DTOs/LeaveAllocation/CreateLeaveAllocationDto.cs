using HR.LeaveManagement.Application.DTOs.Common;

namespace HR.LeaveManagement.Application.DTOs.LeaveAllocation;

public class CreateLeaveAllocationDto 
{
    public required int NumberOfDays { get; set; }
    public required int LeaveTypeId { get; set; }
    public required int Period { get; set; }
}