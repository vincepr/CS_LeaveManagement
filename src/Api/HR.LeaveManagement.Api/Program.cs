using HR.LeaveManagement.Application;
using HR.LeaveManagement.Infrastructure;
using HR.LeaveManagement.Persistence;

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