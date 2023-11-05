# Continue
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

Now we can hook up migrations for the api project
```
dotnet ef --startup-project ./src/Infrastructure/HR.LeaveManagement.Persistence migrations add initialMigration
dotnet ef --startup-project ./src/Infrastructure/HR.LeaveManagement.Persistence database update

```