using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System;
using System.Threading.Tasks;
using SBS.Application.Features.Contacts.Commands.CreateContactRequest;
using SBS.Application.Features.Contacts.Queries.GetMyContactHistory;
using SBS.Application.Common.Interfaces;

namespace SBS.Api.Controllers.Customer;

[ApiController]
[Route("api/contacts")]
public class ContactController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ContactController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateContactRequest([FromBody] CreateContactRequestCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Message = "Yêu cầu liên hệ của bạn đã được gửi thành công. Chúng tôi sẽ sớm phản hồi." });
    }

    [HttpGet("my-history")]
    [Authorize]
    public async Task<IActionResult> GetMyContactHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var userId))
        {
            return Unauthorized(new { Message = "User ID không hợp lệ." });
        }

        var query = new GetMyContactHistoryQuery(userId, pageNumber, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
