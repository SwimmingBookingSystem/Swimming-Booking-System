using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using SBS.Application.Features.Contacts.Commands.CreateContactRequest;

namespace SBS.Api.Controllers.Customer;

[ApiController]
[Route("api/contacts")]
public class ContactController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContactController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateContactRequest([FromBody] CreateContactRequestCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Message = "Yêu cầu liên hệ của bạn đã được gửi thành công. Chúng tôi sẽ sớm phản hồi." });
    }
}
