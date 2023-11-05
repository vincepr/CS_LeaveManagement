using AutoMapper;
using HR.LeaveManagement.Application.DTOs.LeaveRequest.Validators;
using HR.LeaveManagement.Application.Features.LeaveRequests.Requests.Commands;
using HR.LeaveManagement.Application.Persistence.Contracts;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveRequests.Handlers.Commands;

public class UpdateLeaveRequestCommandHandler : IRequestHandler<UpdateLeaveRequestCommand, Unit>
{   
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IMapper _mapper;
    private readonly ILeaveTypeRepository _leaveTypeRepository;

    public UpdateLeaveRequestCommandHandler(ILeaveRequestRepository leaveRequestRepository, ILeaveTypeRepository leaveTypeRepository, IMapper mapper)
    {
        _leaveTypeRepository = leaveTypeRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _mapper = mapper;
    }

    // here we have two possible States, we either have a full update of a LeaveRequestDto
    // or we just receive a change for the approval - bool:
    // -> it's the IRequestHandler's job to map those to the corresponding action.
    public async Task<Unit> Handle(UpdateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await _leaveRequestRepository.Get(request.Id);
        if (request.UpdateLeaveRequestDto != null)
        {
            var validator = new UpdateLeaveRequestDtoValidator(_leaveTypeRepository);
            var valdiationResult = await validator.ValidateAsync(request.UpdateLeaveRequestDto, CancellationToken.None);
            if (valdiationResult.IsValid == false) throw new Exception();
            
            _mapper.Map(request.UpdateLeaveRequestDto, leaveRequest);
            await _leaveRequestRepository.Update(leaveRequest); 
        } 
        else if (request.ChangeLeaveRequestApprovalDto != null)
        {
            await _leaveRequestRepository.ChangeApprovalStatus(leaveRequest, request.ChangeLeaveRequestApprovalDto.Approved);
        }
        return Unit.Value;
    }
}