namespace HR.LeaveManagement.Domain.Common;

/// <summary>
/// our min-requirement every table needs to have
/// </summary>
public abstract class BaseDomainEntity
{
    public int Id { get; set; }
    public DateTime DateCreated { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTime LastModifiedDate { get; set; }
    public string LastModifiedBy { get; set; } = null!;
}