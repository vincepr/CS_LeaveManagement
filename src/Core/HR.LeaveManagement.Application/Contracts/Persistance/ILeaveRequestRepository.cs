using HR.LeaveManagement.Domain;

namespace HR.LeaveManagement.Application.Contracts.Persistance;

public interface ILeaveRequestRepository : IGenericRepository<LeaveRequest>
{
    Task<LeaveRequest?> GetLeaveRequestWithDetails(int id);
    Task<List<LeaveRequest>> GetAllLeaveRequestsWithDetails();
    Task ChangeApprovalStatus(LeaveRequest leaveRequest, bool? approvalStatus);
}