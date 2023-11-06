using HR.LeaveManagement.Application.DTOs;
using HR.LeaveManagement.Application.DTOs.LeaveAllocation;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveAllocations.Requests.Queries;

public class GetAllLeaveAllocationListRequest : IRequest<List<LeaveAllocationDto>>, IRequest<LeaveAllocationDto>
{
    
}