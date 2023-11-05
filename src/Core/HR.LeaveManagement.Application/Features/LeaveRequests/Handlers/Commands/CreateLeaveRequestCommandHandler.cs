using AutoMapper;
using HR.LeaveManagement.Application.DTOs.Exceptions;
using HR.LeaveManagement.Application.DTOs.LeaveRequest.Validators;
using HR.LeaveManagement.Application.Features.LeaveRequests.Requests.Commands;
using HR.LeaveManagement.Application.Persistence.Contracts;
using HR.LeaveManagement.Domain;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveRequests.Handlers.Commands;

public class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand, int>
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
    
    public async Task<int> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateLeaveRequestDtoValidator(_leaveTypeRepository);
        var validationResult = await validator.ValidateAsync(request.CreateLeaveRequestDto, CancellationToken.None);
        if (validationResult.IsValid == false)
            throw new ValidationException(validationResult); 
        
        var newLeaveRequest = _mapper.Map<LeaveRequest>(request.CreateLeaveRequestDto);
        return (await _leaveRequestRepository.Add(newLeaveRequest)).Id;
    }
}