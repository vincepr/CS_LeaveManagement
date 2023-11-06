using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR.LeaveManagement.Application.DTOs.LeaveType;
using HR.LeaveManagement.Application.Features.LeaveTypes.Requests.Commands;
using HR.LeaveManagement.Application.Features.LeaveTypes.Requests.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR.LeaveManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveTypeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LeaveTypeController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        // GET: api/LeaveType/
        [HttpGet]
        public async Task<ActionResult<List<LeaveTypeDto>>> Get()
        {
            var leaveTypes = await _mediator.Send(new GetLeaveTypeListRequest());
            return Ok(leaveTypes);
        }

        // GET: api/LeaveType/5
        [HttpGet("{id:int}", Name = "Get")]
        public async Task<ActionResult<LeaveTypeDto>> Get(int id)
        {
            return Ok(await _mediator.Send(new GetLeaveTypeDetailRequest { Id = id }));
        }

        // POST: api/LeaveType/
        [HttpPost]
        public async Task<ActionResult<int>> Post([FromBody] CreateLeaveTypeDto newLeaveType)
        {
            var cmd = new CreateLeaveTypeCommand { LeaveTypeDto = newLeaveType };
            var response = await _mediator.Send(cmd);
            return Ok(response);
        }

        // PUT: api/LeaveType/
        [HttpPut]
        public async Task<ActionResult<int>> Put([FromBody] LeaveTypeDto updateLeaveType)
        {
            var cmd = new UpdateLeaveTypeCommand { LeaveTypeDto = updateLeaveType };
            await _mediator.Send<Unit>(cmd); // has the Unit== "no-return-type"
            return NoContent();
        }

        // DELETE: api/LeaveType/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var cmd = new DeleteLeaveTypeCommand { Id = id };
            await _mediator.Send(cmd);
            return NoContent();
        }
    }
}
