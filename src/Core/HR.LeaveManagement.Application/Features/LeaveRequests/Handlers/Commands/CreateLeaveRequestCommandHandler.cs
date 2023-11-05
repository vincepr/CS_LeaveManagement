using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Infrastructure;
using HR.LeaveManagement.Application.Contracts.Persistance;
using HR.LeaveManagement.Application.DTOs.Exceptions;
using HR.LeaveManagement.Application.DTOs.LeaveRequest.Validators;
using HR.LeaveManagement.Application.Features.LeaveRequests.Requests.Commands;
using HR.LeaveManagement.Application.Models.Email;
using HR.LeaveManagement.Application.Responses;
using HR.LeaveManagement.Domain;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveRequests.Handlers.Commands;

public class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand, BaseCommandResponse>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IMapper _mapper;
    private readonly ILeaveTypeRepository _leaveTypeRepository;
    private readonly IEmailSender _emailSender;

    public CreateLeaveRequestCommandHandler(
        ILeaveRequestRepository leaveRequestRepository,
        ILeaveTypeRepository leaveTypeRepository,
        IEmailSender emailSender,
        IMapper mapper)
    {
        _emailSender = emailSender;
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
        await SendMailResponse(request);
        return response;
    }

    private async Task SendMailResponse(CreateLeaveRequestCommand request)
    {
        var email = new Email
        {
            To = "employee@org.com",
            // :D inside the string interpolation to specify the long date formating
            Body = $"Your leave request for {request.CreateLeaveRequestDto.StartDate:D} to {request.CreateLeaveRequestDto.EndDate}" +
                $"has been submitted successfully.",
            Subject = "Leave Request Submitted"
        };
        try
        {
            await _emailSender.SendEmail(email);
        }
        catch (Exception e)
        {
            // log error
        }
    }
}