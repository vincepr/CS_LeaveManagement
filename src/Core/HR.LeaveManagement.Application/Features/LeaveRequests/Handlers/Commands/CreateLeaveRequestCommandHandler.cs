using AutoMapper;
using HR.LeaveManagement.Application.DTOs.Exceptions;
using HR.LeaveManagement.Application.DTOs.LeaveRequest.Validators;
using HR.LeaveManagement.Application.Features.LeaveRequests.Requests.Commands;
using HR.LeaveManagement.Application.Persistence.Contracts;
using HR.LeaveManagement.Application.Responses;
using HR.LeaveManagement.Domain;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveRequests.Handlers.Commands;

public class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand, BaseCommandResponse>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IMapper _mapper;
    private readonly ILeaveTypeRepository _leaveTypeRepository;

    public CreateLeaveRequestCommandHandler(ILeaveRequestRepository leaveRequestRepository, ILeaveTypeRepository leaveTypeRepository, IMapper mapper)
    {
        _leaveTypeRepository = leaveTypeRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _mapper = mapper;
    }
    
    public async Task<BaseCommandResponse> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();
        var validator = new CreateLeaveRequestDtoValidator(_leaveTypeRepository);
        var validationResult = await validator.ValidateAsync(request.CreateLeaveRequestDto, CancellationToken.None);
        
        if (validationResult.IsValid == false)
        {
            response.Success = false;
            response.Message = "Creation has failed";
            response.Errors = validationResult.Errors.Select(el => el.ErrorMessage).ToList();
            return response;
        }
        
        var newLeaveRequest = _mapper.Map<LeaveRequest>(request.CreateLeaveRequestDto);
        response.Id = (await _leaveRequestRepository.Add(newLeaveRequest)).Id;
        response.Message = "Creation successful";
        return response;
    }
}