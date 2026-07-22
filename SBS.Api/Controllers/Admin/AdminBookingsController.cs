using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Admin.Queries.GetBookings;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/bookings")]
public class AdminBookingsController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminBookingsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetBookings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] string? fromDate = null,
        [FromQuery] string? toDate = null)
    {
        var result = await _mediator.Send(new GetBookingsQuery(page, pageSize, status, search, fromDate, toDate));
        return Ok(result);
    }
}
