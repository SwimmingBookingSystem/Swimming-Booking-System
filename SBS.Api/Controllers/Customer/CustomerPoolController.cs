using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Customer.CustomerViewPoolList;
using SBS.Application.Features.Customer.CustomerViewPoolDetail;
using System;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Customer;

[ApiController]
[Route("api/customer/pools")]
[Authorize(Roles = "Customer")]
public class CustomerPoolController : ControllerBase
{
    private readonly ISender _mediator;

    public CustomerPoolController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetPools(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchName = null,
        [FromQuery] string? address = null,
        [FromQuery] string? openingTime = null,
        [FromQuery] string? closingTime = null)
    {
        TimeSpan? parsedOpening = null;
        if (!string.IsNullOrEmpty(openingTime))
        {
            if (!TimeSpan.TryParse(openingTime, out var ot))
            {
                return BadRequest(new { message = "Định dạng giờ mở cửa không hợp lệ. Vui lòng nhập định dạng hh:mm hoặc hh:mm:ss (Ví dụ: 08:30 hoặc 08:30:00)." });
            }
            parsedOpening = ot;
        }

        TimeSpan? parsedClosing = null;
        if (!string.IsNullOrEmpty(closingTime))
        {
            if (!TimeSpan.TryParse(closingTime, out var ct))
            {
                return BadRequest(new { message = "Định dạng giờ đóng cửa không hợp lệ. Vui lòng nhập định dạng hh:mm hoặc hh:mm:ss (Ví dụ: 21:00 hoặc 21:00:00)." });
            }
            parsedClosing = ct;
        }

        var query = new GetCustomerPoolsQuery(
            page, 
            pageSize, 
            searchName, 
            address, 
            parsedOpening, 
            parsedClosing
        );

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPoolDetail(int id)
    {
        var query = new GetCustomerPoolDetailQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
