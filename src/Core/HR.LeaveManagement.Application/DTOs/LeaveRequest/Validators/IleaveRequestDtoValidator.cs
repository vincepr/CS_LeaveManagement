using FluentValidation;
using HR.LeaveManagement.Application.Persistence.Contracts;

namespace HR.LeaveManagement.Application.DTOs.LeaveRequest.Validators;

public class IleaveRequestDtoValidator : AbstractValidator<ILeaveRequestDto>
{
    private readonly ILeaveTypeRepository _leaveTypeRepository;

    public IleaveRequestDtoValidator(ILeaveTypeRepository leaveTyperepository)
    {
        _leaveTypeRepository = leaveTyperepository;
        RuleFor(p => p.StartDate)
            .LessThan(p => p.EndDate)
            .WithMessage("{PropertyName} must be before {CpmarisonValue}");
        
        RuleFor(p => p.EndDate)
            .GreaterThan(p => p.StartDate)
            .WithMessage("{PropertyName} must be after {ComparisonValue}");

        RuleFor(p => p.StartDate)
            .GreaterThanOrEqualTo(DateTime.Now)
            .WithMessage("{PropertyName} must be in the future.");
        
        RuleFor(p => p.LeaveTypeId)
            .GreaterThan(0)
            // the referenced leaveType MUST exists:
            .MustAsync(async (id, token) =>
            {
                var leaveTypeExists = await _leaveTypeRepository.Exists(id);
                return !leaveTypeExists;
            }).WithMessage("{PropertyName} does not exist.");
    } 
}