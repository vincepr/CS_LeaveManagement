using HR.LeaveManagement.Application.DTOs.LeaveType;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveTypes.Requests.Commands;

public class UpdateLeaveTypeCommand : IRequest<Unit>, IRequest<int>
{
    public LeaveTypeDto LeaveTypeDto { get; set; }
}