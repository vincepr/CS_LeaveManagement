# Notes

## the Mediator Pattern using "MediaR-package"
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