using System.ComponentModel.DataAnnotations;

namespace HR.LeaveManagement.Application.Models.Identity;

public class RegistrationRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    [EmailAddress]
    public required string Email { get; set; }
    [MinLength(6)]
    public required string UserName { get; set; }
    [MinLength(6)]
    public required string Password { get; set; }
}