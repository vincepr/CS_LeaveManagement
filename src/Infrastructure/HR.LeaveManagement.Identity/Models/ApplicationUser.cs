using Microsoft.AspNetCore.Identity;

namespace HR.LeaveManagement.Identity.Models;

// we extend the default User with a First/Lastname field
public class ApplicationUser : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}