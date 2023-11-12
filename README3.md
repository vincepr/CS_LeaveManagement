# Part 3
## The consumer of the API - a MVC-Webapp
Using Nswag to autogenerate most of the code.
- we can get the generated "json description" directly from the swagger ui
![openai_json.png](openai_json.png)


## Refactoring
- we can wrap our BeaseResponse arround our errors/success with dto object
```csharp
public async Task<BaseCommandResponse> Handle(CreateLeaveTypeCommand request, CancellationToken cancellationToken)
    {
        var response = new BaseCommandResponse();
        var validator = new CreateLeaveTypeDtoValidator();
        var validationResult = await validator.ValidateAsync(request.LeaveTypeDto, CancellationToken.None);

        if (validationResult.IsValid == false)
        {
            response.Success = false;
            response.Message = "Creation failed";
            response.Errors = validationResult.Errors.Select(q => q.ErrorMessage).ToList();
        }
        else
        {
            var newLeaveType = _mapper.Map<LeaveType>(request.LeaveTypeDto);
            response.Success = true;
            response.Message = "Creation Successful";
            response.Id = newLeaveType.Id;
        }
        return response;
    }
}

```

- in our controller we can use our BaseCommandResponse now instead of the int we used previously
- we also added some more info for swagger about produced Response codes
```csharp
// POST: api/LeaveType/
[HttpPost]
[ProducesResponseType(200)]
[ProducesResponseType(400)]
public async Task<ActionResult<BaseCommandResponse>> Post([FromBody] CreateLeaveTypeDto newLeaveType)
{
    var cmd = new CreateLeaveTypeCommand { LeaveTypeDto = newLeaveType };
    var response = await _mediator.Send(cmd);
    return Ok(response);
}
```

- timestamp 4:04 