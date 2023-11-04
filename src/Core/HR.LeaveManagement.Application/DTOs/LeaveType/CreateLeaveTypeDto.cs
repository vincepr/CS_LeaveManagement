namespace HR.LeaveManagement.Application.DTOs.LeaveType;

public class CreateLeaveTypeDto
{
    public string Name { get; set; } = null!;
    public int DefaultDays { get; set; }
}