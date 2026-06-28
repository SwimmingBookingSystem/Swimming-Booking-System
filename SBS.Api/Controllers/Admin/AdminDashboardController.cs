using MediatR;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Admin.Queries.GetDashboard;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
public class AdminDashboardController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminDashboardController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _mediator.Send(new GetDashboardQuery());
        return Ok(dashboard);
    }
}
