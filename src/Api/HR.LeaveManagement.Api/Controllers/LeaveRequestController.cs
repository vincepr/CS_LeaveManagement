using HR.LeaveManagement.Application.DTOs.LeaveRequest;
using HR.LeaveManagement.Application.Features.LeaveRequests.Requests.Commands;
using HR.LeaveManagement.Application.Features.LeaveRequests.Requests.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HR.LeaveManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LeaveRequestController : ControllerBase
{
    private readonly IMediator _mediator;
    public LeaveRequestController(IMediator mediator)
    {
        _mediator = mediator;
    }
        
    // GET: api/LeaveRequest
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> Get()
    {
        var leaveRequests = await _mediator.Send(new GetLeaveRequestListRequest());
        return Ok(leaveRequests);
    }

    // GET: api/LeaveRequest/5
    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveRequestDto>> Get(int id)
    {
        return Ok(await _mediator.Send<LeaveRequestDto>(new GetLeaveRequestDetailRequest() { Id = id }));
    }

    // POST: api/LeaveRequest
    [HttpPost]
    public async Task<ActionResult> Post([FromBody] CreateLeaveRequestDto leaveRequest)
    {
        var cmd = new CreateLeaveRequestCommand() { CreateLeaveRequestDto = leaveRequest };
        return Ok(await _mediator.Send(cmd));
    }

    // PUT: api/LeaveRequest/5
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] UpdateLeaveRequestDto leaveRequest)
    {
        var cmd = new UpdateLeaveRequestCommand() { Id = id, UpdateLeaveRequestDto = leaveRequest };
        await _mediator.Send(cmd);
        return NoContent();
    }
        
    // PUT: api/LeaveRequest/change_approval     - changes to "approved"-status
    [HttpPut("change_approval/{id}")]
    public async Task<ActionResult> ChangeApproval(int id, [FromBody] ChangeLeaveRequestApprovalDto approvedRequest)
    {
        var cmd = new UpdateLeaveRequestCommand() {Id = id, ChangeLeaveRequestApprovalDto = approvedRequest };
        await _mediator.Send(cmd);
        return NoContent();
    }

    // DELETE: api/LeaveRequest/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var cmd = new DeleteLeaveRequestCommand() { Id = id };
        return Ok(await _mediator.Send(cmd));
    }
}