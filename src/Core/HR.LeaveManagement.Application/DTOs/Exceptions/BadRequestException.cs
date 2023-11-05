namespace HR.LeaveManagement.Application.DTOs.Exceptions;

public class BadRequestException : ApplicationException
{
    public BadRequestException(string message) : base(message)
    {
        
    }
}