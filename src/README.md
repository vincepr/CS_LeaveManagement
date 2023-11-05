# Project HR.LeaveManagement - Clean Architecture
- ASP.NET Core - Clean Architecture - by Trevoir Williams on youtube: https://www.youtube.com/watch?v=gGa7SLk1-0Q&list=PLUl9BcvgsrKa4mR2sJyGuAAGSos_-daYC&index=20

## MediaR

### the Mediator Pattern using "MediaR-package"
- allows to separate the **RequestHandler** from the **Request** itself 
```csharp
using MediaR;
public class CreateLeaveRequestCommand : IRequest<int>
{
    public CreateLeaveRequestDto CreateLeaveRequestDto{ get; set; }
}

```
```csharp
using MediaR;
public class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand, int>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IMapper _mapper;

    public CreateLeaveRequestCommandHandler(ILeaveRequestRepository leaveRequestRepository, IMapper mapper)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _mapper = mapper;
    }
    
    public async Task<int> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var newLeaveRequest = _mapper.Map<LeaveRequest>(request.CreateLeaveRequestDto);
        return (await _leaveRequestRepository.Add(newLeaveRequest)).Id;
    }
}

```

- it is possible to filter for different combinations/states the request might be in:

```csharp
using MediaR;
public class UpdateLeaveRequestCommand : IRequest<Unit>
{
    public int Id { get; set; }
    
    // here we have two possible States, we either have a full update of a LeaveRequestDto
    public UpdateLeaveRequestDto? UpdateLeaveRequestDto { get; set; }
    
    // or we just receive a change for the approval - bool:
    public ChangeLeaveRequestApprovalDto? ChangeLeaveRequestApprovalDto { get; set; }
    
    // -> it's the IRequestHandler's job to map those to the corresponding action.
}

```

```csharp
using MediaR;
public class UpdateLeaveRequestCommandHandler : IRequestHandler<UpdateLeaveRequestCommand, Unit>
{   
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IMapper _mapper;

    public UpdateLeaveRequestCommandHandler(ILeaveRequestRepository leaveRequestRepository, IMapper mapper)
    {
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
```

## Data Validation - using FluentValidation-package
- keeping Data always in a valid state.
- could use data annotation etc. but in this case solved with the FluentValidation-package
- `FluentValidation.DependencyInjectionExtensions`


We only need validators for the Requests that modify data. For example the update/create requests:

- `DTOs/LeaveType/Validators/CreateLeaveTypeDtoValidator.cs`
```csharp
public class CreateLeaveTypeDtoValidator : AbstractValidator<CreateLeaveTypeDto>
{
    public CreateLeaveTypeDtoValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .MaximumLength(50).WithMessage("{PropertyName} must not exceed 50 characters.");

        RuleFor(p => p.DefaultDays)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .GreaterThan(0).WithMessage("{PropertyName} must be bigger than 0.")
            .LessThan(100).WithMessage("{PropertyName} must be less than 100.");
    }
}
```
- in the corresponding handler `Features/LeaveTypes/Handlers/CommandsCreateLeaveTypeCommandHandler.cs` we add the validation checking
```csharp
public class CreateLeaveTypeCommandHandler : IRequestHandler<CreateLeaveTypeCommand, int>
{
    // ... 
    public async Task<int> Handle(CreateLeaveTypeCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateLeaveTypeDtoValidator();
        var validationResult = await validator.ValidateAsync(request.LeaveTypeDto, CancellationToken.None);
        if (validationResult.IsValid == false)
            throw new Exception();
        
        var newLeaveType = _mapper.Map<LeaveType>(request.LeaveTypeDto);
        return (await _leaveTypeRepository.Add(newLeaveType)).Id;
    }
}
```

Note about my thoughts on this approach. 
- Seems to generate a lot of (is it really unnecessary?) code duplication
- Instead of validating each Dto by itself why not validate on the Model-level?
  - this would lead to way less "GreaterThan(0)" as it would happen after the Dto to corresponding Model.