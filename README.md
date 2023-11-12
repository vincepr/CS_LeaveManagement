# Project HR.LeaveManagement - Clean Architecture
- ASP.NET Core - Clean Architecture - by Trevoir Williams on youtube: https://www.youtube.com/watch?v=gGa7SLk1-0Q&list=PLUl9BcvgsrKa4mR2sJyGuAAGSos_-daYC&index=20
## Project structure
- HR.LeaveManagement.Application - all dtos/handlers
- HR.LeaveManagement.Domain - all of the models/entities
- HR.LeaveManagement.Infrastructure - all implementations for third party services
- HR.LeaveManagement.Persistence - repository/db related things

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
- more complex validation is possible, for example checking if a reference-type id exists in the corresponding table
```csharp
public class IleaveRequestDtoValidator : AbstractValidator<ILeaveRequestDto>
{
    private readonly ILeaveTypeRepository _leaveTypeRepository;

    public IleaveRequestDtoValidator(ILeaveTypeRepository leaveTyperepository)
    {
        _leaveTypeRepository = leaveTyperepository;
        RuleFor(p => p.StartDate)
            .LessThan(p => p.EndDate)
            .WithMessage("{PropertyName} must be before {CpmarisonValue}");
        
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
```

Note about my thoughts on this approach. 
- Seems to generate a lot of (is it really unnecessary?) code duplication
- Instead of validating each Dto by itself why not validate on the Model-level?
  - this would lead to way less "GreaterThan(0)" as it would happen after the Dto to corresponding Model.

## Cusom Exceptions - to enable cleaner error handling/filtering

- `DTOs/Exceptions/BadRequestException.cs`
```csharp
public class BadRequestException : ApplicationException
{
    public BadRequestException(string message) : base(message) { }
}

```


- `DTOs/Exceptions/NotFoundException.cs`
```csharp
public class NotFoundException : ApplicationException
{
    public NotFoundException(string name, object key) 
        : base($"{name} ({key}) was not found.") { }
}

```

- `DTOs/Exceptions/ValdationException.cs`
```csharp
public class ValidationException : ApplicationException
{
    public List<string> Errors { get; set; } = new();
    public ValidationException(FluentValidation.Results.ValidationResult validationResult)
    {
        foreach( var error in validationResult.Errors)
            Errors.Add(error.ErrorMessage);
    } 
}
```

- now we can use our custom errors instead of generic ones, for example when validating:
```csharp
// in our create/update handlers validation errors change from:
if (validationResult.IsValid == false)
    throw new Exception();
// to
if (validationResult.IsValid == false)
    throw new ValidationException(validationResult);

// in our delete handlers we add:
var leaveAllocation = await _leaveAllocationRepository.Get(request.Id);
if (leaveAllocation is null)
    throw new NotFoundException(nameof(leaveAllocation), request.Id);
```
### Custom Response Types
While here there is little reason to do so, it is possible create a Base/Individual Response for each Query.

That basically wraps information we want to return, like a collection of errors, etc...

- `Responses/BasecommandResponse.cs`
```csharp
public class BaseCommandResponse
{
    public int Id { get; set; } 
    public bool Success { get; set; } = true;
    public string Message { get; set; }
    public List<string> Errors { get; set; } = new();
}
```

- `DTOs/LeaveRequests/Handlers/commands/CreateLeaveRequestHandler.cs`
```csharp
// previously:
public async Task<int> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
{
    var validator = new CreateLeaveRequestDtoValidator(_leaveTypeRepository);
    var validationResult = await validator.ValidateAsync(request.CreateLeaveRequestDto, CancellationToken.None);
    if (validationResult.IsValid == false)
        throw new ValidationException(validationResult); 
    
    var newLeaveRequest = _mapper.Map<LeaveRequest>(request.CreateLeaveRequestDto);
    return (await _leaveRequestRepository.Add(newLeaveRequest)).Id;
}

// becomes:
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
```


## EntityFramework
- in our DbContext we override SaveChangesAsync to automatically update Timestamps for createAt and modifiedAt
```csharp
public class HrDbContext : DbContext
{
    public HrDbContext(DbContextOptions<HrDbContext> options)
        : base(options) { }
    
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<LeaveType> LeaveTypes { get; set; }
    public DbSet<LeaveAllocation> LeaveAllocations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var item in ChangeTracker.Entries<BaseDomainEntity>())
        {
            // on every change we want to automatically set LastModifiedDate
            item.Entity.LastModifiedDate = DateTime.Now;
            
            // if we newly create the entry we want ot set DateCreate
            if (item.State == EntityState.Added)
                item.Entity.DateCreated = DateTime.Now;
        }
        // afterwards we just call the base SaveChangesAsync()
        return base.SaveChangesAsync(cancellationToken);
    }
}
```
- we create a implementations of the Repositories. First the defaultOne: `HR.Persistance/Repositories/GenericRepository.cs` 
- this generic implementation will compile down for all of our 3 Types supplied by the HrDbContext
```csharp
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly HrDbContext _dbContext;

    protected GenericRepository(HrDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<T?> Get(int id)
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }

    public async Task<IReadOnlyList<T>> GetAll()
    {
        return await _dbContext.Set<T>().ToListAsync();
    }

    public async Task<T> Add(T entity)
    {
        await _dbContext.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task Update(T entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }

    public async Task Delete(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> Exists(int id)
    {
        var entity = await Get(id);
        return entity is not null;
    }
}
```
- And ontop of this always supported base repository we can add our specific functionality:
```csharp
public class LeaveRequestRepository : GenericRepository<LeaveRequest>, ILeaveRequestRepository
{
    private readonly HrDbContext _dbContext;
    public LeaveRequestRepository(HrDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LeaveRequest?> GetLeaveRequestWithDetails(int id)
    {
        var leaveRequest = await _dbContext
            .LeaveRequests
            .Include(l => l.LeaveType)
            .FirstOrDefaultAsync(q => q.Id == id);
        return leaveRequest;
    }

    public async Task<List<LeaveRequest>> GetAllLeaveRequestsWithDetails()
    {
        var leaveRequests = await _dbContext
            .LeaveRequests
            .Include(l => l.LeaveType)
            .ToListAsync();
        return leaveRequests;
    }

    public async Task ChangeApprovalStatus(LeaveRequest leaveRequest, bool? approvalStatus)
    {
        leaveRequest.Approved = approvalStatus;
        _dbContext.Entry(leaveRequest).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();
    }
}
```

- to make Setting up all dependency injections easy we make a static function that handles all that at once:
```csharp
public static class PersistenceServicesRegistration
{
    public static IServiceCollection ConfigurePersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<HrDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("LeaveManagementConnectionString"));
        });
        
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        
        services.AddScoped<ILeaveAllocationRepository, LeaveAllocationRepository>();
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<ILeaveTypeRepository, LeaveTypeRepositiory>();

        return services;
    }
}
```