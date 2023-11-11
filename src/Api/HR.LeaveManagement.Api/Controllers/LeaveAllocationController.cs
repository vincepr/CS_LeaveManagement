using HR.LeaveManagement.Application.DTOs.LeaveAllocation;
using HR.LeaveManagement.Application.Features.LeaveAllocations.Requests.Commands;
using HR.LeaveManagement.Application.Features.LeaveAllocations.Requests.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HR.LeaveManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveAllocationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LeaveAllocationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/LeaveAllocation
        [HttpGet]
        public async Task<ActionResult<List<LeaveAllocationDto>>> Get()
        {
            var leaveAllocations =
                await _mediator.Send<List<LeaveAllocationDto>>(new GetAllLeaveAllocationListRequest());
            return Ok(leaveAllocations);
        }

        // GET: api/LeaveAllocation/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveAllocationDto>> Get(int id)
        {
            var leaveAllocation = await _mediator.Send(new GetLeaveAllocationDetailRequest() { Id = id });
            return Ok(leaveAllocation);
        }

        // POST: api/LeaveAllocation
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateLeaveAllocationDto allocation)
        {
            var cmd = new CreateLeaveAllocationCommand() { CreateLeaveAllocationDto = allocation };
            var response = await _mediator.Send(cmd);
            return Ok(response);
        }

        // PUT: api/LeaveAllocation/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] UpdateLeaveAllocationDto allocation)
        {
            var cmd = new UpdateLeaveAllocationCommand() { LeaveAllocationDto = allocation };
            await _mediator.Send(cmd);
            return NoContent();
        }

        // DELETE: api/LeaveAllocation/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteLeaveAllocationCommand { Id = id });
            return NoContent();
        }
    }
}