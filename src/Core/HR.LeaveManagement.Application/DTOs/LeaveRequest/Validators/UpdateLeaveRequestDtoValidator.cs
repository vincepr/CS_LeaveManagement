using FluentValidation;
using HR.LeaveManagement.Application.Contracts.Persistance;

namespace HR.LeaveManagement.Application.DTOs.LeaveRequest.Validators;

public class UpdateLeaveRequestDtoValidator : AbstractValidator<UpdateLeaveRequestDto>
{
    public UpdateLeaveRequestDtoValidator(ILeaveTypeRepository leaveTypeRepository)
    {
        Include(new IleaveRequestDtoValidator(leaveTypeRepository));
        RuleFor(p => p.Id).NotNull().WithMessage("{PropertyName} must be present.");
    }
}