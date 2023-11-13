# 3 Project HR.LeaveManagement - Clean Architecture
Dotnet Auth
## setup in the application project
- `HR.Application/Models/Identity/AuthRequest.cs`
```csharp
public class AuthRequest{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
```

- `HR.Application/Models/Identity/AuthResponse.cs`
```csharp
public class AuthResponse{
    public required string Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
}
```

- `HR.Application/Models/Identity/JwtSettings.cs`
```csharp
public class JwtSettings{
    public required string Key { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required double DurationInMinutes { get; set; }
}
```

- `HR.Application/Models/Identity/RegistrationRequest.cs`
```csharp
public class RegistrationRequest{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    [EmailAddress]
    public required string Email { get; set; }
    [MinLength(6)]
    public required string UserName { get; set; }
    [MinLength(6)]
    public required string Password { get; set; }
}
```

- `HR.Application/Models/Identity/RegistrationResponse.cs`
```csharp
public class RegistrationResponse{
    public required string UserId { get; set; }
}
```

- `HR.Application/Contracts/Identity/IAuthService.cs`
```csharp
public interface IAuthService{
    Task<AuthResponse> Login(AuthRequest request);
    Task<RegistrationResponse> Register(RegistrationRequest request);
} 
```

- `HR.Application/Models/Email/Email.cs`
```csharp
public class Email{
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}
```

- `HR.Application/Models/Email/EmailSettings.cs`
```csharp
public class EmailSettings{
    public string ApiKey { get; set; }
    public string FromAdress { get; set; }
    public string FromName { get; set; }
}
```
## the Infrastructure project
- `HR.Identity/Models/ApplicationUser.cs`
```csharp
// we extend the default User with a First/Lastname field
public class ApplicationUser : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}
```
- `HR.Identity/Configurations/xyzConfiguration.cs`
```csharp
public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                Id = "cac43a6e-f7bb-4448-baaf-1add431ccbbf",
                Name = "Employee",
                NormalizedName = "EMPLOYEE"
            },
            new IdentityRole
            {
                Id = "cbc43a8e-f7bb-4445-baaf-1add431ffbbf",
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR"
            }
        );
    }
}
public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        var hasher = new PasswordHasher<ApplicationUser>();
        builder.HasData(
            new ApplicationUser
            {
                Id = "8e445865-a24d-4543-a6c6-9443d048cdb9",
                Email = "admin@localhost.com",
                NormalizedEmail = "ADMIN@LOCALHOST.COM",
                FirstName = "System",
                LastName = "Admin",
                UserName = "admin@localhost.com",
                NormalizedUserName = "ADMIN@LOCALHOST.COM",
                PasswordHash = hasher.HashPassword(null, "P@ssword1"),
                EmailConfirmed = true
            },
            new ApplicationUser
            {
                Id = "9e224968-33e4-4652-b7b7-8574d048cdb9",
                Email = "user@localhost.com",
                NormalizedEmail = "USER@LOCALHOST.COM",
                FirstName = "System",
                LastName = "User",
                UserName = "user@localhost.com",
                NormalizedUserName = "USER@LOCALHOST.COM",
                PasswordHash = hasher.HashPassword(null, "P@ssword1"),
                EmailConfirmed = true
            }
        );
    }
}
public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
    {
        builder.HasData(
            new IdentityUserRole<string>
            {
                RoleId = "cbc43a8e-f7bb-4445-baaf-1add431ffbbf",
                UserId = "8e445865-a24d-4543-a6c6-9443d048cdb9"
            },
            new IdentityUserRole<string>
            {
                RoleId = "cac43a6e-f7bb-4448-baaf-1add431ccbbf",
                UserId = "9e224968-33e4-4652-b7b7-8574d048cdb9"
            }
        );
    }
}
```
- `HR.Identity/LeaveManagementDbContext`
```csharp
// gets it's own db context to separate it from the other db
public class LeaveManagementIdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public LeaveManagementIdentityDbContext(DbContextOptions<LeaveManagementIdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
    }
}
```
- `HR.Identity/IdentityServiceRegistration.cs`
```csharp
public static class IdentityServiceRegistration
{
    // hooks up this whole Auth Project in the API-Project
    public static IServiceCollection ConfigureIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddDbContext<LeaveManagementIdentityDbContext>(options => 
            options.UseSqlite("Data Source = ./hr_user_management_sqlite",
                b => b.MigrationsAssembly(typeof(LeaveManagementIdentityDbContext).Assembly.FullName)));

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<LeaveManagementIdentityDbContext>().AddDefaultTokenProviders();

        services.AddTransient<IAuthService, AuthService>();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidAudience = configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]))
                };
            });
        return services;
    }
}
```

## Adding a controller to our API-Project
- This controller
```csharp
using HR.LeaveManagement.Application.Contracts.Identity;
using HR.LeaveManagement.Application.Models.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HR.LeaveManagement.Api.Controllers;

public class AccountController : ControllerBase
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(AuthRequest request)
    {
        return Ok(await _authService.Login(request));
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegistrationResponse>> Register(RegistrationRequest request)
    {
        return Ok(await _authService.Register(request));
    }
}
```

- we can setup swagger to use auth in `Program.cs` of our api-project
```csharp
CustomSwaggerGenWithAuthEnabled(builder.Services);

void CustomSwaggerGenWithAuthEnabled(IServiceCollection services)
{
    services.AddSwaggerGen(c =>
    {
        // setup auth-info for swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\nEnter 
                            'Bearer' [space] and then the token ...",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        
        // enables swagger to enforce jwt
        c.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            { new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });
        
        
        // addition info about version/title for swagger ui
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hr LeaveManagement Api", Version = "v1" });
    });
}
// ...
// we also must add:
app.UseAuthorization();
app.UseAuthentication();
```

- we can now put on of our endpoints behind "authentication" via:
```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]     // this will lock the whole Controller behind auth
public class LeaveTypeController : ControllerBase
{
    // ...

    // GET: api/LeaveType/
    [HttpGet]
    [Authorize] // this will lock only this endpoint behind auth
    public async Task<ActionResult<List<LeaveTypeDto>>> Get()
    {
        var leaveTypes = await _mediator.Send(new GetLeaveTypeListRequest());
        return Ok(leaveTypes);
    }
```

- now after we create/migrate our auth db we can run our API with auth
```
dotnet ef --startup-project ./src/Api/HR.LeaveManagement.Api --project ./src/Infrastructure/HR.LeaveManagement.Identity migrations add AccountMigration --context LeaveManagementIdentityDbContext
dotnet ef --startup-project ./src/Api/HR.LeaveManagement.Api --project ./src/Infrastructure/HR.LeaveManagement.Identity  database update --context LeaveManagementIdentityDbContext
```