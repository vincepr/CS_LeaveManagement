# 2 Project HR.LeaveManagement - Clean Architecture
## External Services 
Using an external Email Service, to for example send automatic emails when a new LeaveRequest is created.

- we create an interface for our mail-sending-functionality
```csharp
public interface IEmailSender
{
    Task<bool> SendEmail(Email email);
}
```
- and then in our Handler for the CreateLeaveRequest just call this interface:
```csharp
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
```

the actual implementation gets it's own package, where we implement the used library etc...
```csharp
using SendGrid;
using SendGrid.Helpers.Mail;
public class EmailSender : IEmailSender
{
    private EmailSettings _emailSettings { get; }

    public EmailSender(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }
    
    public async Task<bool> SendEmail(Email email)
    {
        var client = new SendGridClient(_emailSettings.ApiKey);

        var to = new EmailAddress(email.To);
        var from = new EmailAddress
        {
            Email = _emailSettings.FromAdress,
            Name = _emailSettings.FromName,
        };
        var message = MailHelper.CreateSingleEmail(from, to, email.Subject, email.Body, email.Body);
        var response = await client.SendEmailAsync(message);

        return response.StatusCode == HttpStatusCode.Accepted ||
               response.StatusCode == HttpStatusCode.OK;
    }
}
```

## Create the Api Project
- hook up the Services:
```csharp
// default Services:
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// custom Services:
builder.Services.ConfigureApplicationServices();
builder.Services.ConfigureInfrastructureServices(builder.Configuration);
builder.Services.ConfigurePersistenceServices(builder.Configuration);
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("CorsPolicy",
//         corsBuilder => corsBuilder
//             .AllowAnyOrigin()
//             .AllowAnyMethod()
//             .AllowAnyHeader());
// });

// default app-settings
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
// custom app-settings:
// app.UseCors("CorsPolicy");

app.Run();
```
### Entity Framework when using a Class Library
Now we can generate migrations for the api project
- we want the migrations with the corresponding db-context/repositoires. `.Persistence` Project in this case. So we `--project PATH` to it
- we get an **ERROR** if we try to build migrations: `Unable to create an object of type '[DBContext's Name]'. For the different patterns supported at design time`
  - This is because EntityFramework needs to know about the db-connection string etc, that gets referenced in our `.Api`-Project.
  - To solve we can reference our "frontend"-Project with `--startup.project PATH`
```
dotnet ef --startup-project ./src/Api/HR.LeaveManagement.Api --project ./src/Infrastructure/HR.LeaveManagement.Persistence migrations add initialMigration
dotnet ef --startup-project ./src/Api/HR.LeaveManagement.Api --project ./src/Infrastructure/HR.LeaveManagement.Persistence database update
```

### Creating the 3 Controllers
- the 3 controllers are really similar, we just use the xzy_Request objects with our Mediator.  
```csharp
namespace HR.LeaveManagement.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class LeaveRequestController : ControllerBase
{
    private readonly IMediator _mediator;
    public LeaveRequestController(IMediator mediator)
    {
        _mediator = mediator;
    }
        
    // GET: api/LeaveRequest
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> Get()
    {
        var leaveRequests = await _mediator.Send(new GetLeaveRequestListRequest());
        return Ok(leaveRequests);
    }

    // GET: api/LeaveRequest/5
    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveRequestDto>> Get(int id)
    {
        return Ok(await _mediator.Send<LeaveRequestDto>(new GetLeaveRequestDetailRequest() { Id = id }));
    }

    // POST: api/LeaveRequest
    [HttpPost]
    public async Task<ActionResult> Post([FromBody] CreateLeaveRequestDto leaveRequest)
    {
        var cmd = new CreateLeaveRequestCommand() { CreateLeaveRequestDto = leaveRequest };
        return Ok(await _mediator.Send(cmd));
    }

    // PUT: api/LeaveRequest/5
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] UpdateLeaveRequestDto leaveRequest)
    {
        var cmd = new UpdateLeaveRequestCommand() { Id = id, UpdateLeaveRequestDto = leaveRequest };
        await _mediator.Send(cmd);
        return NoContent();
    }
        
    // PUT: api/LeaveRequest/change_approval     - changes to "approved"-status
    [HttpPut("change_approval/{id}")]
    public async Task<ActionResult> ChangeApproval(int id, [FromBody] ChangeLeaveRequestApprovalDto approvedRequest)
    {
        var cmd = new UpdateLeaveRequestCommand() {Id = id, ChangeLeaveRequestApprovalDto = approvedRequest };
        await _mediator.Send(cmd);
        return NoContent();
    }

    // DELETE: api/LeaveRequest/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var cmd = new DeleteLeaveRequestCommand() { Id = id };
        return Ok(await _mediator.Send(cmd));
    }
}
```

