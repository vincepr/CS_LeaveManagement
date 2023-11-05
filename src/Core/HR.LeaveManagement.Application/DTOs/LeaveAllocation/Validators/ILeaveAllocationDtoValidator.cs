using FluentValidation;
using HR.LeaveManagement.Application.Contracts.Persistance;

namespace HR.LeaveManagement.Application.DTOs.LeaveAllocation.Validators;

public class ILeaveAllocationDtoValidator : AbstractValidator<ILeaveAllocationDto>
{
    public ILeaveAllocationDtoValidator(ILeaveTypeRepository leaveTypeRepository)
    {
        RuleFor(p => p.NumberOfDays)
            .GreaterThan(0)
            .WithMessage("{PropertyName} must be greater than {ComparsionValue}");

        RuleFor(p => p.Period)
            .GreaterThanOrEqualTo(DateTime.Now.Year)
            .WithMessage("{PropertyName}must be after current Year.");

        RuleFor(p => p.LeaveTypeId)
            .GreaterThan(0)
            .MustAsync(async (id, token) =>
            {
                var leaveTypeExists = await leaveTypeRepository.Exists(id);
                return leaveTypeExists;
            })
            .WithMessage("{PropertyName} does not exist");
    }
}