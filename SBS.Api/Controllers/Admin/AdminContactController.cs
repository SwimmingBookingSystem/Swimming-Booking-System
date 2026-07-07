using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Admin.Commands.RespondContactRequest;
using SBS.Application.Features.Admin.Queries.GetContactRequests;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/contacts")]
public class AdminContactController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IValidator<AdminRespondContactRequestCommand> _respondValidator;

    public AdminContactController(
        ISender mediator,
        IValidator<AdminRespondContactRequestCommand> respondValidator)
    {
        _mediator = mediator;
        _respondValidator = respondValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetContactRequests()
    {
        var contacts = await _mediator.Send(new GetContactRequestsQuery());
        return Ok(contacts);
    }

    [HttpPost("{id:int}/respond")]
    public async Task<IActionResult> RespondContactRequest(
        [FromRoute] int id,
        [FromBody] AdminRespondContactRequestCommand command)
    {
        if (id != command.ContactRequestId)
            return BadRequest(new { errors = new[] { "ContactRequestId trong URL không khớp với body." } });

        var validationResult = await _respondValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });

        var result = await _mediator.Send(command);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { message = "Phản hồi yêu cầu hỗ trợ thành công. Email đã được gửi tới khách hàng." });
    }
}
