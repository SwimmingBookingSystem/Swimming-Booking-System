using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Staff.Commands.ResolveContactRequest;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Staff;

/// <summary>
/// Hỗ trợ khách hàng qua contact request - dành cho nhân viên lễ tân.
/// </summary>
[ApiController]
[Route("api/staff/contacts")]
[Authorize(Roles = "Staff,Manager,Admin")]
public class StaffContactController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IValidator<StaffResolveContactRequestCommand> _resolveValidator;

    public StaffContactController(
        ISender mediator,
        IValidator<StaffResolveContactRequestCommand> resolveValidator)
    {
        _mediator = mediator;
        _resolveValidator = resolveValidator;
    }

    /// <summary>
    /// POST /api/staff/contacts/{id}/resolve
    /// Đánh dấu đã xử lý xong yêu cầu hỗ trợ.
    /// </summary>
    [HttpPost("{id:int}/resolve")]
    public async Task<IActionResult> ResolveContactRequest([FromRoute] int id)
    {
        var command = new StaffResolveContactRequestCommand { ContactRequestId = id };

        var validationResult = await _resolveValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { errors });
        }

        var result = await _mediator.Send(command);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { message = "Yêu cầu hỗ trợ đã được đánh dấu là đã xử lý." });
    }
}
