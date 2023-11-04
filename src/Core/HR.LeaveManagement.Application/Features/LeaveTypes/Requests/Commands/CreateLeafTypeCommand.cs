using HR.LeaveManagement.Application.DTOs.LeaveType;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveTypes.Requests.Commands;

public class CreateLeafTypeCommand : IRequest<int>
{
    public LeaveTypeDto LeaveTypeDto { get; set; }
}