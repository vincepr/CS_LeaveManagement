using FluentValidation;

namespace HR.LeaveManagement.Application.DTOs.LeaveAllocation.Validators;

public class CreateLeaveAllocationDtoValidator : AbstractValidator<CreateLeaveAllocationDto> 
{
    public CreateLeaveAllocationDtoValidator()
    {
        RuleFor(p => p.NumberOfDays)
            .GreaterThan(0).WithMessage("{PropertyName} must be bigger than 0.");

        RuleFor(p => p.LeaveTypeId)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} must be bigger than -1.");
    }
}