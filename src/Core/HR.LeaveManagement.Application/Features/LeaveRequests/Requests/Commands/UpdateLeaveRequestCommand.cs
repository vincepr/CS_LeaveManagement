using HR.LeaveManagement.Application.DTOs.LeaveRequest;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveRequests.Requests.Commands;

public class UpdateLeaveRequestCommand : IRequest<Unit>
{
    public int Id { get; set; }
    
    // here we have two possible States, we either have a full update of a LeaveRequestDto
    public UpdateLeaveRequestDto? UpdateLeaveRequestDto { get; set; }
    
    // or we just receive a change for the approval - bool:
    public ChangeLeaveRequestApprovalDto? ChangeLeaveRequestApprovalDto { get; set; }
    
    // -> it's the IRequestHandler's job to map those to the corresponding action.
}