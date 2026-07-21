using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Public.Queries.GetHomepageData;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Public;

[ApiController]
[Route("api/public")]
[AllowAnonymous]
public class PublicController : ControllerBase
{
    private readonly ISender _mediator;

    public PublicController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("homepage")]
    public async Task<IActionResult> GetHomepageData()
    {
        var result = await _mediator.Send(new GetHomepageDataQuery());
        return Ok(result);
    }
}
