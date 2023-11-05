namespace HR.LeaveManagement.Application.DTOs.Exceptions;

public class ValidationException : ApplicationException
{
    public List<string> Errors { get; set; } = new();
    public ValidationException(FluentValidation.Results.ValidationResult validationResult)
    {
        foreach( var error in validationResult.Errors)
            Errors.Add(error.ErrorMessage);
    } 
}