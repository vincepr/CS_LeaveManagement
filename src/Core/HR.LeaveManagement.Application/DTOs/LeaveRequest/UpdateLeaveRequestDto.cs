using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using HR.LeaveManagement.Application.DTOs.Common;

namespace HR.LeaveManagement.Application.DTOs.LeaveRequest;

public class UpdateLeaveRequestDto : BaseDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int LeaveTypeId { get; set; }
    public string RequestComments { get; set; } = null!;
    public bool Cannceled { get; set; }
}