## Seeding the Database
- `HR.Persistence/Configuration/Entities/`

```csharp
public class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.HasData(
            new LeaveType()
            {
                Id = 1,
                DefaultDays = 10,
                Name = "Vacation",
                CreatedBy = "root-user",
                LastModifiedBy = "root-user",
            },
            new LeaveType
            {
                Id = 2,
                DefaultDays = 0,
                Name = "Sick",
                CreatedBy = "root-user",
                LastModifiedBy = "root-user",
            });
    }
}

```
- since we previously used the .Assembly we dont have to one by one include our configurations:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrDbContext).Assembly);
    // again the typeof(HrDbContext).Assembly catches all the IEntityTypeConfiguration<...>
    // so we dont have to model.Builder.ApplyConfiguration(new LeaveTypeConfiguration()); for each one
}
```
- so we can directly create our migrations and put them in the db:
```
dotnet ef --startup-project ./src/Api/HR.LeaveManagement.Api --project ./src/Infrastructure/HR.LeaveManagement.Persistence migrations add SeedintLeaveTypes
dotnet ef --startup-project ./src/Api/HR.LeaveManagement.Api --project ./src/Infrastructure/HR.LeaveManagement.Persistence database update
```

## Swagger Ui
- in our Program.cs we can add version info to the swagger ui (among other config options).
```csharp
builder.AddSwaggerGen(c =>
{
    // we can add info/terms-of-service/version to our swagger-endpoint
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hr LeaveManagement Api", Version = "v1" });
});
// ...
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();    // Exception -> html -> readable errors
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HR.LeaveManagement.Api v1"));
}
```

## The consumer of the API - a MVC-Webapp
Using Nswag to autogenerate most of the code.
- we can get the generated "json description" directly from the swagger ui
  ![openai_json.png](openai_json.png)


## Refactoring
- we can wrap our BeaseResponse arround our errors/success with dto object
```csharp
public async Task<BaseCommandResponse> Handle(CreateLeaveTypeCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();
        var validator = new CreateLeaveTypeDtoValidator();
        var validationResult = await validator.ValidateAsync(request.LeaveTypeDto, CancellationToken.None);

        if (validationResult.IsValid == false)
        {
            response.Success = false;
            response.Message = "Creation failed";
            response.Errors = validationResult.Errors.Select(q => q.ErrorMessage).ToList();
        }
        else
        {
            var newLeaveType = _mapper.Map<LeaveType>(request.LeaveTypeDto);
            response.Success = true;
            response.Message = "Creation Successful";
            response.Id = newLeaveType.Id;
        }
        return response;
    }
}

```

- in our controller we can use our BaseCommandResponse now instead of the int we used previously
- we also added some more info for swagger about produced Response codes
```csharp
// POST: api/LeaveType/
[HttpPost]
[ProducesResponseType(200)]
[ProducesResponseType(400)]
public async Task<ActionResult<BaseCommandResponse>> Post([FromBody] CreateLeaveTypeDto newLeaveType)
{
    var cmd = new CreateLeaveTypeCommand { LeaveTypeDto = newLeaveType };
    var response = await _mediator.Send(cmd);
    return Ok(response);
}
```